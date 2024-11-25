using System.ComponentModel.DataAnnotations;

namespace WarehouseWebApp.Models;

public record LoginModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string email { get; init; }

    [Required(ErrorMessage = "Password is required.")]
    public string password { get; init; }
}