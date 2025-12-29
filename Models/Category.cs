using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ExchangeWebsite.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Category name is required.")]
        public string CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        
        [ValidateNever]
        public virtual Category ParentCategory { get; set; }
        
        [ValidateNever]
        public virtual ICollection<Category> SubCategories { get; set; }

        // Add the missing property to fix CS1061  
        public bool IsActive { get; set; }
    }
}
