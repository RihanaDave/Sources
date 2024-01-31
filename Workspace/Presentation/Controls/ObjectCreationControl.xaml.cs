using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.Graph;
using GPAS.Workspace.Presentation.Windows.DataImport;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// منطق تعامل برای ObjectCreationControl.xaml
    /// </summary>
    public partial class ObjectCreationControl
    {
        private readonly string[] supportedUnstructuredFileExtensions = OntologyProvider.GetOntology().GetDocumentSubTypeURIs();

        public bool CanCreateObject
        {
            get { return (bool)GetValue(CanCreateObjectProperty); }
            set { SetValue(CanCreateObjectProperty, value); }
        }
                
        public static readonly DependencyProperty CanCreateObjectProperty =
            DependencyProperty.Register(nameof(CanCreateObject), typeof(bool), typeof(ObjectCreationControl), 
                new PropertyMetadata(false));
               
        #region مدیریت رخداد

        /// <summary>
        /// کلاس آرگومان های مورد نیاز جهت فراخوانی رخداد «صدور درخواست ایجاد شی»
        /// </summary>
        public class ObjectCreationRequestSubmitedEventAgrs : EventArgs
        {
            public ObjectCreationRequestSubmitedEventAgrs(string DisplayName, Ontology.OntologyNode Type)
            {
                if (DisplayName == null)
                    throw new ArgumentNullException("DisplayName");
                if (Type == null)
                    throw new ArgumentNullException("Type");
                displayName = DisplayName;
                type = Type;
            }

            private string displayName;
            public string DisplayName
            { get { return displayName; } }

            private Ontology.OntologyNode type;
            public Ontology.OntologyNode Type
            { get { return type; } }
        }       

        /// <summary>
        /// رخداد «صدور درخواست ایجاد شی» 
        /// </summary>
        public event EventHandler<ObjectCreationRequestSubmitedEventAgrs> ObjectCreationRequestSubmited;

        /// <summary>
        /// عملگر صدور رخداد صدور درخواست ایجاد شی
        /// </summary>
        protected virtual void OnObjectCreationRequestSubmited()
        {
            if (ObjectCreationRequestSubmited != null)
            {
                ObjectCreationRequestSubmited(this, new ObjectCreationRequestSubmitedEventAgrs(txtNewObjectDisplayName.Text, MainObjectTypePicker.SelectedItem));
                txtNewObjectDisplayName.Focus();
                txtNewObjectDisplayName.SelectAll();
                if (string.IsNullOrWhiteSpace(txtNewObjectDisplayName.Text))
                {
                    CanCreateObject = false;
                }
                else
                {
                    CanCreateObject = true;
                }
            }
        }

        public event EventHandler<DocumentCreationRequestSubmitedEventAgrs> DocumentCreationRequestSubmited;
       
        protected virtual void OnDocumentCreationRequestSubmited(string filePath)
        {
            DocumentCreationRequestSubmited?.Invoke(this, new DocumentCreationRequestSubmitedEventAgrs(filePath));
        }

        public event EventHandler<EventArgs> OpenPopupRequest;

        protected virtual void OnOpenPopupRequest()
        {
            OpenPopupRequest?.Invoke(this, new EventArgs());
        }

        public class DocumentsImportedEventArgs
        {
            public DocumentsImportedEventArgs(List<KWObject> generatedDocuments)
            {
                if (generatedDocuments == null)
                    throw new ArgumentNullException("generatedDocuments");

                GeneratedDocuments = generatedDocuments;

            }

            public List<KWObject> GeneratedDocuments
            {
                get;
                private set;
            }
        }
        public event EventHandler<DocumentsImportedEventArgs> DocumentsImported;

        protected virtual void OnDocumentsImported(List<KWObject> generatedDocuments)
        {
            if (generatedDocuments == null)
                throw new ArgumentNullException("generatedDocuments");

            DocumentsImported?.Invoke(this, new DocumentsImportedEventArgs(generatedDocuments));
        }

        #endregion

        /// <summary>
        /// سازنده کنترل
        /// </summary>
        public ObjectCreationControl()
        {
            // آماده سازی اولیه اجزا کنترل
            InitializeComponent();
            DataContext = this;
        }

        private string filePath;

        /// <summary>
        /// رخدادگردان کلیک روی کلید «ایجاد»
        /// </summary>
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (MainObjectTypePicker.SelectedItem?.TypeUri == OntologyProvider.GetOntology().GetDocumentTypeURI())
            {
                OnDocumentCreationRequestSubmited(filePath);

            }
            else
            {
                if (IsControlEnteredValuesValid())
                    OnObjectCreationRequestSubmited();
            }           
        } 

        /// <summary>
        /// بررسی اینکه آیا مقادیر وارد شده توسط کاربر برای ساخت یک شی معتبر (و کافی) ند یا خیر
        /// </summary>
        protected bool IsControlEnteredValuesValid()
        {
            // می بایست متنی به عنوان نام نمایشی شی وارد و نیز یک گره نوع هستان شناسی انتخاب شده باشد
            return
                (!string.IsNullOrWhiteSpace(txtNewObjectDisplayName.Text) && MainObjectTypePicker.SelectedItem != null);
        }

        /// <summary>
        /// رخدادگردان تغییر نوع شی انتخاب شده در کنترل سلسله مراتب نوع شی
        /// </summary>
        private void MainObjectTypePicker_SelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MainObjectTypePicker.SelectedItem?.TypeUri == OntologyProvider.GetOntology().GetDocumentTypeURI())
            {
                txtNewObjectDisplayName.Visibility = Visibility.Collapsed;
                DocumentGrid.Visibility = Visibility.Visible;
                CanCreateObject = false;
                FilePathTextBox.Text = string.Empty;
            }
            else
            {
                txtNewObjectDisplayName.Visibility = Visibility.Visible;
                DocumentGrid.Visibility = Visibility.Collapsed;
                ApplyValidationToControl();
                txtNewObjectDisplayName.SelectAll();
            }
        }

        /// <summary>
        /// رخدادگردان تغییر متن جعبه متن «نام نمایشی»
        /// </summary>
        private void txtNewObjectDisplayName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // اعمال اعتبار مقادیر در حال انتخاب به اجزای کنترل
            ApplyValidationToControl();
        }

        /// <summary>
        /// اجزای کنترل را براساس اعتبار مقادیر وارد شده توسط کاربر تنظیم می کند
        /// </summary>
        private void ApplyValidationToControl()
        {
            // در صورت قابل قبول بودن مقادیر وارد شده در اجزای کنترل، کلید «ایجاد» فعال می شود و در غیر اینصورت غیرفعال خواهد شد
            CanCreateObject = IsControlEnteredValuesValid();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog();
        }

        private void OpenFileDialog()
        {
            StringBuilder allSupportedFilesExtension = new StringBuilder(string.Empty);
            StringBuilder addFileDialogFilter = new StringBuilder(string.Empty);

            foreach (string extension in supportedUnstructuredFileExtensions.Distinct())
            {
                addFileDialogFilter.Append(string.Format("|{0} files (*.{1}) | *.{1};", extension.ToUpper(), extension.ToLower()));
                allSupportedFilesExtension.Append($" *.{extension.ToLower()};");
            }

            string filter = $"All supported files |{allSupportedFilesExtension}" + addFileDialogFilter;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = filter
            };

            bool? result = openFileDialog.ShowDialog();
            if (result != true)
                return;

            PrepareCreateDocument(openFileDialog.FileName);
        }

        private void PrepareCreateDocument(string selectedFilePath)
        {
            FilePathTextBox.Text = Path.GetFileName(selectedFilePath);
            filePath = selectedFilePath;
            CanCreateObject = true;
            OnOpenPopupRequest();
        }

        public void Reset()
        {
            MainObjectTypePicker.RemoveSelectedItem();
            txtNewObjectDisplayName.Text = string.Empty;
        }
    }
}
