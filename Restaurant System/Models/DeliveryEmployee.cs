using System.ComponentModel.DataAnnotations;

namespace Restaurant_System.Models
{
    public class DeliveryEmployee
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string MachineType { get; set; }

        public string MachineNumber { get; set; }

        // ✅ العمود اللي في قاعدة البيانات
        public int DailyOrdersCount { get; set; } = 0;

    }
}
