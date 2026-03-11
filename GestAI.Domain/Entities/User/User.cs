using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestAI.Domain.Entities;

public class User : IdentityUser
{
    [Required]
    public string Nombre { get; set; } = null!;

    [Required]
    public string Apellido { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAtUtc { get; set; }
    public int? DefaultPropertyId { get; set; }
    public int DefaultAccountId { get; set; }
}
