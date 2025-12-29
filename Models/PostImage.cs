using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExchangeWebsite.Models
{
    public class PostImage
    {
        [Key]
        public int PostImageId { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        // Foreign key
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}