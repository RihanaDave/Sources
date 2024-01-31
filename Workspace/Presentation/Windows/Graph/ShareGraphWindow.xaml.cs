using GPAS.AccessControl;
using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    public partial class ShareGraphWindow : Window
    {
        private string graphTitle;
        private string graphDescription = string.Empty;
        private GraphArrangment graphArrangment;
        private Tuple<long[], long[]> subsetConceptsToShow = null;

        public ShareGraphWindow(GraphArrangment arrangment)
        {
            InitializeComponent();
            Init(arrangment);
        }

        private void Init(GraphArrangment arrangment)
        {
            graphArrangment = arrangment;

            ACLModel acl = LoadAcl();
            ACLViewModel aclViewModel = new ACLViewModel(acl);
            aclViewModel.AclChanged += AclViewModel_AclChanged;
            SetPermissionUserControl.DataContext = aclViewModel;
        }

        private async void AclViewModel_AclChanged(object sender, EventArgs e)
        {
            UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
            Tuple<long[], long[]> readableSubsetConcepts = userAccountControlProvider.GetReadableSubsetOfConcepts(
                graphArrangment.Objects.Select(o => o.NotResolvedObjectId).ToArray(),
                graphArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Select(r => r.RelationshipId).ToArray(),
                ((ACLViewModel)SetPermissionUserControl.DataContext).Acl.Permissions.Select(g => g.GroupName).ToArray());

            if (SubsetConceptsHasChanged(readableSubsetConcepts))
                await ShowGraph(readableSubsetConcepts);
        }

        private bool SubsetConceptsHasChanged(Tuple<long[], long[]> readableSubsetConcepts)
        {
            if (subsetConceptsToShow == null && readableSubsetConcepts == null)
                return false;

            if (subsetConceptsToShow == null || readableSubsetConcepts == null)
                return true;

            if (subsetConceptsToShow == readableSubsetConcepts ||
                (subsetConceptsToShow.Item1 == readableSubsetConcepts.Item1 && subsetConceptsToShow.Item2 == readableSubsetConcepts.Item2))
                return false;

            if (!EqualArrays(subsetConceptsToShow.Item1, readableSubsetConcepts.Item1))
                return true;

            if (!EqualArrays(subsetConceptsToShow.Item2, readableSubsetConcepts.Item2))
                return true;

            return false;
        }

        private bool EqualArrays(long[] source, long[] target)
        {
            if (source.Length != target.Length)
                return false;

            List<long> orderedSourceArray = source.OrderBy(o => o).ToList();
            List<long> orderedTargetArray = target.OrderBy(o => o).ToList();

            for (int i = 0; i < source.Length; i++)
            {
                if (orderedSourceArray[i] != orderedTargetArray[i])
                    return false;
            }

            return true;
        }

        private ACLModel LoadAcl()
        {
            AccessControl.ACL oldAcl = UserAccountControlProvider.PublishGraphACL;

            ACLModel acl = new ACLModel()
            {
                Classification = new ClassificationModel()
                {
                    Identifier = oldAcl.Classification,
                }
            };

            foreach (ACI permission in oldAcl.Permissions)
            {
                acl.Permissions.Add(new ACIModel
                {
                    GroupName = permission.GroupName,
                    AccessLevel = permission.AccessLevel
                });
            }

            return acl;
        }

        private async Task ShowGraph(Tuple<long[], long[]> readableSubsetConcepts)
        {
            subsetConceptsToShow = readableSubsetConcepts;
            if (graphControl.IsControlInitialized)
            {
                if (graphControl.graphviewerMain.IsViewerInitialized)
                {
                    await ShowGraphArrangementByRemovingNotReadableConcepts();
                }
            }
            else
            {
                graphControl.ControlInitializationCompleted += GraphControl_ControlInitializationCompleted;
            }
        }

        private async Task ShowGraphArrangementByRemovingNotReadableConcepts()
        {
            await graphControl.ShowGraphByArrangmentAsync(graphArrangment);
            List<KWObject> objectsToRemove = graphControl.GetShowingObjects().Except(await ObjectManager.GetObjectsById(subsetConceptsToShow.Item1)).ToList();
            await Task.Delay(1000);
            graphControl.RemoveObjects(objectsToRemove);
            GraphArrangment newGraphArrangment = graphControl.GetGraphArrangment();
            await graphControl.ShowGraphByArrangmentAsync(newGraphArrangment);
        }

        private async void GraphControl_ControlInitializationCompleted(object sender, EventArgs e)
        {
            if (subsetConceptsToShow != null)
            {
                await ShowGraphArrangementByRemovingNotReadableConcepts();
            }
        }

        private async void ShareGraph()
        {
            try
            {
                MainWaitingControl.Message = Properties.Resources.Sharing_Graph_;
                MainWaitingControl.TaskIncrement();

                graphDescription = DescriptionTextBox.Text;
                await GraphRepositoryManager.PublishGraphAsync(graphTitle, graphDescription, graphArrangment,
                    graphControl.RenderTargetBitmapToByteArrangement(graphControl.TakeImageOfGraph()));
                DialogResult = true;

                KWMessageBox.Show(Properties.Resources.Graph_Shared, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DialogResult = false;
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Share_Graph, ex.Message),
                     MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
                Close();
            }
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                graphControl.SelectAllObjects();
                graphControl.RelayoutSelectedVertices((GPAS.Graph.GraphViewer.LayoutAlgorithms.LayoutAlgorithmTypeEnum)(((ListViewItem)sender).Tag));
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (graphControl.graphviewerMain.IsViewerInitialized)
            {
                await graphControl.ShowGraphByArrangmentAsync(graphArrangment);
            }
        }

        private void GraphNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GraphNameTextBox.Text))
            {
                graphTitle = GraphNameTextBox.Text;
                ShareButton.IsEnabled = true;
            }
            else
            {
                ShareButton.IsEnabled = false;
            }
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            ShareGraph();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
