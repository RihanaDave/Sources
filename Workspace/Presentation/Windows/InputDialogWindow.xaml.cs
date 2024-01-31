using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Windows
{
    /// <summary>
    /// Interaction logic for InputDialogWindow.xaml
    /// </summary>
    public partial class InputDialogWindow : INotifyPropertyChanged
    {
        public string Answer { get; set; }
        private string question;
        public string Question
        {
            get => question;
            set
            {
                if (question != value)
                {
                    question = value;
                    NotifyPropertyChanged("Question");
                }
            }
        }        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public InputDialogWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        public void ShowQuestion(string question)
        {
            Question = question ?? throw new ArgumentNullException();
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            Answer = txtAnswer.Text;
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void txtAnswer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtAnswer.Text) &&
                !string.IsNullOrWhiteSpace(txtAnswer.Text))
            {
                btnDialogOk.IsEnabled = true;
            }
            else
            {
                btnDialogOk.IsEnabled = false;
            }
        }
    }
}
