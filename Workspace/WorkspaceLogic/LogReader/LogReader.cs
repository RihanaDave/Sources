using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GPAS.Workspace.Logic.LogReader
{
    public class LogReader
    {
        private NetworkStream stream;
        private readonly byte[] buffer = new byte[5 * 1024];

        public event EventHandler<DataSourceImportingStateEventArgs> DataSourceImportingStateChanged;
        protected void OnDataSourceImportingStateChanged(DataSourceImportingStateEventArgs e)
        {
            DataSourceImportingStateChanged?.Invoke(this, e);
        }

        public LogReader()
        {
        }

        public void Start(IPAddress serviceAddress, int port)
        {
            Connect(serviceAddress, port);
            stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReceiveData), null);
        }

        private void Connect(IPAddress serviceAddress, int port)
        {
            if (stream == null)
            {
                TcpClient client = new TcpClient();
                client.Connect(serviceAddress, port);
                stream = client.GetStream();
            }
        }

        private void OnReceiveData(IAsyncResult ar)
        {
            try
            {
                if (stream != null)
                {
                    int length = stream.EndRead(ar);
                    string data = Encoding.UTF8.GetString(buffer, 0, length);
                    string[] splitedData = data.Split(':');

                    long dataSourceId = -1;
                    string messsage = string.Empty;
                    if (splitedData.Length >= 2)
                    {
                        messsage = splitedData[splitedData.Length - 1];
                        if (!long.TryParse(splitedData[splitedData.Length - 2], out dataSourceId))
                            dataSourceId = -1;
                    }
                    OnDataSourceImportingStateChanged(new DataSourceImportingStateEventArgs(dataSourceId, messsage));
                }
            }
            finally
            {
                stream?.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReceiveData), null);
            }
        }

        public void Stop()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
        }
    }
}
