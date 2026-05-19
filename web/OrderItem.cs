using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("OrderItem")]
    public class OrderItem
    {
        [Key]
        [Column("OrderItemID")]
        public int OrderItemID { get; set; }

        [Column("OrderKey")]
        public int OrderKey { get; set; }

        [Column("BookKey")]
        public int BookKey { get; set; }

        [Column("OrderBookQuantity")]
        public string OrderBookQuantity { get; set; } = string.Empty;

        [Column("BookTypekey")]
        public int BookTypekey { get; set; }

        [ForeignKey("BookTypekey")]
        public BookType BookType { get; set; } = null!;

        [ForeignKey("OrderKey")]
        public Order Order { get; set; } = null!;
    }
}