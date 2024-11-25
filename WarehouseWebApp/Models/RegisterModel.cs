namespace WarehouseWebApp.Models;

public record RegisterModel : LoginModel
{
    public string fullName { get; set; }
    public string confirmPassword { get; set; }
}