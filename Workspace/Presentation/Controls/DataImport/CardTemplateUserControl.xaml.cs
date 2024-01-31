using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    public partial class CardTemplateUserControl : UserControl
    {
        public double IconSize
        {
            get { return (double)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(CardTemplateUserControl), new PropertyMetadata(90.0));

        public double CardHeight
        {
            get { return (double)GetValue(CardHeightProperty); }
            set { SetValue(CardHeightProperty, value); }
        }

        public static readonly DependencyProperty CardHeightProperty =
            DependencyProperty.Register(nameof(CardHeight), typeof(double), typeof(CardTemplateUserControl), new PropertyMetadata(0.0));

        public double CardWidth
        {
            get { return (double)GetValue(CardWidthProperty); }
            set { SetValue(CardWidthProperty, value); }
        }

        public static readonly DependencyProperty CardWidthProperty =
            DependencyProperty.Register(nameof(CardWidth), typeof(double), typeof(CardTemplateUserControl), new PropertyMetadata(0.0));

        public bool ShowActionButtons
        {
            get { return (bool)GetValue(ShowActionButtonsProperty); }
            set { SetValue(ShowActionButtonsProperty, value); }
        }

        public static readonly DependencyProperty ShowActionButtonsProperty =
            DependencyProperty.Register(nameof(ShowActionButtons), typeof(bool), typeof(CardTemplateUserControl),
                new PropertyMetadata(true));

        public bool ShowFileName
        {
            get { return (bool)GetValue(ShowFileNameProperty); }
            set { SetValue(ShowFileNameProperty, value); }
        }

        public static readonly DependencyProperty ShowFileNameProperty =
            DependencyProperty.Register(nameof(ShowFileName), typeof(bool), typeof(CardTemplateUserControl),
                new PropertyMetadata(true));

        public DataSourceImportStatus ImportingObjectsGenerationStatus
        {
            get { return (DataSourceImportStatus)GetValue(ImportingObjectsGenerationStatusProperty); }
            set { SetValue(ImportingObjectsGenerationStatusProperty, value); }
        }

        public static readonly DependencyProperty ImportingObjectsGenerationStatusProperty =
            DependencyProperty.Register(nameof(ImportingObjectsGenerationStatus), typeof(DataSourceImportStatus), typeof(CardTemplateUserControl),
                new PropertyMetadata(DataSourceImportStatus.Ready));


        public event EventHandler<EventArgs> MappingButtonClicked;

        protected void OnMappingButtonClicked(EventArgs eventArgs)
        {
            MappingButtonClicked?.Invoke(this, eventArgs);
        }

        public event EventHandler<EventArgs> AclButtonClicked;

        protected void OnAclButtonClicked(EventArgs eventArgs)
        {
            AclButtonClicked?.Invoke(this, eventArgs);
        }

        public event EventHandler<EventArgs> DefectionsListItemMouseDown;

        protected void OnDefectionsListItemMouseDown(EventArgs eventArgs)
        {
            DefectionsListItemMouseDown?.Invoke(this, eventArgs);
        }

        public event EventHandler<EventArgs> DefectiosButtonPreviewMouseLeftButtonDown;

        protected void OnDefectiosButtonPreviewMouseLeftButtonDown(EventArgs eventArgs)
        {
            DefectiosButtonPreviewMouseLeftButtonDown?.Invoke(this, eventArgs);
        }

        public event EventHandler<EventArgs> DefectiosButtonMouseLeftButtonUp;

        protected void OnDefectiosButtonMouseLeftButtonUp(EventArgs eventArgs)
        {
            DefectiosButtonMouseLeftButtonUp?.Invoke(this, eventArgs);
        }


        public CardTemplateUserControl()
        {
            InitializeComponent();
        }

        private void MappingButtonOnClick(object sender, RoutedEventArgs e)
        {
            OnMappingButtonClicked(e);
        }

        private void AclButtonOnClick(object sender, RoutedEventArgs e)
        {
            OnAclButtonClicked(e);
        }

        private void DefectionsListItemOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            OnDefectionsListItemMouseDown(e);
        }

        private void DefectiosButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnDefectiosButtonPreviewMouseLeftButtonDown(e);
        }

        private void DefectiosButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OnDefectiosButtonMouseLeftButtonUp(e);
        }
    }
}
