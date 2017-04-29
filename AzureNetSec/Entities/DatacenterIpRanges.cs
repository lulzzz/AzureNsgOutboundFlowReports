using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureNetSec.Entities
{
    public class DatacenterIpRanges : TableEntity
    {
        public string Subnet { get; set; }
    }
}