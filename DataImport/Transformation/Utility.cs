using GPAS.Logger;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GPAS.DataImport.Transformation
{
    public class Utility
    {
        private readonly int SemiStructuredDataColumnsLimit = 500;
        string HasSameColumnMessageException = "Data source '{0}' of file '{1}' has columns with the same name.\n\n" +
            "Please first correct the name of the data source columns and re-enter it.";

        internal string[][] GetFieldsFromTable(DataTable preview)
        {
            string[][] result = new string[preview.Rows.Count + 1][];

            result[0] = new string[preview.Columns.Count];
            foreach (DataColumn col in preview.Columns)
            {
                result[0][col.Ordinal] = col.ColumnName;
            }

            int rowIndex = 0;
            foreach (DataRow row in preview.Rows)
            {
                result[rowIndex + 1] = new string[preview.Columns.Count];
                for (int columnIndex = 0; columnIndex < preview.Columns.Count; columnIndex++)
                {
                    result[rowIndex + 1][columnIndex] = (row[columnIndex] == null) ? "" : row[columnIndex].ToString();
                }
                rowIndex++;
            }

            return result;
        }

        /// <summary>Get fields of parsable rows from CSV content stream for transformation process</summary>
        /// <param name="csvStream">Source CSV stream</param>
        /// <param name="separator">Separator character for CSV content</param>
        /// <param name="logger">Report process and exceptions and avoid parse break; If equals 'null' no exception may thrown</param>
        /// <param name="readLimitedNumberOfRows">If 'True' only read the specified number of rows, otherwise read total rows and avoid rows count limit</param>
        /// <param name="parsableRowsCountLimit">In case of readLimitedNumberOfRows equals 'True', indicates number of rows that may read form start of the stream (Header row is discounted from the limitatiom)</param>
        /// <returns>Matrix of fields; Rows are the data records from CSV lines</returns>
        internal string[][] GetParsableFieldsFromCsvContentStream(Stream csvStream, char separator, ProcessLogger logger = null, bool readLimitedNumberOfRows = false, int parsableRowsCountLimit = 10, string filePath = "", int timeOut = -1)
        {
            List<string[]> totalFields = new List<string[]>(parsableRowsCountLimit);

            using (StreamReader reader = new StreamReader(csvStream))
            {
                using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(reader))
                {
                    csvReader.Configuration.Delimiter = separator.ToString();
                    csvReader.Configuration.BadDataFound = (readerContext) =>
                    {
                        if (logger != null)
                            logger.WriteLog($"Bad data found at row #{readerContext.Row}");
                    };

                    if (readLimitedNumberOfRows)
                    {
                        DateTime start = DateTime.Now;
                        while ((timeOut <= 0 || totalFields.Count < 2 || (DateTime.Now - start).TotalMilliseconds < timeOut) &&
                            csvReader.Read() && parsableRowsCountLimit > (totalFields.Count - 1))
                        {
                            try
                            {
                                totalFields.Add(csvReader.Context.Record);
                            }
                            catch (Exception ex)
                            {
                                SaveExceptionLog(ex, logger);
                            }
                        }
                    }
                    else
                    {
                        while (csvReader.Read())
                        {
                            try
                            {
                                totalFields.Add(csvReader.Context.Record);
                            }
                            catch (Exception ex)
                            {
                                SaveExceptionLog(ex, logger);
                            }
                        }
                    }
                }
            }

            string fileName = Path.GetFileName(filePath);
            string dsName = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(fileName))
                fileName = "CSV";

            if (totalFields.Count > 0)
                if (HasSameColumn(totalFields[0]))
                    throw new Exception(string.Format(HasSameColumnMessageException, dsName, fileName));

            return totalFields.ToArray();
        }

        private bool HasSameColumn(string[] header)
        {
            return header.Length != header.Distinct().Count();
            //اگر تعداد آیتم های آرایه 
            //header
            //با تعداد آیتم های آرایه 
            //header
            //پس از 
            //distinct شدن برابر نبود بدان معناست که نام تکراری در هدر وجود داشته است.
        }

        private void SaveExceptionLog(Exception ex, ProcessLogger logger)
        {
            if (logger == null)
            {
                //throw;
            }
            else
            {
                ExceptionDetailGenerator exDetail = new ExceptionDetailGenerator();
                string logMessage = exDetail.GetDetails(ex);
                logger.WriteLog(logMessage);
            }
        }

        public DataTable GenerateDataTableFromCsvLines(IEnumerable<string> csvLines, char separator, string filePath)
        {
            if (csvLines == null)
                throw new ArgumentNullException("csvStream");

            string[][] dataSourceFields = csvLines.Select(l => l.Split(separator)).ToArray();

            string fileName = Path.GetFileName(filePath);
            string dsName = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(fileName))
                fileName = "CSV";

            return GenerateDataTableFromStringArray(dataSourceFields, fileName, dsName);
        }

        public DataTable GenerateDataTableFromCsvContentStream(Stream csvStream, char separator, int numberOfRows, string filePath, int timeOut = -1)
        {
            if (csvStream == null)
                throw new ArgumentNullException("csvStream");
            if (numberOfRows <= 0)
                throw new ArgumentOutOfRangeException("numberOfRows");

            string[][] dataSourceFields = GetParsableFieldsFromCsvContentStream(csvStream, separator, null, true, numberOfRows, filePath, timeOut);

            string fileName = Path.GetFileName(filePath);
            string dsName = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(fileName))
                fileName = "CSV";

            return GenerateDataTableFromStringArray(dataSourceFields, fileName, dsName);
        }

        public DataTable GenerateDataTableFromStringArray(string[][] dataSourceFields, string fileName, string dsName)
        {
            DataTable result = new DataTable();

            if (dataSourceFields.Length > 0)
            {
                if (HasSameColumn(dataSourceFields[0]))
                    throw new Exception(string.Format(HasSameColumnMessageException, dsName, fileName));

                foreach (var currentColumnHeader in dataSourceFields[0])
                {
                    result.Columns.Add(new DataColumn(currentColumnHeader.Trim()));
                }
                for (int i = 1; i < dataSourceFields.Length; i++)
                {
                    DataRow row = result.NewRow();
                    row.ItemArray = dataSourceFields[i].Take(result.Columns.Count).ToArray();
                    result.Rows.Add(row);
                }
            }

            return result;
        }

        private string GetAccessConnectionString(string path)
        {
            return string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Persist Security Info=False", path);
        }

        public List<string> GetAccessFileTables(string accessFilePath)
        {
            if (accessFilePath == null)
            {
                throw new ArgumentNullException(nameof(accessFilePath));
            }

            string connectionString = GetAccessConnectionString(accessFilePath);
            List<string> result = new List<string>();
            OleDbConnection connection = null;

            try
            {
                using (connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    var restriction = new string[4] { null, null, null, "Table" };
                    DataTable tables = connection.GetSchema("Tables", restriction);
                    foreach (DataRow row in tables.Rows)
                    {
                        var tableName = row.Field<string>("TABLE_NAME");
                        result.Add(tableName);
                    }
                    connection.Dispose();
                    connection.Close();
                }
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                    connection.Close();
                }
            }

            return result;
        }

        public string[] GetExcelFileSheets(string excelFilePath)
        {
            string[] result = null;
            if (!File.Exists(excelFilePath))
            {
                throw new FileNotFoundException("excel File Not Found ");
            }

            var package = new ExcelPackage(new FileInfo(excelFilePath));
            var sheets = package.Workbook.Worksheets;
            result = sheets.Select(sh => sh.Name).ToArray();
            return result;
        }

        public DataTable GetDataTableFromAccessFile(string accessFilePath, string tableName, int numberOfRows = 10, bool readLimitedNumberOfRows = false, bool hasHeader = true)
        {
            if (!File.Exists(accessFilePath))
            {
                throw new FileNotFoundException("Access File Not Found ");
            }

            if (tableName == null)
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            string[][] parsableFieldsFromExcelSheet = GetParsableFieldsFromAccessTable(accessFilePath, tableName, readLimitedNumberOfRows, numberOfRows, hasHeader);
            DataTable table = new DataTable();
            DataRow row = null;
            for (int rowIndex = 0; rowIndex < parsableFieldsFromExcelSheet.GetLength(0); rowIndex++)
            {
                if (rowIndex != 0)
                {
                    row = table.Rows.Add();
                }
                for (int columnIndex = 0; columnIndex < parsableFieldsFromExcelSheet.FirstOrDefault().Count(); columnIndex++)
                {
                    if (rowIndex == 0)
                    {
                        table.Columns.Add(parsableFieldsFromExcelSheet[0].ElementAt(columnIndex));
                    }
                    else
                    {
                        row[columnIndex] = parsableFieldsFromExcelSheet[rowIndex].ElementAt(columnIndex);
                    }
                }
            }
            return table;
        }

        public DataTable GetDataTableFromExcel(string excelFilePath, string sheetName, int numberOfRows = 10, bool readLimitedNumberOfRows = false, bool hasHeader = true)
        {
            if (!File.Exists(excelFilePath))
            {
                throw new FileNotFoundException("excel File Not Found ");
            }

            string[][] parsableFieldsFromExcelSheet = GetParsableFieldsFromExcelSheet(excelFilePath, sheetName, readLimitedNumberOfRows, numberOfRows, hasHeader);

            if (hasHeader && parsableFieldsFromExcelSheet.Length > 0)
                if (HasSameColumn(parsableFieldsFromExcelSheet[0]))
                    throw new Exception(HasSameColumnMessageException);

            DataTable table = new DataTable();
            DataRow row = null;
            for (int rowIndex = 0; rowIndex < parsableFieldsFromExcelSheet.GetLength(0); rowIndex++)
            {
                if (rowIndex != 0)
                {
                    row = table.Rows.Add();
                }
                for (int columnIndex = 0; columnIndex < parsableFieldsFromExcelSheet.FirstOrDefault().Count(); columnIndex++)
                {
                    if (rowIndex == 0)
                    {
                        table.Columns.Add(parsableFieldsFromExcelSheet[0].ElementAt(columnIndex));
                    }
                    else
                    {
                        row[columnIndex] = parsableFieldsFromExcelSheet[rowIndex].ElementAt(columnIndex);
                    }
                }
            }
            return table;
        }

        public Dictionary<string, DataTable> GetAccessTableToDataTableMapping(string excelFilePath,
            int numberOfRows = 10, bool readLimitedNumberOfRows = false, bool hasHeader = true)
        {
            if (!File.Exists(excelFilePath))
            {
                throw new FileNotFoundException("excel File Not Found ");
            }
            Dictionary<string, DataTable> result = new Dictionary<string, DataTable>();
            List<string> tables = GetAccessFileTables(excelFilePath);
            foreach (string tableName in tables)
            {
                DataTable dataTable = GetDataTableFromAccessFile(excelFilePath, tableName, numberOfRows, readLimitedNumberOfRows, hasHeader);

                if (dataTable == null || dataTable.Rows.Count == 0 || dataTable.Columns.Count == 0)
                {
                    continue;
                }
                if (dataTable.Columns.Count > SemiStructuredDataColumnsLimit)
                {
                    throw new Exception
                        (string.Format
                            (Properties.Resources.The_Access_file_has_a_Table_or_View_with_too_much_columns
                            , dataTable.TableName, dataTable.Columns.Count, SemiStructuredDataColumnsLimit));
                }

                result.Add(tableName, dataTable);
            }
            return result;
        }

        public Dictionary<string, DataTable> GetExcelSheetToDataTableMapping(string excelFilePath
            , int numberOfRows = 10, bool readLimitedNumberOfRows = false, bool hasHeader = true)
        {
            if (!File.Exists(excelFilePath))
            {
                throw new FileNotFoundException(Properties.Resources.Excel_file_not_found);
            }

            Dictionary<string, DataTable> result = new Dictionary<string, DataTable>();

            ExcelWorksheets excelWorksheets = GetExcelWorksheets(excelFilePath);
            foreach (ExcelWorksheet currentWorksheet in excelWorksheets)
            {
                if (currentWorksheet.Dimension == null
                || currentWorksheet.Dimension.Rows == 0
                || currentWorksheet.Dimension.Columns == 0)
                {
                    continue;
                }
                if (currentWorksheet.Dimension.Columns > SemiStructuredDataColumnsLimit)
                {
                    throw new Exception(string.Format(Properties.Resources.The_Excel_file_has_a_worksheet_with_too_much_columns_, currentWorksheet.Name, currentWorksheet.Dimension.Columns, SemiStructuredDataColumnsLimit));
                }

                string[][] worksheetFieldsMatrix = GetParsableFieldsFromExcelWorksheet(currentWorksheet, readLimitedNumberOfRows, numberOfRows, hasHeader);
                DataTable table = new DataTable();
                DataRow row = null;
                for (int rowIndex = 0; rowIndex < worksheetFieldsMatrix.GetLength(0); rowIndex++)
                {
                    if (rowIndex != 0)
                    {
                        row = table.Rows.Add();
                    }
                    for (int columnIndex = 0; columnIndex < worksheetFieldsMatrix[0].Length; columnIndex++)
                    {
                        string fieldValue = worksheetFieldsMatrix[rowIndex][columnIndex];
                        if (rowIndex == 0)
                        {
                            if (table.Columns.Contains(fieldValue))
                            {
                                throw new Exception(string.Format(HasSameColumnMessageException, currentWorksheet, Path.GetFileName(excelFilePath)));
                            }
                            else
                            {
                                table.Columns.Add(fieldValue);
                            }
                        }
                        else
                        {
                            row[columnIndex] = fieldValue;
                        }
                    }
                }

                if (!result.ContainsKey(currentWorksheet.Name))
                {
                    result.Add(currentWorksheet.Name, table);
                }
            }
            return result;
        }

        private ExcelWorksheets GetExcelWorksheets(string excelFilePath)
        {
            FileInfo ff = new FileInfo(excelFilePath);
            ExcelPackage pck;
            try
            {
                pck = new ExcelPackage(ff);
            }
            catch (Exception ex)
            {
                throw new Exception(Properties.Resources.Unable_to_load_Excel_file, ex);
            }
            return pck.Workbook.Worksheets;
        }

        public long GetNumberOfRowAccessFile(string accessFilePath, string tableName)
        {
            int total = 0;
            string connString = GetAccessConnectionString(accessFilePath);
            using (OleDbConnection connection = new OleDbConnection(connString))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand($"SELECT count(*) FROM `{tableName}`", connection);
                total = (int)command.ExecuteScalar();
            }
            return total;
        }


        public string[][] GetParsableFieldsFromAccessTable(string accessFilePath, string tableName, bool readLimitedNumberOfRows = false, int numberOfRows = 10, bool hasHeader = true)
        {
            if (!File.Exists(accessFilePath))
            {
                throw new FileNotFoundException("excel File Not Found ");
            }

            long total = GetNumberOfRowAccessFile(accessFilePath, tableName);
            List<string[]> totalFields = null;
            long previewRowsCount;

            if (readLimitedNumberOfRows)
            {
                previewRowsCount = numberOfRows;
            }
            else
            {
                previewRowsCount = total;
            }

            totalFields = new List<string[]>();

            using (OleDbConnection connection = new OleDbConnection(GetAccessConnectionString(accessFilePath)))
            {
                connection.Open();
                OleDbDataReader reader = null;
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                    new Object[] { null, null, tableName, null });

                string colName = null;
                List<string> headers = new List<string>();

                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    headers.Add(schemaTable.Rows[i].ItemArray[3].ToString());
                    colName += $"`{schemaTable.Rows[i].ItemArray[3].ToString()}` ,";
                }

                colName = colName.Remove(colName.Length - 2, 2);

                string query = $"SELECT TOP {previewRowsCount} {colName} from `{tableName}`";
                OleDbCommand command = new OleDbCommand(query, connection);
                reader = command.ExecuteReader();
                var fieldCount = reader.FieldCount;

                //List the column name from each row in the schema table.

                totalFields.Add(headers.ToArray());
                while (reader.Read())
                {
                    List<string> currentRow = new List<string>(fieldCount);
                    for (int i = 0; i < fieldCount; i++)
                    {
                        currentRow.Add(reader[i].ToString());
                    }
                    totalFields.Add(currentRow.ToArray());
                }
            }
            return totalFields.ToArray();
        }


        public string[][] GetParsableFieldsFromExcelSheet(string excelFilePath, string sheetName, bool readLimitedNumberOfRows = false, int numberOfRows = 10, bool hasHeader = true)
        {
            if (!File.Exists(excelFilePath))
            {
                throw new FileNotFoundException("excel File Not Found ");
            }

            List<string[]> totalFields = null;
            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(excelFilePath))
                {
                    pck.Load(stream);
                }
                var sheets = pck.Workbook.Worksheets;
                var ws = sheets.Where(sh => sh.Name.Equals(sheetName)).First();
                var startRow = hasHeader ? ws.Dimension.Start.Row + 1 : ws.Dimension.Start.Row;
                int previewRowsCount;
                if (readLimitedNumberOfRows)
                {
                    previewRowsCount = (numberOfRows < ws.Dimension.End.Row) ? (numberOfRows + startRow) : ws.Dimension.End.Row;
                }
                else
                {
                    previewRowsCount = ws.Dimension.End.Row;
                }
                totalFields = new List<string[]>(previewRowsCount);
                List<string> headers = new List<string>();

                var firstRow = ws.Cells[ws.Dimension.Start.Row, 1, ws.Dimension.Start.Column, ws.Dimension.End.Column];
                int columnsCount = 0;
                foreach (string firstRowCell in (firstRow.Value as object[,]))
                {
                    columnsCount++;
                    headers.Add(!string.IsNullOrEmpty(firstRowCell) ? firstRowCell.Trim() : string.Format("Column {0}", columnsCount));
                }

                totalFields.Add(headers.ToArray());

                for (int rowNum = startRow; rowNum <= previewRowsCount; rowNum++)
                {
                    List<string> currentRow = new List<string>(ws.Dimension.End.Column);
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    foreach (var cell in (wsRow.Value as object[,]))
                    {
                        if (cell == null)
                        {
                            currentRow.Add(null);
                        }
                        else
                        {
                            currentRow.Add(cell.ToString().Trim());
                        }
                    }
                    totalFields.Add(currentRow.ToArray());
                }
                return totalFields.ToArray();
            }
        }

        public long GetRowCountFromExcel(string filePath, string sheetName, bool hasHeader)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("excel File Not Found ");
            }

            using (var pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    pck.Load(stream);
                }
                var sheets = pck.Workbook.Worksheets;
                var ws = sheets.Where(sh => sh.Name.Equals(sheetName)).First();
                return hasHeader ? ws.Dimension.End.Row - 1 : ws.Dimension.End.Row;
            }
        }

        public bool IsBigCSV(string filePath, bool hasHeader, int limit = 1000)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("CSV File Not Found ");
            }

            IEnumerable<string> lines = File.ReadLines(filePath);
            if (hasHeader)
                lines = lines.Skip(1);
            return IsBigIEnumerable(lines, limit);
        }

        private bool IsBigIEnumerable<T>(IEnumerable<T> enumerable, int limit = 1000)
        {
            if (enumerable == null)
                return false;

            return enumerable.ElementAtOrDefault(limit) != null;
        }

        public long GetRowCountFromCSV(string filePath, bool hasHeader, bool hasLimit = false, int limit = 1000)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("CSV File Not Found ");
            }

            long count = 0;
            IEnumerable<string> lines = File.ReadLines(filePath);
            if (hasLimit)
            {
                bool isBig = IsBigIEnumerable(hasHeader ? lines.Skip(1) : lines, limit);
                if (isBig)
                {
                    count = limit;
                }
                else
                {
                    count = lines.Take(hasHeader ? limit + 1 : limit).Count();
                    count = hasHeader ? count - 1 : count;
                }
            }
            else
            {
                count = lines.LongCount();
                count = hasHeader ? count - 1 : count;
            }

            return count;
        }

        public string[][] GetParsableFieldsFromExcelWorksheet(ExcelWorksheet excelWorksheet, bool readLimitedNumberOfRows = false, int numberOfRows = 10, bool hasHeader = true)
        {
            if (excelWorksheet == null)
            {
                throw new ArgumentNullException(nameof(excelWorksheet));
            }

            int startRow;
            try
            {
                startRow = hasHeader ? excelWorksheet.Dimension.Start.Row + 1 : excelWorksheet.Dimension.Start.Row;
            }
            catch
            {
                return new string[][] { };
            }

            int previewRowsCount;
            if (readLimitedNumberOfRows)
            {
                previewRowsCount = (numberOfRows < excelWorksheet.Dimension.End.Row) ? (numberOfRows + startRow) : excelWorksheet.Dimension.End.Row;
            }
            else
            {
                previewRowsCount = excelWorksheet.Dimension.End.Row;
            }

            List<string> headers = new List<string>(excelWorksheet.Dimension.End.Column);
            ExcelRange firstRow = excelWorksheet.Cells[excelWorksheet.Dimension.Start.Row, 1, excelWorksheet.Dimension.Start.Row, excelWorksheet.Dimension.End.Column];
            int columnsCount = 0;
            foreach (object firstRowCell in (firstRow.Value as object[,]))
            {
                columnsCount++;
                headers.Add(GetExcelCellStringValue(firstRowCell, string.Format("Column {0}", columnsCount), true));
            }

            List<string[]> totalFields = new List<string[]>(previewRowsCount);
            totalFields.Add(headers.ToArray());
            for (int rowNum = startRow; rowNum <= previewRowsCount; rowNum++)
            {
                List<string> currentRow = new List<string>(excelWorksheet.Dimension.End.Column);
                ExcelRange wsRow = excelWorksheet.Cells[rowNum, 1, rowNum, excelWorksheet.Dimension.End.Column];
                foreach (object cell in (wsRow.Value as object[,]))
                {
                    currentRow.Add(GetExcelCellStringValue(cell, string.Empty, true));
                }
                totalFields.Add(currentRow.ToArray());
            }
            return totalFields.ToArray();

            //using (var pck = new OfficeOpenXml.ExcelPackage())
            //{
            //    using (var stream = File.OpenRead(excelFilePath))
            //    {
            //        pck.Load(stream);
            //    }
            //    var sheets = pck.Workbook.Worksheets;
            //    var ws = sheets.Where(sh => sh.Name.Equals(sheetName)).First();
            //    var startRow = hasHeader ? ws.Dimension.Start.Row + 1 : ws.Dimension.Start.Row;
            //    int previewRowsCount;
            //    if (readLimitedNumberOfRows)
            //    {
            //        previewRowsCount = (numberOfRows < ws.Dimension.End.Row) ? (numberOfRows + startRow) : ws.Dimension.End.Row;
            //    }
            //    else
            //    {
            //        previewRowsCount = ws.Dimension.End.Row;
            //    }
            //    totalFields = new List<string[]>(previewRowsCount);
            //    List<string> headers = new List<string>();

            //    var firstRow = ws.Cells[ws.Dimension.Start.Row, 1, ws.Dimension.Start.Column, ws.Dimension.End.Column];
            //    int columnsCount = 0;
            //    foreach (string firstRowCell in (firstRow.Value as object[,]))
            //    {
            //        columnsCount++;
            //        headers.Add(!string.IsNullOrEmpty(firstRowCell) ? firstRowCell.Trim() : string.Format("Column {0}", columnsCount));
            //    }

            //    totalFields.Add(headers.ToArray());

            //    for (int rowNum = startRow; rowNum <= previewRowsCount; rowNum++)
            //    {
            //        List<string> currentRow = new List<string>(ws.Dimension.End.Column);
            //        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
            //        foreach (var cell in (wsRow.Value as object[,]))
            //        {
            //            if (cell == null)
            //            {
            //                currentRow.Add(null);
            //            }
            //            else
            //            {
            //                currentRow.Add(cell.ToString().Trim());
            //            }
            //        }
            //        totalFields.Add(currentRow.ToArray());
            //    }
            //    return totalFields.ToArray();
            //}
        }

        private string GetExcelCellStringValue(object firstRowCell, string defaultValue = "", bool trimValue = false)
        {
            if (firstRowCell == null)
            {
                return defaultValue;
            }
            string cellStringValue = firstRowCell.ToString();
            if (string.IsNullOrEmpty(cellStringValue))
            {
                return defaultValue;
            }
            else
            {
                if (trimValue)
                {
                    return cellStringValue.Trim();
                }
                else
                {
                    return cellStringValue;
                }
            }
        }

        public bool IsExcelSheetRowsGreaterThan(string excelFilePath, string sheetName, int checkingRowsCount, bool hasHeader = true)
        {
            if (!File.Exists(excelFilePath))
            {
                throw new FileNotFoundException("excel File Not Found ");
            }

            bool result = false;

            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(excelFilePath))
                {
                    pck.Load(stream);
                }

                var sheets = pck.Workbook.Worksheets;
                var ws = sheets.Where(sh => sh.Name.Equals(sheetName)).First();

                result = (checkingRowsCount < ws.Dimension.End.Row) ? true : false;
                return result;
            }
        }

        public bool IsAccessTableRowsGreaterThan(string accessFilePath, string tableName, int checkingRowsCount, bool hasHeader = true)
        {
            if (!File.Exists(accessFilePath))
            {
                throw new FileNotFoundException("access File Not Found ");
            }

            bool result = false;

            result = (checkingRowsCount < GetNumberOfRowAccessFile(accessFilePath, tableName)) ? true : false;
            return result;
        }
    }
}
