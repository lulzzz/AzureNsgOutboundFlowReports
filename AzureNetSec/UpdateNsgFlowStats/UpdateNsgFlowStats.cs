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
                IPAddress destAddress = IPAddress.Parse(flowItem.DestIP);
                bool flowItemMatched = false;

                IEnumerable<NsgFlowStats> nsgStats = nsgFlowStatsIn.Where(x => x.PartitionKey == flowItem.NsgID.Split('/')[8]);

                foreach (AzureDatacenterSubnet azureNetwork in azureDcList)
                {
                    if (IPNetwork.Contains(azureNetwork.Network, destAddress))
                    {
                        NsgFlowStats nsgStatsItem = nsgStats.Where(x => x.DestIP == flowItem.DestIP && x.DestPort == flowItem.DestPort).FirstOrDefault();

                        if (nsgStatsItem == null)
                        {
                            nsgStatsItem = new NsgFlowStats
                            {
                                PartitionKey = flowItem.NsgID.Split('/')[8],
                                RowKey = Guid.NewGuid().ToString(),
                                DestIP = flowItem.DestIP,
                                DestPort = flowItem.DestPort,
                                DestProtocol = flowItem.Protocol,
                                AzureDatacenter = azureNetwork.Partition,
                                AzureSubnet = azureNetwork.Subnet
                            };
                        }

                        TableOperation operation = TableOperation.InsertOrReplace(nsgStatsItem);
                        nsgFlowStatsOut.Execute(operation);

                        log.Info($"{flowItem.DestIP}:{flowItem.DestPort} is in Azure subnet {azureNetwork.Network.ToString()}");
                        flowItemMatched = true;
                        break;
                    }
                }

                if (flowItemMatched == false)
                {
                    NsgFlowStats nsgStatsItem = nsgStats.Where(x => x.DestIP == flowItem.DestIP && x.DestPort == flowItem.DestPort).FirstOrDefault();

                    if (nsgStatsItem == null)
                    {
                        nsgStatsItem = new NsgFlowStats
                        {
                            PartitionKey = flowItem.NsgID.Split('/')[8],
                            RowKey = Guid.NewGuid().ToString(),
                            DestIP = flowItem.DestIP,
                            DestPort = flowItem.DestPort,
                            DestProtocol = flowItem.Protocol,
                            AzureDatacenter = "INTERNET",
                            AzureSubnet = ""
                        };
                    }

                    TableOperation operation = TableOperation.InsertOrReplace(nsgStatsItem);
                    nsgFlowStatsOut.Execute(operation);
                    
                    log.Info($"{flowItem.DestIP}:{flowItem.DestPort} is INTERNET");
                }
            }
        }
    }
}