using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    public class OTPVerificationCrudFactory : CrudFactory
    {
        public OTPVerificationCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var otp = baseDTO as OTPVerification;
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "CRE_OTP_VERIFICATION_PR"
            };

            sqlOperation.AddIntParam("P_UserId", otp.UserId);
            sqlOperation.AddStringParameter("P_VerificationType", otp.VerificationType);
            sqlOperation.AddStringParameter("P_Code", otp.Code);
            sqlOperation.AddDateTimeParam("P_ExpiresAt", otp.ExpiresAt);
            sqlOperation.AddBooleanParam("P_IsVerified", otp.IsVerified);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }


        public override void Update(BaseDTO baseDTO)
        {
            var otp = baseDTO as OTPVerification;
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "UPD_OTP_VERIFICATION_PR"
            };

            sqlOperation.AddIntParam("P_OtpId", otp.Id);
            sqlOperation.AddIntParam("P_UserId", otp.UserId);
            sqlOperation.AddStringParameter("P_VerificationType", otp.VerificationType);
            sqlOperation.AddStringParameter("P_Code", otp.Code);
            sqlOperation.AddDateTimeParam("P_ExpiresAt", otp.ExpiresAt);
            sqlOperation.AddBooleanParam("P_IsVerified", otp.IsVerified);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var otp = baseDTO as OTPVerification;
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "DEL_OTP_VERIFICATION_PR"
            };

            sqlOperation.AddIntParam("P_OtpId", otp.Id);
            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override T Retrieve<T>()
        {
            throw new NotImplementedException("Retrieve without parameters is not implemented for OTPVerification.");
        }

        public override List<T> RetrieveAll<T>()
        {
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "RET_ALL_OTP_VERIFICATIONS_PR"
            };
            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);
            return results.Select(row => (T)Convert.ChangeType(BuildOTPVerification(row), typeof(T))).ToList();
        }

        public override T RetrieveById<T>(int otpId)
        {
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "RET_OTP_VERIFICATION_BY_ID_PR"
            };
            sqlOperation.AddIntParam("P_OtpId", otpId);

            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);
            if (results.Count > 0)
            {
                var otp = BuildOTPVerification(results[0]);
                return (T)Convert.ChangeType(otp, typeof(T));
            }
            return default;
        }

        public T RetrieveByUserId<T>(int userId)
        {
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "RET_OTP_VERIFICATION_BY_USERID_PR"
            };
            sqlOperation.AddIntParam("P_UserId", userId);

            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);
            if (results.Count > 0)
            {
                var otp = BuildOTPVerification(results[0]);
                return (T)Convert.ChangeType(otp, typeof(T));
            }
            return default;
        }

        private OTPVerification BuildOTPVerification(Dictionary<string, object> row)
        {
            return new OTPVerification
            {
                Id = Convert.ToInt32(row["OtpId"]),
                Created = Convert.ToDateTime(row["CreatedAt"]),
                Updated = (DateTime)(row["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UpdatedAt"])),
                UserId = Convert.ToInt32(row["UserId"]),
                VerificationType = row["VerificationType"].ToString(),
                Code = row["Code"].ToString(),
                ExpiresAt = Convert.ToDateTime(row["ExpiresAt"]),
                IsVerified = Convert.ToBoolean(row["IsVerified"])
            };
        }
    }
}
