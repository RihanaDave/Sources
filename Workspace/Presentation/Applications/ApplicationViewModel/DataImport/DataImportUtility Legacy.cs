using GPAS.AccessControl;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using GPAS.Workspace.ViewModel.DataImport;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport
{
    public partial class DataImportUtility
    {
        public static ACL ConvertACLModelToAccessControlACL(ACLModel aclModel)
        {
            ACL acl = new ACL()
            {
                Classification = aclModel.Classification.Identifier,
                Permissions = new List<ACI>(aclModel.Permissions.Select(p => ConvertACIModelToAccessControlACI(p))),
            };

            return acl;
        }

        private static ACI ConvertACIModelToAccessControlACI(ACIModel aciModel)
        {
            ACI aci = new ACI()
            {
                AccessLevel = aciModel.AccessLevel,
                GroupName = aciModel.GroupName,
            };

            return aci;
        }

        public static SQLServerDataSourceModel ConvertTableForImportToSQLServerDataSourceModel(TableForImport tableForImport, string databaseUri)
        {
            return CreateSQlServerDataSource(tableForImport?.UniqueName, databaseUri, tableForImport.Preview);
        }

        public static async Task<bool> IsExistDatabase(string databaseUri)
        {
            if (string.IsNullOrWhiteSpace(databaseUri))
                return false;

            List<string> allDatabasesUri = await ImportProvider.GetUriOfDatabasesForImport();
            return allDatabasesUri.Contains(databaseUri);
        }

        public static async Task<DataTable> GetTableOrViewFromDatabase(string tableName, string databaseUri)
        {
            if (string.IsNullOrWhiteSpace(tableName) || !IsExistDatabase(databaseUri).Result)
                return null;

            List<TableForImport> tableForImports = await ImportProvider.GetTablesAndViewsOfDatabaseForImport(databaseUri);
            return tableForImports?.FirstOrDefault(tfi => tfi.UniqueName == tableName)?.Preview;
        }

        public static async Task<IEnumerable<DatabaseModel>> GetAllTablesAndViewsInDatabases()
        {
            List<string> allDatabasesUri = await ImportProvider.GetUriOfDatabasesForImport();
            List<DatabaseModel> databases = new List<DatabaseModel>();
            foreach (string uri in allDatabasesUri)
            {
                DatabaseModel databaseModel = new DatabaseModel();
                databaseModel.FullPath = uri;
                databaseModel.TablesAndViewsCollection = new ObservableCollection<SQLServerDataSourceModel>(await GetTablesAndViewsFromDatabaseUri(uri));
                databases.Add(databaseModel);
            }

            return databases;
        }

        private static async Task<IEnumerable<SQLServerDataSourceModel>> GetTablesAndViewsFromDatabaseUri(string databaseUri)
        {
            List<TableForImport> tableForImports = await ImportProvider.GetTablesAndViewsOfDatabaseForImport(databaseUri);
            return tableForImports.Select(tfi => ConvertTableForImportToSQLServerDataSourceModel(tfi, databaseUri));
        }
    }
}
