using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GPAS.TimelineViewer.BinSize;

namespace GPAS.TimelineViewer
{
    /// <summary>
    /// Interaction logic for TimelineViewer.xaml
    /// </summary>
    public partial class TimelineViewer : UserControl
    {
        #region توابع موقت
        private List<DateTime> GenerateTestData()
        {
            List<DateTime> result = new List<DateTime>();

            result.Add(new DateTime(1391, 4, 26, 12, 31, 26));
            result.Add(new DateTime(1391, 5, 26, 8, 31, 26));
            result.Add(new DateTime(1391, 5, 26, 8, 31, 26));
            result.Add(new DateTime(1391, 5, 26, 8, 31, 26));
            result.Add(new DateTime(1392, 5, 26, 20, 31, 26));
            result.Add(new DateTime(1392, 5, 26, 6, 31, 26));
            result.Add(new DateTime(1392, 5, 26, 7, 31, 26));
            result.Add(new DateTime(1392, 5, 26, 20, 31, 26));
            result.Add(new DateTime(1390, 5, 26, 6, 31, 26));
            result.Add(new DateTime(1392, 5, 26, 7, 31, 26));
            result.Add(new DateTime(1392, 5, 26, 20, 31, 26));
            result.Add(new DateTime(1395, 5, 26, 6, 31, 26));
            result.Add(new DateTime(1395, 5, 26, 7, 31, 26));
            result.Add(new DateTime(1397, 1, 26, 22, 31, 26));
            result.Add(new DateTime(1394, 2, 7, 19, 31, 26));
            result.Add(new DateTime(1401, 2, 7, 7, 31, 26));
            result.Add(new DateTime(1407, 2, 7, 6, 31, 26));
            result.Add(new DateTime(1410, 2, 7, 9, 31, 26));
            result.Add(new DateTime(1410, 2, 7, 9, 31, 26));

            // Sorted!
            //result.Add(new DateTime(1390, 5, 26, 6, 31, 26));

            //result.Add(new DateTime(1391, 4, 26, 12, 31, 26));
            //result.Add(new DateTime(1391, 5, 26, 8, 31, 26));
            //result.Add(new DateTime(1391, 5, 26, 8, 31, 26));
            //result.Add(new DateTime(1391, 5, 26, 8, 31, 26));

            //result.Add(new DateTime(1392, 5, 26, 6, 31, 26));
            //result.Add(new DateTime(1392, 5, 26, 7, 31, 26));
            //result.Add(new DateTime(1392, 5, 26, 7, 31, 26));
            //result.Add(new DateTime(1392, 6, 26, 20, 31, 26));
            //result.Add(new DateTime(1392, 7, 26, 20, 31, 26));
            //result.Add(new DateTime(1392, 5, 26, 20, 31, 26));

            //result.Add(new DateTime(1394, 2, 7, 19, 31, 26));

            //result.Add(new DateTime(1395, 5, 26, 6, 31, 26));
            //result.Add(new DateTime(1395, 5, 26, 7, 31, 26));

            //result.Add(new DateTime(1397, 1, 26, 22, 31, 26));

            //result.Add(new DateTime(1401, 2, 7, 7, 31, 26));

            //result.Add(new DateTime(1407, 2, 7, 6, 31, 26));

            //result.Add(new DateTime(1410, 2, 7, 9, 31, 26));
            //result.Add(new DateTime(1410, 2, 7, 9, 31, 26));

            return result;
        }
        private List<DateTime> GenerateTestData2()
        {
            List<DateTime> result = new List<DateTime>();
            DateTime d = new DateTime(2010, 1, 1, 0, 0, 0);
            DateTime end = new DateTime(2018, 12, 30, 0, 0, 0);
            do
            {
                result.Add(d);
                d = d.AddDays(1);
            } while (d <= end);
            return result;
        }
        #endregion

        #region رخدادگردان‌های کنترل‌های نمایشی
        private void BinSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBinSize = BinSizeComboBox.SelectedItem as BinSize.BinSize;
            UpdateTimelineForShowingData(selectedBinSize);
        }

        bool moveWithMouse = false;
        double moveStartOffset;
        private void VisualisationScroller_MouseDown(object sender, MouseButtonEventArgs e)
        {
            moveWithMouse = true;
            moveStartOffset = VisualisationScroller.HorizontalOffset + Mouse.GetPosition(VisualisationScroller).X;
        }
        private void VisualisationScroller_MouseUp(object sender, MouseButtonEventArgs e)
        {
            moveWithMouse = false;
        }
        private void VisualisationScroller_MouseLeave(object sender, MouseEventArgs e)
        {
            moveWithMouse = false;
        }
        private void VisualisationScroller_MouseMove(object sender, MouseEventArgs e)
        {
            if (moveWithMouse)
                VisualisationScroller.ScrollToHorizontalOffset(moveStartOffset - Mouse.GetPosition(VisualisationScroller).X);
        }

        private void VisualisationScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TimeAxisScroller.ScrollToHorizontalOffset(VisualisationScroller.HorizontalOffset);
        }

        private void TimeAxisScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            VisualisationScroller.ScrollToHorizontalOffset(TimeAxisScroller.HorizontalOffset);
        }

        private void VisualisationScroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                var offsetBeforeZoom = VisualisationScroller.HorizontalOffset;
                var scrollableWidthBeforeZoom = VisualisationScroller.ScrollableWidth;
                if (e.Delta > 0 && BinSizeComboBox.SelectedIndex < BinSizeComboBox.Items.Count)
                {
                    BinSizeComboBox.SelectedIndex++;
                }
                else if (e.Delta < 0 && BinSizeComboBox.SelectedIndex > 0)
                {
                    BinSizeComboBox.SelectedIndex--;
                }
                if (scrollableWidthBeforeZoom != 0 && offsetBeforeZoom != 0)
                {
                    VisualisationScroller.UpdateLayout();
                    VisualisationScroller.ScrollToHorizontalOffset(offsetBeforeZoom * VisualisationScroller.ScrollableWidth / scrollableWidthBeforeZoom);
                }
            }
            else
            {
                if (e.Delta > 0)
                    VisualisationScroller.ScrollToHorizontalOffset(VisualisationScroller.HorizontalOffset - (MouseScrollScrollerMoveStepFactor * BinWidth));
                else
                    VisualisationScroller.ScrollToHorizontalOffset(VisualisationScroller.HorizontalOffset + (MouseScrollScrollerMoveStepFactor * BinWidth));
            }
        }

        private void TimeAxisScroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                UpdateTimelineWithOneStepZoomIn();
            else
                UpdateTimelineWithOneStepZoomOut();
        }
        #endregion

        private List<DateTime> showingDates = new List<DateTime>();
        int BinWidth = 20;
        public int MouseScrollScrollerMoveStepFactor = 2;

        #region راه‌اندازی کنترل
        public TimelineViewer()
        {
            InitializeComponent();

            ZeroFreqLabel.Content = "0\n";
            NoItemLabel.Visibility = Visibility.Visible;

            InitBinSizeComboBox();
            CurrentBinSize = null;

            //ShowTimesFrequecny(GenerateTestData());
        }
        private void InitBinSizeComboBox()
        {
            foreach (var item in BinSize.BinSizes.BinSizesList)
            {
                BinSizeComboBox.Items.Add(item);
            }
        }
        #endregion

        #region توابع/رخدادهای عمومی
        public void ShowTimesFrequecny(IEnumerable<DateTime> timesToShow)
        {
            if (timesToShow == null)
                throw new ArgumentNullException("timesToShow");
            if (timesToShow.Any())
            {
                showingDates = timesToShow.ToList();
                UpdateTimelineForShowingData(BinSizes.DefaultBinSize);
                NoItemLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                NoItemLabel.Visibility = Visibility.Visible;
            }
        }
        public void Clear()
        {
            ShowTimesFrequecny(new DateTime[] { });
        }

        public event EventHandler<DateTimesSelectedEventArgs> DateTimesSelected;
        #endregion


        internal BinSize.BinSize CurrentBinSize
        {
            private set;
            get;
        }

        private void UpdateTimelineForShowingData(BinSize.BinSize bin)
        {
            //var previousBin = CurrentBinSize;
            //double previousScrollerOffset = 0, previousScrollerViewPortWidth = 1;
            //if (previousBin != null)
            //{
            //    previousScrollerOffset = VisualisationScroller.HorizontalOffset;
            //    previousScrollerViewPortWidth = VisualizationGrid.ActualWidth;
            //}

            CurrentBinSize = bin;

            //var chartView = new ChartInformation(showingDates, bin);
            //ShowChart(chartView, bin);
            var chartView = new ChartMetadata(showingDates, bin);
            ShowChart2(chartView, bin);

            BinSizeComboBox.SelectedItem = CurrentBinSize;

            //if (previousBin != null)
            //    VisualisationScroller.ScrollToHorizontalOffset
            //        (((previousScrollerOffset + VisualisationScroller.ActualWidth)
            //            * VisualizationGrid.ActualWidth / previousScrollerViewPortWidth)
            //            - VisualisationScroller.ActualWidth);
        }

        private void UpdateTimelineWithOneStepZoomIn()
        {
            int indexOfCurrentBin = BinSizes.BinSizesList.IndexOf(CurrentBinSize);
            if (indexOfCurrentBin != BinSizes.BinSizesList.Count - 1)
                UpdateTimelineForShowingData(BinSizes.BinSizesList[indexOfCurrentBin + 1]);
        }
        private void UpdateTimelineWithOneStepZoomOut()
        {
            int indexOfCurrentBin = BinSizes.BinSizesList.IndexOf(CurrentBinSize);
            if (indexOfCurrentBin != 0)
                UpdateTimelineForShowingData(BinSizes.BinSizesList[indexOfCurrentBin - 1]);
        }

        private void ShowChart2(ChartMetadata chart, BinSize.BinSize bin)
        {
            ShowTimeInterval(chart);
            UpdateVisualizationChart(chart, bin);
            UpdateFreqAxisLabelAndGuidelines(chart);
            UpdateTimeAxisLabels(chart, bin);
            VisualisationScroller.ScrollToHorizontalOffset(VisualisationScroller.ScrollableWidth / 2);
        }

        private void UpdateTimeAxisLabels(ChartMetadata chart, BinSize.BinSize bin)
        {
            ClearGridContentAndDefenitions(TimeAxisLabelsGrid);
            TimeAxisLabelsGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(2) });
            TimeAxisLabelsGrid.RowDefinitions.Add(new RowDefinition());
            var timeAxisHalfColumnWidth = new GridLength(BinWidth / 2);
            int binsPerLabel = chart.Bin.GapBetweenLabels;

            int intervalCounter = 0;
            while (intervalCounter < chart.Intervals.Count
                && AreTimesInSameRange(chart.Intervals[intervalCounter], chart.Intervals[0], bin.TimeAxisLabelsScale))
            {
                f11(timeAxisHalfColumnWidth);
                intervalCounter++;
            }

            int candidatesLabelsCount = 0;
            for (; intervalCounter < chart.Intervals.Count; intervalCounter++)
            {
                f11(timeAxisHalfColumnWidth);

                var currentInterval = chart.Intervals[intervalCounter];

                // X-Axis Labels
                if (intervalCounter >= 4
                    //&& intervalCounter % binsPerLabel == 0
                    //&& rejectedLabelsFromLastShownLabel >= binsPerLabel
                    //&& Utility.TimeIntervals.IsIntervalTimeOnLabelScaleBorder(chart.Intervals[intervalCounter], bin.TimeAxisLabelsScale)
                    && !AreTimesInSameRange(chart.Intervals[intervalCounter], chart.Intervals[intervalCounter - 1], bin.TimeAxisLabelsScale))
                {
                    if (candidatesLabelsCount % (bin.GapBetweenLabels + 1) == 0)
                    {
                        Label l = new Label();
                        l.Content = Utility.TimeIntervals.GetTimeAxisLabelForStartingTime(currentInterval, bin.TimeAxisLabelsScale);
#if DEBUG
                        l.ToolTip = currentInterval.ToString();
                        //l.BorderBrush = Brushes.Aqua;
                        //l.BorderThickness = new Thickness(1, 0, 1, 0);
#endif
                        l.VerticalContentAlignment = VerticalAlignment.Top;
                        l.HorizontalContentAlignment = HorizontalAlignment.Center;
                        Grid.SetRow(l, 1);
                        Grid.SetColumn(l, (intervalCounter - 1 - 1) * 2/* - binsPerLabel*/);
                        Grid.SetColumnSpan(l, /*binsPerLabel * 2*/ 4);
                        TimeAxisLabelsGrid.Children.Add(l);
                    }
                    candidatesLabelsCount++;
                }

                // Time-Axis Guide-Marks
                Border b = new Border();
                b.BorderBrush = Brushes.Black;
                b.BorderThickness = new Thickness(1, 0, 0, 0);
                Grid.SetColumn(b, intervalCounter * 2);
                TimeAxisLabelsGrid.Children.Add(b);

                if (AreTimesInSameRange(currentInterval, chart.Intervals[chart.Intervals.Count - 1], chart.Bin.TimeAxisLabelsScale))
                {
                    while (++intervalCounter < chart.Intervals.Count
                        && AreTimesInSameRange(chart.Intervals[intervalCounter], chart.Intervals[chart.Intervals.Count - 1], bin.TimeAxisLabelsScale))
                    {
                        f11(timeAxisHalfColumnWidth);
                    }
                    break;
                }
            }
        }

        private void f11(GridLength timeAxisHalfColumnWidth)
        {
            // برای اینکه برچسب دقیقا در نقطه‌ی تلاقی دو ستون قرار بگیرد، به‌ازای
            // هر ستون داده دو ستون در گرید پایینی ایجاد می‌شود تا از طریق آن‌ها
            // برچسب‌ها و نشانگرهای بین ستون‌ها نمایش داده شوند
            TimeAxisLabelsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = timeAxisHalfColumnWidth });
            TimeAxisLabelsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = timeAxisHalfColumnWidth });
        }

        private bool AreTimesInSameRange(DateTime checkingTime, DateTime baseTime, BinScaleLevel scaleLevel)
        {
            switch (scaleLevel)
            {
                case BinScaleLevel.Year:
                    return baseTime.Year.Equals(checkingTime.Year);
                case BinScaleLevel.Month:
                    return baseTime.Year.Equals(checkingTime.Year)
                        && baseTime.Month.Equals(checkingTime.Month);
                case BinScaleLevel.Day:
                    return baseTime.Year.Equals(checkingTime.Year)
                        && baseTime.Month.Equals(checkingTime.Month)
                        && baseTime.Day.Equals(checkingTime.Day);
                case BinScaleLevel.Hour:
                    return baseTime.Year.Equals(checkingTime.Year)
                        && baseTime.Month.Equals(checkingTime.Month)
                        && baseTime.Day.Equals(checkingTime.Day)
                        && baseTime.Hour.Equals(checkingTime.Hour);
                case BinScaleLevel.Minute:
                    return baseTime.Year.Equals(checkingTime.Year)
                        && baseTime.Month.Equals(checkingTime.Month)
                        && baseTime.Day.Equals(checkingTime.Day)
                        && baseTime.Hour.Equals(checkingTime.Hour)
                        && baseTime.Minute.Equals(checkingTime.Minute);
                case BinScaleLevel.Second:
                    return baseTime.Year.Equals(checkingTime.Year)
                        && baseTime.Month.Equals(checkingTime.Month)
                        && baseTime.Day.Equals(checkingTime.Day)
                        && baseTime.Hour.Equals(checkingTime.Hour)
                        && baseTime.Minute.Equals(checkingTime.Minute)
                        && baseTime.Second.Equals(checkingTime.Second);
                default:
                    throw new NotSupportedException();
            }
        }

        private void UpdateFreqAxisLabelAndGuidelines(ChartMetadata chart)
        {
            ClearGridContentAndDefenitions(FreqAxisLabelsGrid);
            ClearGridContentAndDefenitions(FreqAxisGuidlinesGrid);
            for (int freqCounter = chart.RowNumber; freqCounter >= 0; freqCounter--)
            {
                if (freqCounter == chart.RowNumber)
                {
                    FreqAxisLabelsGrid.RowDefinitions.Add(new RowDefinition() { MaxHeight = 2.5 });
                    FreqAxisLabelsGrid.RowDefinitions.Add(new RowDefinition() { MaxHeight = 2.5 });
                    FreqAxisGuidlinesGrid.RowDefinitions.Add(new RowDefinition() { MaxHeight = 5 });
                }
                else
                {
                    FreqAxisLabelsGrid.RowDefinitions.Add(new RowDefinition());
                    FreqAxisLabelsGrid.RowDefinitions.Add(new RowDefinition());
                    FreqAxisGuidlinesGrid.RowDefinitions.Add(new RowDefinition());
                }

                // شرط عدم نمایش برچسب ردیف‌های زوج
                //if (freqCounter % 2 != 0 && freqCounter != chart.RowNumber)
                //    continue;

                // Labels
                if (freqCounter != 0)
                {
                    Label l = new Label();
                    l.Content = freqCounter;
                    l.VerticalContentAlignment = (freqCounter == chart.RowNumber) ? VerticalAlignment.Top : VerticalAlignment.Center;
                    l.HorizontalContentAlignment = HorizontalAlignment.Right;
                    l.Padding = new Thickness(5, 0, 5, 0);
                    Grid.SetRow(l, (chart.RowNumber - freqCounter) * 2 + 1);
                    Grid.SetRowSpan(l, 2);
                    FreqAxisLabelsGrid.Children.Add(l);
                }

                // Guideline
                Border b = new Border();
                b.BorderBrush = Brushes.LightGray;
                b.BorderThickness = new Thickness(0, 0, 0, 1);
                Grid.SetRow(b, chart.RowNumber - freqCounter);
                FreqAxisGuidlinesGrid.Children.Add(b);
            }
        }

        private void UpdateVisualizationChart(ChartMetadata chart, BinSize.BinSize bin)
        {
            ClearGridContentAndDefenitions(VisualizationGrid);

            // یک ردیف اضافه برای فضای نمایش بهتر درنظر گرفته می‌شود
            VisualizationGrid.RowDefinitions.Add(new RowDefinition() { MaxHeight = 5 });
            for (int i = 1; i <= chart.MaximumFrequency; i++)
            {
                VisualizationGrid.RowDefinitions.Add(new RowDefinition());
            }

            var intervalFreqsArray = chart.ShowingTimesPerIntervalStartTime.ToArray();

            // ستون اول و آخر که با داده‌ی خالی پر شده‌اند کنار گذاشته می‌شوند
            for (int i = /*0*/1; i < chart.DataColumnsNumber - 1; i++)
            {
                var currentIntervalFreqPair = intervalFreqsArray[i];

                //if (i != 0)
                {
                    // مقایسه‌ی فاصله‌ی با فراوانی صفر، بین هر فراوانی غیرصفر با
                    // فراوانی غیرصفر قبلی
                    var previousIntervalFreqPair = intervalFreqsArray[i - 1];

                    int intervalsDifference = Utility.TimeIntervals.GetBinsCountBetweenIntervalsStartTime
                        (previousIntervalFreqPair.Key, currentIntervalFreqPair.Key, bin);
                    if (intervalsDifference != 0)
                    {
                        // به جای ایجاد یک فضای خالی به‌ازای هر زمان بدون مقدار، یک
                        // فضا با طول چند برابر ایجاد می‌شود، بدین ترتیب در فراوانی
                        // با مقیاس پایین‌تر، و پراکندگی داده‌ای زیاد، کارایی بسیار
                        // حفظ می‌شود
                        VisualizationGrid.ColumnDefinitions.Add
                            (new ColumnDefinition()
                            { Width = new GridLength(BinWidth * intervalsDifference) });
                    }
                }

                VisualizationGrid.ColumnDefinitions.Add
                    (new ColumnDefinition()
                    { Width = new GridLength(BinWidth) });

                var currentIntervalEndTime = currentIntervalFreqPair.Key;
                Utility.TimeIntervals.IncreaseTimeOneStep(ref currentIntervalEndTime, bin);
                currentIntervalEndTime = currentIntervalEndTime.AddTicks(-1);

                Label l = new Label();
                l.Background = Brushes.SkyBlue; // new SolidColorBrush(new Color() { A = 255, R = 205, G = 224, B = 253 });//#FFCDE0FD
                l.ToolTip = string.Format("From: '{0}' To: '{1}'{2}Count: {3}", currentIntervalFreqPair.Key, currentIntervalEndTime, Environment.NewLine, currentIntervalFreqPair.Value.Count);
                l.Margin = new Thickness(1, 0, 0, 0);
                Grid.SetColumn(l, VisualizationGrid.ColumnDefinitions.Count - 1);
                Grid.SetRow(l, chart.RowNumber - currentIntervalFreqPair.Value.Count + 1);
                Grid.SetRowSpan(l, currentIntervalFreqPair.Value.Count);
                l.Tag = currentIntervalFreqPair.Value;
                l.MouseDown += L_MouseDown;
                VisualizationGrid.Children.Add(l);
            }
        }

        private void L_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnDateTimesSelected((sender as Label).Tag as List<DateTime>);
        }

        protected void OnDateTimesSelected(List<DateTime> selectedDateTimes)
        {
            if (DateTimesSelected != null)
                DateTimesSelected(this, new DateTimesSelectedEventArgs(selectedDateTimes));
        }

        private void ClearGridContentAndDefenitions(Grid grid)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
        }

        private void ShowTimeInterval(ChartMetadata result)
        {
            ShowTimeIntervalLabel.Content = "Showing " + result.LowerBound + " -- " + result.UpperBound;
        }
    }
}
