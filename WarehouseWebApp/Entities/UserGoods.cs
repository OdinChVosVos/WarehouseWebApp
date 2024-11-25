using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseWebApp.Entities;

[Table("user_goods")]
public class UserGoods
{
    [Key]
    [Column("id")]
    public long id { get; init; }
    
    [Required]
    [Column("goods")]
    public long goods_id { get; set; }
    [ForeignKey("goods_id")]
    public Goods goods { get; set; }
    
    [Required]
    [Column("user")]
    public string user_id { get; set; }
    [ForeignKey("user_id")]
    public User user { get; set; }
    
    [Required]
    [Column("delivery_date")]
    public DateTime deliveryDate { get; set; }
    
    [Required]
    [Column("quantity")]
    public int quantity { get; set; }
    
}