using WarehouseWebApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WarehouseWebApp.Data;

public class DataContext : IdentityDbContext<User>
{
    
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Goods> Goods { get; set; }
    public DbSet<UserGoods> UserGoods { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Goods>()
            .HasIndex(r => r.name)
            .IsUnique();
        
        modelBuilder.Entity<Goods>().HasData(
            new Goods
            {
                id = 1, name = "Сахар", quantity = 1000, costPerUnit = 100,
                description = "Наш сахар белый песок - это высококачественный продукт, который идеально подходит для повседневного использования. Он имеет мелкие кристаллы, которые быстро растворяются в горячих и холодных напитках, выпечке и десертах. Сахар производится из тщательно отобранных свеклы или тростника, что гарантирует чистоту и приятный вкус продукта.",
            },
            new Goods
            {
                id = 2, name = "Соль", quantity = 1000, costPerUnit = 80,
                description = "Поваренная соль - это необходимый продукт для каждой кухни. Используется для приготовления пищи и консервации. Наш продукт проходит тщательную очистку и не содержит примесей, что обеспечивает его высокое качество и безопасность.",
            },
            new Goods
            {
                id = 3, name = "Водка", quantity = 600, costPerUnit = 1000,
                description = "Классическая водка - это традиционный продукт, изготовленный по старинным рецептам. Обладает чистым и мягким вкусом, благодаря использованию высококачественного спирта и многократной фильтрации. Подходит как для торжественных мероприятий, так и для дружеских встреч.",
            },
            new Goods
            {
                id = 4, name = "Сигареты", quantity = 500, costPerUnit = 200,
                description = "Классические сигареты - это продукт высшего качества, созданный из отборного табака. Обладают мягким и насыщенным вкусом, что делает их популярным выбором среди курильщиков. Продукт упакован в удобные пачки, сохраняющие свежесть табака.",
            },
            new Goods
            {
                id = 5, name = "Колбаса", quantity = 100, costPerUnit = 500,
                description = "Докторская колбаса - это классический продукт, известный своим нежным вкусом и качественным составом. Изготавливается из отборного мяса, с добавлением специй и пряностей для создания неповторимого аромата. Идеально подходит для приготовления бутербродов, салатов и горячих блюд.",
            }
        );
        
        base.OnModelCreating(modelBuilder);
    }
    
}