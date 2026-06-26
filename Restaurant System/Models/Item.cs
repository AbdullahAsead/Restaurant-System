using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_System.Models
{
    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Categories Category { get; set; }

        public string? Barcode { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Weight { get; set; }

        // ✅ خاصية إضافية للعرض في DataGrid (مش مخزنة في قاعدة البيانات)
        [NotMapped]
        public string CategoryName { get; set; } = string.Empty;
    }
}
