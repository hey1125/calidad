using DataAccess.CRUD;
using System.Collections.Generic;

namespace CoreApp
{
    public class RoleManager : BaseManager
    {
        private readonly RoleCrudFactory _crud;

        public RoleManager()
        {
            _crud = new RoleCrudFactory();
        }

        public List<Dictionary<string, object>> RetrieveAll()
            => _crud.RetrieveAll();

        public Dictionary<string, object> CreateAndAssignToUser(string roleName, string nationalId, int commerceId)
            => _crud.CreateAndAssignToUser(roleName, nationalId, commerceId);
    }
}
