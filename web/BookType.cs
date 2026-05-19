using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("BookType")]
    public class BookType
    {
        [Key]
        [Column("BookTypeID")]
        public int BookTypeID { get; set; }

        [Column("BookTypeName")]
        public string BookTypeName { get; set; } = string.Empty;

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}