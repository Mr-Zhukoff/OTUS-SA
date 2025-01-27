using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreLogic.Models;

[Table("users")]
public class User
{
    [Column("id")]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }

    [Column("firstname")]
    public string FirstName { get; set; }

    [Column("lastname")]
    public string LastName { get; set; }

    [Column("middlename")]
    public string MiddleName { get; set; }

    [Column("email")]
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Column("salt")]
    [SwaggerSchema(ReadOnly = true)]
    public string? PasswordSalt { get; set; }

    [Column("hash")]
    [SwaggerSchema(ReadOnly = true)]
    public string? PasswordHash { get; set; }

    public override string ToString()
    {
        return $"{Id}: {Email}";
    }
}
