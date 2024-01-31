using GPAS.AccessControl.Groups;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.SearchServer.Logic.Synchronization
{
    public class SchemaChangeProvider
    {
        public void AddNewGroupFieldsToSearchServer(List<string> newGroupsName)
        {
            if (newGroupsName == null)
                throw new ArgumentNullException(nameof(newGroupsName));
            if (newGroupsName.Any(g => !GroupNameValidator.IsGroupNameValid(g)))
                throw new ArgumentException("At least one group name is not valid!");

            foreach (string groupName in newGroupsName)
            {
                SearchEngineProvider.GetNewDefaultSearchEngineClient().AddFieldToSchema(groupName, Entities.SearchEngine.DataType.Int);
            }
        }
    }
}
