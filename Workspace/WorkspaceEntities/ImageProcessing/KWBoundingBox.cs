using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GPAS.Workspace.Entities.ImageProcessing
{
    public class KWBoundingBox
    {
        public Point TopLeft { get; set; }
        
        public int Width { get; set; }
        
        public int Height { get; set; }

        public KWLandmarks Landmarks { get; set; }

        public string Caption{ get; set; }

        bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
        }

        public event SelectEventHandler SelectionChanged;

        public void Select()
        {
            isSelected = true; ;

            SelectionChanged?.Invoke(this, new SelectEventArgs() { Select = isSelected });
        }

        public void DeSelect()
        {
            isSelected = false; ;

            SelectionChanged?.Invoke(this, new SelectEventArgs() { Select = isSelected });
        }
    }

    public delegate void SelectEventHandler(object source, SelectEventArgs e);

    public class SelectEventArgs : EventArgs
    {
        public Boolean Select
        {
            get;
            set;
        }
    }
}
