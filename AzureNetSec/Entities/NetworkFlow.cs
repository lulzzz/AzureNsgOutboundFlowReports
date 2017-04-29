using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureNetSec.Entities
{
    public class NetworkFlow
    {
        public string NsgID;
        public string SourceIP;
        public string DestIP;
        public string DestPort;
        public string Protocol;
        public override bool Equals(object obj)
        {
            NetworkFlow msg = obj as NetworkFlow;
            return msg != null && msg.SourceIP == this.SourceIP && msg.DestIP == this.DestIP && msg.DestPort == this.DestPort;
        }

        public override int GetHashCode()
        {
            return this.SourceIP.GetHashCode() ^ this.DestIP.GetHashCode() ^ this.DestPort.GetHashCode();
        }
    }
}