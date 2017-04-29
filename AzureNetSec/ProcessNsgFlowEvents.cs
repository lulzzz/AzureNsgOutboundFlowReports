using AzureNetSec.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureNetSec
{
    public class ProcessNsgFlowEvents
    {
        public static async void Run(string nsgFlowBlob, ICollector<string> outputQueueItem, TraceWriter log)
        {
            HashSet<NetworkFlow> outboundMsgList = new HashSet<NetworkFlow>();
            NsgFlowEvents nsgEvents = JsonConvert.DeserializeObject<NsgFlowEvents>(nsgFlowBlob);

            foreach(NsgFlowEvents.Record flowEvent in nsgEvents.records)
            {
                foreach(NsgFlowEvents.Flows flowCollection in flowEvent.properties.flows)
                {
                    if (flowCollection.rule == "DefaultRule_AllowInternetOutBound")
                    {
                        foreach(NsgFlowEvents.Flow flowItem in flowCollection.flows )
                        {
                            string[] tupleItems = flowItem.flowTuples;

                            foreach(string tuple in tupleItems)
                            {
                                string[] tupleData = tuple.Split(',');

                                NetworkFlow msg = new NetworkFlow
                                {
                                    NsgID = flowEvent.resourceId,
                                    SourceIP = tupleData[1],
                                    DestIP = tupleData[2],
                                    DestPort = tupleData[4],
                                    Protocol = tupleData[5]
                                 };

                                outboundMsgList.Add(msg);
                            }
                        }
                    }
                }                
            }

            NetworkFlowMessage message = new NetworkFlowMessage { NetworkFlows = outboundMsgList.ToArray() };
            string messageText = JsonConvert.SerializeObject(message);
            outputQueueItem.Add(messageText);

        }
    }
}