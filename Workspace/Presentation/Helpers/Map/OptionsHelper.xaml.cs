using GPAS.Workspace.Presentation.Controls;
using System;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Helpers.Map
{
    /// <summary>
    /// Interaction logic for OptionsHelper.xaml
    /// </summary>
    public partial class OptionsHelper : PresentationHelper
    {
        public OptionsHelper()
        {
            InitializeComponent();
            DataContext = this;
        }

        MapControl RelatedMapControl = null;

        public void Init(MapControl relatedMapControl)
        {
            if (RelatedMapControl != null)
            {
                throw new InvalidOperationException("Helper intitialized before");
            }
            RelatedMapControl = relatedMapControl;
            RelatedMapControl.MapTileLoadStarted += RelatedMapControl_MapTileLoadStarted;
            RelatedMapControl.MapTileLoadCompleted += RelatedMapControl_MapTileLoadCompleted;
            RelatedMapControl.MapTileLoadAborted += RelatedMapControl_MapTileLoadAborted;
            foreach (string tileSource in RelatedMapControl.GetMapTileImageSources())
            {
                MapTileSourceComboBox.Items.Add(tileSource);
            }
            MapTileSourceComboBox.SelectedItem = RelatedMapControl.SelectedMapTileSource;
            MapTileSourceComboBox.SelectionChanged += MapTileSourceComboBox_SelectionChanged;
        }

        private void RelatedMapControl_MapTileLoadAborted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new NoInputNoOutputDelegate(ApplyMapTileLoadedAppearance));
        }

        delegate void NoInputNoOutputDelegate();
        void ApplyMapTileLoadedAppearance()
        {
            WaitingControl.TaskDecrement();
        }
        private void RelatedMapControl_MapTileLoadCompleted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new NoInputNoOutputDelegate(ApplyMapTileLoadedAppearance));
        }
        
        void ApplyMapTileIsLoadingAppearance()
        {
            WaitingControl.TaskIncrement();
        }
        private void RelatedMapControl_MapTileLoadStarted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new NoInputNoOutputDelegate(ApplyMapTileIsLoadingAppearance));
        }

        private void MapTileSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RelatedMapControl.ChangeMapTileSource((string)MapTileSourceComboBox.SelectedItem);
        }

        public override void Reset()
        {
            MapTileSourceComboBox.SelectedIndex = 0;
        }
    }
}