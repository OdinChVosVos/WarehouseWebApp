namespace WarehouseWebApp.Models;

public class MakingOrderViewModel
{
    public long goodsId { get; init; }
    public string deliveryDate { get; set; }
    public int quantity { get; set; }
   

    public MakingOrderViewModel()
    {
        
    }

    public MakingOrderViewModel(long goodsId, string deliveryDate, int quantity)
    {
        this.goodsId = goodsId;
        this.deliveryDate = deliveryDate;
        this.quantity = quantity;
    }
}