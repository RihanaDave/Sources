using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GPAS.Dispatch.AdminTools.Model
{
    /// <inheritdoc>
    /// این کلاس از 
    /// INotifyPropertyChanged
    /// ارث بری می‌کند
    /// </inheritdoc>
    /// <summary>
    /// کلاس پایه مدل ها 
    /// مدل‌های دیگر برای انتشار تغییرات ویژگی‌های خود
    /// به سایر قسمت ها از این کلاس ارث ‌بری می کنند
    /// </summary>
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// این تابع در قسمت 
        /// Set
        /// ویژگی های مدل های دیگر فراخوانی می‌شود
        /// </summary>
        /// <param name="caller">
        /// با استفاده از 
        /// CallerMemberName
        /// دیگر نیازی به فرستادن نام ویژگی به این تابع نیست
        /// و به طور خودکار نام ویژگی‌‌ای که فراخوانی را انجام داده است شناسایی می‌شود
        /// </param>
        protected void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }
    }
}
