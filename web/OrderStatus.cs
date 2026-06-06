using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("OrderStatuses")]
    public class OrderStatus
    {
        [Key]
        [Column("StatusID")]
        public int StatusID { get; set; }

        [Column("StatusName")]
        public string StatusName { get; set; } = string.Empty;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}