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

        public string UserId { get; set; }

        public string UserName { get; set; }

        public DateTime Date { get; set; }

        public int Mb { get; set; }
        public int M46 { get; set; }
        public int M62 { get; set; }
        public int MAb { get; set; }
        public int MA46 { get; set; }
        public int MA62 { get; set; }
        public int MTRb { get; set; }
        public int MTR46 { get; set; }
        public int MTR62 { get; set; }
        public int MTb { get; set; }
        public int MT46 { get; set; }
        public int MT62 { get; set; }

        public int Amount { get; set; }
        public int Cost { get; set; }

        public override string ToString()
        {
            string res = "";
            res += UserName+": ";

            res += Mb != 0 ? "Mb - " + Mb + ", " : "";
            res += M46 != 0 ? "M46 - " + M46 + ", " : "";
            res += M62 != 0 ? "M62 - " + M62 + ", " : "";

            res += MAb != 0 ? "MAb - " + MAb + ", " : "";
            res += MA46 != 0 ? "MA46 - " + MA46 + ", " : "";
            res += MA62 != 0 ? "MA62 - " + MA62 + ", " : "";

            res += MTRb != 0 ? "MTRb - " + MTRb + ", " : "";
            res += MTR46 != 0 ? "MTR46 - " + MTR46 + ", " : "";
            res += MTR62 != 0 ? "MTR62 - " + MTR62 + ", " : "";

            res += MTb != 0 ? "MTb - " + MTb + ", " : "";
            res += MT46 != 0 ? "MT46 - " + MT46 + ", " : "";
            res += MT62 != 0 ? "MT62 - " + MT62 + ", " : "";

            return res;
        }
    }
}
