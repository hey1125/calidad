using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public class FinalUser : BaseDTO
    {
        public string NationalId { get; set; }
        public string FirstName { get; set; }
        public string LastName1 { get; set; }
        public string LastName2 { get; set; }
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public decimal LocationLat { get; set; }
        public decimal LocationLng { get; set; }
        public string SelfieUrl { get; set; } = "";
        public bool FacialVerificationPassed { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public string IdCardFrontPhotoUrl { get; set; } = "";
        public string IdCardBackPhotoUrl { get; set; } = "";
        public string Status { get; set; }
    }
}
