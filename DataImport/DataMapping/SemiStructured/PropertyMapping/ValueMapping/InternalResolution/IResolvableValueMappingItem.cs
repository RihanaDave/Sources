using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    public interface IResolvableValueMappingItem
    {
        PropertyInternalResolutionOption ResolutionOption { get; set; }
    }
}