using System;

namespace RelayServer.Server
{
    [Serializable]
    public class DomainLink
    {
        public string DomainName { get; set; } = "doma.in";

        public string LocalIp { get; set; } = "127.0.0.1:80";
    }
}