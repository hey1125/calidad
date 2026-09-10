using DataAccess.CRUD;
using System.Collections.Generic;

namespace CoreApp
{
    public class RolesManager : BaseManager
    {
        private readonly RolesCrudFactory crudFactory;

        public RolesManager()
        {
            crudFactory = new RolesCrudFactory();
        }

        public void AddCashierByNationalId(string nationalId, int commerceId, int roleId)
        {
            crudFactory.AddCashierByNationalId(nationalId, commerceId, roleId);
        }

        public List<Dictionary<string, object>> RetrieveCashiers(int commerceId)
            => crudFactory.RetrieveCashiers(commerceId);

        public void DeleteCashier(int userId, int commerceId)
            => crudFactory.DeleteCashier(userId, commerceId);
    }

}
