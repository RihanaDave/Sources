using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    /// <summary>
    /// Interaction logic for SetPathWindow.xaml
    /// </summary>
    public partial class SetPathWindow : Window
    {
        #region Peroperty

        public DocumentMapModel Document
        {
            get { return (DocumentMapModel)GetValue(DocumentMapModelProperty); }
            set { SetValue(DocumentMapModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentMapModelProperty =
            DependencyProperty.Register(nameof(Document), typeof(DocumentMapModel), typeof(SetPathWindow),
                new PropertyMetadata(null));

        private ObservableCollection<ValueMapModel> PreviousValueCollection = new ObservableCollection<ValueMapModel>();
        private DocumentPathOption PreviousDocumenPathOption = DocumentPathOption.File;
        private bool PreviousPathInternalResolution = false;
        private ResolutionPolicy PreviousResolutionPolicy = ResolutionPolicy.SetNew;

        #endregion

        #region Function
        public SetPathWindow(MappingViewModel mappingViewModel, DocumentMapModel documentMapModel)
        {
            InitializeComponent();
            DataContext = mappingViewModel;
            Document = documentMapModel;
            CopyValueCollection(Document.Path.ValueCollection, PreviousValueCollection);
            CopyPathOption(Document.PathOption);
            PreviousPathInternalResolution = Document.Path.HasResolution;
            PreviousResolutionPolicy = Document.ResolutionPolicy;
        }

        private void CopyValueCollection(ObservableCollection<ValueMapModel> source, ObservableCollection<ValueMapModel> target)
        {
            target.Clear();
            foreach (var item in source)
            {
                target.Add(new ValueMapModel()
                {
                    Field = item.Field,
                    HasRegularExpression = item.HasRegularExpression,
                    Id = item.Id,
                    IsSelected = item.IsSelected,
                    OwnerProperty = item.OwnerProperty,
                    RegularExpressionPattern = item.RegularExpressionPattern,
                    SampleValue = item.SampleValue,
                });
            }
        }

        private void CopyPathOption(DocumentPathOption source)
        {
            switch (source)
            {
                case DocumentPathOption.File:
                    PreviousDocumenPathOption = DocumentPathOption.File;
                    break;
                case DocumentPathOption.FolderWithSubFolder:
                    PreviousDocumenPathOption = DocumentPathOption.FolderWithSubFolder;
                    break;
                default:
                    break;
            }
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CancelSetPathWindow_Click(object sender, RoutedEventArgs e)
        {
            Document.Path.HasResolution = PreviousPathInternalResolution;
            Document.ResolutionPolicy = PreviousResolutionPolicy;
            Document.PathOption = PreviousDocumenPathOption;
            CopyValueCollection(PreviousValueCollection, Document.Path.ValueCollection);

            Close();
        }

        private void OkSetPathWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
