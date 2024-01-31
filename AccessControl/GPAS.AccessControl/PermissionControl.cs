using System.Collections.Generic;
using System.Linq;

namespace GPAS.AccessControl
{
    public class PermissionControl
    {
        public bool CanWriteWithPermissions(ACL acl, List<GroupClassificationBasedPermission> groupClassificationBasedPermissionList)
        {
            bool classification = false;
            bool permission = false;
            foreach (var groupClassificationBasedPermission in groupClassificationBasedPermissionList)
            {
                if (acl.Permissions.Select(a => a.GroupName).Contains(groupClassificationBasedPermission.GroupName))
                {
                    foreach (var classificationBasedPermission in groupClassificationBasedPermission.Permissions)
                    {
                        List<ClassificationEntry> permittedClassification = Classification.GetClassificationsWithLowerPriorityThan(classificationBasedPermission.Classification);
                        if (permittedClassification.Select(p => p.IdentifierString).Contains(acl.Classification) &&
                             (byte)classificationBasedPermission.AccessLevel >= (byte)Permission.Write)
                        {
                            classification = true;
                            break;
                        }
                    }
                    if (classification)
                        break;
                }
            }
            foreach (string groupName in groupClassificationBasedPermissionList.Select(g => g.GroupName))
            {
                var acis = acl.Permissions.Where(p => p.GroupName == groupName);
                foreach (var aci in acis)
                {
                    if ((byte)aci.AccessLevel >= (byte)Permission.Write)
                    {
                        permission = true;
                        break;
                    }
                }
                if (permission)
                    break;
            }
            return (classification && permission);
        }
    }
}

