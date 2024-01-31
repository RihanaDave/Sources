using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers.Base
{
    public abstract class ObserverBase<TListener, TArgs>
    {
        private List<TListener> listeners = new List<TListener>();
        
        /// <summary>
        /// افزودن یک شنونده جدید به ناظر
        /// </summary>
        public void AddListener(TListener listener)
        {
            if (listeners.Contains(listener))
                return;
            listeners.Add(listener);
        }
        /// <summary>
        /// حذف یکی از شنونده‌های کنونی ناظر
        /// ناظر = Obsever
        /// </summary>
        public void RemoveListener(TListener listener)
        {
            if (!listeners.Contains(listener))
                return;
            listeners.Remove(listener);
        }

        /// <summary>
        /// گزارش رخداد به ناظر؛
        /// با فراخوانی این تابع، ناظر شنونده‌ها را با خبر می کند
        /// </summary>
        /// <param name="arguments">آرگومانی مربوط به رخداد گزارش شده؛ این آرگومان به شونده‌ها پاس داده خواهد شد</param>
        public void ReportAction(TArgs arguments)
        {
            WakeupListeners(listeners, arguments);
        }
        /// <summary>
        /// نحوه اطلاع یافتن شنونده‌های ناظر در این تابع تعریف خواهد شد
        /// </summary>
        /// <param name="listener">لیست شنونده‌ها</param>
        /// <param name="arguments">آرگومان دریافت شده از فراخواننده ناظر؛ در صورت نیاز می‌تواند به شنونده‌ها پاس داده شود</param>
        protected abstract void WakeupListeners(IEnumerable<TListener> listener, TArgs arguments);
    }
}
