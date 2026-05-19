using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("BookSupply")]
    public class BookSupply
    {
        [Key]
        [Column("BookSupplyID")]
        public int BookSupplyID { get; set; }

        [Column("BookKey")]
        public int BookKey { get; set; }

        [Column("SupplierKey")]
        public int SupplierKey { get; set; }

        [ForeignKey("SupplierKey")]
        public Supplier Supplier { get; set; } = null!;

        [Column("SupplyQuantity")]
        public string SupplyQuantity { get; set; } = string.Empty;

        [Column("SupplyDate")]
        public DateTime SupplyDate { get; set; }

        [Column("BookSupplyPiecePrice")]
        public decimal BookSupplyPiecePrice { get; set; }
    }
}