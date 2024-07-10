using System.Text.Json.Serialization;

namespace VideoCrypt.Image.Data.Models;

public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
}