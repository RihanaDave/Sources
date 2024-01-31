using GPAS.FilterSearch;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.EventsArgs;
using GPAS.Workspace.Presentation.Windows;
using System;
using System.Windows;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias
{
    public abstract partial class BaseFilterSearchControl : PresentationControl, IBaseFilterSearchControl
    {
        #region Dependencies
        public ValidationStatus ValidationStatus
        {
            get { return (ValidationStatus)GetValue(ValidationStatusProperty); }
            set { SetValue(ValidationStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationStatusProperty =
            DependencyProperty.Register(nameof(ValidationStatus), typeof(ValidationStatus), typeof(BaseFilterSearchControl),
                new PropertyMetadata(ValidationStatus.Invalid, OnSetValidationStatusChanged));

        private static void OnSetValidationStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BaseFilterSearchControl)d).OnSetValidationStatusChanged(e);
        }

        private void OnSetValidationStatusChanged(DependencyPropertyChangedEventArgs e)
        {
            OnValidationStatusChanged(new ValidationStatusChangedEventArgs((ValidationStatus)e.OldValue, (ValidationStatus)e.NewValue));
        }

        public string ValidationMessage
        {
            get { return (string)GetValue(ValidationMessageProperty); }
            set { SetValue(ValidationMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationMessageProperty =
            DependencyProperty.Register(nameof(ValidationMessage), typeof(string), typeof(BaseFilterSearchControl), new PropertyMetadata(Properties.Resources.InValid));


        public ContainerFilterSearchControl ParentContainerControl
        {
            get { return (ContainerFilterSearchControl)GetValue(ParentContainerControlProperty); }
            set { SetValue(ParentContainerControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParentContainerControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParentContainerControlProperty =
            DependencyProperty.Register(nameof(ParentContainerControl), typeof(ContainerFilterSearchControl), typeof(BaseFilterSearchControl), new PropertyMetadata(null));


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(BaseFilterSearchControl), new PropertyMetadata(false, OnSetIsSelectedChanged));

        private static void OnSetIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BaseFilterSearchControl)d).OnSetIsSelectedChanged(e);
        }

        private void OnSetIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsSelected)
            {
                if (ParentContainerControl != null)
                {
                    ParentContainerControl.SetSelectedCriteiraItem(this);
                }
                else
                {
                    if (this is ContainerFilterSearchControl)
                        (this as ContainerFilterSearchControl).SetSelectedCriteiraItem(this);
                }

                OnSelected();
            }
            else
            {
                OnUnSelected();
            }
        }

        public CriteriaBase CriteriaBase
        {
            get { return (CriteriaBase)GetValue(CriteriaBaseProperty); }
            set { SetValue(CriteriaBaseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CriteriaBase.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CriteriaBaseProperty =
            DependencyProperty.Register(nameof(CriteriaBase), typeof(CriteriaBase), typeof(BaseFilterSearchControl),
                new PropertyMetadata(null, OnSetCriteriaBaseChanged));

        private static void OnSetCriteriaBaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BaseFilterSearchControl)d).OnSetCriteriaBaseChanged(e);
        }

        private void OnSetCriteriaBaseChanged(DependencyPropertyChangedEventArgs e)
        {
            AfterSetCriteriaBase();
        }

        protected ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        protected static readonly DependencyProperty CurrentThemeProperty = DependencyProperty.Register(nameof(CurrentTheme),
            typeof(ThemeApplication), typeof(BaseFilterSearchControl), new PropertyMetadata(null));


        protected MainWindow MainWindow
        {
            get { return (MainWindow)GetValue(MainWindowProperty); }
            set { SetValue(MainWindowProperty, value); }
        }

        protected static readonly DependencyProperty MainWindowProperty =
            DependencyProperty.Register(nameof(MainWindow), typeof(MainWindow), typeof(BaseFilterSearchControl), new PropertyMetadata(null));

        #endregion

        #region Methodes

        public BaseFilterSearchControl()
        {
            Init();
            Loaded += BaseFilterSearchControl_Loaded;
        }

        private void BaseFilterSearchControl_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            MainWindow = window as MainWindow;

            if (MainWindow == null)
                return;

            MainWindow.CurrentThemeChanged -= MainWindow_CurrentThemeChanged;
            MainWindow.CurrentThemeChanged += MainWindow_CurrentThemeChanged;            
        }

        private void MainWindow_CurrentThemeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CurrentTheme = MainWindow.CurrentTheme;
        }

        protected virtual void AfterSetCriteriaBase()
        {

        }

        protected virtual void SetValidationStatus()
        {
            ValidationStatus = ValidationStatus.Invalid;
        }

        protected ContainerFilterSearchControl GetRoot(BaseFilterSearchControl criteriaControl)
        {
            if (criteriaControl.ParentContainerControl == null)
                return criteriaControl as ContainerFilterSearchControl;
            else
                return GetRoot(criteriaControl.ParentContainerControl);
        }

        private void Init()
        {
            IsHitTestVisible = true;
            Focusable = true;
            IsTabStop = true;
            Background = Brushes.Transparent;

            GotFocus += BaseFilterSearchControl_GotFocus;
            MouseLeftButtonDown += BaseCriteriaControl_MouseLeftButtonDown;
        }

        private void SetParentSettingsAfterRemove()
        {
            if (ParentContainerControl != null)
            {
                ParentContainerControl.IsSelected = true;
                ParentContainerControl.CriteriaItems.Remove(CriteriaBase);
            }
        }

        private void BaseFilterSearchControl_GotFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            IsSelected = true;
        }

        private void BaseCriteriaControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            IsSelected = true;
        }

        #endregion

        #region Events

        public event EventHandler RemoveButtonClicked;
        protected void OnRemoveButtonClicked()
        {
            SetParentSettingsAfterRemove();

            RemoveButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<ValidationStatusChangedEventArgs> ValidationStatusChanged;
        protected void OnValidationStatusChanged(ValidationStatusChangedEventArgs args)
        {
            ValidationStatusChanged?.Invoke(this, args);
        }

        public event EventHandler Selected;
        protected void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler UnSelected;
        protected void OnUnSelected()
        {
            UnSelected?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
