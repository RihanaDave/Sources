using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Presentation.Windows.Investigation.Enums;
using GPAS.Workspace.Presentation.Windows.Investigation.EventArguments;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Windows.Investigation.Models
{
    public class InvestigationModel : BaseModel
    {
        public InvestigationModel()
        {
            SelectItemCommand = new RelayCommand(SelectItem);
        }
        #region Properties

        private long iDentifier;
        public long IDentifier
        {
            get => iDentifier;
            set
            {
                SetValue(ref iDentifier, value);
            }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                SetValue(ref isSelected, value);
            }
        }

        private string title;
        public string Title
        {
            get => title;
            set
            {
                SetValue(ref title, value);
            }
        }

        private string groupCategory;
        public string GroupCategory
        {
            get => groupCategory;
            set
            {
                SetValue(ref groupCategory, value);
            }
        }

        private string description;
        public string Description
        {
            get => description;
            set
            {
                SetValue(ref description, value);
            }
        }

        private string createdBy;
        public string CreatedBy
        {
            get => createdBy;
            set
            {
                SetValue(ref createdBy, value);
            }
        }

        private DateTime createdTime;
        public DateTime CreatedTime
        {
            get => createdTime;
            set
            {
                SetValue(ref createdTime, value);
                SetGroup();
            }
        }

        private BitmapImage graphImage;
        public BitmapImage GraphImage
        {
            get => graphImage;
            set
            {
                SetValue(ref graphImage, value);
            }
        }

        private InvestigationStatus status;
        public InvestigationStatus Status
        {
            get => status;
            set
            {
                SetValue(ref status, value);
            }
        }

        private string groupName;
        public string GroupName
        {
            get => groupName;
            set
            {
                SetValue(ref groupName, value);
            }
        }

        #endregion

        #region Functions

        private DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        private DateTime StartOfMonth(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        private void SetGroup()
        {
            DateTime todayTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            DateTime yesterdayTime = todayTime.AddDays(-1);
            DateTime thisWeek = StartOfWeek(todayTime, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            DateTime lastWeekTime = thisWeek.AddDays(-7);
            DateTime ThisMonth = StartOfMonth(todayTime);
            DateTime LastMonth = ThisMonth.AddMonths(-1);
            DateTime SixMonthAgo = ThisMonth.AddMonths(-5);
            DateTime ThisYear = new DateTime(todayTime.Year, 1, 1);
            DateTime LastYear = ThisYear.AddYears(-1);

            if (CreatedTime >= todayTime && CreatedTime < todayTime.AddDays(1))
            {
                GroupName = "Today";
            }
            else if (CreatedTime >= yesterdayTime && CreatedTime < yesterdayTime.AddDays(1))
            {
                GroupName = "Yesterday";
            }
            else if (CreatedTime >= thisWeek && CreatedTime < todayTime.AddDays(1))
            {
                GroupName = "This Week";

            }
            else if (CreatedTime >= lastWeekTime && CreatedTime < todayTime.AddDays(1))
            {
                GroupName = "Last Week";

            }
            else if (CreatedTime >= ThisMonth && CreatedTime < todayTime.AddDays(1))
            {
                GroupName = "This Month";

            }
            else if (CreatedTime >= LastMonth && CreatedTime < todayTime.AddDays(1))
            {
                GroupName = "Last Month";

            }
            else if (CreatedTime >= SixMonthAgo && CreatedTime < todayTime.AddDays(1))
            {
                GroupName = "Six Month Ago";

            }
            else if (CreatedTime >= ThisYear && CreatedTime < todayTime.AddDays(1))
            {
                GroupName = "This Year";

            }
            else if (CreatedTime >= LastYear && CreatedTime < todayTime.AddDays(1))
            {
                GroupName = "Last Year";

            }
            else
            {
                GroupName = "Other";
            }
        }
        private void SelectItem(object parameter)
        {
            IsSelected = true;
        }
        #endregion

        #region Command
        public RelayCommand SelectItemCommand { get; set; }

        public static explicit operator InvestigationModel(LoadInvestigationEventArgs v)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
