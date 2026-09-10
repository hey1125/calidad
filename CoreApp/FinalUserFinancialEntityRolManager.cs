using DataAccess.CRUD;
using DTOs;
using System.Collections.Generic;

namespace CoreApp
{
    public class FinalUserFinancialEntityRolManager : BaseManager
    {
        private readonly FinalUserFinancialEntityRolCrudFactory crudFactory;

        public FinalUserFinancialEntityRolManager()
        {
            crudFactory = new FinalUserFinancialEntityRolCrudFactory();
        }

        public List<FinancialEntity> RetrieveEntitiesByUser(int userId)
        {
            return crudFactory.RetrieveEntitiesByUser<FinancialEntity>(userId);
        }

        public List<FinalUserFinancialEntityRol> RetrieveRolesByUser(int userId)
        {
            return crudFactory.RetrieveRolesByUser<FinalUserFinancialEntityRol>(userId);
        }
    }
}
