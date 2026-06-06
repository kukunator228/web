using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("OrderItem")]
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        [Column("OrderKey")]
        public int OrderKey { get; set; }

        [Column("BookKey")]
        public int BookKey { get; set; }

        [Column("OrderBookQuantity")]
        public string OrderBookQuantity { get; set; } = string.Empty;

        [ForeignKey("BookKey")]
        public virtual Book? Book { get; set; }
    }
}
