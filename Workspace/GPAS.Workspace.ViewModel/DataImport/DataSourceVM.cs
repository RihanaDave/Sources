using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class DataSourceVM
    {
        public DataSourceVM() { }
        public string Title { get; set; }
        public BitmapImage Icon { get; set; }
        public MaterialBaseVM relatedMaterialBaseVM { get; set; }
    }
}
