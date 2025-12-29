using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ExchangeWebsite.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be 5-100 characters.")]
        public string? Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be 10-1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1000, 1000000000, ErrorMessage = "Price must be between 1,000 and 1,000,000,000 VND.")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Thành phố là bắt buộc")]
        [StringLength(100)]
        public string? City { get; set; }           // Tỉnh/Thành phố
        [Required(ErrorMessage = "District is required.")]
        [StringLength(100)]
        public string? District { get; set; }
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(100)]// Quận/Huyện
        public string? StreetAddress { get; set; }  // Địa chỉ chi tiết

        [Required(ErrorMessage = "ZIP Code is required.")]
        [StringLength(20)]
        public string? ZipCode { get; set; }

        [StringLength(100)]
        public string? Make { get; set; }

        [StringLength(100)]
        public string? ModelNumber { get; set; }

        [Required(ErrorMessage = "Condition is required.")]
        [StringLength(50)]
        public string? Condition { get; set; }

        public bool CryptocurrencyAccepted { get; set; }
        public bool DeliveryAvailable { get; set; }

        [Required(ErrorMessage = "Contact Email is required.")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? ContactEmail { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }

        public bool ShowAddress { get; set; }

        [Required(ErrorMessage = "Language is required.")]
        [StringLength(20)]
        public string? Language { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        [Required]
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public virtual ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();

        // Foreign key
        public string? UserId { get; set; }

        public virtual User? User { get; set; }

        // FIX: Remove [Required] and ensure initialization
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public bool ShippingRequested { get; set; } // Add to Post.cs
    }
}