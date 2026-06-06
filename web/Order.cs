using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("Order")]
    public class Order
    {
        [Key]
        [Column("OrderID")]
        public int OrderID { get; set; }

        [Column("OrderDate")]
        public DateTime OrderDate { get; set; }

        [Column("AdressIndexKey")]
        public string AdressIndexKey { get; set; } = string.Empty;

        [Column("ClientKey")]
        public int ClientKey { get; set; }

        [Column("StatusKey")]
        public int StatusKey { get; set; }

        [ForeignKey("StatusKey")]
        public virtual OrderStatus? OrderStatus { get; set; }

        [ForeignKey("OrderKey")]
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}