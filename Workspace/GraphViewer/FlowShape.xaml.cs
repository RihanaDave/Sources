using GPAS.Graph.GraphViewer.Foundations;
using GraphX;
using GraphX.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPAS.Graph.GraphViewer.Flow
{
    /// <summary>
    /// Interaction logic for FlowShape.xaml
    /// </summary>
    public partial class FlowShape : UserControl, IGraphControl
    {
        public FlowShape()
        {
            InitializeComponent();
            //GraphX.Controls.Animations.
        }

        public List<Foundations.Edge> Edges { get; set; }

        public GraphAreaBase RootArea
        {
            get { return (GraphAreaBase)GetValue(RootAreaProperty); }
            set { SetValue(RootAreaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RootArea.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RootAreaProperty =
            DependencyProperty.Register("RootArea", typeof(GraphAreaBase), typeof(FlowShape), new PropertyMetadata(null));


        public void Clean()
        {

        }
        public void SetPosition(Point pt, bool alsoFinal = true)
        {
            GraphAreaBase.SetX(this, pt.X, alsoFinal);
            GraphAreaBase.SetY(this, pt.Y, alsoFinal);
        }

        public void SetPosition(double x, double y, bool alsoFinal = true)
        {
            GraphAreaBase.SetX(this, x, alsoFinal);
            GraphAreaBase.SetY(this, y, alsoFinal);
        }

        /// <summary>
        /// Get control position on the GraphArea panel in attached coords X and Y
        /// </summary>
        /// <param name="final"></param>
        /// <param name="round"></param>
        public Point GetPosition(bool final = false, bool round = false)
        {
            return round ?
                new Point(final ? (int)GraphAreaBase.GetFinalX(this) : (int)GraphAreaBase.GetX(this), final ? (int)GraphAreaBase.GetFinalY(this) : (int)GraphAreaBase.GetY(this)) :
                new Point(final ? GraphAreaBase.GetFinalX(this) : GraphAreaBase.GetX(this), final ? GraphAreaBase.GetFinalY(this) : GraphAreaBase.GetY(this));
        }
    }
}
