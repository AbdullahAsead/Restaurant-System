using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Restaurant_System.Models
{
    public class OrderDetails
    {
        public string Time { get; set; }
        public List<string> Items { get; set; }
        public double Total { get; set; }
    }
   
}
