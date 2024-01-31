using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csv2CassandraInsertQuery
{
    class Program
    {
        const char SepratorChar = ',';

        static void Main(string[] args)
        {
            foreach (var csvFilePath in args)
            {
                string inputFileName = Path.GetFileNameWithoutExtension(csvFilePath);
                string inputFilePath = Path.GetFullPath(csvFilePath).TrimEnd((Path.GetFileName(csvFilePath)).ToArray());
                StreamReader reader = new StreamReader(csvFilePath);
                string columnsTitleLine;
                if (!reader.EndOfStream)
                {
                    columnsTitleLine = reader.ReadLine();
                }
                else
                {
                    Console.WriteLine("file does not contain header line (atleast one line of data)!");
                    continue;
                }
                //string[] columns = columnsTitleLine.Split(SepratorChar);

                StreamWriter writer = new StreamWriter(inputFilePath + inputFileName + ".cql");
                while (!reader.EndOfStream)
                {
                    string dataLine = reader.ReadLine();
                    //string[] dataFields = dataLine.Split(SepratorChar);

                    string equivalentInsertQuery = string.Format
                        ("INSERT INTO {0} ({1}) VALUES ({2});"
                        , inputFileName
                        , columnsTitleLine
                        , dataLine);
                    writer.WriteLine(equivalentInsertQuery);
                    writer.Flush();
                }
                reader.Close();
                writer.Close();
            }
        }
    }
}