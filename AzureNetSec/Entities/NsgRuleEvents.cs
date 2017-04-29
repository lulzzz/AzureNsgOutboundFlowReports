using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureNetSec.Entities
{
    public class NsgRuleEvents
    {
        public Record[] records;

        public class Record
        {
            public string time;
            public string systemId;
            public string category;
            public string resourceId;
            public string operationName;
            public Properties properties;
        }

        public class Properties
        {
            public string vnetResourceGuid;
            public string subnetPrefix;
            public string macAddress;
            public string primaryIPv4Address;
            public string ruleName;
            public string direction;
            public string priority;
            public string type;
            public Conditions conditions;
        }

        public class Conditions
        {
            public string protocols;
            public string destinationPortRange;
            public string destinationIP;
            public string sourceIP;
        }
    }
}

/*
      "time": "2017-04-28T03:11:55.6840000Z",
      "systemId": "6d00e2cc-a873-42dc-bf71-d7defcb2370a",
      "category": "NetworkSecurityGroupEvent",
      "resourceId": "/SUBSCRIPTIONS/CECEA5C9-0754-4A7F-B5A9-46AE68DCAFD3/RESOURCEGROUPS/NSG-SEN-PROD/PROVIDERS/MICROSOFT.NETWORK/NETWORKSECURITYGROUPS/APIPOC-WEB",
      "operationName": "NetworkSecurityGroupEvents",
      "properties": {
        "vnetResourceGuid": "{9D350862-7525-4AC9-9E06-8E44A452CC8C}",
        "subnetPrefix": "10.1.0.0/24",
        "macAddress": "00-0D-3A-60-43-C3",
        "primaryIPv4Address": "10.1.0.9",
        "ruleName": "UserRule_Allow_Azure_Out_104.43.128.0-17",
        "direction": "Out",
        "priority": 2833,
        "type": "allow",
        "conditions": {
          "protocols": "6",
          "destinationPortRange": "0-65535",
          "sourcePortRange": "0-65535",
          "destinationIP": "104.43.128.0/17",
          "sourceIP": "0.0.0.0/0"
        }
      }
*/