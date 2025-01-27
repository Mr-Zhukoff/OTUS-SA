using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    [Column("products")]
    public List<OrderProduct> Products { get; set; }

    [Column("total")]
    public decimal Total { get; set; }

    [Column("state")]
    public OrderStatus Status { get; set; }

    [Column("statusreason")]
    public string StatusReason { get; set; }

    public override string ToString()
    {
        return $"{Id}: {Title} = {Total} Status: {Status}";
    }
}


