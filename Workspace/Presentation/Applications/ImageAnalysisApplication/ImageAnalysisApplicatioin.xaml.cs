using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.ImageProcessing;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.ImageProcessing;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Applications
{
    public partial class ImageAnalysisApplicatioin : INotifyPropertyChanged, IObjectsShowableListener
    {
        #region متغیرهای سراسری        
        string imageFileName = string.Empty;
        Dictionary<long, string> objectToFileLocationMapping = new Dictionary<long, string>();
        ObservableCollection<ShowingObject> showingObjects = new ObservableCollection<ShowingObject>();
        private ImageAnalysisStep currentStep;
        public ImageAnalysisStep CurrentStep
        {
            get { return this.currentStep; }
            set
            {
                if (value == ImageAnalysisStep.FaceExtraction)
                {
                    ChangeSearchButtonStatus(value);
                }

                if (this.currentStep != value)
                {
                    this.currentStep = value;
                    this.NotifyPropertyChanged("CurrentStep");
                }
            }
        }
        public ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(nameof(CurrentTheme), typeof(ThemeApplication), typeof(ImageAnalysisApplicatioin),
                new PropertyMetadata(ThemeApplication.Dark));

        protected MainWindow MainWindow
        {
            get { return (MainWindow)GetValue(MainWindowProperty); }
            set { SetValue(MainWindowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainWindow.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty MainWindowProperty =
            DependencyProperty.Register(nameof(MainWindow), typeof(MainWindow), typeof(ImageAnalysisApplicatioin), new PropertyMetadata(null));

        #endregion

        #region رخدادها

        public class ShowOnGraphRequestedEventArgs
        {
            public ShowOnGraphRequestedEventArgs(KWObject objectRequestedToShowOnGraph)
            {
                ObjectRequestedToShowOnGraph = objectRequestedToShowOnGraph;
            }
            public KWObject ObjectRequestedToShowOnGraph
            {
                get;
                private set;
            }
        }
        public event EventHandler<ShowOnGraphRequestedEventArgs> ShowOnGraphRequested;
        protected void OnShowOnGraphRequested(KWObject objectRequestedToShowOnGraph)
        {
            if (objectRequestedToShowOnGraph == null)
                throw new ArgumentNullException("objectRequestedToShowOnMap");

            if (ShowOnGraphRequested != null)
            {
                ShowOnGraphRequested(this, new ShowOnGraphRequestedEventArgs(objectRequestedToShowOnGraph));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region توابع
        public ImageAnalysisApplicatioin()
        {
            InitializeComponent();
            Init();
            Loaded += ImageAnalysisApplicatioin_Loaded;
        }

        private void ImageAnalysisApplicatioin_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(this);
            MainWindow = w as MainWindow;

            if (MainWindow == null)
                return;

            MainWindow.CurrentThemeChanged -= MainWindow_CurrentThemeChanged;
            MainWindow.CurrentThemeChanged += MainWindow_CurrentThemeChanged;
        }

        private void MainWindow_CurrentThemeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CurrentTheme = MainWindow.CurrentTheme == ThemeApplication.Dark ? ThemeApplication.Dark : ThemeApplication.Light;
        }
        public void Init()
        {
            DataContext = this;
            CurrentStep = ImageAnalysisStep.RawImageLoading;
        }

        private BitmapSource GetSubImageFromImage(string imageFilePath, KWBoundingBox boundingBox)
        {
            int rotate = 0;
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(imageFilePath))
            {

                foreach (System.Drawing.Imaging.PropertyItem prop in image.PropertyItems)
                {
                    if (prop.Id == 0x112)
                    {
                        if (prop.Value[0] == 6)
                            rotate = 90;
                        if (prop.Value[0] == 8)
                            rotate = -90;
                        if (prop.Value[0] == 3)
                            rotate = 180;
                    }
                }
            }

            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(imageFilePath, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            if (boundingBox.TopLeft.X < 0)
            {
                boundingBox.TopLeft = new System.Drawing.Point(0, boundingBox.TopLeft.Y);
            }
            if (boundingBox.TopLeft.Y < 0)
            {
                boundingBox.TopLeft = new System.Drawing.Point(boundingBox.TopLeft.X, 0);
            }
            if ((boundingBox.TopLeft.X + boundingBox.Width) > src.PixelWidth)
            {
                boundingBox.Width = Math.Abs(boundingBox.TopLeft.X - src.PixelWidth);
            }
            if ((boundingBox.TopLeft.Y + boundingBox.Height) > src.PixelHeight)
            {
                boundingBox.Height = Math.Abs(boundingBox.TopLeft.Y - src.PixelHeight);
            }

            TransformedBitmap result = new TransformedBitmap();

            result.BeginInit();
            result.Source = src;
            RotateTransform transform = new RotateTransform((-1) * rotate);
            result.Transform = transform;
            result.EndInit();

            return new CroppedBitmap(result,
                        new Int32Rect(boundingBox.TopLeft.X, boundingBox.TopLeft.Y, boundingBox.Width, boundingBox.Height));
        }

        public BitmapImage ConvertToBitmapImage(BitmapSource bitmapSource)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
        }

        private async Task ShowOpenFileDialog()
        {
            OpenFileDialog addFileDialog = new OpenFileDialog();
            addFileDialog.Multiselect = false;
            addFileDialog.Filter = "Image files |*.jpg;*.jpeg;*.png;*.gif;*.tif;";

            if (addFileDialog.ShowDialog().Value)
            {
                string fileName = addFileDialog.FileName;

                await ShowFaceExtractionStep(fileName);

            }
        }

        private async Task ShowFaceExtractionStep(string fileName)
        {
            ClearFaceExtractionStep();

            List<KWBoundingBox> retrievedFaces = null;
            if (!(string.IsNullOrEmpty(fileName) ||
                    string.IsNullOrWhiteSpace(fileName)))
            {

                TransformedBitmap transformedBitmap = CorrectImageRotation(fileName);

                imageFileName = fileName;
                try
                {
                    WaitingControl.Message = Properties.Resources.Extracting_Faces;
                    WaitingControl.TaskIncrement();
                    ImageProcessingProvider imgProcessingProvider = new ImageProcessingProvider();
                    byte[] imageFile = File.ReadAllBytes(fileName);
                    retrievedFaces = await imgProcessingProvider.FaceDetection(imageFile, string.Empty);
                }
                catch (Exception ex)
                {
                    ExceptionHandler exceptionHandler = new ExceptionHandler();
                    exceptionHandler.WriteErrorLog(ex);
                    KWMessageBox.Show(string.Format("{0}\n\n{1}", Properties.Resources.Invalid_Server_Response, ex.Message),
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                finally
                {
                    WaitingControl.TaskDecrement();
                }

                loadedImage.Source = transformedBitmap;

                if (retrievedFaces != null)
                {
                    CurrentStep = ImageAnalysisStep.FaceExtraction;
                    ShowKWBoundingBoxs(retrievedFaces, loadedImage, loadedImageCanvas);
                }
            }
        }

        public const int OrientationId = 0x0112;

        private TransformedBitmap CorrectImageRotation(string fileName)
        {
            int rotate = 0;
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(fileName))
            {
                foreach (System.Drawing.Imaging.PropertyItem prop in image.PropertyItems)
                {
                    if (prop.Id == OrientationId)
                    {
                        if (prop.Value[0] == 6)
                            rotate = 90;
                        if (prop.Value[0] == 8)
                            rotate = -90;
                        if (prop.Value[0] == 3)
                            rotate = 180;
                    }
                }
            }

            BitmapSource imageSource = new BitmapImage(new Uri(fileName));
            TransformedBitmap result = new TransformedBitmap();

            result.BeginInit();
            result.Source = imageSource;
            RotateTransform transform = new RotateTransform((-1) * rotate);
            result.Transform = transform;
            result.EndInit();

            return result;
        }

        private void ClearBoundingBoxes(Canvas canvas)
        {
            canvas.Children.Clear();
        }

        private void ClearFaceExtractionStep()
        {
            loadedImage.Source = null;
            ClearBoundingBoxes(loadedImageCanvas);
        }

        private void SelectBox(BoundingBoxControl box, Canvas canvas)
        {

            foreach (System.Object b in canvas.Children)
            {
                if (b is BoundingBoxControl)
                {
                    BoundingBoxControl bb = b as BoundingBoxControl;
                    if (bb != box)
                    {
                        bb.Box.DeSelect();
                    }
                }
            }

            box.Box.Select();
        }

        private ObservableCollection<ShowingObject> ConvertRetrievedFaceKWObjectToShowingObject(List<RetrievedFaceKWObject> searchResults)
        {
            ObservableCollection<ShowingObject> result = new ObservableCollection<ShowingObject>();
            foreach (RetrievedFaceKWObject currentRetrievedFace in searchResults)
            {
                string imagePath = objectToFileLocationMapping[currentRetrievedFace.kwObject.ID];
                result.Add(new ShowingObject()
                {
                    Description = currentRetrievedFace.kwObject.GetObjectLabel(),
                    Distance = currentRetrievedFace.distance,
                    IsSelected = false,
                    RelatedObject = currentRetrievedFace.kwObject,
                    Icon = GetSubImageFromImage(imagePath, currentRetrievedFace.boundingBox.ElementAt(0)),
                    RelatedKWBoundingBox = currentRetrievedFace.boundingBox
                });
            }
            return result;
        }

        internal void RefreshViewToFaceExtractionStep()
        {
            ClearFaceExtractionStep();
            CurrentStep = ImageAnalysisStep.FaceExtraction;
        }

        private async Task ShowPreviewResultStep(List<RetrievedFaceKWObject> searchResults,
            KWBoundingBox[] selectedBoundingBoxes)
        {
            if (searchResults == null)
            {
                return;
            }

            await GenerateObjectToFileLocationMapping(searchResults);

            ClearPreviewResultStep();
            CurrentStep = ImageAnalysisStep.ResultPreview;

            if (!(string.IsNullOrEmpty(imageFileName) || string.IsNullOrWhiteSpace(imageFileName)) &&
                selectedBoundingBoxes.Count() != 0)
            {
                selectedFaceImage.Source = GetSubImageFromImage(imageFileName, selectedBoundingBoxes.First());
            }

            if (loadedImage.Source != null)
            {
                previewImage.Source = loadedImage.Source;
            }

            if (searchResults == null)
            {
                NothingToPreviewPromptLabelForSearchResults.Visibility = Visibility.Visible;
                NothingToPreviewPromptLabel.Visibility = Visibility.Visible;
            }
            else
            {
                NothingToPreviewPromptLabelForSearchResults.Visibility = Visibility.Collapsed;
                NothingToPreviewPromptLabel.Visibility = Visibility.Collapsed;

                showingObjects = ConvertRetrievedFaceKWObjectToShowingObject(searchResults);
                searchResultsItemControl.ItemsSource = showingObjects;
            }

            searchResultScrollViewer.ScrollToVerticalOffset(0);
        }

        private async Task GenerateObjectToFileLocationMapping(List<RetrievedFaceKWObject> searchResults)
        {
            WaitingControl.Message = Properties.Resources.Downloading;
            WaitingControl.TaskIncrement();
            foreach (RetrievedFaceKWObject currentObject in searchResults)
            {
                string imageFilePath = await DataSourceProvider.DownloadDocumentAsync(currentObject.kwObject);
                if (objectToFileLocationMapping.ContainsKey(currentObject.kwObject.ID))
                {
                    objectToFileLocationMapping[currentObject.kwObject.ID] = imageFilePath;
                }
                else
                {
                    objectToFileLocationMapping.Add(currentObject.kwObject.ID, imageFilePath);
                }
            }
            WaitingControl.TaskDecrement();
        }

        private void ClearPreviewResultStep()
        {
            selectedFaceImage.Source = null;
            previewImage.Source = null;
            retrievedImage.Source = null;
            retrievedBorder.Visibility = Visibility.Collapsed;
            showingObjects.Clear();
            ClearBoundingBoxes(retrievedImageCanvas);
        }

        private void ShowRetrievedImage(ShowingObject selectedShowingObject)
        {
            try
            {
                retrievedImage.Source = null;
                NothingToPreviewPromptLabel.Visibility = Visibility.Collapsed;
                retrievedBorder.Visibility = Visibility.Visible;
                string localPath = objectToFileLocationMapping[selectedShowingObject.RelatedObject.ID];

                TransformedBitmap transformedBitmap = CorrectImageRotation(localPath);

                retrievedImage.Source = transformedBitmap;
                ClearBoundingBoxes(retrievedImageCanvas);
                ShowKWBoundingBoxs(selectedShowingObject.RelatedKWBoundingBox, retrievedImage, retrievedImageCanvas);
            }
            catch (FileNotFoundException)
            {
                NothingToPreviewPromptLabel.Visibility = Visibility.Visible;
                retrievedBorder.Visibility = Visibility.Collapsed;
                KWMessageBox.Show(Properties.Resources.There_is_no_file_for_the_document, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                NothingToPreviewPromptLabel.Visibility = Visibility.Visible;
                retrievedBorder.Visibility = Visibility.Collapsed;
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowKWBoundingBoxs(List<KWBoundingBox> boundingBoxs, Image image, Canvas canvas)
        {
            BoundingBoxControl[] Boxes = new BoundingBoxControl[boundingBoxs.Count];

            BitmapSource source = image.Source as BitmapSource;
            Size ScaleSizeBox = new Size(image.ActualWidth / source.PixelWidth, image.ActualHeight / source.PixelHeight);

            int i = 0;
            foreach (KWBoundingBox box in boundingBoxs)
            {
                BoundingBoxControl curBox = new BoundingBoxControl();
                curBox.MouseLeftButtonDown += CurBox_MouseLeftButtonDown;
                curBox.MouseDoubleClick += CurBox_MouseDoubleClick;
                curBox.Box = box;
                curBox.Caption = box.Caption;
                curBox.ScaleSize = ScaleSizeBox;

                SetBoxLocation(curBox);

                canvas.Children.Add(curBox);

                Boxes[i++] = curBox;
            }
        }

        public async Task ShowObjectsAsync(IEnumerable<KWObject> objectsToShow)
        {
            RefreshViewToFaceExtractionStep();
            await GenerateObjectToFileLocationMapping(objectsToShow);

            if (objectsToShow.Count() == 1)
            {
                if (objectToFileLocationMapping.ContainsKey(objectsToShow.FirstOrDefault().ID))
                {
                    await ShowFaceExtractionStep(objectToFileLocationMapping[objectsToShow.FirstOrDefault().ID]);
                }
            }
        }

        private async Task GenerateObjectToFileLocationMapping(IEnumerable<KWObject> objectsToShow)
        {
            WaitingControl.Message = Properties.Resources.Downloading;
            foreach (KWObject currentObject in objectsToShow)
            {
                string imageFilePath = await DataSourceProvider.DownloadDocumentAsync(currentObject);
                if (objectToFileLocationMapping.ContainsKey(currentObject.ID))
                {
                    objectToFileLocationMapping[currentObject.ID] = imageFilePath;
                }
                else
                {
                    objectToFileLocationMapping.Add(currentObject.ID, imageFilePath);
                }
            }
            WaitingControl.TaskDecrement();
        }


        #endregion

        #region رخدادگردان‌ها
        private async void uploadImageGrid_Drop(object sender, DragEventArgs e)
        {
            string[] dropedFileLocations = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (dropedFileLocations.Count() > 0)
            {
                await ShowFaceExtractionStep(dropedFileLocations.Last());
            }
        }

        private async void chooseImageFileButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowOpenFileDialog();
        }
        private void loadedImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeScaleBoundingBoxes(sender as Image, loadedImageCanvas);
        }

        private void ChangeScaleBoundingBoxes(Image image, Canvas canvas)
        {
            BitmapSource source = (image.Source as BitmapSource);
            if (source == null)
            {
                return;
            }
            Size ScaleSizeBox = new Size(image.ActualWidth / source.PixelWidth, image.ActualHeight / source.PixelHeight);

            List<BoundingBoxControl> Boxes = GetBoundingBoxesFromCanvasChildren(canvas);

            foreach (BoundingBoxControl box in Boxes)
            {
                box.ScaleSize = ScaleSizeBox;
                SetBoxLocation(box);
            }
        }

        private void SetBoxLocation(BoundingBoxControl box)
        {
            //double left = box.Box.TopLeft.X * box.ScaleSize.Width;
            //double top = box.Box.TopLeft.Y * box.ScaleSize.Height;

            //box.Margin = new Thickness(left, top, 0, 0);
        }

        private List<BoundingBoxControl> GetBoundingBoxesFromCanvasChildren(Canvas canvas)
        {
            List<BoundingBoxControl> Boxes = new List<Controls.ImageProcessing.BoundingBoxControl>(canvas.Children.Count);

            foreach (System.Object child in canvas.Children)
            {
                if (child is BoundingBoxControl)
                {
                    Boxes.Add(child as BoundingBoxControl);
                }
            }

            return Boxes;
        }

        private void CurBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BoundingBoxControl box = sender as BoundingBoxControl;
            SelectBox(box, loadedImageCanvas);
            ChangeSearchButtonStatus(CurrentStep);
        }

        private void ChangeSearchButtonStatus(ImageAnalysisStep step)
        {
            if (step == ImageAnalysisStep.FaceExtraction || step == ImageAnalysisStep.RawImageLoading)
            {
                List<BoundingBoxControl> Boxes = GetBoundingBoxesFromCanvasChildren(loadedImageCanvas);
                KWBoundingBox[] selectedBoundingBoxes = (from b in Boxes where b.Box.IsSelected select b.Box).ToArray();
                if (selectedBoundingBoxes.Count() > 0)
                {
                    BtnSearch.IsEnabled = true;
                }
                else
                {
                    BtnSearch.IsEnabled = false;
                }
            }

        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            await SearchSelectedFace();
        }

        private async Task SearchSelectedFace()
        {
            List<BoundingBoxControl> Boxes = GetBoundingBoxesFromCanvasChildren(loadedImageCanvas);
            KWBoundingBox[] selectedBoundingBoxes = (from b in Boxes where b.Box.IsSelected select b.Box).ToArray();

            List<RetrievedFaceKWObject> searchResults = null;
            try
            {
                WaitingControl.Message = Properties.Resources.Extracting_Faces;
                WaitingControl.TaskIncrement();
                ImageProcessingProvider imgProcessingProvider = new ImageProcessingProvider();

                searchResults = await imgProcessingProvider.FaceRecognition(File.ReadAllBytes(imageFileName)
                    , string.Empty,
                    selectedBoundingBoxes, int.Parse(((System.Windows.Controls.ComboBoxItem)ResultCount.SelectedItem)?.Content.ToString()));
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(string.Format("{0}\n\n{1}", Properties.Resources.Invalid_Server_Response, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                WaitingControl.TaskDecrement();
            }

            await ShowPreviewResultStep(searchResults, selectedBoundingBoxes);
        }

        private void DeselectAnotherShowingObjects()
        {
            foreach (ShowingObject currentShowingObject in showingObjects)
            {
                currentShowingObject.IsSelected = false;
            }
        }

        private async void uploadImageIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await ShowOpenFileDialog();
        }


        private void previewImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ClearPreviewResultStep();
                CurrentStep = ImageAnalysisStep.FaceExtraction;
                //ChangeSearchButtonStatus();
            }
        }

        private void BtnShowOnGraph_Click(object sender, RoutedEventArgs e)
        {
            KWObject selectedObject = showingObjects.Where(so => so.IsSelected).Select(o => o.RelatedObject).FirstOrDefault();
            if (selectedObject != null)
            {
                OnShowOnGraphRequested(selectedObject);
            }
        }

        private void ShowingObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowingObject selectedShowingObject = ((sender as Border).DataContext as ShowingObject);
            DeselectAnotherShowingObjects();
            selectedShowingObject.IsSelected = true;

            ShowRetrievedImage(selectedShowingObject);
        }
        private void retrievedImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeScaleBoundingBoxes(sender as Image, retrievedImageCanvas);
        }

        private async void CurBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CurrentStep == ImageAnalysisStep.FaceExtraction)
            {
                await SearchSelectedFace();
            }
            else if (CurrentStep == ImageAnalysisStep.ResultPreview)
            {
                KWObject selectedObject = showingObjects.Where(so => so.IsSelected).Select(o => o.RelatedObject).FirstOrDefault();
                if (selectedObject != null)
                {
                    OnShowOnGraphRequested(selectedObject);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentStep = ImageAnalysisStep.RawImageLoading;
        }
        private void PreviewImgCloseIconButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFaceExtractionStep();
            CurrentStep = ImageAnalysisStep.RawImageLoading;
        }

        public override void Reset()
        {
            ClearFaceExtractionStep();
            CurrentStep = ImageAnalysisStep.RawImageLoading;
            ResultCount.SelectedIndex = 0;
        }

        #endregion
    }
}
