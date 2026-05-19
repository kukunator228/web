using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("BookAuthors")]
    public class BookAuthor
    {
        [Key]
        [Column("BookAuthorID")]
        public int BookAuthorID { get; set; }

        [Column("BookID")]
        public int BookID { get; set; }

        [Column("AuthorID")]
        public int AuthorID { get; set; }

        [ForeignKey("BookID")]
        public virtual Book Book { get; set; } = null!;

        [ForeignKey("AuthorID")]
        public virtual Author Author { get; set; } = null!;
    }
}