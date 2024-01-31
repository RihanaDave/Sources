using System;
using System.Windows.Input;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    /// <summary>
    /// Interaction logic for SidebarMenuTemplateUserControl.xaml
    /// </summary>
    public partial class SidebarMenuTemplateUserControl 
    {
        public SidebarMenuTemplateUserControl()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> SidebarEvent;
        
        public void OnSidebarEvent()
        {
            SidebarEvent?.Invoke(this, new EventArgs());
        }

        private void EventSetter_Handler(object sender, MouseButtonEventArgs e)
        {
            OnSidebarEvent();
        }
    }
}
