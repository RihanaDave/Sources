using GPAS.Horizon.Logic;
using GPAS.Horizon.Logic.Synchronization;
using GPAS.Logger;
using System;
using System.Configuration;
using System.Threading;
using System.Web.Configuration;

namespace GPAS.Horizon.ServerManager
{
    class Program
    {
        static string currentConfigPath = "";

        static void Main(string[] args)
        {
            if (!PreventMultipleInstanceRunning())
            {
                return;
            }
            ConsoleInput input = CheckArguments(args);
            if (input == null)
            {
                PromptHelp();
                return;
            }

            LoadConfigFileFromWebService();

            Initialization();
            DoOperation(input);
        }
        private static string appGuid = "ServerManager969476a0 - 08ce - 4d38 - a3c1 - d125b41f60ce";
        private static Mutex mutex;
        /// <summary></summary>
        /// <returns>If no other instance running returns 'True' otherwise returns 'False'</returns>
        private static bool PreventMultipleInstanceRunning()
        {
            string proccessName = string.Format("Global\\{0}", appGuid);
            mutex = new Mutex(false, proccessName);
            if (!mutex.WaitOne(0, false))
            {
                Console.WriteLine("An instance of Server Manager is already running");
                //Current.Shutdown();
                return false;
            }
            return true;
        }

        private static void PromptHelp()
        {
            Console.WriteLine("Horizon Server Manager console");
            Console.WriteLine();
            Console.WriteLine("-R | --Reset [-ConcurrencyLevel=n] [-BatchSize=n]");
            Console.WriteLine();
            Console.WriteLine("Provides Horizon Server data reset (reindex entire or part of objects' data)");
            Console.WriteLine();
            Console.WriteLine("\t-ConcurrencyLevel");
            Console.WriteLine("\t\tIndicates concorrency level of retrieve and sync.");
            Console.WriteLine("\t\tDefault: {read from Horizon Service configuration}");
            Console.WriteLine();
            Console.WriteLine("\t-BatchSize");
            Console.WriteLine("\t\tObjects retrieve and sync. Batch size");
            Console.WriteLine("\t\tDefault: {read from Horizon Service configuration}");
            Console.WriteLine();
            Console.WriteLine("-S | --Synchronize");
            Console.WriteLine();
            Console.WriteLine("Try synchronize Horizon Server unsync. indexes");
            Console.WriteLine();
            Console.WriteLine("/? | -h | --Help");
            Console.WriteLine();
            Console.WriteLine("Show this (help) screen");
            Console.WriteLine();
        }

        private static void DoOperation(ConsoleInput input)
        {
            try
            {
                if (input is ResetArguments)
                {
                    ResetArguments resetArguments = input as ResetArguments;

                    var indexProvider = new IndexingProvider();
                    if (resetArguments.IsMaximumConcurrentSynchronizationsSet)
                    {
                        indexProvider.ResetMaximumConcurrentSynchronizations = resetArguments.MaximumConcurrentSynchronizations;
                    }
                    if (resetArguments.IsNumberOfObjectsForGetSequentialSet)
                    {
                        indexProvider.ResetNumberOfConceptsToGetSequential = resetArguments.NumberOfObjectsForGetSequential;
                    }
                    indexProvider.ApplyOntologyLastChanges = true;
                    indexProvider.DeleteExistingIndexes = IsUserAcceptDeletingExistIndexes();
                    indexProvider.ResetAllIndexes().Wait();
                }
                else if (input is SynchronizeArguments)
                {
                    IndexingProvider indexProvider = new IndexingProvider();
                    indexProvider.SynchronizeNotSyncIndexes().Wait();
                }
                else
                {
                    throw new NotSupportedException("Invalid input");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                throw;
            }
        }

        private static bool IsUserAcceptDeletingExistIndexes()
        {
            Console.WriteLine();
            while (true)
            {
                Console.Write("YOU ARE ABOUT TO DELETE ALL EXISTING INDEXES BEFORE RESET START; ARE YOU SURE (y/n)?");
                char readValue = Console.ReadKey().KeyChar;
                Console.WriteLine();
                switch (char.ToLower(readValue))
                {
                    case 'y':
                        return true;
                    case 'n':
                        return false;
                    default:
                        break;
                }
            }
        }

        private static ConsoleInput CheckArguments(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("-h") || args[0].Equals("-H") || args[0].Equals("-?") || args[0].Equals("/?") || args[0].Equals("--Help"))
            {
                return null;
            }
            else if (args[0].Equals("-S") || args[0].Equals("--Synchronize"))
            {
                SynchronizeArguments result = new SynchronizeArguments();
                return result;
            }
            else if (args[0].Equals("-R") || args[0].Equals("--Reset"))
            {
                ResetArguments result = new ResetArguments();
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg.ToLower().StartsWith("-concurrencylevel="))
                    {
                        result.IsMaximumConcurrentSynchronizationsSet = true;
                        result.MaximumConcurrentSynchronizations = byte.Parse(arg.Substring(arg.IndexOf('=') + 1));
                    }
                    else if (arg.ToLower().StartsWith("-batchsize="))
                    {
                        result.IsNumberOfObjectsForGetSequentialSet = true;
                        result.NumberOfObjectsForGetSequential = int.Parse(arg.Substring(arg.IndexOf('=') + 1));
                    }
                    else
                    {
                        throw new ArgumentException("Invalid argument", arg);
                    }
                }
                return result;
            }
            throw new NotSupportedException("Invalid argument(s)");
        }

        private static void Initialization()
        {
            ExceptionHandler.Init();
            OntologyProvider.Init();
        }

        private static void LoadConfigFileFromWebService()
        {
            currentConfigPath = string.Format(".\\{0}.config", AppDomain.CurrentDomain.FriendlyName);

            Configuration serviceConfig = WebConfigurationManager.OpenWebConfiguration("\\", "Horizon Service");
            serviceConfig.SaveAs(currentConfigPath, ConfigurationSaveMode.Modified);
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (ConfigurationSectionGroup sectionGroup in appConfig.SectionGroups)
            {
                RefreshSections(sectionGroup.Sections);
            }
            RefreshSections(appConfig.Sections);
        }

        private static void RefreshSections(ConfigurationSectionCollection sections)
        {
            foreach (ConfigurationSection section in sections)
            {
                ConfigurationManager.RefreshSection(section.SectionInformation.Name);
            }
        }
    }
}