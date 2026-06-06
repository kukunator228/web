using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("BookReviews")]
    public class BookReview
    {
        [Key]
        public int ReviewID { get; set; }

        public int BookKey { get; set; }
        public int UserKey { get; set; }

        public string ReviewText { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; } = DateTime.Now;
        public int RatingSum { get; set; } = 0;

        [ForeignKey("BookKey")]
        public virtual Book? Book { get; set; }

        [ForeignKey("UserKey")]
        public virtual User? User { get; set; }

        [Column("BookScore")]
        public int BookScore { get; set; } = 5;
    }
}
