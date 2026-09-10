using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public class CommercePromotion : BaseDTO
    {
        public string CoPromoId { get; set; }
        public int CommerceId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal? MinAmount { get; set; }
        public decimal? AmountPercentage { get; set; }
        public decimal? AmountDiscount { get; set; }

        public int? TotalCoupon { get; set; }
        public int? UsedCoupon { get; set; }

        public string Status { get; set; }
        public string Description { get; set; }
    }
}
