using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    /// Representa la información básica de un comercio.
    public class Commerce : BaseDTO
    {
        public string LegalId { get; set; } /// Identificación legal del comercio.
        public string Name { get; set; } /// Nombre del comercio.
        public string Phone { get; set; } /// Número de teléfono del comercio.
        public string Email { get; set; } /// Dirección de correo electrónico del comercio.
        public decimal Latitude { get; set; } /// Latitud de la ubicación del comercio.
        public decimal Longitude { get; set; } /// Longitud de la ubicación del comercio.
        public string Status { get; set; } /// Estado actual del comercio (Pediente, Activo, Rechazado).
        public string IBAN { get; set; } /// Código IBAN del comercio.
        public decimal CommissionRate { get; set; } /// Tasa de comisión aplicada al comercio.
        public string NationalId { get; set; } // Cédula nacional nvarchar(9) - Costa Rica
    }
}
