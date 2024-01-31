using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class FirstObjectToSecondObjectConverter : IValueConverter
    {
        public object TrueValue { get; set; }
        public object FalseValue { get; set; } = "value"; 
        public object Condition { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#pragma warning disable CS0252 // Possible unintended reference comparison; to get a value comparison, cast the left hand side to type 'string'
            if (FalseValue == "value")
#pragma warning restore CS0252 // Possible unintended reference comparison; to get a value comparison, cast the left hand side to type 'string'
                FalseValue = value;

            if (value == null && Condition == null)
                return TrueValue;

            if (value == null)
                return FalseValue;

            if (value.Equals(Condition))
                return TrueValue;
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
