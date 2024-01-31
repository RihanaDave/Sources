using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Dispatch.Entities.NLP;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GPAS.Logger;
using GPAS.Workspace.Presentation.Controls.TagCloud;

namespace GPAS.Workspace.Presentation.Windows
{
    public partial class TagCloudWindow
    {
        public bool CanGenerate
        {
            get { return (bool)GetValue(CanGenerateProperty); }
            set { SetValue(CanGenerateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanGenerate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanGenerateProperty =
            DependencyProperty.Register("CanGenerate", typeof(bool), typeof(TagCloudWindow), new PropertyMetadata(false));




        private readonly string ContentDocumentPropertyName = "متن سند";
        private List<KWObject> ObjectsToShowPropertiesTagCloud { get; set; }
        private List<string> StringTypeProperties = new List<string>();
        ObservableCollection<ShowingProperty> ShowingProerties = new ObservableCollection<ShowingProperty>();
        #region رخدادها
        public class ShowOnGraphButtonClickEventArgs
        {
            public ShowOnGraphButtonClickEventArgs(List<KWObject> objectsToShowOnGraph)
            {
                if (objectsToShowOnGraph == null)
                    throw new ArgumentNullException("objectsToShowOnGraph");

                ObjectsToShowOnGraph = objectsToShowOnGraph;
            }

            public List<KWObject> ObjectsToShowOnGraph
            {
                get;
                private set;
            }
        }
        public event EventHandler<ShowOnGraphButtonClickEventArgs> ShowOnGraphButtonClicked;
        private void OnShowOnGraphButtonClicked(List<KWObject> objectsToShowOnGraph)
        {
            if (objectsToShowOnGraph == null)
                throw new ArgumentNullException("objectsToShowOnGraph");

            if (ShowOnGraphButtonClicked != null)
                ShowOnGraphButtonClicked(this, new ShowOnGraphButtonClickEventArgs(objectsToShowOnGraph));
        }
        #endregion

        public TagCloudWindow()
        {
            InitializeComponent();
            Owner = App.MainWindow;
            DataContext = this;
        }

        #region توابع
        internal void Init(List<KWObject> objectsToShowPropertiesTagCloud)
        {

            ObjectsToShowPropertiesTagCloud = objectsToShowPropertiesTagCloud;
            StringTypeProperties = OntologyProvider.GetOntology()
                .GetStringTypeProperties(ObjectsToShowPropertiesTagCloud.Select(o => o.TypeURI).ToList());
           

            var documents = ObjectsToShowPropertiesTagCloud.Where(o => OntologyProvider.GetOntology().IsTextDocument(o.TypeURI));
            if (documents.Any())
            {
                StringTypeProperties.Add(ContentDocumentPropertyName);
            }
            ShowingProerties = ConvertStringTypePropertiesToShowingProerties(StringTypeProperties);
            peopertiesItemsControl.ItemsSource = ShowingProerties;
            SetCanGenerate();
        }

        private ObservableCollection<ShowingProperty> ConvertStringTypePropertiesToShowingProerties(List<string> stringTypeProperties)
        {
            ObservableCollection<ShowingProperty> result = new ObservableCollection<ShowingProperty>();
            foreach (var currentType in stringTypeProperties)
            {
                result.Add(new ShowingProperty()
                {
                    IsSelected = true,
                    PropertyTypeFriendlyTitle = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(currentType),
                    PropertyTypeUri = currentType,
                    PropertyTypeFriendlyIcon=new BitmapImage(OntologyIconProvider.GetPropertyTypeIconPath(currentType)),
                });
            }

            return result;
        }

        private async Task ShowTagCloud(string contentToLoadTagCloud)
        {
            try
            {
                WaitingControl.Message = Properties.Resources.Loading_Tag_Cloud_;
                WaitingControl.TaskIncrement();
                NLPProvider provider = new NLPProvider();
                TagCloudKeyPhrase[] keyPhrases = await provider.GetTagCloudAsync(contentToLoadTagCloud);
                ValidateKeyPhrases(keyPhrases);
                if (keyPhrases != null)
                {
                    tagCloudControl.KeyPhrasesCollection =
                        new ObservableCollection<KeyPhraseModel>(PrepareKeyPhraseModelList(keyPhrases));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                WaitingControl.Message = string.Empty;
                WaitingControl.TaskDecrement();
            }
        }

        private KeyPhraseModel[] PrepareKeyPhraseModelList(TagCloudKeyPhrase[] tagCloudKeyPhrases)
        {
            return tagCloudKeyPhrases.Select(keyPhrase => new KeyPhraseModel
            { Key = keyPhrase.TextOfKeyPhrase, Weight = keyPhrase.Score }).ToArray();
        }

        private void ValidateKeyPhrases(TagCloudKeyPhrase[] keyPhrases)
        {
            foreach (TagCloudKeyPhrase phrase in keyPhrases)
            {
                if (string.IsNullOrWhiteSpace(phrase.TextOfKeyPhrase))
                    //throw new ArgumentException(Properties.Resources.Invalid_Key_Phrase_Title);
                    continue;
                if (phrase.Score < 0 || phrase.Score > 1)
                    throw new ArgumentOutOfRangeException(Properties.Resources.Invalid_Key_Phrase_Score);
            }
        }
        #endregion

        #region رخدادگردانها
        private void BtnShowOnGraph_Click(object sender, RoutedEventArgs e)
        {
            OnShowOnGraphButtonClicked(ObjectsToShowPropertiesTagCloud);
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string stringToLoadTagCloud = string.Empty;
                List<string> selectedStringTypeProperties = ShowingProerties.Where(p => p.IsSelected).Select(sp => sp.PropertyTypeUri).ToList();
                if (selectedStringTypeProperties.Count > 0)
                {
                    WaitingControl.Message = Properties.Resources.Loading_Properties;
                    WaitingControl.TaskIncrement();
                    List<KWProperty> objectProperties = (await Logic.PropertyManager.GetPropertiesOfObjectsAsync(ObjectsToShowPropertiesTagCloud
                    , selectedStringTypeProperties)).ToList();
                    foreach (var currentProperty in objectProperties)
                    {
                        if (string.IsNullOrWhiteSpace(currentProperty.Value))
                            continue;
                        stringToLoadTagCloud += currentProperty.Value + "\n";
                    }
                    Logic.NLPProvider nlpProvider = new NLPProvider();
                    if (selectedStringTypeProperties.Contains(ContentDocumentPropertyName))
                    {
                        foreach (var item in ObjectsToShowPropertiesTagCloud.Where(o => OntologyProvider.GetOntology().IsDocument(o.TypeURI)))
                        {
                            if (OntologyProvider.GetOntology().IsTextDocument(item.TypeURI))
                            {
                                stringToLoadTagCloud += (await nlpProvider.GetDocumentPlaneTextAsync(item.ID)) + "\n";
                            }
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(stringToLoadTagCloud))
                    {
                        // to remove newline character from end of statement"\n"
                        stringToLoadTagCloud = stringToLoadTagCloud.Substring(0, stringToLoadTagCloud.Length - 1);
                        await ShowTagCloud(stringToLoadTagCloud);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler errorLogger = new ExceptionHandler();
                errorLogger.WriteErrorLog(ex);
                throw;
            }
            finally
            {
                WaitingControl.Message = string.Empty;
                WaitingControl.TaskDecrement();
            }
        }


        private bool IsSelectedAnyProperty()
        {
            return ShowingProerties.Where(p => p.IsSelected).Count() != 0;
        }
        private void SetCanGenerate()
        {
            CanGenerate = IsSelectedAnyProperty();
        }

        private void propertySelectionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).DataContext as ShowingProperty).IsSelected = true;
            SetIsChechedForAllPropertiesItem();            
            SetCanGenerate();
        }

        private void SetIsChechedForAllPropertiesItem()
        {
            if (ShowingProerties.All(p => p.IsSelected))
            {
                CheckItem.IsChecked = true;
            }
            else if (ShowingProerties.All(p => !p.IsSelected))
            {
                CheckItem.IsChecked = false;
            }
            else
            {
                CheckItem.IsChecked = null;
            }
        }

        private void propertySelectionCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ((sender as CheckBox).DataContext as ShowingProperty).IsSelected = false;
            SetIsChechedForAllPropertiesItem();
            SetCanGenerate();
        }

        #endregion

        private void CheckItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsSelectedAnyProperty())
            {
                foreach (var currentProperty in ShowingProerties)
                {
                    currentProperty.IsSelected = true;
                }
            }
        }

        private void CheckItem_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var currentProperty in ShowingProerties)
            {
                currentProperty.IsSelected = false;
                currentProperty.IsSelected = false;
            }
        }
    }

    public class ShowingProperty : INotifyPropertyChanged
    {
        private string propertyTypeFriendlyTitle;
        public string PropertyTypeFriendlyTitle
        {
            get { return this.propertyTypeFriendlyTitle; }
            set
            {
                if (this.propertyTypeFriendlyTitle != value)
                {
                    this.propertyTypeFriendlyTitle = value;
                    this.NotifyPropertyChanged("PropertyTypeFriendlyTitle");
                }
            }
        }

        private BitmapImage propertyTypeFriendlyIcon;
        public BitmapImage PropertyTypeFriendlyIcon
        {
            get { return this.propertyTypeFriendlyIcon; }
            set
            {
                if (this.propertyTypeFriendlyIcon != value)
                {
                    this.propertyTypeFriendlyIcon = value;
                    this.NotifyPropertyChanged("propertyTypeFriendlyIcon");
                }
            }
        }

        public string PropertyTypeUri { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.NotifyPropertyChanged("IsSelected");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

}
