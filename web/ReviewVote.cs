using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("ReviewVotes")]
    public class ReviewVote
    {
        [Key]
        public int VoteID { get; set; }

        public int ReviewKey { get; set; }
        public int UserKey { get; set; }
        public int VoteValue { get; set; }

        [ForeignKey("ReviewKey")]
        public virtual BookReview? BookReview { get; set; }

        [ForeignKey("UserKey")]
        public virtual User? User { get; set; }
    }
}
