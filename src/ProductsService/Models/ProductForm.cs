using CoreLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductsService.Models;

public class ProductForm
{
    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; }

    public Product ToProduct(int productId = 0)
    {
        return new Product
        {
            Id = productId,
            Title = Title,
            Description = Description,
            Quantity = Quantity,
            Price = Price,
            CreatedOn = DateTime.UtcNow
        };
    }
}
