using System.Net;

namespace Status.Models
{
    public class MuebWithIp
    {
        public int MuebId { get; set; }
        public IPAddress IpAddress { get; set; }
        public bool IpConflict { get; set; }
        public int RoomId { get; set; }
    }
}