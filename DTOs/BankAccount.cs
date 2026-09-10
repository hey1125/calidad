using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    
    // DTO que representa una cuenta bancaria.
    
    public class BankAccount : BaseDTO
    {
        public int UserId { get; set; } // El ID del usuario propietario de la cuenta bancaria
        public int BankId { get; set; } // El ID del banco al que pertenece la cuenta
        public string IBAN { get; set; } // El número IBAN de la cuenta bancaria
    }
}