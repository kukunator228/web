using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("Client")]
    public class Client
    {
        [Key]
        [Column("ClientID")]
        public int ClientID { get; set; }

        [Column("ClientFirstName")]
        public string ClientFirstName { get; set; } = string.Empty;

        [Column("ClientSecondName")]
        public string ClientSecondName { get; set; } = string.Empty;

        [Column("ClientPatronymic")]
        public string ClientPatronymic { get; set; } = string.Empty;

        [Column("ClientEmail")]
        public string ClientEmail { get; set; } = string.Empty;

        public List<Order> Orders { get; set; } = new();
    }
}