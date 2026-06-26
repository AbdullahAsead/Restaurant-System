using System.ComponentModel.DataAnnotations;

namespace Restaurant_System.Models
{
    public class DeliveryPlace
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }  // اسم المنطقة (مثلاً شارع الإسكندرية)

        [Required]
        public decimal DeliveryFee { get; set; }  // سعر التوصيل (مثلاً 10 جنيه)
    }
}
