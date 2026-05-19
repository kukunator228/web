using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("Users")]
    public class Users
    {
        [Key]
        [Column("UserID")]
        public int UserID { get; set; }

        [Column("UserLogin")]
        public string UserLogin { get; set; } = string.Empty;

        [Column("UserPassword")]
        public string UserPasssword { get; set; } = string.Empty;

        [Column("RoleKey")]
        public int RoleKey { get; set; }

        [ForeignKey("RoleKey")]
        public Roles Role { get; set; } = null!;
    }
}