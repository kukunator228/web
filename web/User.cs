using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace web
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("UserID")]
        public int UserID { get; set; }

        [Column("UserLogIn")]
        public string UserLogIn { get; set; } = string.Empty;

        [Column("UserPassword")]
        public string UserPasssword { get; set; } = string.Empty;

        [Column("RoleKey")]
        public int RoleKey { get; set; }

        [ForeignKey("RoleKey")]
        public virtual Role? Role { get; set; }
    }
}