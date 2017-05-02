using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureNetSec.Entities
{
    public class NsgFlowStats : TableEntity
    {
        public string DestIP { get; set; }
        public string DestPort { get; set; }
        public string DestProtocol { get; set; }
        public string AzureDatacenter { get; set; }
        public string AzureSubnet { get; set; }
    }
}