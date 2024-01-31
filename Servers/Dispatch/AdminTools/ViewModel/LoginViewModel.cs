using System.Threading.Tasks;
using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.Logic;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            LoginModel = new LoginModel();
        }

        public LoginModel LoginModel { get; set; }

        public async Task<bool> UserLogin()
        {
            bool result = false;

            await Task.Run(() =>
            {
                Authentication authentication = new Authentication();
                result = authentication.Authenticate(LoginModel.UserName, LoginModel.Password);
            });

            return result;
        }
    }
}
