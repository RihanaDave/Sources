using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System.ServiceModel;
using System.Linq;
using GPAS.Workspace.ServiceAccess;

namespace GPAS.Workspace.Logic
{
    public class MediaManager
    {
        /// <summary>
        /// افزودن یک رسانه به یک شیء
        /// </summary>
        /// <param name="objectToAddMedia">شیء مورد نظر</param>
        /// <param name="description">توضیح</param>
        /// <param name="mediaDirectoryToSave">آدرس مکانی که فایل در آن ذخیره شده است.</param>
        /// <returns></returns>
        public static KWMedia AssignMediaToObject(KWObject objectToAddMedia, string description, MediaPathContent mediaDirectoryToSave)
        {
            if (objectToAddMedia == null)
                throw new ArgumentNullException("objectToAddMedia");           
            if (mediaDirectoryToSave == null)
                throw new ArgumentNullException("mediaDirectoryToSave");
            if (string.IsNullOrEmpty(mediaDirectoryToSave.UriAddress))
                throw new ArgumentNullException("mediaDirectoryToSave");
            
            if (mediaDirectoryToSave.Type == MediaPathContentType.Directory)
                throw new ArgumentException("mediaDirectoryToSave.Type");

            return DataAccessManager.MediaManager.CreateNewMediaForObject
                (mediaDirectoryToSave, description, objectToAddMedia);            
        }
        /// <summary>
        /// بازیابی تمام رسانه های مربوط به یک شیء
        /// </summary>
        /// <param name="objectToFindMedia"></param>
        /// <returns>لیستی از رسانه ها</returns>
        public async static Task<List<KWMedia>> GetMediasAssignedToObjectAsync(KWObject objectToFindMedia)
        {
            if (objectToFindMedia == null)
                throw new ArgumentNullException("objectToFindMedia");

            return (await DataAccessManager.MediaManager
                .GetMediaForObjectAsync(objectToFindMedia))
                .ToList();
        }
        /// <summary>
        /// حذف یک رسانه از یک شیء
        /// این رسانه در سرور باقی می ماند و فقط انتساب آن به شیء حذف می شود.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public static bool DeleteMediaAssignment(KWMedia media)
        {
            if (media == null)
                throw new ArgumentNullException("media");
            //if (media.ID < 0)
            //    throw new ArgumentOutOfRangeException("media");

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            DataAccessManager.MediaManager.DeleteMedia(media);
            bool result = true;
            return result;
        }

        internal static bool IsDeletedMedia(KWMedia kwMedia)
        {
           return DataAccessManager.MediaManager.IsDeletedMedia(kwMedia);
        }


        internal static async Task<IEnumerable<KWMedia>> RetriveMediaByIdAsync(KWObject kwObject)
        {
            return await DataAccessManager.MediaManager.GetMediaForObjectAsync(kwObject);
        }
    }
}