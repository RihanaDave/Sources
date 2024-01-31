using GPAS.Ontology;
using System;
using System.Collections.Generic;
using System.IO;

namespace GPAS.Workspace.DynamicOntology
{
    public class DynamicOntology
    {
        /// <summary>
        /// این تابع یک مفهوم جدید به همراه پدر و لیستی از ویژگی‌ها آن را دریافت کرده و این شیء را به هستان‌شناسی مورد نظر اضافه می نماید.
        /// </summary>
        /// <param name="newConceptName"></param>
        /// <param name="parentName"></param>
        /// <param name="propertyList"></param>
        /// <param name="ontologyFilePath"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public string InsertNewConceptIntoTheOntology(string newConceptName, string parentName, List<Ontology.DataType> propertyList, string ontologyFilePath, string baseUri, int count)
        {
            List<string> propertyListCopy = new List<string>();
            foreach (var item in propertyList)
            {
                propertyListCopy.Add(item.TypeName);
            }
            FileStream file = new FileStream(ontologyFilePath, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file);
            string exten = "";
            string extenDes = "";

            if ((count - 1) % 3 == 0 && (count != 1))
            {
                exten = "3";
                extenDes = ((count - 1) % 3 + 1).ToString();
            }
            else
            {
                if (count != 1)
                {
                    exten = ((count - 1) % 3).ToString();
                    extenDes = ((count - 1) % 3 + 1).ToString();
                }

            }
            string tempPath = "";
            if (count == 1)
            {
                tempPath = ontologyFilePath.ToLower().Replace("4.rdf", "1.rdf");
            }
            else
            {
                tempPath = ontologyFilePath.ToLower().Replace(exten + ".rdf", extenDes + ".rdf");
            }

            FileStream tempFile = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer = new StreamWriter(tempFile);

            using (file)
            {
                while (reader.Peek() != -1 && propertyListCopy.Count != 0)
                {
                    string currentLine = reader.ReadLine();
                    if (currentLine.Contains("owl:DatatypeProperty rdf:about="))
                    {

                        string propertyName = currentLine.Split('#', '\"')[2];
                        if (propertyListCopy.Contains(propertyName))
                        {
                            writer.WriteLine(currentLine);
                            writer.WriteLine("<rdfs:domain rdf:resource=\"" + baseUri + "#" + newConceptName + "\"/>");
                            propertyListCopy.Remove(propertyName);
                            continue;
                        }
                    }
                    writer.WriteLine(currentLine);
                }
                while (reader.Peek() != -1)
                {
                    string currentLine = reader.ReadLine();
                    if (currentLine.Contains("</rdf:RDF>"))
                    {
                        writer.WriteLine("<!-- " + baseUri + "#" + newConceptName + "-->");
                        writer.WriteLine("<owl:Class rdf:about=\"" + baseUri + "#" + newConceptName + "\">");
                        writer.WriteLine("<rdfs:subClassOf rdf:resource=\"" + baseUri + "#" + parentName + "\"/>");
                        writer.WriteLine("</owl:Class>");
                        writer.WriteLine("</rdf:RDF>");
                        break;
                    }
                    writer.WriteLine(currentLine);
                }
            }
            writer.Close();
            reader.Close();
            file.Dispose();
            tempFile.Dispose();


            string metadataFilePath = Path.GetDirectoryName(ontologyFilePath);
            metadataFilePath += "\\Ontology Metadata.csv";

            using (StreamWriter streamWriter = File.AppendText(metadataFilePath))
            {
                streamWriter.WriteLine(newConceptName + ", false, true");
            }

            return (tempPath);
        }
        /// <summary>
        /// این تابع نام و نوع یک ویژگی جدید را دریافت کرده و آن ویژگی را به هستان‌شناسی که آدرس آن به تابع ارسال شده، اضافه می‌نماید
        /// در نهایت این تابع یک هستان‌شناسی جدید ایجاد کرده و آدرس فایل آن را به عنوان خروجی ارسال می‌کند
        /// </summary>
        /// <param name="newPropertyName"></param>
        /// <param name="newPropertyType"></param>
        /// <param name="ontologyFilePath"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public string InsertNewPropertyIntoTheOntology(string newPropertyName, string newPropertyType, string ontologyFilePath, string baseUri, int count)
        {
            try
            {
                FileStream file = new FileStream(ontologyFilePath, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file);
                string exten = "";
                string extenDes = "";

                if ((count - 1) % 3 == 0 && (count != 1))
                {
                    exten = "3";
                    extenDes = ((count - 1) % 3 + 1).ToString();
                }
                else
                {
                    if (count != 1)
                    {
                        exten = ((count - 1) % 3).ToString();
                        extenDes = ((count - 1) % 3 + 1).ToString();
                    }

                }
                string tempPath = "";
                if (count == 1)
                {
                    tempPath = ontologyFilePath.ToLower().Replace("4.rdf", "1.rdf");
                }
                else
                {
                    tempPath = ontologyFilePath.ToLower().Replace(exten + ".rdf", extenDes + ".rdf");
                }
                FileStream tempFile = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(tempFile);

                using (file)
                {
                    while (reader.Peek() != -1)
                    {
                        string currentLine = reader.ReadLine();
                        if (currentLine.Contains("owl:DatatypeProperty rdf:about="))
                        {
                            writer.WriteLine("<!-- New Data Type Property -->");
                            writer.WriteLine("<owl:DatatypeProperty rdf:about=\"" + baseUri + "#" + newPropertyName + "\">");
                            writer.WriteLine("<rdfs:range rdf:resource=\"http://www.w3.org/2001/XMLSchema#" + newPropertyType + "\"/>");
                            writer.WriteLine("</owl:DatatypeProperty>");
                            writer.WriteLine("<!-- New Data Type Property -->");
                            writer.WriteLine(currentLine);
                            break;
                        }
                        writer.WriteLine(currentLine);
                    }
                    while (reader.Peek() != -1)
                    {
                        string currentLine = reader.ReadLine();
                        if (currentLine.Contains("</rdf:RDF>"))
                        {
                            writer.WriteLine("</rdf:RDF>");
                            break;
                        }
                        writer.WriteLine(currentLine);
                    }
                }
                writer.Close();
                reader.Close();
                file.Dispose();
                tempFile.Dispose();
                return (tempPath);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
        /// <summary>
        /// این تابع با دریافت نام یک ویژگی و یک مفهوم از هستان‌شناسی، ویژگی مذکور را به مفهوم مورد نظر، انتساب می‌دهد
        /// در نهایت یک هستان‌شناسی جدید با اعمال این تغییرات ایجاد و به عنوان خروجی ارسال می‌شود
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="newDomainName"></param>
        /// <param name="ontologyFilePath"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public string AddNewDomainToProperty(string propertyName, string newDomainName, string ontologyFilePath, string baseUri, int count)
        {
            try
            {
                FileStream file = new FileStream(ontologyFilePath, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file);
                string exten = "";
                string extenDes = "";

                if ((count - 1) % 3 == 0 && (count != 1))
                {
                    exten = "3";
                    extenDes = ((count - 1) % 3 + 1).ToString();
                }
                else
                {
                    if (count != 1)
                    {
                        exten = ((count - 1) % 3).ToString();
                        extenDes = ((count - 1) % 3 + 1).ToString();
                    }

                }
                string tempPath = "";
                if (count == 1)
                {
                    tempPath = ontologyFilePath.ToLower().Replace("4.rdf", "1.rdf");
                }
                else
                {
                    tempPath = ontologyFilePath.ToLower().Replace(exten + ".rdf", extenDes + ".rdf");
                }
                FileStream tempFile = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(tempFile);
                writer.Flush();

                using (file)
                {
                    while (reader.Peek() != -1)
                    {
                        string currentLine = reader.ReadLine();
                        if (currentLine.Contains("owl:DatatypeProperty rdf:about=") && currentLine.Contains("#" + propertyName + "\""))
                        {
                            writer.WriteLine(currentLine);
                            writer.WriteLine("<rdfs:domain rdf:resource=\"" + baseUri + "#" + newDomainName + "\"/>");
                            continue;
                        }
                        if (currentLine.Contains("</rdf:RDF>"))
                        {
                            writer.WriteLine("</rdf:RDF>");
                            break;
                        }

                        writer.WriteLine(currentLine);
                    }
                }
                writer.Close();
                reader.Close();
                file.Dispose();
                tempFile.Dispose();
                return (tempPath);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }


        /// <summary>
        /// این تابع با دریافت نام و نوع یک ویژگی، این ویژگی را به شیء مورد نظر که به عنوان وردوی دریافت می‌کند، انتساب خواهد داد.
        /// در نهایت فایل هستان‌شناسی جدید که تغییرات لازم در آن اعمال شده ذخیره گردیده و مسیر آن به عنوان خروجی ارسال می‌گردد
        /// </summary>
        /// <param name="newPropertyName"></param>
        /// <param name="newPropertyType"></param>
        /// <param name="objectType"></param>
        /// <param name="ontologyFilePath"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public string AssignNewPropertyToTheObject(string newPropertyName, string newPropertyType, string objectType, string ontologyFilePath, string baseUri, int count)
        {
            try
            {
                FileStream file = new FileStream(ontologyFilePath, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file);
                string exten = "";
                string extenDes = "";

                if ((count - 1) % 3 == 0 && (count != 1))
                {
                    exten = "3";
                    extenDes = ((count - 1) % 3 + 1).ToString();
                }
                else
                {
                    if (count != 1)
                    {
                        exten = ((count - 1) % 3).ToString();
                        extenDes = ((count - 1) % 3 + 1).ToString();
                    }

                }
                string tempPath = "";
                if (count == 1)
                {
                    tempPath = ontologyFilePath.ToLower().Replace("4.rdf", "1.rdf");
                }
                else
                {
                    tempPath = ontologyFilePath.ToLower().Replace(exten + ".rdf", extenDes + ".rdf");
                }
                FileStream tempFile = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(tempFile);

                using (file)
                {
                    while (reader.Peek() != -1)
                    {
                        string currentLine = reader.ReadLine();
                        if (currentLine.Contains("owl:DatatypeProperty rdf:about="))
                        {
                            writer.WriteLine("<!-- New Data Type Property -->");
                            writer.WriteLine("<owl:DatatypeProperty rdf:about=\"" + baseUri + "#" + newPropertyName + "\">");
                            writer.WriteLine("<rdfs:domain rdf:resource=\"" + baseUri + "#" + objectType + "\"/>");
                            writer.WriteLine("<rdfs:range rdf:resource=\"http://www.w3.org/2001/XMLSchema#" + newPropertyType + "\"/>");
                            writer.WriteLine("</owl:DatatypeProperty>");
                            writer.WriteLine("<!-- New Data Type Property -->");
                            writer.WriteLine(currentLine);
                            break;
                        }
                        writer.WriteLine(currentLine);
                    }

                    while (reader.Peek() != -1)
                    {
                        string currentLine = reader.ReadLine();
                        if (currentLine.Contains("</rdf:RDF>"))
                        {
                            writer.WriteLine("</rdf:RDF>");
                            break;
                        }
                        writer.WriteLine(currentLine);
                    }
                }
                writer.Close();
                reader.Close();
                file.Dispose();
                tempFile.Dispose();
                return (tempPath);
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }


    }
}
