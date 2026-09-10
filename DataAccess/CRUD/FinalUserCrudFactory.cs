using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    public class FinalUserCrudFactory : CrudFactory
    {
        public FinalUserCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var user = baseDTO as FinalUser;
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "CRE_FINALUSER_PR"
            };
            if (user.SelfieUrl != null)
            {
                sqlOperation.AddStringParameter("P_SelfieUrl", user.SelfieUrl);
            }
            sqlOperation.AddStringParameter("P_NationalId", user.NationalId);
            sqlOperation.AddStringParameter("P_FirstName", user.FirstName);
            sqlOperation.AddStringParameter("P_LastName1", user.LastName1);
            sqlOperation.AddStringParameter("P_LastName2", user.LastName2);
            sqlOperation.AddStringParameter("P_PasswordHash", user.PasswordHash);
            sqlOperation.AddStringParameter("P_Phone", user.Phone);
            sqlOperation.AddStringParameter("P_Email", user.Email);
            sqlOperation.AddDateTimeParam("P_DateOfBirth", user.DateOfBirth);
            sqlOperation.AddDecimalParameter("P_LocationLat", user.LocationLat);
            sqlOperation.AddDecimalParameter("P_LocationLng", user.LocationLng);
            sqlOperation.AddBooleanParam("P_FacialVerificationPassed", user.FacialVerificationPassed);
            sqlOperation.AddStringParameter("P_ProfilePhotoUrl", user.ProfilePhotoUrl);
            sqlOperation.AddStringParameter("P_IdCardFrontPhotoUrl", user.IdCardFrontPhotoUrl);
            sqlOperation.AddStringParameter("P_IdCardBackPhotoUrl", user.IdCardBackPhotoUrl);
            sqlOperation.AddStringParameter("P_Status", user.Status);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var user = baseDTO as FinalUser;
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "UPD_FINALUSER_PR"
            };

            sqlOperation.AddIntParam("P_Id", user.Id);
            sqlOperation.AddStringParameter("P_NationalId", user.NationalId);
            sqlOperation.AddStringParameter("P_FirstName", user.FirstName);
            sqlOperation.AddStringParameter("P_LastName1", user.LastName1);
            sqlOperation.AddStringParameter("P_LastName2", user.LastName2);
            sqlOperation.AddStringParameter("P_PasswordHash", user.PasswordHash);
            sqlOperation.AddStringParameter("P_Phone", user.Phone);
            sqlOperation.AddStringParameter("P_Email", user.Email);
            sqlOperation.AddDateTimeParam("P_DateOfBirth", user.DateOfBirth);
            sqlOperation.AddDecimalParameter("P_LocationLat", user.LocationLat);
            sqlOperation.AddDecimalParameter("P_LocationLng", user.LocationLng);
            sqlOperation.AddStringParameter("P_SelfieUrl", user.SelfieUrl);
            sqlOperation.AddBooleanParam("P_FacialVerificationPassed", user.FacialVerificationPassed);
            sqlOperation.AddStringParameter("P_ProfilePhotoUrl", user.ProfilePhotoUrl);
            sqlOperation.AddStringParameter("P_IdCardFrontPhotoUrl", user.IdCardFrontPhotoUrl);
            sqlOperation.AddStringParameter("P_IdCardBackPhotoUrl", user.IdCardBackPhotoUrl);
            sqlOperation.AddStringParameter("P_Status", user.Status);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var user = baseDTO as FinalUser;
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "DEL_FINALUSER_PR"
            };

            sqlOperation.AddIntParam("P_Id", user.Id);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override T Retrieve<T>()
        {
            throw new NotImplementedException();
        }

        public override T RetrieveById<T>(int id)
        {
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "RET_FINALUSER_BY_ID_PR"
            };
            sqlOperation.AddIntParam("P_Id", id);

            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);
            if (results.Count > 0)
            {
                var user = BuildFinalUser(results[0]);
                return (T)Convert.ChangeType(user, typeof(T));
            }

            return default;
        }

        public T RetrieveByEmail<T>(string email)
        {
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "RET_FINALUSER_BY_EMAIL_PR"
            };
            sqlOperation.AddStringParameter("P_Email", email);

            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);
            if (results.Count > 0)
            {
                var user = BuildFinalUser(results[0]);
                return (T)Convert.ChangeType(user, typeof(T));
            }

            return default;
        }

        public T RetrieveByNationalId<T>(string nationalId)
        {
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "RET_FINALUSER_BY_NATIONALID_PR"
            };
            sqlOperation.AddStringParameter("P_NationalId", nationalId);

            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);
            if (results.Count > 0)
            {
                var user = BuildFinalUser(results[0]);
                return (T)Convert.ChangeType(user, typeof(T));
            }

            return default;
        }

        public override List<T> RetrieveAll<T>()
        {
            var lst = new List<T>();
            var sqlOperation = new SqlOperation
            {
                ProcedureName = "RET_ALL_FINALUSER_PR"
            };

            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);
            foreach (var row in results)
            {
                var user = BuildFinalUser(row);
                lst.Add((T)Convert.ChangeType(user, typeof(T)));
            }

            return lst;
        }

        private FinalUser BuildFinalUser(Dictionary<string, object> row)
        {
            return new FinalUser
            {
                Id = Convert.ToInt32(row["Id"]),
                Created = Convert.ToDateTime(row["CreatedAt"]),
                Updated = row["UpdatedAt"] == DBNull.Value
                    ? default(DateTime)
                    : Convert.ToDateTime(row["UpdatedAt"]),
                NationalId = row["NationalId"].ToString(),
                FirstName = row["FirstName"].ToString(),
                LastName1 = row["LastName1"].ToString(),
                LastName2 = row["LastName2"].ToString(),
                PasswordHash = row["PasswordHash"].ToString(),
                Phone = row["Phone"].ToString(),
                Email = row["Email"].ToString(),
                DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]),
                LocationLat = Convert.ToDecimal(row["LocationLat"]),
                LocationLng = Convert.ToDecimal(row["LocationLng"]),
                SelfieUrl = row["SelfieUrl"].ToString(),
                FacialVerificationPassed = Convert.ToBoolean(row["FacialVerificationPassed"]),
                ProfilePhotoUrl = row["ProfilePhotoUrl"].ToString(),
                IdCardFrontPhotoUrl = row["IdCardFrontPhotoUrl"].ToString(),
                IdCardBackPhotoUrl = row["IdCardBackPhotoUrl"].ToString(),
                Status = row["Status"].ToString()
            };
        }
    }
}
