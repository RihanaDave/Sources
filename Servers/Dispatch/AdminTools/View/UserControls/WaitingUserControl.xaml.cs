using System.Windows;
using System.Windows.Controls;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    /// <summary>
    /// Interaction logic for WaitingUserControl.xaml
    /// </summary>
    public partial class WaitingUserControl : UserControl
    {
        public WaitingUserControl()
        {
            InitializeComponent();
        }

        private int tasksCount;

        public int TasksCount
        {
            protected set
            {
                tasksCount = value < 0 ? 0 : value;

                Visibility = tasksCount == 0 ? Visibility.Collapsed : Visibility.Visible;
            }

            get => tasksCount;
        }

        public void TaskIncrement()
        {
            TasksCount++;
        }

        public void TaskDecrement()
        {
            TasksCount--;
        }
    }
}
