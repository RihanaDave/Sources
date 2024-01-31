using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class UngroupDataSourceEventArgs : EventArgs
    {
        public UngroupDataSourceEventArgs(GroupDataSourceModel groupDataSource, IEnumerable<IDataSource> ungroupedDataSources)
        {
            GroupDataSource = groupDataSource;
            UngroupedDataSources = ungroupedDataSources;
        }

        public GroupDataSourceModel GroupDataSource { get; protected set; }
        public IEnumerable<IDataSource> UngroupedDataSources { get; protected set; }
    }
}
