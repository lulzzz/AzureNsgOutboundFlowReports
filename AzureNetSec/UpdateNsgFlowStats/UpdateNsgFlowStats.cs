using AzureNetSec.Entities;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;

namespace AzureNetSec
{
    public class UpdateNsgFlowStats
    {
        public static async void Run(string nsgFlowsIn, IQueryable<DatacenterIpRanges> azureNetworksIn, IQueryable<NsgFlowStats> nsgFlowStatsIn, 
            CloudTable nsgFlowStatsOut, TraceWriter log )
        {
            IEnumerable<NetworkFlow> nsgFlows = JsonConvert.DeserializeObject<IEnumerable<NetworkFlow>>(nsgFlowsIn);
            string nsgId = nsgFlows.FirstOrDefault().NsgID.Split('/')[8];
            IEnumerable<NsgFlowStats> nsgStats = nsgFlowStatsIn.Where(x => x.PartitionKey == nsgId);

            // Filter input table on only US datacenter networks
            // NOTE: Table storate does not support certain LINQ queries such as 'StartsWith and Contains'
            // useast, useast2, uswest, usnorth, uswest2, uscentral, ussouth, uswestcentral, uscentraleuap, useast2euap 
            // https://feedback.azure.com/forums/217298-storage/suggestions/415607-add-tablestorage-linq-query-support-for-select-co
            IEnumerable<DatacenterIpRanges> dcList = azureNetworksIn.Where(x => x.PartitionKey == "useast" || x.PartitionKey== "useast2"
                || x.PartitionKey == "uswest" || x.PartitionKey == "usnorth" || x.PartitionKey == "uswest2" || x.PartitionKey == "uscentral"
                || x.PartitionKey == "ussouth" || x.PartitionKey == "uswestcentral" || x.PartitionKey == "uscentraleuap" || x.PartitionKey == "useast2euap");

            // Create new list of DC subnets that contain an IPNetwork object for calculations
            List<AzureDatacenterSubnet> azureDcList = new List<AzureDatacenterSubnet>();
            foreach (DatacenterIpRanges dcSubnet in dcList)
            {
                azureDcList.Add(new AzureDatacenterSubnet
                {   Network = IPNetwork.Parse(dcSubnet.Subnet),
                    Partition = dcSubnet.PartitionKey,
                    Subnet = dcSubnet.Subnet
                });
            }

            foreach (NetworkFlow flowItem in nsgFlows)
            {
                NsgFlowStats nsgStatsItem = null;

                try
                {
                    nsgStatsItem = nsgStats.Where(x => x.DestIP == flowItem.DestIP && x.DestPort == flowItem.DestPort).First();
                }
                catch(Exception ex)
                {
                    log.Warning($"Outbound IP address {flowItem.DestIP}:{flowItem.DestPort} not found in stats table");
                }

                if (nsgStatsItem != null)
                {
                    // Perform an update of the item so that we get an updated timestamp (i.e. last recoreded)
                    TableOperation operation = TableOperation.InsertOrReplace(nsgStatsItem);
                    nsgFlowStatsOut.Execute(operation);
                }
                else
                {
                    // we have not seen this destination IP address / port before. See if it matches an Azure datacenter subnet and add a record about it
                    IPAddress destAddress = IPAddress.Parse(flowItem.DestIP);
                    bool flowItemMatchesAzureSubnet = false;

                    nsgStatsItem = new NsgFlowStats
                    {
                        PartitionKey = nsgId,
                        RowKey = Guid.NewGuid().ToString(),
                        DestIP = flowItem.DestIP,
                        DestPort = flowItem.DestPort,
                        DestProtocol = flowItem.Protocol
                    };

                    foreach (AzureDatacenterSubnet azureNetwork in azureDcList)
                    {
                        if (IPNetwork.Contains(azureNetwork.Network, destAddress))
                        {
                            nsgStatsItem.AzureDatacenter = azureNetwork.Partition;
                            nsgStatsItem.AzureSubnet = azureNetwork.Subnet;
                            log.Info($"{flowItem.DestIP}:{flowItem.DestPort} is in Azure subnet {azureNetwork.Network.ToString()}");
                            flowItemMatchesAzureSubnet = true;
                            break;
                        }
                    }

                    if (flowItemMatchesAzureSubnet == false)
                    {
                        nsgStatsItem.AzureDatacenter = "INTERNET";
                        nsgStatsItem.AzureSubnet = "";
                        log.Info($"{flowItem.DestIP}:{flowItem.DestPort} is INTERNET");
                    }  

                    TableOperation operation = TableOperation.InsertOrReplace(nsgStatsItem);
                    nsgFlowStatsOut.Execute(operation);
                }
            }
        }
    }
}