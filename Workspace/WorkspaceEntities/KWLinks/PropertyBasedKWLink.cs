using System;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Entities
{
    /// <summary>
    /// کلاس نگهداری و استفاده از رابطه (لینک) مبتنی بر ویژگی با مقدار مشترک بین دو شی
    /// این نوع رابطه، استنتاجی است (در مخزن داده‌ها صراحتا ذخیره نمی‌شود) و نشاندهنده این است که دو شی دارای یک ویژگی با نوع و مقدار یکسان می‌باشند
    /// اجزای این کلاس با پیش فرض استفاده صحیح توسط استفاده کننده قابل تغییر هستند و مدیریت مقداردهی معتبر اجزای در دسترس آن، برعهده استفاده کننده می باشد
    /// </summary>
    public class PropertyBasedKWLink : KWLink
    {
        public string SamePropertyTypeUri { get; set; }
        public string SamePropertyValue { get; set; }

        /// <summary>
        /// جهت رابطه را برمی گرداند؛ این مقدار برای این نوع لینک، همیشه دوطرفه است
        /// </summary>
        public override LinkDirection LinkDirection
        {
            get { return LinkDirection.Bidirectional; }
        }
        /// <summary>
        /// قابلیت ادغام/باز شدن برای رابطه را برمی گرداند؛ این قابلیت برای این نوع لینک همیشه غیرقابل ادغام است
        /// </summary>
        public override bool IsUnmergable
        {
            get { return false; }
        }
        /// <summary>
        /// یکی از اشیا طرف رابطه را نگهداری می‌کند؛
        /// این شئ به عنوان مبدا رابطه درنظر گرفته می‌شود و تنها تفاوت مبدا و مقصد این رابطه، ویژگی مبدا و مقصد مربوطه در این کلاس است
        /// </summary>
        public override KWObject Source
        {
            get;
            set;
        }
        /// <summary>
        /// یکی از اشیا طرف رابطه را نگهداری می‌کند؛
        /// این شئ به عنوان مقصد رابطه درنظر گرفته می‌شود و تنها تفاوت مبدا و مقصد این رابطه، ویژگی مبدا و مقصد مربوطه در این کلاس است
        /// </summary>
        public override KWObject Target
        {
            get;
            set;
        }
        /// <summary>
        /// عنوان لینک را نگهداری می‌کند؛ مقدار عنوان نمایشی براساس این ویژگی برگدانده می شود
        /// </summary>
        public string LinkCaption
        {
            get;
            set;
        }
        /// <summary>
        /// نوع متناسب با ویژگی مشترک بین دو شئ را برمی‌گرداند
        /// </summary>
        public override string TypeURI
        {
            get { return SamePropertyTypeUri; }
        }
        /// <summary>
        /// عنوان نمایشی برای رابطه را برمی گرداند؛ این عنوان براساس مقداری که به
        /// LinkCaption
        /// داده می‌شود، برگردانده می‌شود
        /// </summary>
        public override string Text
        {
            get { return LinkCaption; }
        }

        public bool Equals(KWLink compairLink)
        {
            if (!(compairLink is PropertyBasedKWLink))
                return false;

            if (SamePropertyValue.Equals((compairLink as PropertyBasedKWLink).SamePropertyValue)
                && SamePropertyTypeUri.Equals((compairLink as PropertyBasedKWLink).SamePropertyTypeUri)
                && (Source.Equals((compairLink as PropertyBasedKWLink).Source) || Source.Equals((compairLink as PropertyBasedKWLink).Target))
                && (Target.Equals((compairLink as PropertyBasedKWLink).Target) || Target.Equals((compairLink as PropertyBasedKWLink).Source)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (Source.ID ^ Target.ID).GetHashCode()
                ^ SamePropertyTypeUri.GetHashCode()
                ^ SamePropertyValue.GetHashCode();
        }
    }
}