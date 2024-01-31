using GPAS.Dispatch.DataAccess;
using GPAS.Dispatch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Logic
{
    public class InvestigationManagement
    {
        string CallerUserName = string.Empty;
        public InvestigationManagement(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        public InvestigationManagement()
        {
        }

        public void Init()
        {
            InvestigationDatabaseAccess databaseAccess = new InvestigationDatabaseAccess();
            databaseAccess.CreateDataBase();
            databaseAccess.CreateTable();
        }

        public void SaveInvestigation(KInvestigation kInvestigation)
        {
            //if (kInvestigation.CreatedBy == null)
            //    throw new ArgumentException("CreatedBy");
            if (kInvestigation.Description == null)
                throw new ArgumentNullException("Description");
            if (kInvestigation.Id < 1)
                throw new ArgumentNullException("Id");
            if (kInvestigation.Title == null)
                throw new ArgumentNullException("Title");
            if (kInvestigation.InvestigationStatus == null)
                throw new ArgumentNullException("InvestigationStatus");

            InvestigationDatabaseAccess databaseAccess = new InvestigationDatabaseAccess();
            databaseAccess.SaveInvestigation(kInvestigation, CallerUserName);
        }

        public byte[] GetSavedInvestigationStatus(long id)
        {
            if (id < 1)
                throw new ArgumentNullException("Id");
            InvestigationDatabaseAccess databaseAccess = new InvestigationDatabaseAccess();
            return databaseAccess.GetSavedInvestigationStatus(id, CallerUserName);
        }

        public byte[] GetSavedInvestigationImage(long id)
        {
            if (id < 1)
                throw new ArgumentNullException("Id");
            InvestigationDatabaseAccess databaseAccess = new InvestigationDatabaseAccess();
            return databaseAccess.GetSavedInvestigationImage(id, CallerUserName);
        }

        public List<InvestigationInfo> GetSavedInvestigations()
        {
            InvestigationDatabaseAccess databaseAccess = new InvestigationDatabaseAccess();
            return databaseAccess.GetSavedInvestigations(CallerUserName);
        }

        public void TruncateInvestigationTable(string tableName)
        {
            InvestigationDatabaseAccess databaseAccess = new InvestigationDatabaseAccess();
            databaseAccess.TruncateInvestigationTable(tableName);
        }
    }
}
