using System.Runtime.InteropServices.JavaScript;
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
public class GoodsController(UserManager<User> userManager, DataContext context)
    : Controller
{


    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllGoods()
    {
        try
        {
            var goodsModels = context.Goods.Select(g => new GoodsModel
            (
                g.id,
                g.description,
                g.name,
                g.quantity,
                g.costPerUnit
            )).ToList();

            return View(goodsModels);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while fetching Goods.");
            return View(new List<GoodsModel>());
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult MakingOrder(long goodsId)
    {
        var goods = context.Goods.FirstOrDefault(g => g.id == goodsId);
        if (goods == null)
        {
            ModelState.AddModelError(string.Empty, "Товар не найден");
            return RedirectToAction("GetAllGoods", "Goods");
        }
    
        var makingOrderViewModel = new MakingOrderViewModel(
            goodsId,
            new DateTime().AddDays(7).ToString("yyyy-MM-dd"),
            100
            );
        
        return View(makingOrderViewModel);
    }
    
    
    [HttpPost("[action]")]
    [Authorize]
    public async Task<IActionResult> OrderGoods(MakingOrderViewModel model)
    {
        // Validate request
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Получены не корректные значения";
            return RedirectToAction("MakingOrder", "Goods", new { goodsId = model.goodsId });
        }
    
        // Get the current user
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Пользователь не найден";
            return RedirectToAction("MakingOrder", "Goods", new { goodsId = model.goodsId });
        }
    
        var goods = await context.Goods.FindAsync(model.goodsId);
        try
        {
            if (goods == null && !CheckQuantity(model))
            {
                TempData["ErrorMessage"] = "Товар не найден или мы не можем доставить такое количество товара";
                return RedirectToAction("MakingOrder", "Goods", new { goodsId = model.goodsId });
            }
        }
        catch (NullReferenceException e)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            return RedirectToAction("GetAllGoods", "Goods");
        }
        

        var userGoods = new UserGoods
        {
            user_id = user.Id,
            goods_id = model.goodsId,
            deliveryDate = DateTime.Parse(model.deliveryDate),
            quantity = model.quantity
        };

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Save to the database
            context.UserGoods.Add(userGoods);

            goods.quantity -= model.quantity;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Заказ успешно сформирован!";
            return RedirectToAction("UserOrders", "Goods");
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = "Заказать товар не удалось, попробуйте позже";
            return RedirectToAction("GetAllGoods", "Goods");
        }
        
    }


    private bool CheckQuantity(MakingOrderViewModel model)
    {
        var result = context.Goods.FirstOrDefaultAsync(g => g.id == model.goodsId).Result;
        if (result == null)
        {
            throw new NullReferenceException("Товар не найден");
        }

        return  10 < model.quantity && model.quantity <= result.quantity;
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpGet("[action]")]
    public IActionResult AddGoods()
    {
        return View();
    }
    
    
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddGoods(AddGoodsModel model)
    {
        if (ModelState.IsValid)
        {
            var newGoods = new Goods
            {
                name = model.name,
                quantity = model.quantity,
                costPerUnit = model.cost,
                description = model.description
            };

            context.Goods.Add(newGoods);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Товар успешно добавлен!";
            return RedirectToAction("GetAllGoods");
        }

        TempData["ErrorMessage"] = "Произошла ошибка при добавлении товара.";
        return View(model);
    }
    
    
    [Authorize(Roles = "Admin")]
    [HttpPost("[action]")]
    public async Task<IActionResult> IncreaseGoods(long goodsId, int quantityToAdd)
    {
        var goods = await context.Goods.FirstOrDefaultAsync(g => g.id == goodsId);

        if (goods == null)
        {
            TempData["ErrorMessage"] = "Товар не найден.";
            return RedirectToAction("GetAllGoods");
        }

        if (quantityToAdd <= 0)
        {
            TempData["ErrorMessage"] = "Количество для добавления должно быть больше нуля.";
            return RedirectToAction("GetAllGoods");
        }

        goods.quantity += quantityToAdd;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Добавлено {quantityToAdd} единиц к товару {goods.name}.";
        return RedirectToAction("GetAllGoods");
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost("[action]")]
    public async Task<IActionResult> DecreaseGoods(long goodsId, int quantityToRemove)
    {
        var goods = await context.Goods.FirstOrDefaultAsync(g => g.id == goodsId);

        if (goods == null)
        {
            TempData["ErrorMessage"] = "Товар не найден.";
            return RedirectToAction("GetAllGoods");
        }

        if (quantityToRemove <= 0 || quantityToRemove > goods.quantity)
        {
            TempData["ErrorMessage"] = "Неверное количество для удаления.";
            return RedirectToAction("GetAllGoods");
        }

        goods.quantity -= quantityToRemove;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Удалено {quantityToRemove} единиц у товара {goods.name}.";
        return RedirectToAction("GetAllGoods");
    }
    
    
    [HttpGet("[action]")]
    [Authorize]
    public async Task<IActionResult> UserOrders()
    {
        List<OrderView> orders;
        
        if (User.IsInRole("Admin"))
        {
            orders = await context.UserGoods
                .Select(ug => new OrderView
                {
                    orderId = ug.id,
                    name = ug.goods.name,
                    deliveryDate = ug.deliveryDate,
                    cost = ug.goods.costPerUnit * ug.quantity
                })
                .ToListAsync();
            return View(orders);
        }
        
        var userId = userManager.GetUserId(User);
        orders = await context.UserGoods
            .Where(ur => ur.user_id == userId)
            .Select(ug => new OrderView
            {
                orderId = ug.id,
                name = ug.goods.name,
                deliveryDate = ug.deliveryDate,
                cost = ug.goods.costPerUnit * ug.quantity
            }).ToListAsync();
    
        return View(orders);
    }
    
    
    [HttpPost("[action]")]
    [Authorize]
    
    public async Task<IActionResult> DeleteOrder(long orderId)
    {
    
        var userId = userManager.GetUserId(User);
        var order = await context.UserGoods
            .Include(userGoods => userGoods.goods)
            .FirstOrDefaultAsync(ug => ug.id == orderId);
    

        if (order == null)
        {
            TempData["ErrorMessage"] = "Заказ не найден.";
            return RedirectToAction("UserOrders");
        }

        if (!User.IsInRole("Admin") && order.user_id != userId)
        {
            TempData["ErrorMessage"] = "Вы не можете удалить этот заказ.";
            return RedirectToAction("UserOrders");
        }
        
        if (order.deliveryDate <= DateTime.Now.AddDays(7))
        {
            TempData["ErrorMessage"] = "Вы не можете удалить этот заказ.";
            return RedirectToAction("UserOrders");
        }


        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var goods = order.goods;
            goods.quantity -= order.quantity;
            
            context.UserGoods.Remove(order);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["SuccessMessage"] = "Заказ успешно удален";
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = "Не удалось удалить заказ. Попробуйте позже.";
        }
    
        return RedirectToAction("UserOrders");
    }
        
}