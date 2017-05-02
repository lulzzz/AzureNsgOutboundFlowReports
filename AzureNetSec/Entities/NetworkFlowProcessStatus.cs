using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureNetSec.Entities
{
    public class NetworkFlowProcessStatus : TableEntity
    {
        public string LastProcessedTime { get; set; }
    }
}