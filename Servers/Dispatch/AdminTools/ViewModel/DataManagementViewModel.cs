using GPAS.Dispatch.Logic;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class DataManagementViewModel : BaseViewModel
    {
        public async Task<bool> RemoveAllData()
        {
            RemoveAllData removeAllData = new RemoveAllData();
            return await removeAllData.RemoveAll();
        }
    }
}
