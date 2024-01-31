using System;
using System.Collections.Generic;
using System.Text;

namespace GPAS.Utility
{
    public class StringUtility
    {
        public string SeperateIDsByComma(IEnumerable<long> IDs)
        {
            StringBuilder resultBuilder = new StringBuilder();
            foreach (long id in IDs)
            {
                resultBuilder.AppendFormat("{0},", id.ToString());
            }
            if (resultBuilder.Length != 0)
            {
                resultBuilder.Remove(resultBuilder.Length - 1, 1);
            }
            return resultBuilder.ToString();
        }

        public string SeperateByComma(IEnumerable<string> stringList)
        {
            StringBuilder resultBuilder = new StringBuilder();
            foreach (string item in stringList)
            {
                resultBuilder.AppendFormat("{0},", item);
            }
            if (resultBuilder.Length != 0)
            {
                resultBuilder.Remove(resultBuilder.Length - 1, 1);
            }
            return resultBuilder.ToString();
        }

        public string SeperateByInputSeperator(IEnumerable<string> stringList, string seperator)
        {
            StringBuilder resultBuilder = new StringBuilder();
            foreach (string item in stringList)
            {
                resultBuilder.AppendFormat("{0}{1}", item.ToString(), seperator);
            }
            if (resultBuilder.Length != 0)
            {
                resultBuilder.Remove(resultBuilder.Length - seperator.Length, seperator.Length);
            }
            return resultBuilder.ToString();
        }

        private static string ConvertDateTimeToPersianCanlenderDate(DateTime dateTime, bool includeTime = true)
        {
            System.Globalization.PersianCalendar pc = new System.Globalization.PersianCalendar();
            DateTime dt = dateTime;
            string persianDate
                = pc.GetYear(dt).ToString() + "/" + pc.GetMonth(dt).ToString() + "/" + pc.GetDayOfMonth(dt).ToString();
            if (includeTime)
            {
                persianDate = persianDate + " " +
                  pc.GetHour(dateTime) + ":" + pc.GetMinute(dateTime) + ":" + pc.GetSecond(dateTime);
            }
            return persianDate;
        }
    }
}
