using GPAS.LoadTest.Core;

namespace GPAS.LoadTest.Presenter.Model
{
    public class SidebarMenuModel : BaseModel
    {
        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private MaterialDesignThemes.Wpf.PackIconKind icon;
        public MaterialDesignThemes.Wpf.PackIconKind Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private ServerType tag;
        public ServerType Tag
        {
            get => tag;
            set
            {
                tag = value;
                OnPropertyChanged();
            }
        }
    }
}
