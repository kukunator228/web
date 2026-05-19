using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("Roles")]
    public class Roles
    {
        [Key]
        [Column("RoleID")]
        public int RoleID { get; set; }

        [Column("RoleName")]
        public string RoleName { get; set; } = string.Empty;

        public List<Users> Users { get; set; } = new();
    }
}