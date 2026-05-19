using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("Author")]
    public class Author
    {
        [Key]
        [Column("AuthorID")]
        public int AuthorID { get; set; }

        [Column("AuthorFirstName")]
        public string AuthorFirstName { get; set; } = string.Empty;

        [Column("ImagePath")]
        public string? ImagePath { get; set; }

        [Column("AuthorSecondName")]
        public string AuthorSecondName { get; set; } = string.Empty;

        [Column("AuthorPatronymic")]
        public string? AuthorPatronymic { get; set; }

        [Column("AuthorBio")]
        public string? AuthorBio { get; set; }
    }
}