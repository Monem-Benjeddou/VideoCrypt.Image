using System.ComponentModel.DataAnnotations;

namespace VideoCrypt.Image.Data.Models;

public class ImageMetadata
{
    public int Id { get; set; }

    [Required]
    public string FileName { get; set; }

    [Required]
    public string CachedFilePath { get; set; }

    [Required]
    public string Url { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
}