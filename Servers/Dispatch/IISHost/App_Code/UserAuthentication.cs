using GPAS.Dispatch.Logic;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace GPAS.Dispatch.IISHost.App_Code
{
    public class UserAuthentication : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            UserAccountManagement userAccountManagement = new UserAccountManagement();
            bool authenticated = userAccountManagement.Authenticate(userName, password);
            if (!authenticated)
            {
                throw new FaultException("User Name or Password is wrong...");
            }
        }
    }
}