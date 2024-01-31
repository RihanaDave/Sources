using System.Data;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class ExcelSheetMaterialVM : SemiStructuredMaterialVM
    {
        public ExcelSheetMaterialVM(bool isSelected, string title, string excelFilePath, string sheetName, DataTable preview)
            : base(isSelected, title, preview)
        {
            ExcelLocalFilePath = excelFilePath;
        }
        public string ExcelLocalFilePath { get; set; }
        public string SheetName { get; set; }
    }
}
