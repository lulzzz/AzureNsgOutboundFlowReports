using System;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using AzureNetSec.Entities;

namespace AzureNetSec
{
    public class UpdateAzureIpRanges
    {
        // public static async void Run(TimerInfo myTimer, TraceWriter log, ICollection<DatacenterIpRange> outputTable)
        public static async void Run(TimerInfo myTimer, TraceWriter log, CloudTable outputTable)
        {
            // Load the datacenter IP ranges from the web
            HttpClient client = new HttpClient();
            HttpResponseMessage downloadPageResponse = await client.GetAsync("https://www.microsoft.com/en-us/download/confirmation.aspx?id=41653");

            if (!downloadPageResponse.IsSuccessStatusCode)
            {
                log.Error("Failure to access download page.");
                return;
            }

            string downloadPageText = await downloadPageResponse.Content.ReadAsStringAsync();
            string xmlDownloadLink = downloadPageText.Split('"').Where(x => x.Contains("/PublicIPs_")).FirstOrDefault();
            HttpResponseMessage datacenterIpXmlResp = await client.GetAsync(xmlDownloadLink);
            
            if(!datacenterIpXmlResp.IsSuccessStatusCode)
            {
                log.Error("Failure to get datacanter IP XML");
                return;
            }

            string resultXml = await datacenterIpXmlResp.Content.ReadAsStringAsync();
            XDocument doc = XDocument.Parse(resultXml);

            foreach (XElement region in doc.Element("AzurePublicIpAddresses").Elements("Region"))
            {
                string partition = region.Attribute("Name").Value;
                log.Info($"Processing: {partition}");

                foreach (XElement ipRange in region.Elements("IpRange"))
                {
                    string subnet = ipRange.Attribute("Subnet").Value;

                    /*
                    outputTable.Add(new DatacenterIpRange
                    {
                        PartitionKey = partition,
                        RowKey = Guid.NewGuid().ToString(),
                        Subnet = subnet
                    });
                    */

                    TableOperation insertOperation = TableOperation.Insert(new DatacenterIpRanges
                    {
                        PartitionKey = partition,
                        RowKey = Guid.NewGuid().ToString(),
                        Subnet = subnet
                    });

                    try
                    {
                        TableResult writeResult = outputTable.Execute(insertOperation);
                        // log.Info($"Added subnet range: {subnet}");
                    }
                    catch(Exception ex)
                    {
                        log.Error(ex.Message);
                    }
                }
            }

        }

    }
}