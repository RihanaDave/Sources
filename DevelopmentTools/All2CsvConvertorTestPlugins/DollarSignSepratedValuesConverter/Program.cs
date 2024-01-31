using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleDollarSignSVFileConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() != 3)
            {
                Console.WriteLine("Invalid arguments");
                return;
            }

            char inputSeprator;
            if (!char.TryParse(args[0], out inputSeprator))
            {
                Console.WriteLine("Invalid seprator");
                return;
            }

            string inputFilePath = args[1];
            if(!File.Exists(inputFilePath))
            {
                Console.WriteLine("Input file is not exist/accessable");
                return;
            }
            
            string outputFilePath = args[2];
            // Validate output path
            try
            {
                Uri validationUri = new Uri(outputFilePath, UriKind.RelativeOrAbsolute);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid output path; " + ex.Message);
            }

            StreamReader inputReader = new StreamReader(inputFilePath);
            StreamWriter outputWriter = new StreamWriter(outputFilePath);
            try
            {
                while (!inputReader.EndOfStream)
                {
                    string currentLine = inputReader.ReadLine();
                    outputWriter.WriteLine(currentLine.Replace('$', inputSeprator));
                    outputWriter.Flush();
                }
            }
            finally
            {
                inputReader.Close();
                outputWriter.Close();
            }

            Console.WriteLine("Conversion completed.");
        }
    }
}
