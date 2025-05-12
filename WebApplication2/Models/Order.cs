namespace WebApplication2.Models;

public class Order
{
    public int IdOrder { get; set; }
    public Product Product { get; set; } = null!;
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
}