using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureNetSec.Entities
{
    public class DatacenterIpMatchMessage
    {
        public string nsg;
        public string datacenterName;
        public string subnet;
        public string sourceAddress;
    }
}