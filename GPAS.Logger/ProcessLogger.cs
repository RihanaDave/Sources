using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace GPAS.Logger
{
    public class ProcessLogger
    {
        public ProcessLogger()
        {
        }

        StreamWriter logStream = null;

        static object lockObject = new object();

        public virtual void Initialization(string logFilePath)
        {
            lock (lockObject)
            {
                string logFileFinalPath = "";

                if (IsInitialized())
                    return;
                bool fileCreated = false;
                FileStream fs = null;
                try
                {
                    if (!File.Exists(logFilePath))
                    {
                        fs = File.Create(logFilePath);
                        fs.Close();
                        fileCreated = true;
                        logFileFinalPath = logFilePath;
                    }
                }
                catch (IOException)
                { }

                if (!fileCreated)
                {
                    int suffix = 1;
                    IOException lastIOException = null;
                    do
                    {
                        string suffixedPath = string.Format("{0}_{1}", logFilePath, (++suffix).ToString());
                        try
                        {
                            if (!File.Exists(suffixedPath))
                            {
                                fs = File.Create(suffixedPath);
                                fs.Close();
                                fileCreated = true;
                                logFileFinalPath = suffixedPath;
                            }
                        }
                        catch (IOException ex)
                        {
                            lastIOException = ex;
                        }
                    } while (!fileCreated && suffix <= 10);
                    if (!fileCreated)
                        throw lastIOException;
                }

                logStream = new StreamWriter(logFileFinalPath);
            }
        }
        public virtual void Initialization(MemoryStream memStream)
        {
            lock (lockObject)
            {
                logStream = new StreamWriter(memStream);
            }
        }

        public void WriteLog(Exception ex)
        {
            ExceptionDetailGenerator detailGen = new ExceptionDetailGenerator();
            WriteLog(detailGen.GetDetails(ex));
        }

        public bool IsInitialized()
        {
            return logStream != null;
        }

        public virtual void WriteLog(string message, bool storeInusePrivateMemorySize = false)
        {
            if (!IsInitialized())
                return;
            lock (lockObject)
            {
                string logMessage = string.Format("{0}\t{1}", DateTime.Now.ToString(CultureInfo.InvariantCulture), message);
                if (storeInusePrivateMemorySize)
                {
                    logMessage = string.Format("Private Mem.: {0} MB | {1}", (Process.GetCurrentProcess().PrivateMemorySize64 / 1048576).ToString(), logMessage);
                }
                logStream.WriteLine(logMessage);
                logStream.Flush();
                Console.WriteLine(logMessage);
            }
        }

        public void Finalization()
        {
            if (IsInitialized())
                return;
            lock (lockObject)
            {
                logStream.Close();
                logStream = null;
            }
        }
    }
}
