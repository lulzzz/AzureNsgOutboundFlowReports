using AzureNetSec.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureNetSec
{
    public class FilterNsgFlowEvents
    {
        public static async void Run(string nsgFlowBlob, ICollector<string> outputQueueItem, TraceWriter log,
            IQueryable<NetworkFlowProcessStatus> updateStatusIn, CloudTable updateStatusOut )
        {
            HashSet<NetworkFlow> outboundMsgList = new HashSet<NetworkFlow>();
            NsgFlowEvents nsgEvents = JsonConvert.DeserializeObject<NsgFlowEvents>(nsgFlowBlob);
            string nsg = null;
            DateTime lastProcessed = new DateTime();
            NetworkFlowProcessStatus flowStatusRecord = new NetworkFlowProcessStatus();

            foreach (NsgFlowEvents.Record flowEvent in nsgEvents.records)
            {
                DateTime flowCaptureTime = DateTime.Parse(flowEvent.time);

                // Idenfity the NSG flow log being processed so we can get the last process time from the table
                // This function is triggered on new or *updated* NSG flow log files. Multiple functions could be processing different files
                // in parallel. Uniquely identify each file by the first event timestamp and record the "last processed" records in table storage
                // using the first event timestamp as the primary key. This way, network flows are not processed more than once.
                if (string.IsNullOrEmpty(nsg))
                {
                    nsg = flowEvent.resourceId;
                    flowStatusRecord = updateStatusIn.Where(l => l.RowKey == flowCaptureTime.Ticks.ToString()).FirstOrDefault();

                    if(flowStatusRecord == null)
                    {
                        flowStatusRecord = new NetworkFlowProcessStatus
                        {
                            LastProcessedTime = flowCaptureTime.ToLongTimeString(),
                            PartitionKey = "trniel",
                            RowKey = flowCaptureTime.Ticks.ToString()
                        };

                        log.Info($"No processing history found");
                    }   
                    else
                    {
                        lastProcessed = DateTime.Parse(flowStatusRecord.LastProcessedTime);
                        log.Info($"NSG flow last processed event: {lastProcessed.ToLongTimeString()}");
                    }       
                }
                
                // Skip records we already processed
                if (flowCaptureTime > lastProcessed)
                {
                    foreach (NsgFlowEvents.Flows flowCollection in flowEvent.properties.flows)
                    {
                        if (flowCollection.rule == "DefaultRule_AllowInternetOutBound")
                        {
                            foreach (NsgFlowEvents.Flow flowItem in flowCollection.flows)
                            {
                                string[] tupleItems = flowItem.flowTuples;

                                foreach (string tuple in tupleItems)
                                {
                                    string[] tupleData = tuple.Split(',');

                                    NetworkFlow msg = new NetworkFlow
                                    {
                                        NsgID = flowEvent.resourceId,
                                        DestIP = tupleData[2],
                                        DestPort = tupleData[4],
                                        Protocol = tupleData[5]
                                    };

                                    outboundMsgList.Add(msg);
                                }
                            }
                        }
                    }

                    lastProcessed = flowCaptureTime;
                }
            }
            
            string messageText = JsonConvert.SerializeObject(outboundMsgList);
            outputQueueItem.Add(messageText);

            // Update the processing status table to we don't process these flows again once the flow log is updated
            flowStatusRecord.LastProcessedTime = lastProcessed.ToLongTimeString();
            TableOperation operation = TableOperation.InsertOrReplace(flowStatusRecord);
            updateStatusOut.Execute(operation);
        }
    }
}