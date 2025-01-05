using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreLogic.Models;

[Table("transactions")]
public class Notification
{
    [Column("id")]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }

    [Column("userid")]
    [Required]
    public int UserId { get; set; }

    [Column("title")]
    [Required]
    public string Title { get; set; }

    [Column("body")]
    [Required]
    public string Body { get; set; }
}
