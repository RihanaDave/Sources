using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DollarSignSVFilesConverterAndMerger
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 3)
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

            List<string> inputFilesPath = new List<string>();
            for (int i = 1; i <= args.Count() - 2; i++)
            {
                if (!File.Exists(args[i]))
                {
                    Console.WriteLine("Atleast one input file is not exist/accessable");
                    return;
                }
                inputFilesPath.Add(args[i]);
            }
            
            string outputFilePath = args[args.Count() - 1];
            // Validate output path
            try
            {
                Uri validationUri = new Uri(outputFilePath, UriKind.RelativeOrAbsolute);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid output path; " + ex.Message);
            }
            
            StreamWriter outputWriter = new StreamWriter(outputFilePath);
            try
            {
                foreach (var currentInputFilePath in inputFilesPath)
                {
                    StreamReader inputReader = new StreamReader(currentInputFilePath);
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
                    }
                }
            }
            finally
            {
                outputWriter.Close();
            }

            Console.WriteLine("Conversion completed.");
        }
    }
}
