using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web
{
    [Table("OrderIndex")]
    public class OrderIndex
    {
        [Key]
        [Column("Index")] 
        public string AdressIndex { get; set; } = string.Empty; 
    }
}