namespace VideoCrypt.Image.Main.Models;

public class ApiKeyForCreation
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpireAt { get; set; }
}