using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GPAS.Ontology;
using GPAS.Workspace.Entities.KWLinks;
using GPAS.Workspace.Logic;

namespace GPAS.Workspace.Presentation.Controls.Link.Converters
{
    /// <summary>
    /// این مبدل نمایش جهت‌های لینک را طبق روابط ممکن بین دو
    /// شی مبدا و مقصد تعیین می‌کند
    /// </summary>
    public class PropertiesToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] == null || values[2] == null || string.IsNullOrEmpty((string)values[0]) ||
                values[0].ToString().Equals(Properties.Resources.Not_Initialized) ||
                values[0].ToString().Equals(Properties.Resources.Select_A_Type))
                return Visibility.Visible;

            // جهت های ممکن برای رابطه (وابستگی/رخداد) بین شی مبدا و مقصد از هستان شناسی
            // استخراج و جهت های ممکن برای ایجاد بین دو شی براساس آن تعیین می گردد
            Direction possibleDirection = OntologyProvider.GetOntology().GetLinkDirection(
                (string)values[0], values[1].ToString(), values[2].ToString(), false);

            bool isSourceToTargetPossible = false,
                isTargetToSourcePossible = false,
                isBidirectionalPossible = false;

            switch (possibleDirection)
            {
                case Direction.Neutral:
                    {
                        isSourceToTargetPossible = isTargetToSourcePossible = isBidirectionalPossible = true;
                        break;
                    }
                case Direction.DomainToRange:
                    {
                        isSourceToTargetPossible = true;
                        break;
                    }
                case Direction.RangeToDomain:
                    {
                        isTargetToSourcePossible = true;
                        break;
                    }
                case Direction.Bidirectionl:
                    {
                        isSourceToTargetPossible = isTargetToSourcePossible = isBidirectionalPossible = true;
                        break;
                    }
                case Direction.DomainToRangeOrRangeToDomain:
                    {
                        isSourceToTargetPossible = isTargetToSourcePossible = true;
                        break;
                    }
            }

            switch ((LinkDirection)values[3])
            {
                case LinkDirection.SourceToTarget:
                    return isSourceToTargetPossible ? Visibility.Visible : Visibility.Collapsed;
                case LinkDirection.TargetToSource:
                    return isTargetToSourcePossible ? Visibility.Visible : Visibility.Collapsed;
                case LinkDirection.Bidirectional:
                    return isBidirectionalPossible ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
