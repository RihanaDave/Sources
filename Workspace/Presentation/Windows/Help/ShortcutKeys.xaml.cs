using System;
using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Windows.Help
{
    /// <summary>
    /// Interaction logic for ShortcutKeys.xaml
    /// </summary>
    public partial class ShortcutKeys
    {
        ResourceManager PresentationResources = new ResourceManager(typeof(Properties.Resources));
        ObservableCollection<ShortcutKeyDescriptor> showingDescriptors;

        public ShortcutKeys()
        {
            InitializeComponent();
        }

        string generalShorkcutKeyCategory = "General_shortcutkey_category";
        string[] nonGeneralShorkcutKeyCategories = new string[] { "Browser_Application", "Graph_Application" };

        private void PresentationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            showingDescriptors = new ObservableCollection<ShortcutKeyDescriptor>();
            var groupedShowingDescriptors = new ListCollectionView(showingDescriptors);
            groupedShowingDescriptors.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            MainDataGrid.ItemsSource = groupedShowingDescriptors;

            foreach (var item in EnumUtitlities.GetEnumElements<SupportedShortCutKey>())
            {
                var shortcutKeyTitle = PresentationResources.GetString("ShortcutKey_" + item.ToString() + "_Title");
                if (string.IsNullOrWhiteSpace(shortcutKeyTitle))
                    continue;

                var shortcutKeyGeneralOperation = PresentationResources.GetString("ShortcutKey_" + item.ToString() + "_Operation");
                AddDescriptorIfOperationDefinedForCategory(shortcutKeyTitle, generalShorkcutKeyCategory, shortcutKeyGeneralOperation);

                foreach (var category in nonGeneralShorkcutKeyCategories)
                {
                    var shortcutKeyOperationOnCategory = PresentationResources.GetString("ShortcutKey_" + item.ToString() + "_Operation_on_" + category);
                    AddDescriptorIfOperationDefinedForCategory(shortcutKeyTitle, category, shortcutKeyOperationOnCategory);
                }
            }
        }

        private void AddDescriptorIfOperationDefinedForCategory(string shortcutKeyTitle, string category, string shortcutKeyOperationForCategory)
        {
            if (!string.IsNullOrWhiteSpace(shortcutKeyOperationForCategory))
            {
                var descriptor = new ShortcutKeyDescriptor()
                {
                    Title = shortcutKeyTitle,
                    Category = PresentationResources.GetString(category),
                    Description = shortcutKeyOperationForCategory
                };
                showingDescriptors.Add(descriptor);
            }
        }

        private void MainDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // امکان انتخاب از جدول داده‌ها برچیده شده
            MainDataGrid.SelectedIndex = -1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ShortcutKeyDescriptor
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }
}
