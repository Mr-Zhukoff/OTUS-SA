using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreLogic.Models;

[Table("products")]
public class Product
{
    [Column("id")]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }

    [Column("title")]
    [Required]
    public string Title { get; set; }

    [Column("description")]
    [Required]
    public string Description { get; set; }

    [Column("quantity")]
    [Required]
    public int Quantity { get; set; }

    [Column("price")]
    [Required]
    public decimal Price { get; set; }

    [Column("createdon")]
    [Required]
    public DateTime CreatedOn { get; set; }
}