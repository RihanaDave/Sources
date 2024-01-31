using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class EmlFileMaterialVM : SemiStructuredMaterialVM
    {
        public EmlFileMaterialVM(bool isSelected, string title, DataTable preview)
            : base(isSelected, title, preview)
        {
            EmlFiles = new List<FileInfo>();
            EmlFileNames = new HashSet<string>();
            workspceSideCsvFilePath = string.Empty;
        }

        public void AddEmlFilesIfNotExist(IEnumerable<FileInfo> files)
        {
            foreach (FileInfo file in files)
            {
                AddEmlFileIfNotExist(file);
            }
            Title = string.Format(Properties.Resources._0_eml_file_s_converted_to_csv, EmlFilesCount);
        }
        private void AddEmlFileIfNotExist(FileInfo file)
        {
            if (!EmlFileNames.Contains(file.FullName))
            {
                EmlFiles.Add(file);
                EmlFileNames.Add(file.FullName);
            }
        }

        private List<FileInfo> EmlFiles { get; set; }
        private HashSet<string> EmlFileNames { get; set; }

        public FileInfo[] GetEmlFiles()
        {
            return EmlFiles.ToArray();
        }
        public IEnumerable<string> GetEmlFileNames()
        {
            return EmlFileNames;
        }
        public int EmlFilesCount { get { return EmlFiles.Count; } }

        public string workspceSideCsvFilePath { get; set; }

    }
}
