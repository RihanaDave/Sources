﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping.Unstructured
{
    [DataContract]
    public enum RelationshipBaseLinkMappingRelationDirection
    {
        PrimaryToSecondary,
        SecondaryToPrimary,
        Bidirectional
    }
}
