using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GPAS.TagCloudViewer.Foundations;
using GPAS.TagCloudViewer.LayoutAlgorithms;
using System.Windows.Media.Animation;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using GraphX.Controls.Models;
using GraphX.Controls.Animations;

namespace GPAS.TagCloudViewer
{
    /// <summary>
    /// Interaction logic for GraphControl.xaml
    /// </summary>
    public partial class TagCloudViewer
    {
        /// <summary>
        /// این شی سازوکار داده ای گراف را فراهم می آورد
        /// </summary>
        private GraphData graph = new GraphData();
        /// <summary>
        /// این شی سازوکار منطقی گراف را فراهم می آورد
        /// </summary>
        private GraphLogic logicCore = new GraphLogic();

        public static readonly DependencyProperty LargestTagFontSizeProperty =
            DependencyProperty.Register("LargestTagFontSize", typeof(double), typeof(TagCloudViewer));
        public double LargestTagFontSize
        {
            get { return (double)GetValue(LargestTagFontSizeProperty); }
            set { SetValue(LargestTagFontSizeProperty, value); }
        }

        #region آماده سازی
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public TagCloudViewer()
            : base()
        {
            // آماده سازی اجزای کنترل برای شروع به کار
            InitializeComponent();
            // مقداردهی اولیه نمونه منطق گراف
            area.SetLogicCore(logicCore);
            // جمع کردن کنترل مدیریت بزرگنمایی گراف جهت شروع به کار کنترل
            GraphX.Controls.ZoomControl.SetViewFinderVisibility(zoomControl, Visibility.Hidden);
        }

        /// <summary>
        /// رخدادگردان زمان خاتمه بارگذاری کنترل
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // اعمال تنظیمات اولیه به کنترل نمایش دهنده گراف
            // این کدها بر گرفته از مثال های توضیح داده شده در سایت ارائه دهنده کنترل گراف
            // (PANTHERNET.RU)
            // و نیز کد نمونه پیاده سازی شده این سایت می باشد

            // TODO: بررسی - Optimize exterrnal used codes of init graph
            logicCore.DefaultLayoutAlgorithm = GraphX.PCL.Common.Enums.LayoutAlgorithmTypeEnum.Custom;
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams = logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            (logicCore.DefaultOverlapRemovalAlgorithmParams as OverlapRemovalParameters).HorizontalGap = 2;
            (logicCore.DefaultOverlapRemovalAlgorithmParams as OverlapRemovalParameters).VerticalGap = 2;
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            logicCore.EdgeCurvingEnabled = false;
            logicCore.AsyncAlgorithmCompute = false;

            // برگرفته از مثال "Dynamic Graph"
            area.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Fade, TimeSpan.FromSeconds(0.001));

            // پیکربندی بزرگنمایی دو مرحله ای (رفع مشکل عقب ماندن زوم از تغییرات غیرهمگام)
            zoomControl.ZoomAnimationCompleted += zoomControl_ZoomAnimationCompleted;

            // افزودن رخدادگردان های ارتباط منطق و نمایش گراف در
            // حذف/اضافه گره ها و یال ها؛ این رخدادگردان ها تضمین کننده
            // تطابق منطق و نمایش گراف هستند و مشکل گراف ایکس در عدم
            // تطابق منطق و نمایش گراف را پوشش می دهد.
            area.LogicCore.Graph.VertexAdded += GraphLogicCore_VertexAdded;
            area.LogicCore.Graph.VertexRemoved += GraphLogicCore_VertexRemoved;
            area.LogicCore.Graph.Cleared += GraphLogicCore_Cleared;

            area.ShowAllEdgesLabels(true);
            area.AlignAllEdgesLabels(true);
            area.InvalidateVisual();
        }
        #endregion

        public void ShowTagCloud(TagScore[] tags)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));

            ClearGraph();
            double maxScore = tags.Max(t => t.Score);
            List<Vertex> vertices = new List<Vertex>(tags.Length);
            foreach (var tag in RandomizePhrasesOrder(tags))
            {
                if (tag == null)
                    throw new ArgumentNullException(nameof(tag));
                Vertex v = Vertex.VertexFactory(string.IsNullOrWhiteSpace(tag.Title) ? "(untitled)" : tag.Title);
                v.RelatedVertexControl.FontSize = (tag.Score / maxScore) * LargestTagFontSize;
                v.RelatedVertexControl.ToolTip = string.Format("{0}{1}{2}", tag.Title, Environment.NewLine, tag.Score);
                v.RelatedVertexControl.Foreground = PickRandomBrush();
                AddVertex(v);
                vertices.Add(v);
            }
            UpdateLayout();
            RelayoutTagCloud(vertices);
            ZoomToFill();
        }

        private int lastGivenBrushIndex = 0;
        private static SolidColorBrush[] SpecifiedBrushes = new SolidColorBrush[]
        {
            Brushes.Red, Brushes.Green, Brushes.Blue,
            Brushes.Maroon, Brushes.Olive, Brushes.Navy,
            Brushes.Fuchsia, Brushes.CadetBlue, Brushes.DarkCyan,
            Brushes.Coral, Brushes.DarkBlue, Brushes.LimeGreen,
            Brushes.Crimson, Brushes.Teal, Brushes.BlueViolet
            //Brushes.Aqua, Brushes.Purple,
            //Brushes.Brown, Brushes.Gold, Brushes.HotPink, Brushes.Orange, Brushes.IndianRed,
        };
        private Brush PickRandomBrush()
        {
            if (lastGivenBrushIndex >= SpecifiedBrushes.Length)
                lastGivenBrushIndex = 0;
            return SpecifiedBrushes[lastGivenBrushIndex++];
        }

        private IEnumerable<TagScore> RandomizePhrasesOrder(TagScore[] tags)
        {
            var rnd = new Random();
            return tags.OrderBy(item => rnd.Next());
        }

        public void AddVertex(Vertex vertexToAdd)
        {
            if (vertexToAdd == null)
                throw new ArgumentNullException("vertexToAdd");

            // تلاش برای تنظیم موقعیت مکانی گره جدید در محلی که در دید کنونی کاربر و ترجیحا وسط آن باشد
            Point areaCenterPosition = GetCurrentViewCenterPosition();

            vertexToAdd.RelatedVertexControl.SetPosition(new Point
                (areaCenterPosition.X - ((area.VertexList.Count == 1) ? 0 : 50) /*+ (new Random(DateTime.Now.Millisecond)).Next(100)*/
                , areaCenterPosition.Y - ((area.VertexList.Count == 1) ? 0 : 50) /*+ (new Random(DateTime.Now.Millisecond * DateTime.Now.Second)).Next(100)*/));
            GraphX.GraphAreaBase.SetX(vertexToAdd.RelatedVertexControl, GraphX.GraphAreaBase.GetX(vertexToAdd.RelatedVertexControl) - vertexToAdd.RelatedVertexControl.ActualWidth / 2 - 5);
            GraphX.GraphAreaBase.SetY(vertexToAdd.RelatedVertexControl, GraphX.GraphAreaBase.GetY(vertexToAdd.RelatedVertexControl) - vertexToAdd.RelatedVertexControl.ActualHeight / 2 - 5);

            // افزودن گره به گراف به منطق گراف؛
            // پس از اضافه شدن این گره به منطق گراف و صدور رخداد مربوط به
            // آن، گره -توسط رخداد گردان- به چیدمان (نمایشی) گراف نیز اضافه خواهد شد
            // بدین ترتیب تا گرهی به منطق گراف اضافه نشود، به محتوای
            // نمایشی گراف اضافه نخواهد شد
            area.LogicCore.Graph.AddVertex(vertexToAdd);
        }

        public Point GetCurrentViewCenterPosition()
        {
            return zoomControl.TranslatePoint(new Point(zoomControl.ActualWidth / 2, zoomControl.ActualHeight / 2), area);
        }

        private void GraphLogicCore_VertexAdded(Vertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            // گرهی که به منطق گراف اضافه شده، به نمایش گراف افزوده می شود
            if (!area.VertexList.ContainsKey(vertex))
                area.AddVertex(vertex, vertex.RelatedVertexControl);
        }
        private void GraphLogicCore_VertexRemoved(Vertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            // گرهی که از منطق گراف حذف شده، از نمایش گراف حذف می شود
            area.RemoveVertex(vertex);
            //if (area.LogicCore.Graph.VertexCount != area.VertexList.Count || area.LogicCore.Graph.EdgeCount != area.EdgesList.Count)
            //    // TODO: External Bug | Graph Logic & Area may not balanced | Bug-Test: Use iterative removes instead of GraphClear()
            //    throw new Exception("Component internal error; Difference(s) between Graph Logic and Area found");
        }
        private void GraphLogicCore_Cleared(object sender, EventArgs e)
        {
            // محتوا (گره ها و یال ها)ی نمایش گراف حذف می شود
            area.ClearLayout();
            //if (area.LogicCore.Graph.VertexCount != area.VertexList.Count || area.LogicCore.Graph.EdgeCount != area.EdgesList.Count)
            //    // TODO: External Bug | Graph Logic & Area may not balanced | Bug-Test: Use iterative removes instead of GraphClear()
            //    throw new Exception("Component internal error; Difference(s) between Graph Logic and Area found");
        }


        #region عملکردهای بازچینش گره ها
        /// <summary>
        /// سازنده الگوریتم های چینش گره های گراف
        /// </summary>
        private AlgorithmFactory layoutAlgorithmFactory = new AlgorithmFactory();

        /// <summary>
        /// عملکرد اعمال چینش به گره های گراف
        /// </summary>
        /// <remarks>
        /// بخشی از کد این عملکرد از نمونه کد مثال (Showcase) گراف ایکس (نسخه 2.0.2) گرفته شده است
        /// GraphArea.cs | _relayoutGraph()
        /// </remarks>
        public void RelayoutTagCloud(IEnumerable<Vertex> verticesToRelayout)
        {
            if (verticesToRelayout == null)
                throw new ArgumentNullException(nameof(verticesToRelayout));

            if (verticesToRelayout.Take(2).Count() <= 1)
                return;

            bool applyWithAnimation = true;

            // تبدیل مقادیر ورودی به مقادیر فایل استفاده برای الگوریتم
            GraphData gd = new GraphData();
            Dictionary<Vertex, GraphX.Measure.Point> vp = new Dictionary<Vertex, GraphX.Measure.Point>();
            Dictionary<Vertex, GraphX.Measure.Size> vs = new Dictionary<Vertex, GraphX.Measure.Size>();
            foreach (Vertex item in verticesToRelayout)
            {
                gd.AddVertex(item);
                vp.Add(item, GetVertexControlPosition(item.RelatedVertexControl));
                vs.Add(item, GetVertexSize(item));
            }

            // آماده سازی الگوریتم چینش
            ILayoutAlgorithm alg = layoutAlgorithmFactory.CreateLayoutAlgorithm(LayoutAlgorithms.LayoutAlgorithmTypeEnum.TagCloud, gd, vp, vs, null);
            if (alg == null)
                return;
            // اجرای الگوریتم چینش
            alg.Compute(new System.Threading.CancellationToken());

            // اعمال تغییرات چینش، شامل:
            if (applyWithAnimation)
                AnimateVerticesMove(alg.VertexPositions, true);
            else
                SetVertexPositions((Dictionary<Vertex, GraphX.Measure.Point>)alg.VertexPositions);

            // در صورت نیاز به الگوریتم مسیردهی یال ها(جلوگیری/حداقل سازی روی هم افتادن یال ها)، اینجا می توان از آن استفاده کرده
        }
        #endregion

        /// <summary>
        /// پویانمایی حرکت گره هااز موقعیت کنونی آن ها تا موقعیتی جدید را به صورت غیر همگام شروع می کند
        /// </summary>
        /// <param name="VerticesFinalPosition">دیکشنری گره هایی که می خواهیم جابجا شوند و موقعیت نهایی که می بایست در آن قرار بگیرند</param>
        /// <remarks>
        /// بخشی از کد این عملکرد از نمونه کد مثال (Showcase) گراف ایکس (نسخه 2.0.2) گرفته شده است
        /// GraphArea.cs | _relayoutGraph()
        /// </remarks>
        internal void AnimateVerticesMove(IDictionary<Vertex, GraphX.Measure.Point> VerticesFinalPosition, bool setVerticesVisible = false)
        {
            if (VerticesFinalPosition == null)
                throw new ArgumentNullException("VerticesFinalPosition");

            // آماده سازی محیط گراف
            if (area.MoveAnimation != null)
            {
                area.MoveAnimation.CleanupBaseData();
                area.MoveAnimation.Cleanup();
            }
            // تنظیم موقعیت های جدید گره ها
            foreach (var item in VerticesFinalPosition)
            {
                if (!area.VertexList.ContainsKey(item.Key)) continue;
                var vc = area.VertexList[item.Key];

                GraphX.GraphAreaBase.SetFinalX(vc, item.Value.X);
                GraphX.GraphAreaBase.SetFinalY(vc, item.Value.Y);

                if (area.MoveAnimation == null || double.IsNaN(GraphX.GraphAreaBase.GetX(vc)))
                    vc.SetPosition(item.Value.X, item.Value.Y, false);
                else
                    area.MoveAnimation.AddVertexData(vc, item.Value);

                if (setVerticesVisible)
                {
                    vc.Visibility = Visibility.Visible; //show vertexes with layout positions assigned
                }
            }
            // پویانمایی تغییر موقعیت گره ها
            if (area.MoveAnimation != null)
            {
                if (area.MoveAnimation.VertexStorage.Count > 0)
                    area.MoveAnimation.RunVertexAnimation();

                foreach (var item in area.EdgesList.Values)
                    area.MoveAnimation.AddEdgeData(item);
                if (area.MoveAnimation.EdgeStorage.Count > 0)
                    area.MoveAnimation.RunEdgeAnimation();
            }
            UpdateLayout();
        }

        /// <summary>
        /// موقعیت گره در گراف را برمی گرداند
        /// </summary>
        protected GraphX.Measure.Point GetVertexControlPosition(VertexControl vertexControl)
        {
            if (vertexControl == null)
                throw new ArgumentNullException("vertexControl");
            Point p = vertexControl.GetPosition();
            return new GraphX.Measure.Point(p.X, p.Y);
        }
        private static void SetVertexPositions(Dictionary<Vertex, GraphX.Measure.Point> finalPositions)
        {
            foreach (var fp in finalPositions)
            {
                fp.Key.RelatedVertexControl.SetPosition(new Point(fp.Value.X, fp.Value.Y));
            }
        }
        /// <summary>
        /// اندازه گره در گراف را برمی گرداند
        /// </summary>
        protected GraphX.Measure.Size GetVertexSize(Vertex VertexToGetSize)
        {
            if (VertexToGetSize == null)
                throw new ArgumentNullException("VertexToGetSize");

            return new GraphX.Measure.Size
                (VertexToGetSize.RelatedVertexControl.ActualWidth
                , VertexToGetSize.RelatedVertexControl.ActualHeight);
        }

        /// <summary>
        /// پاک کردن محتوا(گره ها و یال ها)ی گراف
        /// </summary>
        public void ClearGraph()
        {
            // پاک کردن محتوای منطق گراف در حال نمایش؛
            // پس از پاک شدن محتوای منطق گراف و صدور رخداد مربوط
            // به آن، چیدمان (نمایشی) گراف -توسط رخدادگردان- پاک خواهد شد؛
            // بدین ترتیب تا محتوای منطق گراف پاک نشود، محتوای
            // نمایشی گراف پاک نخواهد شد
            area.LogicCore.Graph.Clear();
            area.Children.Clear();
        }

        /// <summary>
        /// نیاز کنترل به بزرگنمایی دومرحله ای را در خود نگه می دارد
        /// </summary>
        /// <remarks>
        /// به خاطر وجود مشکل با بزرگنمایی گراف در زمان استفاده از پردازش های غیرهمگام گراف، راه کار بزرگنمایی دو مرحله ای در نظر گرفته شده است؛
        /// در این حالت پس از پایان پویانمایی جابجایی گره های گراف در بزرگنمایی معمولی (با دریافت رخداد آن)، یک باردیگر بزرگنمایی مدنظر اعمال می شود تا نتیجه مطلوب حاصل آید
        /// </remarks>
        private bool twoStateZoomNeeded = false;
        /// <summary>
        /// نوع بزرگنمایی موردنظر برای بزرگنمایی دومرحله ای را در خود نگه می دارد
        /// </summary>
        /// <remarks>
        /// به خاطر وجود مشکل با بزرگنمایی گراف در زمان استفاده از پردازش های غیرهمگام گراف، راه کار بزرگنمایی دو مرحله ای در نظر گرفته شده است؛
        /// در این حالت پس از پایان پویانمایی جابجایی گره های گراف در بزرگنمایی معمولی (با دریافت رخداد آن)، یک باردیگر بزرگنمایی مدنظر اعمال می شود تا نتیجه مطلوب حاصل آید
        /// </remarks>
        private GraphX.Controls.ZoomControlModes twoStateZoomMode = GraphX.Controls.ZoomControlModes.Custom;
        /// <summary>
        /// بزرگنمایی (دو مرحله ای) محتوای گراف به صورت Zoom-To-Fill
        /// </summary>
        /// <remarks>
        /// به خاطر وجود مشکل با بزرگنمایی گراف در زمان استفاده از پردازش های غیرهمگام گراف، راه کار بزرگنمایی دو مرحله ای در نظر گرفته شده است؛
        /// در این حالت پس از پایان پویانمایی جابجایی گره های گراف در بزرگنمایی معمولی (با دریافت رخداد آن)، یک باردیگر بزرگنمایی مدنظر اعمال می شود تا نتیجه مطلوب حاصل آید
        /// </remarks>
        public void ZoomToFill()
        {
            zoomControl.Mode = twoStateZoomMode = GraphX.Controls.ZoomControlModes.Fill;
            zoomControl.ZoomToFill();
            twoStateZoomNeeded = true;
        }
        /// <summary>
        /// بزرگنمایی (دو مرحله ای) محتوای گراف به صورت Zoom-To-Original
        /// </summary>
        /// <remarks>
        /// به خاطر وجود مشکل با بزرگنمایی گراف در زمان استفاده از پردازش های غیرهمگام گراف، راه کار بزرگنمایی دو مرحله ای در نظر گرفته شده است؛
        /// در این حالت پس از پایان پویانمایی جابجایی گره های گراف در بزرگنمایی معمولی (با دریافت رخداد آن)، یک باردیگر بزرگنمایی مدنظر اعمال می شود تا نتیجه مطلوب حاصل آید
        /// </remarks>
        public void ZoomToOriginal()
        {
            zoomControl.Mode = twoStateZoomMode = GraphX.Controls.ZoomControlModes.Original;
            zoomControl.ZoomToOriginal();
            twoStateZoomNeeded = true;
        }
        /// <summary>
        /// بزرگنمایی (دو مرحله ای) محتوای گراف به صورت Center-Content
        /// </summary>
        /// <remarks>
        /// به خاطر وجود مشکل با بزرگنمایی گراف در زمان استفاده از پردازش های غیرهمگام گراف، راه کار بزرگنمایی دو مرحله ای در نظر گرفته شده است؛
        /// در این حالت پس از پایان پویانمایی جابجایی گره های گراف در بزرگنمایی معمولی (با دریافت رخداد آن)، یک باردیگر بزرگنمایی مدنظر اعمال می شود تا نتیجه مطلوب حاصل آید
        /// </remarks>
        public void ZoomToCenterContent()
        {
            zoomControl.Mode = twoStateZoomMode = GraphX.Controls.ZoomControlModes.Custom;
            zoomControl.CenterContent();
            twoStateZoomNeeded = true;
        }
        /// <summary>
        /// رخدادگردان خاتمه اعمال پویانمایی گره ها در هر بزرگنمایی؛
        /// </summary>
        /// <remarks>
        /// این رخدادگردان در هربار بزرگنمایی صدا زده می شود، در نتیجه در بزرگنمایی  های دو مرحله ای، از تکرار بی نهایت فراخوانی این عملکرد جلوگیری شده
        /// به خاطر وجود مشکل با بزرگنمایی گراف در زمان استفاده از پردازش های غیرهمگام گراف، راه کار بزرگنمایی دو مرحله ای در نظر گرفته شده است؛
        /// در این حالت پس از پایان پویانمایی جابجایی گره های گراف در بزرگنمایی معمولی (با دریافت رخداد آن)، یک باردیگر بزرگنمایی مدنظر اعمال می شود تا نتیجه مطلوب حاصل آید
        /// </remarks>
        private void zoomControl_ZoomAnimationCompleted(object sender, EventArgs e)
        {
            // در صورتی که فراخوانی این عملکرد در یک بزرگنمایی دومرحله ای صورت گرفته باشد،
            if (twoStateZoomNeeded)
            {
                // نیاز به بزرگنمایی مجدد پس از این، رفع شده
                twoStateZoomNeeded = false;
                // و بر اساس بزرگنمایی تعیین شده (برای مرحله دوم)، بزرگنمایی دوم اعمال می گردد
                switch (twoStateZoomMode)
                {
                    case GraphX.Controls.ZoomControlModes.Fill:
                        zoomControl.ZoomToFill();
                        break;
                    case GraphX.Controls.ZoomControlModes.Original:
                        zoomControl.ZoomToOriginal();
                        break;
                    default:
                        zoomControl.CenterContent();
                        break;
                }
            }
        }
    }
}