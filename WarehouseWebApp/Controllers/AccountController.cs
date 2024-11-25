
using WarehouseWebApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WarehouseWebApp.Models;

namespace WarehouseWebApp.Controllers;


public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    
    
    public AccountController(UserManager<User> user, SignInManager<User> signIn)
    {
        _userManager = user;
        _signInManager = signIn;
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.email);
        if (user != null && user.active && await _userManager.CheckPasswordAsync(user, model.password))
        {
            var result = await _signInManager.PasswordSignInAsync(user, model.password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        ModelState.AddModelError(string.Empty, "Неудачная попытка входа");
        return View(model);
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create a new IdentityUser object
        var user = new User
        {
            UserName = model.email,
            Email = model.email,
            fullName = model.fullName,
        };

        // Create the user in the database with the provided password
        var result = await _userManager.CreateAsync(user, model.password);

        if (result.Succeeded)
        {
            // Optional: Add the user to a default role
            await _userManager.AddToRoleAsync(user, "User");

            // Automatically sign the user in after registration
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        // Add any errors to the ModelState to display them in the view
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
    
    
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
    
    
        
}