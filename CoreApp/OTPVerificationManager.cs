using Azure;
using Azure.AI.Vision.Face;
using DataAccess.CRUD;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreApp
{
    public class OTPVerificationManager : BaseManager
    {
        public const string FixedCode = "123456";
        private readonly TimeSpan _expiryDuration = TimeSpan.FromMinutes(5);
        public OTPVerificationManager() { }

        public OTPVerification GenerateOtp(int userId, string type)
        {
            var now = DateTime.UtcNow;
            var otp = new OTPVerification
            {
                UserId = userId,
                VerificationType = type,
                Code = FixedCode,
                ExpiresAt = now.Add(_expiryDuration),
                IsVerified = false
            };

            var otpCrud = new OTPVerificationCrudFactory();
            otpCrud.Create(otp);
            return otp;
        }

    }
}
