using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExchangeWebsite.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // For VIP subscription
        [Required]
        public DateTime SubscriptionStart { get; set; }

        [Required]
        public DateTime SubscriptionEnd { get; set; }

        [StringLength(100)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        // Status: Paid, Pending, Failed, etc.
        [StringLength(30)]
        public string Status { get; set; } = "Paid";
    }
}