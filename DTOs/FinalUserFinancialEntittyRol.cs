using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public class FinalUserFinancialEntityRol : BaseDTO
    {
        public int FinalUserId { get; set; }
        public int FinancialEntityId { get; set; }
        public int RolId { get; set; }
    }
}