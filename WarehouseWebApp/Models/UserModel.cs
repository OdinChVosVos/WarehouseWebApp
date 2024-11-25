namespace WarehouseWebApp.Models;

public record UserModel(
    string userId, 
    string name,
    string fullName,
    string email, 
    DateTime createdAt,
    DateTime? lastLoginAt,
    string? roleName,
    bool active);