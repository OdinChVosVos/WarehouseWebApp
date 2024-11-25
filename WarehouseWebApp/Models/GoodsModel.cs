namespace WarehouseWebApp.Models;

public record GoodsModel(long id, string description, string name, int quantity, decimal costPerUnit);