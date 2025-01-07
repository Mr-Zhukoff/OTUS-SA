using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreLogic.Models;

[Table("orders")]
public class Order
{
    [Column("id")]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }

    [Column("userid")]
    public int UserId { get; set; }

    [Column("accountid")]
    public int AccountId { get; set; }

    [Column("title")]
    [Required]
    public string Title { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("state")]
    public OrderStatus Status { get; set; }
}


