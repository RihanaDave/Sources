using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Dispatch.AdminTools.Model;

namespace GPAS.Dispatch.AdminTools.Model.UserAndGroup
{
    public class UsersToShowInListViewModel : BaseModel
    {
        private string value;
        public string Value
        {
            get => value;
            set
            {
                this.value = value;
            }
        }
        public UserModel userProfileInfo { get; set; }
    }
}
