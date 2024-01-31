using GPAS.Workspace.Entities.KWLinks;
using GPAS.Workspace.Logic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GPAS.Ontology;

namespace GPAS.Workspace.Presentation.Controls.Link
{
    /// <summary>
    /// Interaction logic for CreateLinkUserControl.xaml
    /// </summary>
    public partial class CreateLinkUserControl
    {
        public CreateLinkUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<bool> ValidationChanged;

        public event EventHandler<object> LinkTypeUriChanged;

        private void OnLinkTypeUriChanged(object selectedType)
        {
            LinkTypeUriChanged?.Invoke(this, selectedType);
        }

        private void OnValidationChanged()
        {
            ValidationChanged?.Invoke(this, IsValid());
        }

        /// <summary>
        /// توضیحات لینک
        /// </summary>
        public string Description
        {
            get => GetValue(DescriptionProperty).ToString();
            set => SetValue(DescriptionProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(CreateLinkUserControl),
                new PropertyMetadata(string.Empty, OnSetDescriptionChanged));

        private static void OnSetDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CreateLinkUserControl)d).OnSetDescriptionChanged();
        }

        private void OnSetDescriptionChanged()
        {
            LinkTypePicker.SelectItem(LinkTypeUri);
        }

        /// <summary>
        /// نوع لینک
        /// </summary>
        public string LinkTypeUri
        {
            get => GetValue(LinkTypeUriProperty).ToString();
            set => SetValue(LinkTypeUriProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkTypeUriProperty =
            DependencyProperty.Register(nameof(LinkTypeUri), typeof(string), typeof(CreateLinkUserControl),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// جهت لینک
        /// </summary>
        public LinkDirection Direction
        {
            get => (LinkDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register(nameof(Direction), typeof(LinkDirection), typeof(CreateLinkUserControl));

        /// <summary>
        /// نوع شی مبداء
        /// </summary>
        public string SourceTypeUri
        {
            get => (string)GetValue(SourceTypeUriProperty);
            set => SetValue(SourceTypeUriProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceTypeUriProperty =
            DependencyProperty.Register(nameof(SourceTypeUri), typeof(string), typeof(CreateLinkUserControl),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// نام نمایشی شی مبداء
        /// </summary>
        public string SourceDisplayName
        {
            get => (string)GetValue(SourceDisplayNameProperty);
            set => SetValue(SourceDisplayNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceDisplayNameProperty =
            DependencyProperty.Register(nameof(SourceDisplayName), typeof(string), typeof(CreateLinkUserControl),
                new PropertyMetadata(Properties.Resources.String_Source));

        /// <summary>
        /// نوع شی مقصد
        /// </summary>
        public string TargetTypeUri
        {
            get => (string)GetValue(TargetTypeUriProperty);
            set => SetValue(TargetTypeUriProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetTypeUriProperty =
            DependencyProperty.Register(nameof(TargetTypeUri), typeof(string), typeof(CreateLinkUserControl),
                new PropertyMetadata(null));

        /// <summary>
        /// نام نمایشی شی مقصد
        /// </summary>
        public string TargetDisplayName
        {
            get => (string)GetValue(TargetDisplayNameProperty);
            set => SetValue(TargetDisplayNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetDisplayNameProperty =
            DependencyProperty.Register(nameof(TargetDisplayName), typeof(string), typeof(CreateLinkUserControl),
                new PropertyMetadata(Properties.Resources.String_Target));

        private void LinkTypePicker_OnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((OntologyNode)e.OldValue == null || ((OntologyNode)e.OldValue).TypeUri.Equals(string.Empty))
            {
                if (Description.Equals(string.Empty))
                {
                    DescriptionTextBox.Text = LinkTypeUri.Equals(string.Empty)
                        ? string.Empty
                        : OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(LinkTypeUri);
                }
            }
            else if (OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(((OntologyNode)e.OldValue).TypeUri).Equals(Description))
            {
                DescriptionTextBox.Text = LinkTypeUri.Equals(string.Empty)
                    ? string.Empty
                    : OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(LinkTypeUri);
            }

            OnValidationChanged();
            OnLinkTypeUriChanged(e);
        }

        private void SourceToTargetStackPanel_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SourceRadioButton.IsChecked = true;
        }

        private void TargetToSourceStackPanel_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TargetRadioButton.IsChecked = true;
        }

        private void BridgeStackPanel_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BridgeRadioButton.IsChecked = true;
        }

        private void DescriptionTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OnValidationChanged();
        }

        private void RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            OnValidationChanged();
        }

        private bool IsValid()
        {
            if (LinkTypeUri.Equals(Properties.Resources.Select_A_Type) ||
                LinkTypeUri.Equals(Properties.Resources.Not_Initialized) ||
                LinkTypeUri.Equals(string.Empty))
            {
                return false;
            }

            if (SourceRadioButton.IsChecked == false &&
                TargetRadioButton.IsChecked == false &&
                BridgeRadioButton.IsChecked == false)
            {
                return false;
            }

            return !string.IsNullOrEmpty(Description);
        }
    }
}
