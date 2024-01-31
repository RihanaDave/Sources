using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchAround.DataMapping
{
    [DataContract]
    public enum LinkMappingDirection
    {
        PrimaryToSecondary,
        SecondaryToPrimary,
        Bidirectional
    }
}
