namespace WebApplication2.Models;

public class Warehouse : BaseEntity
{
    public string Name { get; set; }
    public string Address { get; set; }
    public List<ProductWarehouse> Products { get; set; }
}