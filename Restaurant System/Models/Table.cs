using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant_System.Models
{
    public class Table
    {
        public int Id { get; set; }

        public string Location { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
