using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Utility
{
    public class PersianCalander
    {
        public static string ConvertDateTimeToPersianCanlenderDate(DateTime dateTime, bool withTime = true)
        {
            System.Globalization.PersianCalendar pc = new System.Globalization.PersianCalendar();
            DateTime dt = dateTime;
            string persianDate = "";
            persianDate = pc.GetYear(dt).ToString() + "/" + pc.GetMonth(dt).ToString() + "/" + pc.GetDayOfMonth(dt).ToString();
            if (withTime)
            {
                persianDate = persianDate + " " +
                  pc.GetHour(dateTime) + ":" + pc.GetMinute(dateTime) + ":" + pc.GetSecond(dateTime);
            }

            return persianDate;
        }
    }
}
