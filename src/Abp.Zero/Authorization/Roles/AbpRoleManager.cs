using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Authorization.Users;
using Abp.Dependency;
using Abp.MultiTenancy;
using Microsoft.AspNet.Identity;

namespace Abp.Authorization.Roles
{
    /// <summary>
    /// Extends <see cref="RoleManager{TRole,TKey}"/> of ASP.NET Identity Framework.
    /// </summary>
    public abstract class AbpRoleManager<TTenant, TRole, TUser> : RoleManager<TRole, int>, ITransientDependency
        where TTenant : AbpTenant<TTenant, TUser>
        where TRole : AbpRole<TTenant, TUser>
        where TUser : AbpUser<TTenant, TUser>
    {
        private readonly IPermissionManager _permissionManager;

        protected AbpRoleManager(AbpRoleStore<TRole, TTenant, TUser> store, IPermissionManager permissionManager)
            : base(store)
        {
            _permissionManager = permissionManager;
        }

        public async Task<IReadOnlyList<Permission>> GetGrantedPermissionsAsync(int roleId)
        {
            var role = await FindByIdAsync(roleId);
            if (role == null)
            {
                throw new AbpAuthorizationException("There is no role with id = " + roleId);
            }

            var permissionList = new List<Permission>();

            foreach (var permission in _permissionManager.GetAllPermissions())
            {
                if (await HasPermissionInternalAsync(role, permission))
                {
                    permissionList.Add(permission);
                }
            }

            return permissionList;
        }

        /// <summary>
        /// Checks if a role has a permission.
        /// </summary>
        /// <param name="roleName">The role's name to check it's permission</param>
        /// <param name="permissionName">Name of the permission</param>
        /// <returns>True, if the role has the permission</returns>
        public async Task<bool> HasPermissionAsync(string roleName, string permissionName)
        {
            var role = await FindByNameAsync(roleName);
            if (role == null)
            {
                throw new AbpAuthorizationException("There is no role named " + roleName);
            }

            return await HasPermissionInternalAsync(role, permissionName);
        }

        /// <summary>
        /// Checks if a role has a permission.
        /// </summary>
        /// <param name="roleId">The role's id to check it's permission</param>
        /// <param name="permissionName">Name of the permission</param>
        /// <returns>True, if the role has the permission</returns>
        public async Task<bool> HasPermissionAsync(int roleId, string permissionName)
        {
            var role = await FindByIdAsync(roleId);
            if (role == null)
            {
                throw new AbpAuthorizationException("There is no role by id = " + roleId);
            }

            return await HasPermissionInternalAsync(role, permissionName);
        }

        public override Task<IdentityResult> DeleteAsync(TRole role)
        {
            if (role.IsStatic)
            {
                throw new AbpException("Can not delete a static role: " + role);
            }

            return base.DeleteAsync(role);
        }

        private async Task<bool> HasPermissionInternalAsync(TRole role, string permissionName) //TODO: Async
        {
            var permission = _permissionManager.GetPermissionOrNull(permissionName);
            if (permission == null)
            {
                throw new AbpException("There is no permission with name: " + permissionName);
            }

            return await HasPermissionInternalAsync(role, permission);
        }

        private async Task<bool> HasPermissionInternalAsync(TRole role, Permission permission) //TODO: Async
        {
            if (!(Store is IRolePermissionStore<TRole, TTenant, TUser>))
            {
                throw new AbpException("Store is not IRolePermissionStore");
            }

            var permissionStore = Store as IRolePermissionStore<TRole, TTenant, TUser>;

            return permission.IsGrantedByDefault
                ? !(await permissionStore.HasPermissionAsync(role, new PermissionGrantInfo(permission.Name, false)))
                : (await permissionStore.HasPermissionAsync(role, new PermissionGrantInfo(permission.Name, true)));
        }
    }
}