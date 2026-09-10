using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    public class FinalUserFinancialEntityRolCrudFactory : CrudFactory
    {
        public FinalUserFinancialEntityRolCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var rel = baseDTO as FinalUserFinancialEntityRol;

            var sqlOperation = new SqlOperation() { ProcedureName = "CRE_FINALUSER_FINANCIALENTITY_ROL_PR" };

            sqlOperation.AddIntParam("P_UserId", rel.FinalUserId);
            sqlOperation.AddIntParam("P_FinancialEntityId", rel.FinancialEntityId);
            sqlOperation.AddIntParam("P_RolId", rel.RolId);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public List<T> RetrieveEntitiesByUser<T>(int userId)
        {
            var lst = new List<T>();
            var op = new SqlOperation() { ProcedureName = "RET_FINANCIALENTITIES_BY_USER_PR" };
            op.AddIntParam("P_UserId", userId);

            var results = _sqlDao.ExecuteQueryProcedure(op);

            foreach (var row in results)
            {
                var entityId = Convert.ToInt32(row["Id"]);
                var entity = new FinancialEntityCrudFactory().RetrieveById<FinancialEntity>(entityId);
                lst.Add((T)Convert.ChangeType(entity, typeof(T)));
            }

            return lst;
        }

        public List<T> RetrieveRolesByUser<T>(int userId)
        {
            var lst = new List<T>();
            var op = new SqlOperation() { ProcedureName = "RET_ROLES_BY_USER_BANK_PR" };
            op.AddIntParam("P_UserId", userId);

            var results = _sqlDao.ExecuteQueryProcedure(op);

            foreach (var row in results)
            {
                var rel = new FinalUserFinancialEntityRol
                {
                    FinalUserId = userId,
                    FinancialEntityId = Convert.ToInt32(row["FinancialEntityId"]),
                    RolId = Convert.ToInt32(row["RolId"])
                };

                lst.Add((T)Convert.ChangeType(rel, typeof(T)));
            }

            return lst;
        }


        // Otros métodos no requeridos de momento
        public override void Delete(BaseDTO baseDTO) => throw new NotImplementedException();
        public override T Retrieve<T>() => throw new NotImplementedException();
        public override List<T> RetrieveAll<T>() => throw new NotImplementedException();
        public override T RetrieveById<T>(int id) => throw new NotImplementedException();
        public override void Update(BaseDTO baseDTO) => throw new NotImplementedException();


    }

}