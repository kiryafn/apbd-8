namespace WebApplication2.Models;

public class ProductWarehouse : BaseEntity
{
    public Warehouse Warehouse { get; set; }
    public Product Product { get; set; }
    public Order Order { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}