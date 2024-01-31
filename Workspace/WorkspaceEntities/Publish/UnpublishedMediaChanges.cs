using GPAS.Workspace.Entities;
using System.Collections.Generic;

namespace GPAS.Workspace.Entities
{
    public class UnpublishedMediaChanges
    {
        public IEnumerable<KWMedia> AddedMedias
        { get; set; }

        public IEnumerable<KWMedia> DeletedMedias
        { get; set; }
    }
}