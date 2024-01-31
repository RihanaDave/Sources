using GPAS.FilterSearch;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Logic;
using System;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias
{
    /// <summary>
    /// Interaction logic for KeywordFilterSearchControl.xaml
    /// </summary>
    public partial class KeywordFilterSearchControl : BaseFilterSearchControl
    {
        #region Dependencies

        public string Keyword
        {
            get { return (string)GetValue(KeywordProperty); }
            set { SetValue(KeywordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Keyword.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeywordProperty =
            DependencyProperty.Register(nameof(Keyword), typeof(string), typeof(KeywordFilterSearchControl),
                new PropertyMetadata(string.Empty, OnSetKeywordChanged));

        private static void OnSetKeywordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((KeywordFilterSearchControl)d).OnSetKeywordChanged(e);
        }

        private void OnSetKeywordChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidationStatus();
            SetKeywordCriteria();
            OnKeywordChanged();
        }

        public int MaximumChars
        {
            get { return (int)GetValue(MaximumCharsProperty); }
            set { SetValue(MaximumCharsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumChars.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumCharsProperty =
            DependencyProperty.Register(nameof(MaximumChars), typeof(int), typeof(KeywordFilterSearchControl), new PropertyMetadata(1000, OnSetMaximumCharsChanged));

        private static void OnSetMaximumCharsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((KeywordFilterSearchControl)d).OnSetMaximumCharsChanged(e);
        }

        private void OnSetMaximumCharsChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidationStatus();
            OnMaximumCharsChanged();
        }


        #endregion

        #region Events

        public event EventHandler KeywordChanged;
        protected void OnKeywordChanged()
        {
            KeywordChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MaximumCharsChanged;
        protected void OnMaximumCharsChanged()
        {
            MaximumCharsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Variables

        private KeywordCriteria keywordCriteria;

        #endregion

        #region Methodes

        public KeywordFilterSearchControl()
        {
            InitializeComponent();
            Init();
        }

        protected override void AfterSetCriteriaBase()
        {
            keywordCriteria = CriteriaBase as KeywordCriteria;
        }

        protected override void SetValidationStatus()
        {
            var validationResult = PropertyManager.IsPropertyValid(BaseDataTypes.String, Keyword);
            if (validationResult.Status == ValidationStatus.Valid || validationResult.Status == ValidationStatus.Warning)
            {
                if (Keyword.Length > MaximumChars)
                {
                    validationResult.Status = ValidationStatus.Invalid;
                    validationResult.Message = string.Format(Properties.Resources.Property_Values_Characters_Are_More_Than_NUM_Try_Again, MaximumChars);
                }
            }

            ValidationMessage = validationResult.Message;
            ValidationStatus = validationResult.Status;
        }

        private void Init()
        {
            DataContext = this;
            ValidationStatus = ValidationStatus.Valid;

            if (CriteriaBase == null)
            {
                CriteriaBase = new KeywordCriteria();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveButtonClicked();
        }

        private void SetKeywordCriteria()
        {
            if (ValidationStatus != ValidationStatus.Invalid)
            {
                keywordCriteria.Keyword = Keyword;
            }
        }

        private void KeywordTextBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        #endregion
    }
}
