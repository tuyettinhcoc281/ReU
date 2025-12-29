using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExchangeWebsite.Models
{
    public class ShippingRequest
    {
        [Key]
        public int ShippingRequestId { get; set; }

        [Required]
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        [Required]
        [StringLength(100)]
        public string SenderName { get; set; }

        [Required]
        [StringLength(20)]
        public string SenderPhone { get; set; }

        [Required]
        [StringLength(100)]
        public string SenderCity { get; set; }

        [Required]
        [StringLength(100)]
        public string SenderDistrict { get; set; }

        [Required]
        [StringLength(200)]
        public string SenderAddress { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingPrice { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [StringLength(30)]
        public string Status { get; set; } = "Pending";
        public string? ShipperId { get; set; }
        [ForeignKey("ShipperId")]
        public User Shipper { get; set; }
    }
}