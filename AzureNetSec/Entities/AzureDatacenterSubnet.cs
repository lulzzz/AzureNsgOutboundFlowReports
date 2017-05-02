using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureNetSec.Entities
{
    public class AzureDatacenterSubnet
    {
        public string Partition { get; set; }
        public string Subnet { get; set; }
        public System.Net.IPNetwork Network { get; set; }
    }
}