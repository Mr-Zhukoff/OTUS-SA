using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreLogic.Models;

[Table("orderproducts")]
public class OrderProduct
{
    [Column("id")]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }

    [Column("productid")]
    [Required]
    public int ProductId { get; set; }

    [Column("orderid")]
    [Required]
    public int OrderId { get; set; }

    [Column("title")]
    [Required]
    public string Title { get; set; }

    [Column("quantity")]
    [Required]
    public int Quantity { get; set; }

    [Column("price")]
    [Required]
    public decimal Price { get; set; }

    [Column("totalprice")]
    [Required]
    public decimal TotalPrice { get; set; }
}