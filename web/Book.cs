using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("Book")]
    public class Book
    {
        [Key]
        [Column("BookID")]
        public int BookID { get; set; }

        [Column("BookName")]
        public string BookName { get; set; } = string.Empty;

        [Column("BookDesc")]
        public string BookDesc { get; set; } = string.Empty;

        [Column("BookTypeID")]
        public int? BookTypeID { get; set; }

        [Column("ImagePath")]
        public string? ImagePath { get; set; }

        [Column("count")]
        public int? Count { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [ForeignKey("BookTypeID")]
        public virtual BookType? BookType { get; set; }
    }
}