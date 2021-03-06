﻿using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.NHibernate.EntityMappings;

namespace Abp.Zero.NHibernate.EntityMappings
{
    public class UserMap<TTenant, TUser> : EntityMap<TUser, long>
        where TUser : AbpUser<TTenant, TUser>
        where TTenant : AbpTenant<TTenant, TUser>
    {
        protected UserMap()
            : base("AbpUsers")
        {
            Map(x => x.TenantId);
            Map(x => x.UserName);
            Map(x => x.Name);
            Map(x => x.Surname);
            Map(x => x.EmailAddress);
            Map(x => x.IsEmailConfirmed);
            Map(x => x.EmailConfirmationCode);
            Map(x => x.Password);
            Map(x => x.PasswordResetCode);
            Map(x => x.CreationTime);
            Map(x => x.CreatorUserId);
            Map(x => x.LastLoginTime);

            Polymorphism.Explicit();
        }
    }
}
