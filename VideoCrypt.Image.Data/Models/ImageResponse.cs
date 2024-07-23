namespace VideoCrypt.Image.Data.Models;

public class ImageResponse
{
    public bool success { get; set; }
    public int statusCode { get; set; }
    public DateTime timestamp { get; set; }
    public int timeMs { get; set; }
    public Data data { get; set; }
}