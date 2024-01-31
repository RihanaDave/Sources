using System;

namespace GPAS.DataImport.DataMapping
{
    [Serializable]
    public enum PathPartTypeMappingItem
    {
        ComputerName,
        DriveName,
        Directory,
        Extension,
        FileName,
        FullPath,
        None
    }
}
