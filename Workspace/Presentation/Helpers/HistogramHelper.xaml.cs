using GPAS.HistogramViewer;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Controls.Histograms;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Helpers
{
    /// <summary>
    /// Interaction logic for HistogramHelper.xaml
    /// </summary>
    public partial class HistogramHelper : IObjectsSelectableListener
    {
        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(PngBitmapEncoder image, string defaultFileName)
        {
            SnapshotTaken?.Invoke(this, new TakeSnapshotEventArgs(image, defaultFileName));
        }

        public class ObjectsSelectionArgs
        {
            public ObjectsSelectionArgs(IEnumerable<KWObject> selectedObjects)
            {
                SelectedObjects = selectedObjects;
            }

            public IEnumerable<KWObject> SelectedObjects
            {
                private set;
                get;
            }
        }
        public EventHandler<ObjectsSelectionArgs> SelectionChanged;
        public void OnSelectionChanged(IEnumerable<KWObject> selectedObjects)
        {
            if (selectedObjects == null)
                throw new ArgumentNullException("selectedObjects");

            if (SelectionChanged != null)
                SelectionChanged(this, new ObjectsSelectionArgs(selectedObjects));
        }

        public HistogramHelper()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SelectObjects(IEnumerable<KWObject> objectsToSelect)
        {
            if (objectsToSelect == null)
                throw new ArgumentNullException("objectsToSelect");

            if (!objectsToSelect.Any())
                mainHistogramControl.ClearHistogram();
            else
                mainHistogramControl.ShowObjectsHistogramAsync(objectsToSelect);
        }

        private void mainHistogramControl_SelectionChanged(object sender, HistogramControl.ObjectsSelectionArgs e)
        {
            OnSelectionChanged(e.SelectedObjects);
        }

        private void mainHistogramControl_SnapshotTaken(object sender, Histogram.TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e.Snapshot, e.DefaultFileName);
        }

        public override void Reset()
        {
            mainHistogramControl.Reset();
        }
    }

    public class IntToHistogramPropertyNodeOrderByConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                if ((int)value == 0)
                    return HistogramPropertyNodeOrderBy.Count;
                else if ((int)value == 1)
                    return HistogramPropertyNodeOrderBy.Title;
            }
            return HistogramPropertyNodeOrderBy.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((HistogramPropertyNodeOrderBy)value)
            {
                case HistogramPropertyNodeOrderBy.Count:
                    return 0;
                case HistogramPropertyNodeOrderBy.Title:
                    return 1;
            }
            return 0;
        }
    }
}
