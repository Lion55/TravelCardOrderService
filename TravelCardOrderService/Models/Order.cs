using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TravelCardOrderService.Models
{
    public class Order
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        
        public int? Mb { get; set; }
        public int? M46 { get; set; }
        public int? M62 { get; set; }
        public int? MAb { get; set; }
        public int? MA46 { get; set; }
        public int? MA62 { get; set; }
        public int? MTRb { get; set; }
        public int? MTR46 { get; set; }
        public int? MTR62 { get; set; }
        public int? MTb { get; set; }
        public int? MT46 { get; set; }
        public int? MT62 { get; set; }

    }
}
