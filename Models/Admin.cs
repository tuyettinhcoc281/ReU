using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExchangeWebsite.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Optional: Track actions performed by admin
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (optional, for audit/logging)
        public virtual ICollection<Category>? CreatedCategories { get; set; }
        public virtual ICollection<Category>? CreatedSubCategories { get; set; }
        public virtual ICollection<Post>? DeletedPosts { get; set; }
    }
}