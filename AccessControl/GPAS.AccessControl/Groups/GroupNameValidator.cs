using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GPAS.AccessControl.Groups
{
    public class GroupNameValidator
    {
        static int maxLength = 256;
        static string pattern = "^[^0-9 \\-\\+*=/\\&!|?:;,'(){}<>\\[\\]\\^~" + Regex.Escape("\\\"") +
            "][^ \\-\\+*=/\\&!|?:;,'(){}<>\\[\\]\\^~" + Regex.Escape("\\\"") + "]*$";

        public static bool IsGroupNameValid(string groupName)
        {
            return
                !string.IsNullOrWhiteSpace(groupName) &&
                groupName.Length <= maxLength &&
                Regex.IsMatch(groupName, pattern);
        }
    }
}
