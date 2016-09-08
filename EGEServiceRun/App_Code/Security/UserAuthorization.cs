using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using EGEServiceRun.Model;

namespace EGEServiceRun.App_Code.Security
{
    class CustomPrincipal : IPrincipal
    {
        IIdentity _identity;
        string[] _roles;

        public CustomPrincipal(IIdentity identity)
        {
            _identity = identity;
        }

        // helper method for easy access (without casting)
        public static CustomPrincipal Current
        {
            get
            {
                return Thread.CurrentPrincipal as CustomPrincipal;
            }
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }

        // return all roles
        public string[] Roles
        {
            get
            {
                EnsureRoles();
                return _roles;
            }
        }

        // IPrincipal role check
        public bool IsInRole(string role)
        {
            EnsureRoles();

            return _roles.Contains(role);
        }

        // read Role of user from database
        protected virtual void EnsureRoles()
        {
            using (EGESupportEntities context = new EGESupportEntities())
            {
                string role_name = (from u in context.au_users
                                    join r in context.au_roles on u.role_id equals r.id
                                    where u.login == _identity.Name
                                    select r.name).FirstOrDefault();
                _roles = new string[1] { role_name };
            }
        }
    }

    class AuthorizationPolicy : IAuthorizationPolicy
    {
        Guid _id = Guid.NewGuid();

        // this method gets called after the authentication stage
        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            // get the authenticated client identity
            IIdentity client = GetClientIdentity(evaluationContext);

            // set the custom principal
            evaluationContext.Properties["Principal"] = new CustomPrincipal(client);

            return true;
        }

        private IIdentity GetClientIdentity(EvaluationContext evaluationContext)
        {
            object obj;
            if (!evaluationContext.Properties.TryGetValue("Identities", out obj))
                throw new Exception("No Identity found");

            IList<IIdentity> identities = obj as IList<IIdentity>;
            if (identities == null || identities.Count <= 0)
                throw new Exception("No Identity found");

            return identities[0];
        }

        public System.IdentityModel.Claims.ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        public string Id
        {
            get { return _id.ToString(); }
        }
    }

}