using DataAccess.CRUD;
using DTOs;
using System.Collections.Generic;

namespace CoreApp
{
    public class FinalUserCommerceRolManager : BaseManager
    {
        private readonly FinalUserCommerceRolCrudFactory crudFactory;

        public FinalUserCommerceRolManager()
        {
            crudFactory = new FinalUserCommerceRolCrudFactory();
        }

        public List<Commerce> RetrieveCommercesByUser(int userId)
        {
            return crudFactory.RetrieveCommercesByUser<Commerce>(userId);
        }

        public List<FinalUserCommerceRol> RetrieveRolesByUser(int userId)
        {
            return crudFactory.RetrieveRolesByUser<FinalUserCommerceRol>(userId);
        }
        public List<Dictionary<string, object>> RetrieveRolesByCommerce(int commerceId)
        {
            return crudFactory.RetrieveRolesByCommerce(commerceId);
        }
        public bool HasRoleInCommerce(int userId, int commerceId, int roleId)
           => crudFactory.HasRoleInCommerce(userId, commerceId, roleId);

        public bool IsAdmin(int userId, int commerceId)
            => HasRoleInCommerce(userId, commerceId, 1); 
    }
}
