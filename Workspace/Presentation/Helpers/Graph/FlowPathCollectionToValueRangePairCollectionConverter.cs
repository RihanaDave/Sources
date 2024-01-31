using GPAS.BarChartViewer;
using GPAS.Graph.GraphViewer.Foundations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Helpers.Graph
{
    public class FlowPathCollectionToValueRangePairCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<FlowPathVM> flowPathCollection = (ObservableCollection<FlowPathVM>)value;
            ObservableCollection<ValueRangePair> valueRangePairCollection = new ObservableCollection<ValueRangePair>();

            if (flowPathCollection == null || flowPathCollection.Count == 0)
                return valueRangePairCollection;

            int maxBucketCount = 100;
            if (parameter is int)
                maxBucketCount = (int)parameter;

            double min = flowPathCollection.Select(fp => fp.Weight).Min();
            double max = flowPathCollection.Select(fp => fp.Weight).Max();

            if (max - min < 9)
                max = min + 9;

            int step = (int)Math.Ceiling((max - min + 1) / maxBucketCount);

            for (int i = (int)min; i <= (int)max; i += step)
            {
                double start = i;
                double end = i + step;

                valueRangePairCollection.Add(new ValueRangePair()
                {
                    Start = start,
                    End = end,
                    Value = flowPathCollection.Where(fp => fp.Weight >= start && fp.Weight < end).ToList().Count
                });
            }

            return valueRangePairCollection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
