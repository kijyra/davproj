using System;
using System.Collections.Generic;
using System.Text;

namespace ControlManager.DTO
{
    public class WorkstationDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string User { get; set; }
        public string AdUser { get; set; }
        public string Address { get; set; }
        public bool IsDomain { get; set; }
        public bool IsThink { get; set; }

        public ConnectionLinks ProtocolLinksVNC { get; set; }
        public ConnectionLinks ProtocolLinksRDP { get; set; }
        public ConnectionLinks ProtocolLinksWTRC { get; set; }
    }

    public class ConnectionLinks
    {
        public string Full { get; set; }
        public string View { get; set; }
    }
}

