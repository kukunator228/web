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

        [Column("AdressIndexKey")]
        public string AdressIndexKey { get; set; } = string.Empty;

        [ForeignKey("AdressIndexKey")]
        public OrderIndex OrderIndex { get; set; } = null!;

        [Column("OrderDate")]
        public DateTime OrderDate { get; set; }

        [Column("ClientKey")]
        public int ClientKey { get; set; }

        [ForeignKey("ClientKey")]
        public Client Client { get; set; } = null!;

        public List<OrderItem> OrderItems { get; set; } = new();
    }
}