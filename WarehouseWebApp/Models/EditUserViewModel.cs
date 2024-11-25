namespace WarehouseWebApp.Models;

public class EditUserViewModel
{
    public string userId { get; set; }  
    public string email { get; set; }  
    public string userName { get; set; }   
    public string fullName { get; set; }   
    public string? newPassword { get; set; }
}