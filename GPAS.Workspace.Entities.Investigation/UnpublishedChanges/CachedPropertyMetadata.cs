using GPAS.Dispatch.Entities.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.Investigation.UnpublishedChanges
{
    public class CachedPropertyMetadata
    {
        public KProperty CachedProperty;
        public bool IsPublished;
        public bool IsModified;
    }
}
