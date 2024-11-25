using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WarehouseWebApp.Entities;

[Table("user")]
public class User : IdentityUser
{

    [Required]
    [Column("fullName")]
    public string fullName { get; set; }
    
    [Column("active")]
    public bool active { get; set; } = true;

    [Column("created_at")]
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    [Column("last_login_at")]
    public DateTime? lastLogin { get; set; }

}