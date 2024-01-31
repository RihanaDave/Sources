using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    public class GraphContentRightClickEventArgs
    {
        public GraphContentRightClickEventArgs(Point clickPoint)
        {
            if (clickPoint == null)
                throw new ArgumentNullException("clickPoint");

            ClickPoint = clickPoint;
        }

        public Point ClickPoint
        {
            private set;
            get;
        }
    }
}
