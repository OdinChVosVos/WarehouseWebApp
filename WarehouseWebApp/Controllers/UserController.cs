using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WarehouseWebApp.Data;
using WarehouseWebApp.Entities;
using WarehouseWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WarehouseWebApp.Controllers;


[Controller]
[Route("[controller]")]
public class UserController(DataContext context, UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    
    [HttpGet("[action]")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var userModels = context.Users.Select(u => new UserModel
            (
                u.Id,
                u.UserName,
                u.fullName,
                u.Email,
                u.createdAt,
                u.lastLogin,
                context.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Select(ur => ur.RoleId)
                    .Join(context.Roles, 
                        roleId => roleId, 
                        r => r.Id,
                        (roleId, r) => r.Name)
                    .FirstOrDefault(),
                u.active
            )).ToList();
            
            var roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.Roles = roles;

            return View(userModels);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while fetching users.");
            return View(new List<UserModel>());
        }
    }
    
    
    [HttpPost("[action]")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRole(string userId, [FromForm] string newRoleId)
    {

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден";
            return RedirectToAction("GetAllUsers");
        }

        // Ensure the role ID exists in the system
        var role = await roleManager.FindByNameAsync(newRoleId);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Роль не найдена";
            return RedirectToAction("GetAllUsers");
        }

        // Update user's role
        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, role.Name ?? throw new InvalidOperationException());
        
        return RedirectToAction("GetAllUsers");
    }
    
    [HttpGet("[action]")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден";
            return RedirectToAction("GetAllUsers");
        }
        
        var model = new EditUserViewModel
        {
            userId = user.Id,
            email = user.Email,
            userName = user.UserName
        };

        return View(model);
    }

    [HttpPost("[action]")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByIdAsync(model.userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден";
            return RedirectToAction("GetAllUsers");
        }
        if (user.UserName == "Admin")
        {
            TempData["ErrorMessage"] = "Пользователь root админ";
            return RedirectToAction("GetAllUsers");
        }
        
        user.Email = model.email;
        user.UserName = model.userName;
        user.fullName = model.fullName;

        if (!string.IsNullOrEmpty(model.newPassword))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, model.newPassword);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Не удалось изменить пароль");
                return View(model);
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Не удалось обновить пользователя");
            return View(model);
        }

        TempData["SuccessMessage"] = "Данные пользователя успешно обновлены!";
        return RedirectToAction("GetAllUsers");
    }
    
    [HttpPost("[action]")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден";
            return RedirectToAction("GetAllUsers");
        }
        if (user.UserName == "Admin")
        {
            TempData["ErrorMessage"] = "Пользователь root админ";
            return RedirectToAction("GetAllUsers");
        }

        // Mark the user as deleted (logical deletion)
        user.active = false;

        var result = await userManager.UpdateAsync(user);
    
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Не удалось удалить пользователя";
            return RedirectToAction("GetAllUsers");
        }
        
        await userManager.UpdateSecurityStampAsync(user);

        TempData["SuccessMessage"] = "Пользователь успешно удалён";
        return RedirectToAction("GetAllUsers");
    }
    
    [HttpPost("[action]")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RestoreUser(string userId)
    {
        // Retrieve the user by ID
        var user = await userManager.FindByIdAsync(userId);

        // Check if the user exists
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден.";
            return RedirectToAction("GetAllUsers");
        }

        // Check if the user is already active
        if (user.active)
        {
            TempData["ErrorMessage"] = "Пользователь уже активен";
            return RedirectToAction("GetAllUsers");
        }

        // Mark the user as active
        user.active = true;

        // Update the user in the database
        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Пользователь успешно восстановлен";
        }
        else
        {
            TempData["ErrorMessage"] = "Не удалось восстановить пользователя. Попробуйте позже";
        }

        return RedirectToAction("GetAllUsers");
    }
        
}