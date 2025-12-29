using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExchangeWebsite.Models
{
    public class CommentReport
    {
        [Key]
        public int Id { get; set; }
        
        public int CommentId { get; set; }
        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }
        
        public string ReportedByUserId { get; set; }
        [ForeignKey("ReportedByUserId")]
        public User ReportedByUser { get; set; }
        
        [Required]
        public string Reason { get; set; }
        
        public DateTime ReportedAt { get; set; }
            
        public string Status { get; set; }
    }
}