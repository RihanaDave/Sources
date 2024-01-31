using System;
using System.Configuration;

namespace GPAS.JobServer.Logic.Entities.ConfigElements
{
    public class DatabaseServer : ConfigurationElement
    {
        [ConfigurationProperty("key", DefaultValue = "db", IsRequired = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string Key
        {
            get
            {
                return (string)this["key"];
            }
            set
            {
                this["key"] = value;
            }
        }

        [ConfigurationProperty("hostAddress", DefaultValue = ".", IsRequired = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string HostAddress
        {
            get
            {
                return (string)this["hostAddress"];
            }
            set
            {
                this["hostAddress"] = value;
            }
        }

        [ConfigurationProperty("userName", DefaultValue = "sa", IsRequired = true)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string UserName
        {
            get
            {
                return (string)this["userName"];
            }
            set
            {
                this["userName"] = value;
            }
        }

        [ConfigurationProperty("password", DefaultValue = "", IsRequired = true)]
        [StringValidator(InvalidCharacters = "", MinLength = 0, MaxLength = 250)]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }
    }
}
