using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// متصدی نمایه (آیکن) های انواع هستان شناسی
    /// </summary>
    public class OntologyIconProvider
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این شی محلی شده است</remarks>
        private OntologyIconProvider()
        { }

        /// <summary>
        /// نام فایل فشرده بسته نمایه (آیکن) های انواع هستان شناسی
        /// </summary>
        private const string fileNameOfOntologyIconPackCompressedFile = "ontologyiconpack";
        /// <summary>
        /// مسیر موقت مورد استقاده جهت کار با آیکن‌های هستان شناسی را نگه می دارد
        /// </summary>
        protected static readonly string pathToUseForOntologyIcons = OntologyProvider.OntologyTempFolderPath;
        private static object decompressOntologyIconPackLockObject = new object();
        /// <summary>
        /// آماده سازی اولیه متصدی نمایه (ایکن) های انواع هستان شناسی را انجام می هد
        /// </summary>
        public static void InitAsync(Stream compressedOntologyIconPack)
        {            
            // ذخیره سازی فایل دریافت شده به صورت محلی
            string zipPath = string.Format("{0}{1}.zip", pathToUseForOntologyIcons, fileNameOfOntologyIconPackCompressedFile);
            string extractPath = string.Format("{0}{1}\\", pathToUseForOntologyIcons, fileNameOfOntologyIconPackCompressedFile);
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);
            var fileStream = File.Create(zipPath);
            compressedOntologyIconPack.CopyTo(fileStream);
            fileStream.Close();
            // بازکردن فایل فشرده ی هستان شناسی
            WorkspaceTemperoryFiles.Decompress(zipPath, extractPath);
        }
        /// <summary>
        /// جریان (استریم) فایل بسته نمایه انواع هستان شناسی کنونی را از سرویس دهنده توزیع دریافت کرده و برمی گرداند
        /// </summary>
        public async static Task<Stream> GetCompressedOntologyIconPackFromDispatchAsync()
        {
            WorkspaceServiceClient cliect = RemoteServiceClientFactory.GetNewClient();
            Stream result = null;
            try
            {
                cliect.Open();
                result = await cliect.GetIconAsync();
            }
            finally
            {
                cliect.Close();
            }

            return result;
        }
        /// <summary>
        /// مسیر ذخیره سازی نمایه (آیکن) های انواع هستان شناسی در دیسک محلی را برمی گرداند
        /// </summary>
        private static Uri GetTypeIconPackPath()
        {
            return new Uri(string.Format("{0}{1}\\", pathToUseForOntologyIcons, fileNameOfOntologyIconPackCompressedFile), UriKind.Relative);
        }

        // TODO: خارجی - جداسازی منطق استفاده از آیکن ها (که در اصل مربوط به سرور توزیع است، از منطق کار با آیکن ها که مربوط به سمت محیط کاربری است

        ///<summary>
        /// نمایه (آیکن) مربوط به یک نوع در هستان شناسی را برمی گرداند
        ///</summary>
        ///<remarks>
        /// این تابع با دریافت یک نوع از ورودی و نیز استفاده از مسیر پوشه نمایه(آیکن)‌ها و هستان شناسی موجود، نمایه نوع مورد نظر را بر‌می‌گرداند
        /// در صورتی که منایه مورد نظر یافت نشود با استفاده از هستان شناسی به دنبال نمایه پدر نوع مورد نظر می‌گردد؛
        /// در نهایت یک مسیر برای نمایه یافت شده بازگردانده می‌شود یا در صورت بروز استثناء «نال» برگردانده می شود
        /// </remarks>
        public static Uri GetTypeIconPath(string typeURI)
        {
            // بررسی اعتبار حداقلی آرگومانهای ورودی
            if (string.IsNullOrWhiteSpace(typeURI))
                throw new ArgumentException("Invalid Argument (Null, Empty or Whitespace)", "typeURI");

            var currentOntology = OntologyProvider.GetOntology();
            // TODO: درصورت تغییر در انواعی که آیکن برایشان تعریف شده، این دستور می‌بایست تغییر کند
            // در صورتی که نوع داده شده از انواعی باشد که در حال حاضر برای آن‌ها ایکن تعریف نشده، نال برگردانده می‌شود
            // TODO: کارایی | هستان شناسی - می‌توان به جای فراخوانی این پنج تابع، یک تابع برای بررسی این که نوع وارد، مفهوم تعریف شده در هستان‌شناسی هست یا خیر استفاده کرد
            if (!(currentOntology.IsEntity(typeURI) || currentOntology.IsEvent(typeURI) || currentOntology.IsDocument(typeURI) || currentOntology.IsRelationship(typeURI)))
                typeURI = currentOntology.GetAllConceptsTypeURI();

            string iconPath = GetTypeIconPackPath().ToString() + typeURI + ".png";
            string typeToGetItsIcon = typeURI;

            while (!File.Exists(iconPath))
            {
                try
                {
                    typeToGetItsIcon = currentOntology.GetParent(typeToGetItsIcon);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(string.Format("Unable to get parent type for the type '{0}'.\r\nMessage: {1}", typeURI, ex.Message)); // SPARQL Query Or Ontology Configuration Is Wrong
                }
                if (string.IsNullOrWhiteSpace(typeToGetItsIcon))
                    throw new ApplicationException(string.Format("Invalid parent returned for type '{0}'.", typeURI));
                iconPath = GetTypeIconPackPath() + typeToGetItsIcon + ".png";
            }
            return new Uri(iconPath, UriKind.RelativeOrAbsolute);
        }

        public static Uri GetPropertyTypeIconPath(string typeURI)
        {
            if (string.IsNullOrWhiteSpace(typeURI))
                throw new ArgumentException("Invalid Argument (Null, Empty or Whitespace)", "typeURI");

            var currentOntology = OntologyProvider.GetOntology();
           
            string iconPath = GetTypeIconPackPath().ToString() + typeURI + ".png";
            string typeToGetItsIcon = typeURI;

            if (File.Exists(iconPath))
            {
                return new Uri(iconPath, UriKind.RelativeOrAbsolute);
            }
            else
            {
                iconPath = GetTypeIconPackPath().ToString() + currentOntology.GetAllConceptsTypeURI() + ".png";
                return new Uri(iconPath, UriKind.RelativeOrAbsolute);
            }
        }


        public static Uri GetTypeIconPath(string typeURI, Ontology.Ontology newOntology)
        {
            string iconPath = GetTypeIconPackPath().ToString() + typeURI + ".png";
            string typeToGetItsIcon = typeURI;
            try
            {
                while (!File.Exists(iconPath))
                {
                    try
                    {
                        typeToGetItsIcon = newOntology.GetParent(typeToGetItsIcon);
                        if (typeToGetItsIcon == null)
                            throw new ApplicationException();
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException("Unable to get parent type for a type | " + ex.Message); // SPARQL Query Or Ontology Configuration Is Wrong
                    }
                    iconPath = GetTypeIconPackPath() + typeToGetItsIcon + ".png";
                }
                return new Uri(iconPath, UriKind.RelativeOrAbsolute);
            }
            catch
            {
                return null;
            }
        }
    }
}
