using System;

namespace DTOs
{
    public class OTPVerification : BaseDTO
    {
        public int UserId { get; set; }
        public string VerificationType { get; set; }
        public string Code { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsVerified { get; set; }
    }
}
