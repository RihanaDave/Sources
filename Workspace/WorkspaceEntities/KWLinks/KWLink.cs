//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Entities
{
    /// <summary>
    /// کلاس نگهداری و استفاده از رابطه (لینک) ها؛ رابطه (لینک) می تواند مبتنی بر (ریلیشنشیپ) یا رخداد میانی باشد
    /// این کلاس انتزاعی از یک رابطه را صرفنظر از نوع اصلی آن ارائه می دهد و به این ترتیب استفاده های عمومی از یک رابطه ممکن می شود
    /// اجزای این کلاس با پیش فرض استفاده صحیح توسط استفاده کننده قابل تغییر هستند و مدیریت مقداردهی معتبر اجزای در دسترس آن، برعهده استفاده کننده می باشد
    /// </summary>

    public abstract class KWLink

    {
        /// <summary>
        /// شی مبدا رابطه را برمی گرداند و یا مقداردهی می کند
        /// </summary>
		public abstract KWObject Source
		{
			get;
			set;
		}
        /// <summary>
        /// شی مقصد رابطه را برمی گرداند یا مقداردهی می کند
        /// </summary>
        public abstract KWObject Target
		{
			get;
			set;
		}
        /// <summary>
        /// جهت رابطه براساس اشیا مبدا و مقصد را برمی گرداند
        /// </summary>
        public abstract LinkDirection LinkDirection
        {
            get;
        }
        /// <summary>
        /// عنوان نمایشی برای رابطه را برمیگرداند
        /// </summary>
        public abstract string Text
        {
            get;
        }
        /// <summary>
        /// نوع رابطه را براساس نوعی از هستان شناسی که رابطه را شکل داده است برمی گرداند
        /// </summary>
        public abstract string TypeURI
        {
            get;
        }
        /// <summary>
        /// قابلیت ادغام/بازشدن رابطه را برمی گرداند
        /// </summary>
        public abstract bool IsUnmergable
        {
            get;
        }
        
        public override bool Equals(object x)
        {
            if (x == null)
                throw new ArgumentException();
            
            if (this.GetType().Equals(x.GetType()))
            {
                if (x is EventBasedKWLink)
                {
                    return (this as EventBasedKWLink).Equals(x as EventBasedKWLink);
                }
                else if (x is RelationshipBasedKWLink)
                {
                    return (this as RelationshipBasedKWLink).Equals(x as RelationshipBasedKWLink);
                }
                else if (x is PropertyBasedKWLink)
                {
                    return (this as PropertyBasedKWLink).Equals(x as PropertyBasedKWLink);
                }
                else if (x is NotLoadedRelationshipBasedKWLink)
                {
                    return (this as NotLoadedRelationshipBasedKWLink).Equals(x as NotLoadedRelationshipBasedKWLink);
                }
                else if (x is NotLoadedEventBasedKWLink)
                {
                    return (this as NotLoadedEventBasedKWLink).Equals(x as NotLoadedEventBasedKWLink);
                }
                else
                {
                    return ReferenceEquals(this, x);
                }
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            if (this is EventBasedKWLink)
            {
                return (this as EventBasedKWLink).GetHashCode();
            }
            else if (this is RelationshipBasedKWLink)
            {
                return (this as RelationshipBasedKWLink).GetHashCode();            
            }
            else if (this is PropertyBasedKWLink)
            {
                return (this as PropertyBasedKWLink).GetHashCode();
            }
            else if (this is NotLoadedRelationshipBasedKWLink)
            {
                return (this as NotLoadedRelationshipBasedKWLink).GetHashCode();
            }
            else if (this is NotLoadedEventBasedKWLink)
            {
                return (this as NotLoadedEventBasedKWLink).GetHashCode();
            }
            else
            {
                return (this as object).GetHashCode();
            }
        }

        //public abstract int Compare(object x, object y);
    }
}
