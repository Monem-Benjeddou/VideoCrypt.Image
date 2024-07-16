namespace VideoCrypt.Image.Main.Models;

public class ApiKeyForCreation
{
    public string Name { get; set; } = "Api Key";
    public string? Description { get; set; }
    public DateTime? ExpireAt { get; set; }
}