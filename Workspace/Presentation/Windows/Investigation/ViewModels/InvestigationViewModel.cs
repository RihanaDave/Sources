using GPAS.Logger;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Windows.Investigation.Enums;
using GPAS.Workspace.Presentation.Windows.Investigation.Models;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using GPAS.Utility;

namespace GPAS.Workspace.Presentation.Windows.Investigation.ViewModels
{
    public class InvestigationViewModel : BaseViewModel
    {
        #region Variables

        InvestigationProvider InvestigationProvider = new InvestigationProvider();

        #endregion

        #region Properties

        public ObservableCollection<InvestigationModel> Items { get; set; } =
           new ObservableCollection<InvestigationModel>();

        private ShowInvestigationPopupEnum selectedShowPopup;
        public ShowInvestigationPopupEnum SelectedShowPopup
        {
            get => selectedShowPopup;
            set
            {
                SetValue(ref selectedShowPopup, value);
            }
        }

        private InvestigationModel selectedItem;
        public InvestigationModel SelectedItem
        {
            get => selectedItem;
            set
            {
                SetValue(ref selectedItem, value);
            }
        }

        public InvestigationModel InvestigationForSave { get; set; } = new InvestigationModel();

        #endregion

        #region Function

        public InvestigationViewModel(BitmapImage graphPreview, InvestigationStatus investigationStatus)
        {
            InvestigationForSave.GraphImage = graphPreview;
            InvestigationForSave.Status = investigationStatus;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Items);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("GroupName");
            view.GroupDescriptions.Add(groupDescription);
        }

        public void SaveCurrentInvestigation()
        {
            _ = InvestigationProvider.SaveInvestigation(InvestigationForSave.Title, InvestigationForSave.Description, InvestigationForSave.Status, ImageToByteArray(InvestigationForSave.GraphImage));
        }

        public static byte[] ImageToByteArray(BitmapImage img)
        {
            byte[] data;
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }

        public static BitmapImage ByteArrayToImage(byte[] data)
        {

            BitmapImage image = new BitmapImage();
            using (var stream = new MemoryStream(data))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public ObservableCollection<InvestigationModel> LoadSavedInvestigations()
        {
            InvestigationProvider investigationProvider = new InvestigationProvider();
            List<InvestigationInfo> savedInvestigationInfos = investigationProvider.GetSavedInvestigations().Result;
            return ConvertInvestigationInfoToShowingSavedInvestigation(savedInvestigationInfos);
        }

        private ObservableCollection<InvestigationModel> ConvertInvestigationInfoToShowingSavedInvestigation(List<InvestigationInfo> savedInvestigationInfos)
        {
            savedInvestigationInfos.Reverse();
            ObservableCollection<InvestigationModel> result = new ObservableCollection<InvestigationModel>();

            InvestigationModel showingSavedInvestigation = null;
            foreach (var currentSavedInvestigationInfo in savedInvestigationInfos)
            {
                DateTime Ct = new DateTime();
                DateTime investigationCreatedTime = DateTime.Parse(currentSavedInvestigationInfo.CreatedTime, CultureInfo.InvariantCulture);
                showingSavedInvestigation = new InvestigationModel();
                showingSavedInvestigation.IDentifier = currentSavedInvestigationInfo.Id;
                showingSavedInvestigation.Title = currentSavedInvestigationInfo.Title;
                showingSavedInvestigation.Description = currentSavedInvestigationInfo.Description;

                DateTime.TryParse(currentSavedInvestigationInfo.CreatedTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out Ct);

                showingSavedInvestigation.CreatedTime = Ct;
                showingSavedInvestigation.CreatedBy = currentSavedInvestigationInfo.CreatedBy;
                showingSavedInvestigation.GraphImage = CreateImageOfSelectedGraphAsync(showingSavedInvestigation);

                result.Add(showingSavedInvestigation);
            }
            return new ObservableCollection<InvestigationModel>(result.OrderByDescending(r => r.CreatedTime));
        }

        public async Task<InvestigationStatus> GetStatus(long dentifier)
        {
            InvestigationProvider investigationProvider = new InvestigationProvider();
            byte[] savedInvestigationStatus = await investigationProvider.GetSavedInvestigationStatus(dentifier);

            StreamUtility streamUtil = new StreamUtility();
            string serializedSavedInvestigationStatusString = streamUtil.ByteArrayToStringUtf8(savedInvestigationStatus);
            Stream xmlStream = streamUtil.GenerateStreamFromString(serializedSavedInvestigationStatusString);
            InvestigationStatus status = InvestigationStatus.Deserialize(xmlStream);
            return status;
        }

        private BitmapImage CreateImageOfSelectedGraphAsync(InvestigationModel savedInvestigatioin)
        {
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                if (savedInvestigatioin == null)
                    throw new ArgumentNullException(nameof(savedInvestigatioin));

                InvestigationProvider investigationProvider = new InvestigationProvider();
                byte[] graphBytes = investigationProvider.GetSavedInvestigationImage(savedInvestigatioin.IDentifier).Result;
                bitmapImage = ByteArrayToImage(graphBytes);
                return bitmapImage;
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                MessageBox.Show(string.Format("{0}\n\n{1}", Properties.Resources.Unable_To_Load_Published_Graphs_Inforamtion, ex.Message));
            }
            return bitmapImage;
        }

        #endregion
    }
}
