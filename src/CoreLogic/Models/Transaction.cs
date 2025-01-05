
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreLogic.Models;
[Table("transactions")]
public class Transaction
{
    [Column("id")]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }
    
    [Column("userid")]
    [Required]
    public int UserId { get; set; }

    [Column("accountid")]
    [Required]
    public int AccountId { get; set; }

    [Column("amount")]
    [Required]
    public decimal Amount { get; set; }
}
