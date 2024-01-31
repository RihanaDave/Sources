using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public enum DataSourceImportStatus
    {
        /// <summary>
        /// هنوز هیچ اقدامی در راستای ورود داده های مرتبط با این دیتا سورس انجام نشده است.
        /// </summary>
        [Description("Ready")]
        Ready = 0,

        /// <summary>
        /// تمامی فرآیند ورود داده برای داده های مرتبط با این دیتا سورس انجام شده است.
        /// منظور از تمامی فرآیند ورود داده تک تک اجزا و مراحل ورود داده نظیر گذر های ادغام سراسری است.
        /// </summary>
        [Description("Completed")]
        Completed = 1,

        /// <summary>
        /// فرآیند ورود داده برای داده های مرتبط با این دیتا سورس در حال انجام است.
        /// این فرآیند ورود داده می تواند ورود داده مرتبط با کل دیتا سورس یا یک گذر از ادغام سراسری باشد.
        /// </summary>
        [Description("Importing")]
        Importing = 2,

        /// <summary>
        /// فرآیند ورود داده به طور موقت متوقف شده است.
        /// تنها از حالت Importing‌می توان به این حالت سوییچ کرد.
        /// </summary>
        [Description("Pause")]
        Pause = 3,

        /// <summary>
        /// فرآیند ورود داده به طور کامل متوقف شده است.
        /// تنها از حالت Importing‌می توان به این حالت سوییچ کرد.
        /// </summary>
        [Description("Stop")]
        Stop = 4,

        /// <summary>
        /// در فرآیند ورود داده به هر دلیلی مشکلی رخ داده است.
        /// </summary>
        [Description("Failure")]
        Failure = 5,

        /// <summary>
        /// داده ها در مخزن داده ریخته شده اند اما در فرآیند ایندکس گذاری با مشکل مواجه شده است.
        /// </summary>
        [Description("Warning")]
        Warning = 6,

        /// <summary>
        /// فرآیند ورود داده برای داده های مرتبط با این دیتا سورس به طور نصفه انجام شده است.
        /// به طور مثال در فرآیند ادغام سراسری، داده های مرتبط با جند گذر وارد شده اما هنوز همه گذر های ادغام سراسری تکمیل نشده است.
        /// این گزینه تنها برای دیتا سورس هایی که ادغام سراسری دارند معنا دارد.
        /// </summary>
        [Description("Incomplete")]
        Incomplete = 7,

        /// <summary>
        /// منبع داده در صف ورود است.
        /// </summary>
        [Description("In Queue")]
        InQueue = 8,

        /// <summary>
        /// فرآیند ورود داده در مرحله تبدیل یا همان نگاشت است.
        /// </summary>
        [Description("Transforming")]
        Transforming = 9,

        /// <summary>
        /// فرآیند ورود داده در مرحله درج درون پایگاه داده است.
        /// </summary>
        [Description("Publishing")]
        Publishing = 10,
    }
}
