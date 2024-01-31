using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class ContextMenuItemVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //values[0] is selected data sourseces
            //values[1] is selected data sourseces count

            if (!(values[0] is IList) && !(values[1] is int) || parameter == null)
            {
                return Visibility.Collapsed;
            }

            switch ((DataSourceContextMenuItem)parameter)
            {
                case DataSourceContextMenuItem.Mapping:
                case DataSourceContextMenuItem.GlobalResolution:
                case DataSourceContextMenuItem.Permission:
                    return ((IList)values[0]).Count > 1 ? Visibility.Collapsed : Visibility.Visible;

                case DataSourceContextMenuItem.CsvSeparator:

                    if ((int)values[1] == 1 && values[0] is IList list && list.Count > 0 && list[0] is CsvDataSourceModel)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }

                case DataSourceContextMenuItem.CopyMapping:
                case DataSourceContextMenuItem.CopyGlobalResolution:
                case DataSourceContextMenuItem.CopyPermission:
                    return (int)values[1] > 1 ? Visibility.Collapsed : Visibility.Visible;

                case DataSourceContextMenuItem.ManegeGroup:
                    if ((int)values[1] == 1)
                    {
                        if (values[0] is IList iList && iList.Count > 0 && iList[0] is GroupDataSourceModel)
                            return Visibility.Visible;
                        else
                            return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }

                case DataSourceContextMenuItem.AddToGroup:
                    if ((int)values[1] <= 1)
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return ((IList)values[0]).Cast<object>().Any(dataSource => !(dataSource is EmlDataSourceModel))
                            ? Visibility.Collapsed
                            : Visibility.Visible;
                    }
                case DataSourceContextMenuItem.ShowOnGraph:
                    return CkeckShowOnGraphItemVisibility((IList)values[0]);
            }

            return Visibility.Collapsed;
        }

        private Visibility CkeckShowOnGraphItemVisibility(IList datasourceList)
        {
            if (datasourceList == null || datasourceList.Count == 0)
                return Visibility.Collapsed;

            bool allIsCompleted = true;

            foreach (IDataSource item in datasourceList)
            {
                if (item.ImportStatus != DataSourceImportStatus.Completed)
                {
                    allIsCompleted = false;
                    break;
                }
            }

            return allIsCompleted ? Visibility.Visible : Visibility.Collapsed;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
