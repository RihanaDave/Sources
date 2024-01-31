using GPAS.DataImport.DataMapping.SemiStructured;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping
{
    [Serializable]
    public class PathPartMappingItem : ConstValueMappingItem
    {
        public PathPartMappingItem() { }

        public PathPartMappingItem(string constValue, string fullPath, PathPartTypeMappingItem type, PathPartDirectionMappingItem direction, int directoryIndex,
                                        bool hasRegEX = false, string regExPattern = "") : base(constValue)
        {
            SetFullPath(fullPath);
            Type = type;
            Direction = direction;
            DirectoryIndex = directoryIndex;
            HasRegEX = hasRegEX;
            RegEXPattern = regExPattern;
        }

        public void SetFullPath(string fullPath)
        {
            FullPath = fullPath;
        }

        public void SetConstValue(string constValue)
        {
            ConstValue = constValue;
        }

        public string GetFullPath()
        {
            return FullPath;
        }

        string constValue = string.Empty;
        public override string ConstValue
        {
            get
            {
                return GetConstValue(this);
            }
            set
            {
                constValue = value;
            }
        }

        string FullPath { get; set; }
        public PathPartTypeMappingItem Type { get; set; }
        public PathPartDirectionMappingItem Direction { get; set; }
        public int DirectoryIndex { get; set; }
        public bool HasRegEX { get; set; }
        public string RegEXPattern { get; set; }

        public static string GetConstValue(PathPartMappingItem pathPartMappingItem)
        {
            return GetConstValue(pathPartMappingItem, pathPartMappingItem.FullPath);
        }

        public static string GetConstValue(PathPartMappingItem pathPartMappingItem, string fullPath)
        {
            if (pathPartMappingItem == null || string.IsNullOrEmpty(fullPath))
                return pathPartMappingItem.constValue;

            var parts = GetPathParts(fullPath, pathPartMappingItem.Direction);

            string result = string.Empty;
            PathPart part;

            if (pathPartMappingItem.Type == PathPartTypeMappingItem.None)
                return string.Empty;
            else if (pathPartMappingItem.Type == PathPartTypeMappingItem.ComputerName || pathPartMappingItem.Type == PathPartTypeMappingItem.DriveName)
            {
                part = parts.Where(p => p.Type == PathPartTypeMappingItem.ComputerName || p.Type == PathPartTypeMappingItem.DriveName).FirstOrDefault();
            }
            else if (pathPartMappingItem.Type == PathPartTypeMappingItem.FileName)
            {
                part = parts.Where(p => p.Type == PathPartTypeMappingItem.FileName).FirstOrDefault();
            }
            else if (pathPartMappingItem.Type == PathPartTypeMappingItem.Extension)
            {
                part = parts.Where(p => p.Type == PathPartTypeMappingItem.Extension).FirstOrDefault();
            }
            else if (pathPartMappingItem.Type == PathPartTypeMappingItem.Directory)
            {
                part = parts.Where(p => p.Type == PathPartTypeMappingItem.Directory && p.DirectoryIndex == pathPartMappingItem.DirectoryIndex).FirstOrDefault();
            }
            else if (pathPartMappingItem.Type == PathPartTypeMappingItem.FullPath)
            {
                part = parts.Where(p => p.Type == PathPartTypeMappingItem.FullPath).FirstOrDefault();
            }
            else
            {
                throw new NotImplementedException();
            }

            if (part == null)
                result = string.Empty;
            else
                result = part.Text;

            if (pathPartMappingItem.HasRegEX)
            {
                result = ApplyRegEX(result, pathPartMappingItem.RegEXPattern);
            }

            return result;
        }

        public static void UpdateDirectoryIndexForPathParts(List<PathPart> parts, PathPartDirectionMappingItem direction = PathPartDirectionMappingItem.FromBegin)
        {
            if (direction == PathPartDirectionMappingItem.FromEnd)
            {
                int i = 0;
                for (int j = parts.Count - 1; j >= 0; j--)
                {
                    PathPart pp = parts[j];
                    if (pp.Type == PathPartTypeMappingItem.Directory)
                    {
                        pp.DirectoryIndex = ++i;
                    }
                    pp.Direction = PathPartDirectionMappingItem.FromEnd;
                }
            }
            else if (direction == PathPartDirectionMappingItem.FromBegin)
            {
                int i = 0;
                foreach (var pp in parts)
                {
                    if (pp.Type == PathPartTypeMappingItem.Directory)
                    {
                        pp.DirectoryIndex = ++i;
                    }
                    pp.Direction = PathPartDirectionMappingItem.FromBegin;
                }
            }
        }

        public static string ApplyRegEX(string input, string pattern)
        {
            try
            {
                return Regex.Match(input, pattern).Value;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static List<PathPart> GetPathParts(string fullPath, PathPartDirectionMappingItem direction)
        {
            List<PathPart> parts = new List<PathPart>();
            if (direction == PathPartDirectionMappingItem.None)//fullPath
            {
                PathPart fullPart = new PathPart()
                {
                    Direction = direction,
                    Text = fullPath,
                    Type = PathPartTypeMappingItem.FullPath,
                };
                parts.Add(fullPart);
                return parts;
            }

            string extension = Path.GetExtension(fullPath).TrimStart('.');
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            bool netPath = false;

            if (fullPath.StartsWith(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()))// if path net address
            {
                fullPath = fullPath.Substring(2);
                netPath = true;
            }
            var directoryNames = Path.GetDirectoryName(fullPath).Split(Path.DirectorySeparatorChar);

            if (directoryNames?.Length > 0)
            {
                string root = directoryNames[0];
                for (int i = 0; i < directoryNames.Length; i++)
                {
                    if (string.IsNullOrEmpty(directoryNames[i]))
                        continue;

                    PathPart pathPart = new PathPart();
                    pathPart.Text = directoryNames[i];
                    if (i == 0)
                    {
                        if (direction == PathPartDirectionMappingItem.FromBegin)
                        {
                            if (netPath)
                            {
                                pathPart.Type = PathPartTypeMappingItem.ComputerName;
                                pathPart.Text = pathPart.Text;
                            }
                            else
                            {
                                pathPart.Type = PathPartTypeMappingItem.DriveName;
                            }
                            parts.Add(pathPart);
                        }
                    }
                    else
                    {
                        pathPart.Type = PathPartTypeMappingItem.Directory;
                        parts.Add(pathPart);
                    }
                }

                if (direction == PathPartDirectionMappingItem.FromEnd)
                {
                    if (fileNameWithoutExtension != string.Empty)
                        parts.Add(new PathPart() { Text = fileNameWithoutExtension, Type = PathPartTypeMappingItem.FileName });

                    if (extension != string.Empty)
                        parts.Add(new PathPart() { Text = extension, Type = PathPartTypeMappingItem.Extension });
                }
            }

            UpdateDirectoryIndexForPathParts(parts, direction);
            return parts;
        }
    }
}
