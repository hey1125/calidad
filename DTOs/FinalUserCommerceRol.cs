using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace DTOs
{
    public class FinalUserCommerceRol : BaseDTO
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public int FinalUserId { get; set; }
        public int CommerceId { get; set; }
        public int RolId { get; set; }
    }
}

