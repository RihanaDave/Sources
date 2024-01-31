using System;

namespace GPAS.Workspace.Logic.LogReader
{
    public class DataSourceImportingStateEventArgs : EventArgs
    {
        public DataSourceImportingStateEventArgs(long dataSourceId, string message)
        {
            DataSourceId = dataSourceId;
            Message = message;
        }

        public long DataSourceId { get; protected set; }
        public string Message { get; protected set; }
    }
}
