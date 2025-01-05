using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace CoreLogic.Models;

[Table("accounts")]
public class Account
{
    [Column("id")]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }

    [Column("userid")]
    public int UserId { get; set; }

    [Column("number")]
    [Required]
    public string Number { get; set; }

    [Column("description")]
    public string Description { get; set; }
    
    [Column("balance")]
    public decimal Balance { get; set; }
}
