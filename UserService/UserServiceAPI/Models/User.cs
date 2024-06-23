using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserServiceAPI.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }
        [Column("firstname")]
        [Required]
        public string FirstName { get; set; }
        [Column("lastname")]
        [Required]
        public string LastName { get; set; }
        [Column("middlename")]
        public string MiddleName { get; set; }
        [Column("email")]
        [Required]
        public string Email { get; set; }
    }
}
