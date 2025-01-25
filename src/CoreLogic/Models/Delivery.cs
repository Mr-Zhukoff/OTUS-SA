using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreLogic.Models;

[Table("deliveries")]
public class Delivery
{
    [Column("id")]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }

    [Column("userid")]
    public int UserId { get; set; }

    [Column("odrerid")]
    public int OrderId { get; set; }

    [Column("title")]
    [Required]
    public string Title { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("address")]
    public string Address { get; set; }

    [Column("createdon")]
    public DateTime CreatedOn { get; set; }

    [Column("state")]
    public DeliveryStatus Status { get; set; }
}
