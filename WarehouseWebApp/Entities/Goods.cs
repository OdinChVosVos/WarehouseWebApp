using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseWebApp.Entities;

[Table("goods")]
public class Goods
{
    [Key]
    [Column("id")]
    public long id { get; init; }
    
    [Required]
    [Column("name")]
    public string name { get; set; }
    
    [Required]
    [Column("description")]
    public string description { get; set; }
    
    [Required]
    [Column("quantity")]
    public int quantity { get; set; }
    
    [Required]
    [Column("cost_per_unit")]
    public decimal costPerUnit { get; set; }


}