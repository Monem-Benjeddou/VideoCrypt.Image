using System;
using System.ComponentModel.DataAnnotations;

namespace VideoCrypt.Image.Dashboard.Models;

public class ApiKeyForCreation
{
    [Required]
    public string Name { get; set; } = "Api Key";
    [Required]
    public string? Description { get; set; }
    [Required]
    public DateTime? ExpireAt { get; set; }
}