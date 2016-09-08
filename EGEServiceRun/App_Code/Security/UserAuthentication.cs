using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using EGEServiceRun.Model;

namespace EGEServiceRun.App_Code.Security
{
    public class UserAuthentication : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (null == userName || null == password)
            {
                throw new ArgumentNullException();
            }

            if (!IsValidUserPassword(userName, password))
            {
                throw new SecurityTokenValidationException("Неверный логин или пароль");
            }
        }


        private bool IsValidUserPassword(string login, string password)
        {
            bool result = false;
            using (EGESupportEntities context = new EGESupportEntities())
            {
                if (context.au_users.Where(x => x.login == login && x.password == password && x.disable == false).Count() > 0)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}