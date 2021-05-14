using InfoWriterWebSocketClient.Client.Enums;

namespace InfoWriterWebSocketClient.Client.Models
{
    public class Update
    {
        public FrameMessageEnum Frame { get; set; }
        public string Payload { get; set; }
        public long PayloadLength { get; set; }
        public byte[] Mask { get; set; }
        public bool IsFullUpdate { get; set; }
    }
}
