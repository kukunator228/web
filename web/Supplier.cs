using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("Supplier")]
    public class Supplier
    {
        [Key]
        [Column("SupplierID")]
        public int SupplierID { get; set; }

        [Column("SupplierName")]
        public string SupplierName { get; set; } = string.Empty;

        [Column("SupplierINN")]
        public string SupplierINN { get; set; } = string.Empty;

        public List<BookSupply> BookSupplies { get; set; } = new();
    }
}