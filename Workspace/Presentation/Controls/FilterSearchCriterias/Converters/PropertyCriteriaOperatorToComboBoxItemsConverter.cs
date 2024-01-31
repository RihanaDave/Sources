using GPAS.FilterSearch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.Converters
{
    public class PropertyCriteriaOperatorToComboBoxItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PropertyCriteriaOperatorValuePair)
            {
                //Generate List of RelationOperator for each data type
                PropertyCriteriaOperatorValuePair OperatorValuePair = value as PropertyCriteriaOperatorValuePair;
                if (OperatorValuePair is FloatPropertyCriteriaOperatorValuePair)
                {
                    return EnumToStringList((OperatorValuePair as FloatPropertyCriteriaOperatorValuePair).CriteriaOperator);
                }
                else if (OperatorValuePair is LongPropertyCriteriaOperatorValuePair)
                {
                    return EnumToStringList((OperatorValuePair as LongPropertyCriteriaOperatorValuePair).CriteriaOperator);
                }
                else if (OperatorValuePair is StringPropertyCriteriaOperatorValuePair)
                {
                    return EnumToStringList((OperatorValuePair as StringPropertyCriteriaOperatorValuePair).CriteriaOperator);
                }
                else if (OperatorValuePair is BooleanPropertyCriteriaOperatorValuePair)
                {
                    return EnumToStringList((OperatorValuePair as BooleanPropertyCriteriaOperatorValuePair).CriteriaOperator);
                }
                else if (OperatorValuePair is DateTimePropertyCriteriaOperatorValuePair)
                {
                    return EnumToStringList((OperatorValuePair as DateTimePropertyCriteriaOperatorValuePair).CriteriaOperator);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private List<string> EnumListTextToEnumListSymbol(string[] listText)
        {
            List<string> listSymbol = new List<string>();

            foreach (var text in listText)
            {
                listSymbol.Add(TextToSymbol(text));
            }

            return listSymbol;
        }

        private string TextToSymbol(string text)
        {
            switch (text)
            {
                case "Equals":
                    return "=";
                case "GreaterThan":
                    return ">";
                case "LessThan":
                    return "<";
                case "GreaterThanOrEquals":
                    return ">=";
                case "LessThanOrEquals":
                    return "<=";
                case "NotEquals":
                    return "!=";
                case "Like":
                    return "Like";
                default:
                    throw new NotImplementedException(text);
            }
        }

        private List<string> EnumToStringList<T>(T relationOperator)
        {
            if (relationOperator is Enum)
            {
                return EnumListTextToEnumListSymbol(Enum.GetNames(relationOperator.GetType()));
            }

            return null;
        }
    }
}
