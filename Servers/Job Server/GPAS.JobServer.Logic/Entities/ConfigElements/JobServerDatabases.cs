using System;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;

namespace GPAS.JobServer.Logic.Entities.ConfigElements
{
    public class JobServerDatabases : ConfigurationSection
    {
        [ConfigurationProperty("DatabaseServers")]
        [ConfigurationCollection(typeof(DatabaseServersCollection))]
        public DatabaseServersCollection Servers
        {
            get
            {
                return (DatabaseServersCollection)this["DatabaseServers"];
            }
            set
            {
                this["DatabaseServers"] = value;
            }
        }
    }
}
