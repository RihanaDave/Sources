using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVMaker
{
    class Program
    {

        static private List<string> NameOfList()
        {
            List<string> listName = new List<string>();
            using (StreamReader sr = File.OpenText("NameList.txt"))
            {                
                while (!sr.EndOfStream)
                {
                    string myString = sr.ReadLine();
                    listName.Add(myString);
                }                
            }
            return listName;
        }

        static private List<string> GetBrowserNameList()
        {
            List<string> browserlistName = new List<string>();
            using (StreamReader sr = File.OpenText(@"Browser Name List.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string myString = sr.ReadLine();
                    browserlistName.Add(myString);
                }
            }
            return browserlistName;
        }

        static private Int64 GeneratePhoneNumber(Random random)
        {
            Int64 number = new Int64();
            number = 9000000000;
            number += random.Next(0,3) * 100000000;
            number +=  Convert.ToInt64(random.NextDouble() * 10000000);
            return number;
        }

        static private string GenerateIPAddress(Random random)
        {
            string ip = "";
            ip = random.Next(10, 254).ToString() + "." +
                 random.Next(0, 254).ToString() + "." +
                 random.Next(0, 254).ToString() + "." +
                 random.Next(0, 254).ToString();

            return ip;
        }

        static private TimeSpan GenerateRandomTime(Random random)
        {
            TimeSpan time = new TimeSpan(random.Next(0, 24), random.Next(0, 60), random.Next(0, 60));
            return time;
        }

        static private string GenerateRandomDate(Random random)
        {
            string date = "2015-" + random.Next(1, 12).ToString() + "-" + random.Next(1, 30).ToString() + " ";
            return date;
        }

        static void Main(string[] args)
        {
            List<string> listName = NameOfList();
            List<string> browserNameList = GetBrowserNameList();

            Random random = new Random();

            var csv = new StringBuilder();
            for (int k = 0; k < 100; k++)
            {
                int i = 0;
                for (i = 0; i < 100; i++)
                {
                    var name = listName[random.Next(0, 637)];
                    var browserName = browserNameList[random.Next(0, 3)];
                    var time = GenerateRandomTime(random).ToString();
                    var date = GenerateRandomDate(random).ToString();
                    var accountNumber = random.Next(100000, 999999);
                    var ip = GenerateIPAddress(random);

                    var newLine = string.Format("{0}, {1}, {2}, {3}, {4}{5}", browserName, ip, accountNumber, name, date + time, Environment.NewLine);
                    csv.Append(newLine);
                }

                File.AppendAllText("Mapping Test File.CSV", csv.ToString());
                Console.WriteLine(((k+1)*i).ToString() + " Logs Were Added To File");
                csv.Clear();
            }

            Console.WriteLine("**** The CSV File Was Compeletly Created ****");
            Console.Read();
        }
    }
}
