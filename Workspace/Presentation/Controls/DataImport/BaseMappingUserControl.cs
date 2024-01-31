using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    public class BaseMappingUserControl : UserControl
    {
        private readonly string savedMapPath = string.Format(Properties.Settings.Default.SavedMapFolder, System.Windows.Forms.Application.StartupPath);

        protected void SaveMap()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                CheckPathExists = true,
                OverwritePrompt = true,
                Filter = "Import Mapping files (*.imm)|*.imm"
            };

            bool? saveResult = saveFileDialog.ShowDialog();
            if (saveResult != true)
                return;

            try
            {
                ((MappingViewModel)DataContext).SaveMap(saveFileDialog.FileName);

                if (!Directory.Exists(savedMapPath))
                    Directory.CreateDirectory(savedMapPath);

                Utility.Utility.CreateShortcut(saveFileDialog.FileName,
                        Path.Combine(savedMapPath, Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + ".lnk"),
                        null, null);

                ((MappingViewModel)DataContext).FillRecentMapCollection();

                KWMessageBox.Show(Properties.Resources.Save_successfull, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Save_Import_Mapping, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void LoadMap()
        {
            OpenFileDialog addFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Import mapping files (*.imm)|*.imm"
            };

            bool? result = addFileDialog.ShowDialog();

            if (result == true)
            {
                LoadMap(addFileDialog.FileName);
            }
        }

        protected void LoadFromRecentMap(SavedMapModel savedMap)
        {
            if (File.Exists(savedMap.TargetPath))
            {
                LoadMap(savedMap.TargetPath);
            }
            else
            {
                MessageBoxResult result = KWMessageBox.Show(string.Format(Properties.Resources.String_ShortcutDoseNotExist,
                    Path.GetFileName(savedMap.TargetPath)),
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        ((MappingViewModel)DataContext).DeleteSavedMap(savedMap.ShortcutPath);
                    }
                    catch (Exception ex)
                    {
                        KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        protected void DeleteFromRecentMap(SavedMapModel savedMap)
        {
            MessageBoxResult result = KWMessageBox.Show(Properties.Resources.Do_You_Want_To_Delete_File,
            MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ((MappingViewModel)DataContext).DeleteSavedMap(savedMap.ShortcutPath);
                }
                catch (Exception ex)
                {
                    KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected void LoadMap(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            if (((MappingViewModel)DataContext).Map.ObjectCollection.Any())
            {
                MessageBoxResult result = KWMessageBox.Show
                (Properties.Resources.Load_a_mapping_will_remove_currently_defined_mapping
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Warning
                    , MessageBoxResult.No);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                ((MappingViewModel)DataContext).LoadMap(filePath);

                if (!Directory.Exists(savedMapPath))
                    Directory.CreateDirectory(savedMapPath);

                Utility.Utility.CreateShortcut(filePath, Path.Combine(savedMapPath, Path.GetFileNameWithoutExtension(filePath) + ".lnk"),
                        null, null);

                ((MappingViewModel)DataContext).FillRecentMapCollection();
            }
            catch (Exception ex)
            {
                string message;

                if (ex.InnerException != null && ex.InnerException.Source == "System.Xml")
                {
                    message = Properties.Resources
                        .Unable_to_load_saved_mapping_because_selected_datasource_are_not_match_with_saved_mapping;
                }
                else
                {
                    message = ex.Message;
                }

                KWMessageBox.Show(message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
