using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.Logger;
using GPAS.Utility;
using MsgReader.Mime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GPAS.DataImport.StructuredToSemiStructuredConvertors.EmlToCsv
{
    public class Convertor
    {
        public FileInfo[] SourceFiles { get; set; }
        public bool SpliteCsvFiles { get; set; } = true;
        public int SplitedCsvFileMaxLinesCount { get; set; } = 25000;
        public string TargetDirectoryPath { get; set; } = Environment.CurrentDirectory;
        public ProcessLogger Logger { get; set; } = null;
        public bool ReportFullDetails { get; set; } = true;
        public string AttachmentsPathPrefix { get; set; } = string.Empty;
        public static char OutputCsvSeparatorChar { get; private set; } = ',';

        private void ValidateInputsProperties()
        {
            if (SourceFiles == null)
                throw new ArgumentNullException(nameof(SourceFiles));
            if (SourceFiles.Length < 1)
                throw new ArgumentOutOfRangeException(nameof(SourceFiles), "No such source file defined.");
            if (SpliteCsvFiles == true && SplitedCsvFileMaxLinesCount < 1)
                throw new ArgumentOutOfRangeException(nameof(SplitedCsvFileMaxLinesCount), "In 'Splite Csv Files' mode, 'Max Lines Count' must atleast set to 'one'");
            if (string.IsNullOrWhiteSpace(TargetDirectoryPath))
                throw new ArgumentException("Argument not defined", nameof(TargetDirectoryPath));
            if (!Directory.Exists(TargetDirectoryPath))
                throw new DirectoryNotFoundException("'Target Directory' not exist or is not accessable");
        }

        /// <summary></summary>
        /// <returns>Converted CSV files path</returns>
        public string[] PerformConversionToCsvFiles()
        {
            ValidateInputsProperties();
            PrepareInputPathes();

            OutputType = ConvertorOutput.CsvFile;
            dataPartNumber = 1;
            numberOfRowsWritenInCurrentCsv = 0;
            outputCsvFilesPath = new List<string>();
            OutputCsvWriter = GetNewCsvWriter(dataPartNumber);

            try
            {
                ExtractSourceEmlFiles();
            }
            finally
            {
                FinalizeCsvWriter(OutputCsvWriter, dataPartNumber);
            }
            return outputCsvFilesPath.ToArray();
        }

        public string[][] PerformConversionToInMemoryMatrix(int outputRowsLimit = 10)
        {
            ValidateInputsProperties();
            if (outputRowsLimit <= 1)
                throw new ArgumentOutOfRangeException(nameof(outputRowsLimit));
            PrepareInputPathes();

            OutputType = ConvertorOutput.InMemoryMatrix;
            OutputMatrix = new List<string[]>(SourceFiles.Length);
            AddOutputMatrixHeader();
            outputMatrixRowsLimit = outputRowsLimit;

            ExtractSourceEmlFiles();
            return OutputMatrix.ToArray();
        }

        private MsgReader.Reader reader = new MsgReader.Reader();
        private int dataPartNumber;
        private int numberOfRowsWritenInCurrentCsv;
        private int outputMatrixRowsLimit;
        private const int LogWritingPerNSuccessfullyConvertedRows = 500;
        private const string SourceFilesExtension = ".eml";
        private ConvertorOutput OutputType = ConvertorOutput.Unkonwn;

        private List<string[]> OutputMatrix;
        private CsvHelper.CsvWriter OutputCsvWriter;
        private List<string> outputCsvFilesPath;

        private void PrepareInputPathes()
        {
            DirectoryInfo targetDirectoryInfo = new DirectoryInfo(TargetDirectoryPath);
            if (!targetDirectoryInfo.Exists)
            {
                WriteLog("Target path not exist!");
                throw new DirectoryNotFoundException("'TargetDirectoryPath' is not exist/accessable");
            }
            TargetDirectoryPath = targetDirectoryInfo.FullName;
            TargetDirectoryPath = RemovePathEndingSlashIfExist(TargetDirectoryPath);

            if (!Directory.Exists(AttachmentsPathPrefix))
            {
                WriteLog("Warning: Documents' path prefix is not accessable from here; It must be accessable for the CSV target user.");
            }
            AttachmentsPathPrefix = RemovePathEndingSlashIfExist(AttachmentsPathPrefix);
        }

        private string RemovePathEndingSlashIfExist(string path)
        {
            if (path.EndsWith("\\") || path.EndsWith("/"))
            {
                return path.Substring(0, path.Length - 1);
            }
            else
            {
                return path;
            }
        }

        HashSet<FileHash> LoadedFilesHash = new HashSet<FileHash>();

        private void ExtractSourceEmlFiles()
        {
            FileUtility fileUtility = new FileUtility();
            bool IsExtractionLimitAchieved = false;
            foreach (FileInfo sourceEmlFileInfo in SourceFiles)
            {
                if (sourceEmlFileInfo == null
                 || sourceEmlFileInfo.Extension.ToLower() != SourceFilesExtension)
                {
                    continue;
                }
                FileHash sourceEmlFileHash = fileUtility.ComputeFileHashFromFileFilePath(sourceEmlFileInfo.FullName);
                if (LoadedFilesHash.Contains(sourceEmlFileHash))
                {
                    continue;
                }
                // به خاطر حفظ یگانگی مسیر در شرایط از بین رفتن درخت پوشه‌بندی از شناسه یکتا استفاده شده است
                string attachmentsExtractionPath = $"{AttachmentsPathPrefix}\\{Guid.NewGuid().ToString().Replace("-", "")}";
                try
                {
                    // Extract Content
                    Message message = Message.Load(sourceEmlFileInfo);
                    List<ExtractedEmailingEvent> emailingEvents
                        = ExtractedEmailingEvent.GetExtractedEmailingEventsFromMimeMessage
                            (message, attachmentsExtractionPath);
                    if (emailingEvents.Count == 0)
                    {
                        //WriteLog($"No attachment for: {sourceEmlFileInfo.FullName}");
                        continue;
                    }
                    if (OutputType == ConvertorOutput.CsvFile
                        && SpliteCsvFiles == true
                        && numberOfRowsWritenInCurrentCsv + emailingEvents.Count > SplitedCsvFileMaxLinesCount)
                    {
                        FinalizeCsvWriter(OutputCsvWriter, dataPartNumber);
                        OutputCsvWriter = GetNewCsvWriter(++dataPartNumber);
                        numberOfRowsWritenInCurrentCsv = 0;
                        if (emailingEvents.Count > SplitedCsvFileMaxLinesCount)
                        {
                            WriteLog($"Extracted events from \"{sourceEmlFileInfo.FullName}\" are more than 'Max rows in single CSV' option, but to prevent data loss will save to single CSV");
                        }
                    }
                    foreach (ExtractedEmailingEvent currentEvent in emailingEvents)
                    {
                        switch (OutputType)
                        {
                            case ConvertorOutput.InMemoryMatrix:
                                AddNewRowToOutputMatrix(currentEvent);
                                if (OutputMatrix.Count >= outputMatrixRowsLimit + 1)
                                {
                                    IsExtractionLimitAchieved = true;
                                }
                                break;
                            case ConvertorOutput.CsvFile:
                                OutputCsvWriter.WriteRecord(currentEvent);
                                OutputCsvWriter.NextRecord();
                                break;
                            default:
                                throw new NotSupportedException("Convert output is not supported");
                        }
                        numberOfRowsWritenInCurrentCsv++;
                        if (numberOfRowsWritenInCurrentCsv % LogWritingPerNSuccessfullyConvertedRows == 0)
                        {
                            WriteLog($"{numberOfRowsWritenInCurrentCsv} rows were writen at part #{dataPartNumber}");
                        }
                        if (IsExtractionLimitAchieved)
                        {
                            break;
                        }
                    }
                    // Extract attachment(s)
                    if (!Directory.Exists(attachmentsExtractionPath))
                        Directory.CreateDirectory(attachmentsExtractionPath);
                    if (message.Attachments.Count > 0)
                    {
                        string[] extractedAttachments = reader.ExtractToFolderFromCom(sourceEmlFileInfo.FullName, attachmentsExtractionPath);
                        //string subjectWithoutInvalidFileNameChars = RemoveInvalidFileNameChars(message.Headers.Subject);
                        //IEnumerable<string> realAttachments;
                        //if (message.HtmlBody != null)
                        //{
                        //    realAttachments = extractedAttachments.Except(new string[] { $"{attachmentsExtractionFullPath}\\{subjectWithoutInvalidFileNameChars}.htm" }); 
                        //}
                        //else
                        //{
                        //    realAttachments = extractedAttachments;
                        //}
                        DirectoryInfo attachmentsDirectoryInfo = new DirectoryInfo(attachmentsExtractionPath);
                        foreach (FileInfo extractedAdditionalFileInfo in attachmentsDirectoryInfo.GetFiles())
                        {
                            if (!message.Attachments.Any(att => att.FileName.Equals(extractedAdditionalFileInfo.Name))
                                || !extractedAttachments.Contains(extractedAdditionalFileInfo.FullName))
                            {
                                File.Delete(extractedAdditionalFileInfo.FullName);
                            }
                            else
                            {
                                WriteLog($"Extracted attachment: {extractedAdditionalFileInfo.FullName} ({extractedAdditionalFileInfo.Length.ToString("#,##0")} Bytes)");
                            }
                        }
                    }

                    LoadedFilesHash.Add(sourceEmlFileHash);
                    if (IsExtractionLimitAchieved)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Unable to extract data from \"{sourceEmlFileInfo.FullName}\";{Environment.NewLine}Reason: {ex.Message}{Environment.NewLine}Call Stack: {ex.StackTrace}");
                }
            }
        }

        private void AddNewRowToOutputMatrix(ExtractedEmailingEvent currentEvent)
        {
            string[] fieldsValue
                = typeof(ExtractedEmailingEvent).GetRuntimeFields()
                    .Select(f => f.GetValue(currentEvent).ToString())
                    .ToArray();
            OutputMatrix.Add(fieldsValue);
        }

        private void AddOutputMatrixHeader()
        {
            PropertyInfo[] EventPropertiesInfo = typeof(ExtractedEmailingEvent).GetProperties();
            string[] EventPropertiesName = EventPropertiesInfo.Select(i => i.Name).ToArray();
            OutputMatrix.Add(EventPropertiesName);
        }

        private string RemoveInvalidFileNameChars(string cleaingString)
        {
            StringBuilder sb = new StringBuilder(cleaingString);
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                sb.Replace(invalidChar.ToString(), "");
            }
            return sb.ToString();
        }

        private void WriteLog(string logMessage)
        {
            logMessage = $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] {logMessage}";
            Console.WriteLine(logMessage);
            if (Logger != null)
            {
                Logger.WriteLog(logMessage);
            }
        }

        private void FinalizeCsvWriter(CsvHelper.CsvWriter csvWriter, int dataPartNumber)
        {
            csvWriter.Dispose();
            WriteLog($"Part #{dataPartNumber} completed.");
        }

        private CsvHelper.CsvWriter GetNewCsvWriter(int dataPartNumber)
        {
            string newCsvPath = $"{TargetDirectoryPath}\\Data_{dataPartNumber}.csv";
            TextWriter textWriter = new StreamWriter(newCsvPath, false, Encoding.UTF8);
            outputCsvFilesPath.Add(newCsvPath);
            WriteLog($"Start extraction part #{dataPartNumber} ...");
            CsvHelper.CsvWriter writer = new CsvHelper.CsvWriter(textWriter);
            //writer.Configuration.UseNewObjectForNullReferenceMembers = true; // Default is true
            writer.Configuration.CultureInfo = CultureInfo.InvariantCulture;
            writer.Configuration.QuoteAllFields = true;
            writer.Configuration.Delimiter = OutputCsvSeparatorChar.ToString();
            writer.WriteHeader<ExtractedEmailingEvent>();
            writer.NextRecord();
            return writer;
        }

        public static TypeMapping GetDefaultTypeMappingToImportOutputCsv()
        {
            TypeMappingSerializer serializer = new TypeMappingSerializer();
            string runningAssemblyFullPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string mappingPath = $"{runningAssemblyFullPath.Substring(0, runningAssemblyFullPath.LastIndexOf('\\'))}\\StructuredToSemiStructuredConvertors\\EmlToCsv\\CSV-ed EMLs Mapping.imm";
            return serializer.DeserializeFromFile(mappingPath);
        }

        public long GetRowCountFromExtractedTable()
        {
            ValidateInputsProperties();
            PrepareInputPathes();

            return SourceFiles.Select(sf => GetMessageReciverCount(Message.Load(sf))).Sum();
        }

        private long GetMessageReciverCount(Message message)
        {
            return message.Headers.Bcc.Where(bcc => bcc != null).LongCount() +
                message.Headers.Cc.Where(cc => cc != null).LongCount() +
                message.Headers.To.Where(to => to != null).LongCount();
        }
    }
}
