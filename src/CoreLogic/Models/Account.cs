using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public override string ToString()
    {
        return $"{UserId}: {Number} = {Balance}";
    }
}
