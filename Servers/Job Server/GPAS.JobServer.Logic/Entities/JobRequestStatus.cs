namespace GPAS.JobServer.Logic.Entities
{
    /// <summary>
    /// وضعیت‌های مختلف هر کار:
    ///     Pending     (By: JobServer)             =>  درخواست دریافت شده و منتظر اجرا است
    ///     Busy        (By: JobMonitoringAgent)    =>  درخواست برای اجرا به یک انجام دهنده انتساب داده شده است
    ///     Failed      (By: JobWorkerProcess)      =>  اجرای برنامه به یک انجام‌دهنده انتساب داده شده و آن انتساب‌دهنده اعلام کرده که کار با خطا مواجه شده است به عبارتی، موفقیت آمیز نبوده است
    ///     Terminated  (By: JobMonitoringAgent)    =>  اجرای برنامه قبلا به یک انجام‌دهنده انتساب داده شده ولی انجام‌دهنده فعالی برای آن وجود ندارد
    ///     Success     (By: JobWorkerProcess)      =>  اجرا با موفقیت به اتمام رسیده است
    ///     Timeout     (By: JobMonitoringAgent)    =>  اجرای کار از زمان مشخص شده بیشتر طول کشیده است
    /// </summary>
    public enum JobRequestStatus
    {
        Pending,
        Busy,
        Timeout,
        Terminated,
        Failed,
        Success,
        Pause,
        Resume,
    }
}