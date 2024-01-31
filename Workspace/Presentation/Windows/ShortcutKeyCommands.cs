using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows
{
    public static class ShortcutKeyCommands
    {
        public static readonly RoutedUICommand AllShortcutKeyCommand = new RoutedUICommand(); 
    }

    public enum SupportedShortCutKey
    {
        Unknown,
        Esc,
        F1,
        Ctrl_A,
        Ctrl_B,
        Ctrl_F,
        Ctrl_Shift_I,
        Ctrl_L,
        Ctrl_N,
        Ctrl_S,
        RightClickKey,
        Del,
        Shift_Del,
        Ctrl_D,

        // نحوه‌ی افزودن رهنمای برای کلیدهای میانبر:
        //
        // اگر در منابع چیزی اضافه نکنید، به راهنما هم چیزی افزوده نخواهد شد.
        //
        // به ازای هر کلید میانبر می بایست در منابع یک فیلد به صورت
        // ShortcutKey_<ShortCutKeyEnumString>_Title
        // برای عنوان نمایشی کلید افزوده شود.
        // 
        // در صورتی که کلید کاربردی عمومی دارد، در منابع فیلدی به صورت
        // ShortcutKey_<ShortCutKeyEnumString>_Operation
        // افزوده شود و در صورتی که کلید عملکرد مختلفی
        // در کاربردهای مختلف دارد، به ازای هر عملکرد، در منابع فیلدی به صورت
        // ShortcutKey_<ShortCutKeyEnumString>_Operation_on_<OperationApplication>
        // افزوده شود؛ توجه داشته باشید نام کاربرد را از کلیدهای عنوان آن‌ها
        // در منابع استفاده کنید.
        // در حال حاظر فقط کاربردهای گراف و نمایشگر شئ توسط راهنما پشتیبانی می‌شوند
    }
}
