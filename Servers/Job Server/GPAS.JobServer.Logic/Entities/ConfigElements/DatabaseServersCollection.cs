using System;
using System.Configuration;

namespace GPAS.JobServer.Logic.Entities.ConfigElements
{
    public class DatabaseServersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DatabaseServer();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DatabaseServer)element).Key;
        }
    }
}
