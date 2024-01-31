using System;
using System.Xml.Serialization;

namespace GPAS.Workspace.Entities
{
    /// <summary>
    /// کلاس نگهداری و استفاده از ویژگی در محیط کاربری
    /// اجزای این کلاس با پیش فرض استفاده صحیح توسط استفاده کننده قابل تغییر هستند و مدیریت مقداردهی معتبر اجزای در دسترس آن، برعهده استفاده کننده می باشد
    /// </summary>
    public class KWProperty
    {
        /// <summary>
        /// شناسه ویژگی را براساس مقدار آن در مخزن داده ها برمی گرداند یا مقداردهی می کند!
        /// </summary>
        public long ID
        {
            get;
            set;
        }
        /// <summary>
        /// نوع ویژگی را براساس تعاریف هستان شناسی برمی گرداند یا مقداردهی می کند
        /// </summary>
        public string TypeURI
        {
            get;
            set;
        }
        /// <summary>
        /// شئی که این ویژگی مربوط به آن است را برمی گرداند یا مقداردهی می کند
        /// </summary>
        public KWObject Owner
        {
            get;
            set;
        }
        public long? DataSourceId
        {
            get;
            set;
        }
        private string propertyValue;
        
        /// <summary>
        /// مقدار مربوط به این ویژگی را برمی گرداند؛ این مقدار می‌تواند براساس نوع پایه شئ، معتبر نباشد
        /// </summary>
        public string Value
        {
            get
            {
                return propertyValue;
            }
            set
            {
                propertyValue = value;
                OnValueChanged();
            }
        }

        protected void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }
        public event EventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// یکسان بودن دو ویژگی (یکی بودن شناسه‌ها) را بررسی کرده و نتیجه را برمی‌گرداند
        /// </summary>
        public virtual bool Equals(KWProperty otherProperty)
        {
            return ID == otherProperty.ID;
        }

        [XmlIgnore]
        public EventHandler<EventArgs> Deleted;
        public void OnDeleted()
        {
            if (Deleted != null)
                Deleted(this, EventArgs.Empty);
        }
    }
}