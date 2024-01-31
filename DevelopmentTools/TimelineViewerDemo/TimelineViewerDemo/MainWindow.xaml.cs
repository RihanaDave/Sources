using GPAS.TimelineViewer;
using GPAS.TimelineViewer.EventArguments;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TimelineViewerDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            Init();
        }
        private void Init()
        {
        }

        public int CurrentId
        {
            get { return (int)GetValue(CurrentIdProperty); }
            set { SetValue(CurrentIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentIdProperty =
            DependencyProperty.Register("CurrentId", typeof(int), typeof(MainWindow), new PropertyMetadata(1));

        public BitmapImage IconProperty
        {
            get { return (BitmapImage)GetValue(IconPropertyProperty); }
            set { SetValue(IconPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconPropertyProperty =
            DependencyProperty.Register("IconProperty", typeof(BitmapImage), typeof(MainWindow),
                new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/images/no-image.png"))));


        public DateTime? From
        {
            get { return (DateTime?)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        // Using a DependencyProperty as the backing store for From.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(DateTime?), typeof(MainWindow),
                new PropertyMetadata(new DateTime(2000, 1, 1), OnSetFromChanged));

        private static void OnSetFromChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MainWindow).OnSetFromChanged(e);
        }

        private void OnSetFromChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidateDuration();

            SetValidateAddProperty();
        }

        public DateTime? To
        {
            get { return (DateTime?)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for To.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(DateTime?), typeof(MainWindow),
                new PropertyMetadata(new DateTime(2001, 1, 1), OnSetToChanged));

        private static void OnSetToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MainWindow).OnSetToChanged(e);
        }

        private void OnSetToChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidateDuration();

            SetValidateAddProperty();
        }

        public string NumberOfItems
        {
            get { return (string)GetValue(NumberOfItemsProperty); }
            set { SetValue(NumberOfItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumberOfItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfItemsProperty =
            DependencyProperty.Register("NumberOfItems", typeof(string), typeof(MainWindow),
                new PropertyMetadata("5000", OnSetNumberOfItemsChanged));

        private static void OnSetNumberOfItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MainWindow).OnSetNumberOfItemsChanged(e);
        }

        private void OnSetNumberOfItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            int n = 0;
            if (!int.TryParse(NumberOfItems, out n) || n < 1)
            {
                validateNumberOfItems = false;
            }
            else
            {
                validateNumberOfItems = true;
            }
            SetValidateAddProperty();
        }

        private void SetValidateDuration()
        {
            if (From >= To)
            {
                validateDuration = false;
            }
            else
            {
                validateDuration = true;
            }
        }

        private void SetValidateAddProperty()
        {
            if (validateNumberOfItems)
            {
                NumberOfItemsTextBox.Background = Brushes.White;
            }
            else
            {
                NumberOfItemsTextBox.Background = Brushes.Tomato;
            }


            if (validateDuration)
            {
                FromDatePicker.Background = Brushes.White;
                ToDatePicker.Background = Brushes.White;
            }
            else
            {
                FromDatePicker.Background = Brushes.Tomato;
                ToDatePicker.Background = Brushes.Tomato;
            }

            ValidateAddProperty = validateNumberOfItems && validateDuration;
        }

        public bool ValidateAddProperty
        {
            get { return (bool)GetValue(ValidateAddPropertyProperty); }
            set { SetValue(ValidateAddPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidateAddProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidateAddPropertyProperty =
            DependencyProperty.Register("ValidateAddProperty", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));

        
        bool validateDuration = true;
        bool validateNumberOfItems = true;

        List<DateTimePropertyValueStatistics> TotalData = new List<DateTimePropertyValueStatistics>();

        Random random = new Random();

        private List<ValueTimePair> GenerateRandomValueTimes(DateTime minTime, DateTime maxTime, int numberOfItems)
        {
            List<ValueTimePair> valueTimes = new List<ValueTimePair>();
            TimeSpan duration = maxTime - minTime;

            for (int i = 0; i < numberOfItems; i++)
            {
                DateTime time = new DateTime();
                Utility.DateTimeAddTryParse(minTime, new TimeSpan(LongRandom(0, duration.Ticks)), out time);
                int val = random.Next(1, 10);

                ValueTimePair existValueTime = valueTimes.Where(vt => vt.Time.Equals(time)).FirstOrDefault();
                if (existValueTime == null)
                {
                    ValueTimePair valueTime = new ValueTimePair()
                    {
                        Time = time,
                        Value = val,
                    };
                    valueTime.IsSelectedChanged += ValueTime_IsSelectedChanged;

                    valueTimes.Add(valueTime);
                }
                else
                {
                    existValueTime.Value += val;
                }
            }

            valueTimes = valueTimes.OrderBy(vt => vt.Time).ToList();
            return valueTimes;
        }

        long LongRandom(long min, long max)
        {
            byte[] buf = new byte[8];
            random.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return (Math.Abs(longRand % (max - min)) + min);
        }

        private async Task<List<SuperCategory>> FetchData(DateTime beginTime, DateTime endTime, BinSize binSize)
        {
            List<SuperCategory> fetchedData = new List<SuperCategory>();
            foreach (var prp in TotalData)
            {
                SuperCategory currentSuperCat;

                if (fetchedData.Where(supC => supC.Title == prp.Parent).FirstOrDefault() != null)
                {
                    currentSuperCat = fetchedData.Where(supC => supC.Title == prp.Parent).FirstOrDefault();
                }
                else
                {
                    currentSuperCat = new SuperCategory
                    {
                        Title = prp.Parent,
                        Tag = prp.Parent,
                    };

                    fetchedData.Add(currentSuperCat);
                }

                DateTime min = prp.ValueTimes.Select(vt => vt.Time).Min();
                DateTime max = prp.ValueTimes.Select(vt => vt.Time).Max();
                if (beginTime > min)
                    min = beginTime;
                if (endTime < max)
                    max = endTime;

                min = Utility.Floor(min, binSize);

                Utility.DateTimeAddTryParse(max, binSize.GetDuration(max), out max);
                if (max < Utility.MaxValue)
                    max = Utility.Floor(max, binSize);

                Category subCat = new Category()
                {
                    Title = prp.Title,
                    Identifier = prp.Id,
                    Icon = prp.Icon,
                    DataItems = await Task.Run(() => RetriveDataItems(prp.ValueTimes.Where(vt => vt.Time >= min && vt.Time <= max).ToList(), min, max, binSize)),
                };

                currentSuperCat.SubCategories.Add(subCat);
            }

            return fetchedData;
        }

        private ObservableCollection<TimelineItemsSegment> RetriveDataItems(List<ValueTimePair> valueTimes, DateTime min, DateTime max, BinSize binSize)
        {
            ObservableCollection<TimelineItemsSegment> dataItems = new ObservableCollection<TimelineItemsSegment>();

            DateTime index = min;
            while (index < max)
            {
                TimelineItemsSegment dataItem = new TimelineItemsSegment()
                {
                    From = index,
                    To = new DateTime(binSize.BinAddToDate(index, 1).Ticks - 1),
                };

                var curValues = valueTimes.Where(vt => vt.Time >= dataItem.From && vt.Time <= dataItem.To).Select(v => v.Value).ToList();
                if (curValues != null)
                {
                    foreach (var value in curValues)
                    {
                        dataItem.FrequencyCount += value;
                    }

                    dataItems.Add(dataItem);
                }
                else
                {

                }

                index = binSize.BinAddToDate(index, 1);
            }

            return dataItems;
        }

        DateTime TotalLowerBound = Utility.MinValue;
        DateTime TotalUpperBound = Utility.MaxValue;
        public void AssignTotalBounds()
        {
            var lower = FindLowerTime();
            var upper = FindUpperTime();

            var actualBin = timelineViewer.GetActualBin();
            TotalLowerBound = Utility.Floor(lower, actualBin);
            Utility.DateTimeAddTryParse(upper, actualBin.GetDuration(upper), out upper);
            TotalUpperBound = Utility.Floor(upper, actualBin);
        }

        private DateTime FindLowerTime()
        {
            return TotalData.Select(prop => prop.ValueTimes.Select(vt => vt.Time).Min()).Min();
        }

        private DateTime FindUpperTime()
        {
            return TotalData.Select(prop => prop.ValueTimes.Select(vt => vt.Time).Max()).Max();
        }

        private double RetriveMaximumCount(DateTime min, DateTime max, BinSize binSize)
        {
            double maxCount = 0;
            List<DateTimePropertyValueStatistics> allData = new List<DateTimePropertyValueStatistics>();
            foreach (var prop in TotalData)
            {
                allData.Add(new DateTimePropertyValueStatistics()
                {
                    Id = prop.Id,
                    Parent = prop.Parent,
                    Title = prop.Title,
                    ValueTimes = new List<ValueTimePair>(prop.ValueTimes),
                });
            }
            DateTime index = min;
            while (index < max)
            {
                TimelineItemsSegment dataItem = new TimelineItemsSegment()
                {
                    From = index,
                    To = new DateTime(binSize.BinAddToDate(index, 1).Ticks - 1),
                };

                foreach (var prop in allData)
                {
                    var first = prop.ValueTimes.Where(vt => vt.Time >= dataItem.From).FirstOrDefault();
                    if (first == null)
                        continue;

                    var last = prop.ValueTimes.Where(vt => vt.Time <= dataItem.To).LastOrDefault();
                    if (last == null)
                        continue;

                    int firstIndex = prop.ValueTimes.IndexOf(first);
                    int lastIndex = prop.ValueTimes.IndexOf(last);
                    if (lastIndex < firstIndex)
                        continue;

                    for (int i = firstIndex; i <= lastIndex; i++)
                    {
                        dataItem.FrequencyCount += prop.ValueTimes[i].Value;
                    }

                    prop.ValueTimes.RemoveRange(firstIndex, lastIndex - firstIndex + 1);
                }

                if (maxCount < dataItem.FrequencyCount)
                {
                    maxCount = dataItem.FrequencyCount;
                }

                index = binSize.BinAddToDate(index, 1);
            }

            return maxCount;
        }

        private async void GeneratePropertyButton_Click(object sender, RoutedEventArgs e)
        {
            waitingGrid.Visibility = Visibility.Visible;
            string title = TitleTextBox.Text;
            string parent = ParentTextBox.Text;
            DateTime from = From.Value;
            DateTime to = To.Value;

            int numberOfItems = int.Parse(NumberOfItemsTextBox.Text);
            int id = CurrentId;
            BitmapImage icon = IconProperty;

            var property = await Task.Run(() => GenerateProperty(id, title, parent, icon, from, to, numberOfItems));
            CurrentId++;

            TotalData.Add(property);
            timelineViewer.Reset(timelineViewer.Bin);
        }

        private DateTimePropertyValueStatistics GenerateProperty(int id, string title, string parent, BitmapImage icon, DateTime from, DateTime to, int numberOfItems)
        {
           return new DateTimePropertyValueStatistics()
            {
                Id = id,
                Parent = parent,
                Title = title,
                Icon = icon,
                ValueTimes = GenerateRandomValueTimes(from, to, numberOfItems),
            };
        }

        private void timelineViewer_ItemsNeeded(object sender, ItemsNeededEventArgs e)
        {
            GenerateItems(e);
        }

        private async void GenerateItems(ItemsNeededEventArgs e)
        {
            if (e == null)
            {
                timelineViewer.AddItems(e);
                waitingGrid.Visibility = Visibility.Collapsed;
                return;
            }

            waitingGrid.Visibility = Visibility.Visible;
            DateTime beginTime = e.BeginTime;
            DateTime endTime = e.EndTime;
            AssignTotalBounds();

            if (e.Action == ItemsNeededAction.None)
            {
                DateTime centerRange = Utility.CenterRange(TotalLowerBound, TotalUpperBound);

                double halfNumberOfDataSegmentLoaded = (double)timelineViewer.NumberOfDataSegmentLoaded / 2;
                DateTime lb = Utility.MinValue;
                DateTime ub = Utility.MaxValue;
                Utility.DateTimeAddTryParse(centerRange, new TimeSpan((long)(-halfNumberOfDataSegmentLoaded * timelineViewer.DataSegment.Ticks)), out lb);
                if (!Utility.DateTimeAddTryParse(lb, new TimeSpan(timelineViewer.NumberOfDataSegmentLoaded * timelineViewer.DataSegment.Ticks), out ub))
                {
                    Utility.DateTimeAddTryParse(ub, new TimeSpan(-timelineViewer.NumberOfDataSegmentLoaded * timelineViewer.DataSegment.Ticks), out lb);
                }

                beginTime = lb;
                endTime = ub;
            }

            BinSize binSize = new BinSize()
            {
                BinFactor = e.BinFactor,
                BinScale = e.BinScaleLevel,
            };

            List<SuperCategory> fetchedData = new List<SuperCategory>();
            fetchedData = await Task.Run(() => FetchData(beginTime, endTime, binSize));

            int count = fetchedData.Select(f => f.SubCategories.Count).Sum();
            if (count == 0)
                return;

            DateTime min = TotalLowerBound;
            DateTime max = TotalUpperBound;
            e.MaximumCount = await Task.Run(() => RetriveMaximumCount(min, max, binSize));
            e.TotalLowerBound = TotalLowerBound;
            e.TotalUpperBound = TotalUpperBound;
            e.FetchedItems = fetchedData;

            timelineViewer.AddItems(e);
            waitingGrid.Visibility = Visibility.Collapsed;
        }

        private void timelineViewer_SnapshotTaken(object sender, SnapshotTakenEventArgs e)
        {
            TakeSnapshot(e.UIElement);
        }

        public void TakeSnapshot(UIElement uIElement)
        {
            try
            {
                string filePath = string.Empty;

                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.FileName = "Snapshot"; // Default file name
                dlg.DefaultExt = ".png"; // Default file extension
                dlg.Filter = "PNG |*.png"; // Filter files by extension

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = dlg.FileName;

                    int width = (int)uIElement.RenderSize.Width;
                    int height = (int)uIElement.RenderSize.Height;

                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    renderTargetBitmap.Render(uIElement);

                    PngBitmapEncoder pngImage = new PngBitmapEncoder();
                    pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                    using (Stream fileStream = File.Create(filePath))
                    {
                        pngImage.Save(fileStream);
                    }

                    if (MessageBox.Show("Snapshot saved successfully!\n\nDo you want to display image?",
                                        "Timeline Demo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Process.Start(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Timeline Demo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            timelineViewer.ClearAllData();
            TotalData.Clear();
            CurrentId = 1;
        }

        private void OpenDialogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.DefaultExt = ".png"; // Default file extension
                dlg.Filter = "PNG |*.png|JPG |*.jpg"; // Filter files by extension
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    IconPathTextBox.Text = dlg.FileName;
                    IconProperty = new BitmapImage(new Uri(dlg.FileName));
                    IconImage.Source = IconProperty;
                }
            }
            catch
            {

            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string culString = "en";
            if (LanguageComboBox.SelectedIndex == 1)
            {
                culString = "fa";
                this.FlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                culString = "en";
                this.FlowDirection = FlowDirection.LeftToRight;
            }

            CultureInfo culture = new CultureInfo(culString);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void ColorPicker1_SelectedColorChanged(object sender, EventArgs e)
        {
            SetBarColorSelection(ColorPicker1?.SelectedColor, ColorPicker2?.SelectedColor);
        }

        private void ColorPicker2_SelectedColorChanged(object sender, EventArgs e)
        {
            SetBarColorSelection(ColorPicker1?.SelectedColor, ColorPicker2?.SelectedColor);
        }

        private void SetBarColorSelection(Brush color1, Brush color2)
        {
            if (color1 == null)
                color1 = Brushes.Transparent;

            if (color2 == null)
                color2 = Brushes.Transparent;

            timelineViewer.SelectedBarColor = new LinearGradientBrush(new GradientStopCollection(new List<GradientStop>() {
                    new GradientStop(((SolidColorBrush) color1).Color, .8),
                    new GradientStop(((SolidColorBrush) color2).Color, .2),
                }), new Point(1, .5), new Point(0, .5));
        }

        bool changeSelectionWithEvent = false;
        enum GraphSimulationState
        {
            Append,
            Remove,
            Add,
            RemoveAll,
        }
        GraphSimulationState graphState = GraphSimulationState.Add;

        private void AddAnObjectToAllSelectedObjectsButton_Click(object sender, RoutedEventArgs e)
        {
            graphState = GraphSimulationState.Append;
            List<ValueTimePair> DeSelectedValueTimes = new List<ValueTimePair>();
            foreach (var property in TotalData)
            {
                DeSelectedValueTimes.AddRange(property.ValueTimes.Where(vt => !vt.IsSelected));
            }

            if (DeSelectedValueTimes.Count > 0)
                DeSelectedValueTimes[random.Next(0, DeSelectedValueTimes.Count - 1)].IsSelected = true;
        }

        private void DeselectAnObjectButton_Click(object sender, RoutedEventArgs e)
        {
            graphState = GraphSimulationState.Remove;
            List<ValueTimePair> SelectedValueTimes = new List<ValueTimePair>();
            foreach (var property in TotalData)
            {
                SelectedValueTimes.AddRange(property.ValueTimes.Where(vt => vt.IsSelected));
            }

            if (SelectedValueTimes.Count > 0)
                SelectedValueTimes[random.Next(0, SelectedValueTimes.Count - 1)].IsSelected = false;
        }

        private void SelectOnlyAnObjectButton_Click(object sender, RoutedEventArgs e)
        {
            graphState = GraphSimulationState.Add;
            DeselectAllValueTimes();
            List<ValueTimePair> AllValueTimes = new List<ValueTimePair>();
            foreach (var property in TotalData)
            {
                AllValueTimes.AddRange(property.ValueTimes);
            }

            if (AllValueTimes.Count > 0)
                AllValueTimes[random.Next(0, AllValueTimes.Count - 1)].IsSelected = true;
        }

        private void DeselectAllObjectsButton_Click(object sender, RoutedEventArgs e)
        {
            graphState = GraphSimulationState.RemoveAll;
            DeselectAllValueTimes();
            timelineViewer.DeselectAllCoveringSegments();
        }

        private void DeselectAllValueTimes()
        {
            List<ValueTimePair> SelectedValueTimes = new List<ValueTimePair>();
            foreach (var property in TotalData)
            {
                SelectedValueTimes.AddRange(property.ValueTimes.Where(vt => vt.IsSelected));
            }

            foreach (var selected in SelectedValueTimes)
            {
                selected.IsSelected = false;
            }
        }

        private void timelineViewer_SegmentSelectionChanged(object sender, SegmentSelectionChangedEventArgs e)
        {
            changeSelectionWithEvent = true;
            foreach (var prop in TotalData)
            {
                foreach (var valueTime in prop.ValueTimes)
                {
                    valueTime.IsSelected = false;
                }
            }

            foreach (var segment in timelineViewer.SelectedSegments)
            {
                foreach (var prop in TotalData)
                {
                    var selectedRange = prop.ValueTimes.Where(vt => vt.Time >= segment.From && vt.Time <= segment.To).ToList();
                    foreach (var valueTime in selectedRange)
                    {
                        valueTime.IsSelected = true;
                    }
                }
            }

            changeSelectionWithEvent = false;
        }

        private void ValueTime_IsSelectedChanged(object sender, EventArgs e)
        {
            List<Range> segments = new List<Range>();
            ValueTimePair valueTime = (ValueTimePair)sender;
            segments.Add(new Range()
            {
                From = valueTime.Time,
                To = valueTime.Time,
            });

            if (!changeSelectionWithEvent)
            {
                if (graphState == GraphSimulationState.Append)
                {
                    timelineViewer.AppendCoveringSegments(segments);
                }
                else if (graphState == GraphSimulationState.Add)
                {
                    timelineViewer.SelectCoveringSegments(segments);
                }
                else if(graphState == GraphSimulationState.Remove)
                {
                    timelineViewer.DeselectCoveringSegments(segments);
                }
            }
        }
    }
}
