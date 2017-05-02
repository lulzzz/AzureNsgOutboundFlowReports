using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureNetSec.Entities
{
    public class NetworkFlow
    {
        public string NsgID;
        // public string SourceIP;
        public string DestIP;
        public string DestPort;
        public string Protocol;
        public override bool Equals(object obj)
        {
            NetworkFlow netflow = obj as NetworkFlow;
            return netflow != null && netflow.NsgID == this.NsgID && netflow.DestIP == this.DestIP && netflow.DestPort == this.DestPort;
        }

        public override int GetHashCode()
        {
            return this.NsgID.GetHashCode() ^ this.DestIP.GetHashCode() ^ this.DestPort.GetHashCode();
        }
    }
}