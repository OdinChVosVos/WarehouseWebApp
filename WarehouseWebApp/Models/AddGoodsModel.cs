namespace WarehouseWebApp.Models;

public class AddGoodsModel
{
    public string name { get; set; } 
    public int quantity { get; set; } 
    public decimal cost { get; set; }   
    public string description { get; set; }
}