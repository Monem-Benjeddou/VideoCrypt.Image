using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("api_keys")]
public class ApiKey
{
    [Key]
    public int Id { get; set; }

    [Column("key")]
    public string Key { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("expire_at")]
    public DateTime? ExpireAt { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } 

    public IdentityUser User { get; set; }
}