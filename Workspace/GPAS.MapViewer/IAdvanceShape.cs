using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GPAS.MapViewer
{
    public interface IAdvanceShape
    {
        DrawingMode DrawMode { get; set; }

        MapViewer Map { get; set; }

        void CreateShape();

        void Move(double lat, double lng);

        void Delete();

        void Draw();

        void Erase();

        int ZIndex { get; }

        event MouseButtonEventHandler MouseDown;
        event MouseButtonEventHandler MouseUp;
        event MouseEventHandler MouseMove;
        event MouseEventHandler MouseLeave;
        event MouseEventHandler MouseEnter;

        int SendToFront(int minIndex);
    }
}
