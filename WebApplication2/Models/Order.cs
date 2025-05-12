namespace WebApplication2.Models;

public class Order : BaseEntity
{
    public Product Product { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FullFieldAt { get; set; }
    
    
}