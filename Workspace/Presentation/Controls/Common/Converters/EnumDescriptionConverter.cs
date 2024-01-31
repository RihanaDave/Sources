using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            DescriptionAttribute descriptionAttribute = fieldInfo.GetCustomAttributes<DescriptionAttribute>()?.FirstOrDefault();

            string description = string.Empty;

            if (descriptionAttribute == null)
            {
                description = enumObj.ToString();
            }
            else
            {
                description = descriptionAttribute.Description;
            }

            string DescriptionInResource = Properties.Resources.ResourceManager.GetString(description);
            if (DescriptionInResource != null)
            {
                return DescriptionInResource;
            }
            else
            {
                return description;
            }
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetEnumDescription((Enum)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
