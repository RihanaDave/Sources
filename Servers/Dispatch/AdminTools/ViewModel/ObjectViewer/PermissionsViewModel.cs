using System;
using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.Entities.Concepts;
using System.Collections.ObjectModel;

namespace GPAS.Dispatch.AdminTools.ViewModel.ObjectViewer
{
    public class PermissionsViewModel : BaseViewModel
    {
        public PermissionsViewModel()
        {
            AccessControlLimit = new AccessControlLimitModel
            {
                Permissions = new ObservableCollection<PermissionModel>()
            };
        }

        public AccessControlLimitModel AccessControlLimit { get; set; }

        public void ShowPropertyPermissions(object property)
        {
            long propertyDataSourceId;

            try
            {
                propertyDataSourceId = ((PropertiesModel)property).DataSourceId;
            }
            catch
            {
                propertyDataSourceId = ((RelationModel)property).DataSourceId;
            }
            

            foreach (var dataSource in DataSourceACLs)
            {
                if (dataSource.Id == propertyDataSourceId)
                {
                    AccessControlLimit.Classification = dataSource.Acl.Classification;

                    if (AccessControlLimit.Permissions.Count != 0) AccessControlLimit.Permissions.Clear();

                    foreach (var permission in dataSource.Acl.Permissions)
                    {
                        AccessControlLimit.Permissions.Add(new PermissionModel
                        {
                            GroupName = permission.GroupName,
                            AccessLevel = permission.AccessLevel
                        });
                    }
                }
            }
        }
    }
}
