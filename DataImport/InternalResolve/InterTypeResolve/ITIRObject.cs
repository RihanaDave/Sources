using System.Collections.Generic;
using System.Linq;

namespace GPAS.DataImport.InternalResolve.InterTypeResolve
{
    internal class ITIRObject
    {
        internal ITIRObject(IRObject baseObject, HashSet<string> interTypeMustMatchPropertiesTypeURI)
        {
            BaseObject = baseObject;

            IEnumerable<ITIRProperty> mustMatchProeprties
                = BaseObject.MustMatchProperties
                    .Where(mmp => interTypeMustMatchPropertiesTypeURI.Contains(mmp.TypeURI))
                    .Select(mmp => new ITIRProperty(mmp.TypeURI, mmp.Value));
            IEnumerable<ITIRProperty> ignorableProeprties
                = BaseObject.IgnorableProperties
                    .Where(ip => interTypeMustMatchPropertiesTypeURI.Contains(ip.TypeURI))
                    .SelectMany(ip => ip.Values
                        .Select(v => new ITIRProperty(ip.TypeURI, v)));

            InterTypeMustMatchProperties = new ITIRPropertiesCollection
                (mustMatchProeprties.Concat(ignorableProeprties));
        }

        internal IRObject BaseObject { get; private set; }
        internal ITIRPropertiesCollection InterTypeMustMatchProperties { get; private set; }
    }
}