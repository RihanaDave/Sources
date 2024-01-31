using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for ImportingObjectBrowser.xaml
    /// </summary>
    public partial class ImportingObjectBrowser : UserControl
    {
        #region Properties

        public ImportingObject BrowsingObject
        {
            get => (ImportingObject)GetValue(ObjectToShowProperty);
            set => SetValue(ObjectToShowProperty, value);
        }

        // Using a DependencyProperty as the backing store for BrowsingObject.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectToShowProperty =
            DependencyProperty.Register(nameof(BrowsingObject), typeof(ImportingObject), typeof(ImportingObjectBrowser),
                new PropertyMetadata(null));



        #endregion

        #region Methods

        public ImportingObjectBrowser()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        #endregion
    }
}
