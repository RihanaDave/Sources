using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.MapViewer
{
    public interface IMapShape
    {
        void Draw();

        void Erase();

        void Delete();

        event MouseButtonEventHandler MouseDown;
        event MouseButtonEventHandler MouseUp;
        event MouseEventHandler MouseMove;
        event MouseEventHandler MouseLeave;
        event MouseEventHandler MouseEnter;
    }
}
