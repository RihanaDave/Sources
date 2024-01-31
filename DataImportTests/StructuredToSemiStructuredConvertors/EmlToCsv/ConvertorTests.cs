using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace GPAS.DataImport.StructuredToSemiStructuredConvertors.EmlToCsv.Tests
{
    [TestClass()]
    public class ConvertorTests
    {
        private const string TempTargetRootPath = @".\TempTargetPath\";
        private const string TestResourcesRootPath = @".\EmlToCsvConvertorResources\";

        [TestInitialize]
        public void Init()
        {
            if (!Directory.Exists(TempTargetRootPath))
            {
                Directory.CreateDirectory(TempTargetRootPath);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (string subDir in Directory.GetDirectories(TempTargetRootPath))
            {
                Directory.Delete(subDir, true);
            }
        }

        private FileInfo[] GetAllEmlFileInDirectory(string dirPath)
        {
            List<FileInfo> result = new List<FileInfo>();
            AddDirAndSubDirFilesToList(dirPath, ref result);
            return result.ToArray();
        }

        private void AddDirAndSubDirFilesToList(string dirPath, ref List<FileInfo> result)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
                Assert.Fail();
            result.AddRange(dirInfo.GetFiles("*.eml"));
            foreach (var subDir in dirInfo.GetDirectories())
            {
                AddDirAndSubDirFilesToList(subDir.FullName, ref result);
            }
        }
        
        [TestMethod()]
        public void ConvertSmapleEml1()
        {
            // Assign
            string targetPath = $"{TempTargetRootPath}{Guid.NewGuid()}";
            Directory.CreateDirectory(targetPath);
            Convertor convertor = new Convertor()
            {
                SourceFiles = GetAllEmlFileInDirectory($@"{TestResourcesRootPath}1\"),
                TargetDirectoryPath = targetPath
            };
            string[][] resultMatrix;
            // Act
            resultMatrix = convertor.PerformConversionToInMemoryMatrix();
            // Assert
            Assert.AreEqual(3, resultMatrix.Length);
        }

        [TestMethod()]
        public void ConvertSmapleEml2()
        {
            // Assign
            string targetPath = $"{TempTargetRootPath}{Guid.NewGuid()}";
            Directory.CreateDirectory(targetPath);
            Convertor convertor = new Convertor()
            {
                SourceFiles = GetAllEmlFileInDirectory($@"{TestResourcesRootPath}2\"),
                TargetDirectoryPath = targetPath
            };
            string[][] resultMatrix;
            // Act
            resultMatrix = convertor.PerformConversionToInMemoryMatrix();
            // Assert
            Assert.AreEqual(1, resultMatrix.Length);
        }
    }
}