using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Graph.GraphViewer.LayoutAlgorithms;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using GraphX.PCL.Common.Enums;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GPAS.Graph.GraphViewer
{
    /// <summary>
    /// Interaction logic for GraphControl.xaml
    /// </summary>
    public partial class GraphViewer
    {
        // TODO: Regions

        #region مدیریت رخداد
        /// <summary>
        /// این رخداد زمان تغییر در مجموعه گره (راس) های انتخاب شده ی روی گراف اتفاق می افتد
        /// </summary>
        public event EventHandler<EventArgs> VerticesSelectionChanged;
        /// <summary>
        /// عملگر صدور رخداد «تغییر در مجموعه یال های انتخاب شده روی گراف»
        /// </summary>
        protected virtual void OnEdgesSelectionChanged()
        {
            // در صورت معرفی رخدادگردان برای رخداد مربوطه،
            if (EdgesSelectionChanged != null)
            {
                // رخداد با آرگومان خالی صادر می شود
                EdgesSelectionChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// این رخداد زمان تغییر در مجموعه لینک های انتخاب شده ی روی گراف اتفاق می افتد
        /// </summary>
        public event EventHandler<EventArgs> EdgesSelectionChanged;
        /// <summary>
        /// عملگر صدور رخداد «تغییر در مجموعه گره (راس)های انتخاب شده روی گراف»
        /// </summary>
        /// <remarks>
        /// برای مدیریت یکپارچه صدور رخداد مربوطه، از این عملگر استفاده می شود؛
        /// جهت مدیریت صدور رخداد مربوطه، تمام تغییر در انتخاب گره های گراف به عملگر های
        /// Select & Deselect
        /// منتهی شده و تنها این دو عملگر، این رخداد را صادر می کنند
        /// </remarks>
        protected virtual void OnVerticesSelectionChanged()
        {
            // در صورت معرفی رخدادگردان برای رخداد مربوطه،
            if (VerticesSelectionChanged != null)
            {
                // رخداد با آرگومان خالی صادر می شود
                VerticesSelectionChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// این رخداد زمانی اتفاق می افتد که پویانمایی جابجایی گره های گراف به پایان می رسد
        /// </summary>
        public event EventHandler<EventArgs> VerticesMoveAnimationCompleted;
        /// <summary>
        /// عملگر صدور رخداد «پایان پویانمایی جابجایی گره های روی گراف»
        /// </summary>
        protected virtual void OnVerticesMoveAnimationCompleted(IEnumerable<Vertex> animatedVertices)
        {
            // در صورتی که گره‌های جابجا شده از محدوده‌ی کنونی دید کاربر
            // خارج شوند، کوچک‌نمایی در حد مورد نیاز انجام خواهد شد
            ZoomOutToCoverVertices(animatedVertices);
            // در صورت معرفی رخدادگردان برای رخداد مربوطه،
            if (VerticesMoveAnimationCompleted != null)
                // رخداد با آرگومان خالی صادر می شود
                VerticesMoveAnimationCompleted(this, EventArgs.Empty);
        }

        /// <summary>
        /// رخداد «کلیک راست بر روی گره»؛        
        /// </summary>
        public event EventHandler<EventArgs> VertexRightClick;
        /// <summary>
        /// عملگر صدور رخداد «کلیک راست بر روی گره»
        /// </summary>
        protected virtual void OnVertexRightClick()
        {
            // صدور رخداد مربوطه در صورت نیاز
            if (VertexRightClick != null)
                VertexRightClick(this, EventArgs.Empty);
        }

        public event EventHandler<VertexDoubleClickEventArgs> VertexDoubleClick;
        private void OnVertexDoubleClick(VertexControl doubleClickedVertexControl)
        {
            if (doubleClickedVertexControl == null)
                throw new ArgumentNullException("doubleClickedVertexControl");

            if (VertexDoubleClick != null)
                VertexDoubleClick(this, new VertexDoubleClickEventArgs(doubleClickedVertexControl));
        }

        public event EventHandler<EventArgs> EdgeRightClick;
        protected virtual void OnEdgeRightClick()
        {
            if (EdgeRightClick != null)
                EdgeRightClick(this, EventArgs.Empty);
        }

        /// <summary>
        /// رخداد «حذف گره از گراف»؛
        /// این رویداد پس از حذف گره از گراف رخ می دهد
        /// </summary>
        public event EventHandler<VertexRemovedEventArgs> VertexRemoved;
        /// <summary>
        /// عملکرد مدیریت صدور رخداد «حذف گره از گراف»
        /// </summary>
        protected virtual void OnVertexRemoved(Vertex removedVertex)
        {
            if (removedVertex == null)
                throw new ArgumentNullException("removedVertex");

            if (VertexRemoved != null)
                VertexRemoved(this, new VertexRemovedEventArgs(removedVertex));
        }

        /// <summary>
        /// رخداد «حذف یال از گراف»؛
        /// این رویداد پس از حذف یال از گراف رخ می دهد
        /// </summary>
        public event EventHandler<EdgeRemovedEventArgs> EdgeRemoved;
        /// <summary>
        /// عملکرد مدیریت صدور رخداد «حذف یال از گراف»
        /// </summary>
        protected virtual void OnEdgeRemoved(Edge removedEdge)
        {
            if (removedEdge == null)
                throw new ArgumentNullException("removedEdge");

            if (EdgeRemoved != null && !(removedEdge is CompoundEdge))
                EdgeRemoved(this, new EdgeRemovedEventArgs(removedEdge));
        }

        /// <summary>
        /// رخداد «پاک شدن محتوا(گره ها و یال ها)ی گراف»؛
        /// این رویداد پس از پاک شدن محتوای گراف رخ می دهد
        /// </summary>
        public event EventHandler<EventArgs> GraphCleared;
        /// <summary>
        /// مدیریت صدور رخداد «پاک شدن محتوا(گره ها و یال ها)ی گراف»
        /// </summary>
        protected virtual void OnGraphCleared()
        {
            if (GraphCleared != null)
                GraphCleared(this, EventArgs.Empty);
        }

        /// <summary>
        /// رخداد «خاتمه ی جمع شدن انیمیشنی گروه»؛
        /// این رخداد براساس گروهی که در خواست جمع شدن آن داده شده، صادر می‌شود، در نتیجه در صورت جمع‌شدن تودرتوی گروه‌ها، این رخداد فقط برای ریشه صادر خواهد شد.
        /// این رخداد برای جمع‌شدن‌هایی صادر می‌شود که به صورت انیمیشنی انجام می‌گیرند
        /// </summary>
        public event EventHandler<AnimatingCollapseGroupCompletedEventArgs> AnimatingCollapseGroupCompleted;
        /// <summary>
        /// عملگر صدور رخداد «خاتمه ی جمع شدن گروه»
        /// </summary>
        protected virtual void OnAnimatingCollapseGroupCompleted(GroupMasterVertex collapseRootVertex)
        {
            if (collapseRootVertex == null)
                throw new ArgumentNullException("collapseRootVertex");
            // صدور رخداد مربوطه در صورت نیاز
            if (AnimatingCollapseGroupCompleted != null)
                AnimatingCollapseGroupCompleted(this, new AnimatingCollapseGroupCompletedEventArgs(collapseRootVertex));
        }

        /// <summary>
        /// رخداد «خاتمه ی باز شدن انیمیشنی گروه»؛
        /// این رخداد براساس گروهی که درخواست باز شدن آن داده شده، صادر می‌شود، در نتیجه در صورت جمع‌باز شدن تودرتوی گروه‌ها، این رخداد فقط برای ریشه صادر خواهد شد.
        /// این رخداد برای باز‌شدن‌هایی صادر می‌شود که به صورت انیمیشنی انجام می‌گیرند
        /// </summary>
        public event EventHandler<AnimatingExpandGroupCompletedEventArgs> AnimatingExpandGroupCompleted;
        /// <summary>
        /// عملگر صدور رخداد «خاتمه ی باز شدن گروه»
        /// </summary>
        protected virtual void OnAnimatingExpandGroupCompleted(GroupMasterVertex expandRootVertex)
        {
            if (expandRootVertex == null)
                throw new ArgumentNullException("expandRootVertex");
            // صدور رخداد مربوطه در صورت نیاز
            if (AnimatingExpandGroupCompleted != null)
                AnimatingExpandGroupCompleted(this, new AnimatingExpandGroupCompletedEventArgs(expandRootVertex));
        }

        protected async void OnViewerInitializationCompleted()
        {
            await Task.Run(async () => { await Task.Delay(0); });
            if (ViewerInitializationCompleted != null)
                ViewerInitializationCompleted.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler ViewerInitializationCompleted;
        #endregion
        
        public Brush ForegroundBrush
        {
            get => (Brush)GetValue(ForegroundBrushProperty);
            set => SetValue(ForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty ForegroundBrushProperty = DependencyProperty.Register(nameof(ForegroundBrush),
            typeof(Brush), typeof(GraphViewer), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        /// <summary>
        /// این شی سازوکار داده ای گراف را فراهم می آورد
        /// </summary>
        private GraphData graph = new GraphData();
        /// <summary>
        /// این شی سازوکار منطقی گراف را فراهم می آورد
        /// </summary>
        private GraphLogic logicCore = new GraphLogic();

        /// <summary>
        /// Default image resolution
        /// </summary>
        public const double DefaultDPI = 96d;
        /// <summary>
        /// Set pixelformat of image.
        /// </summary>
        private static PixelFormat pixelFormat = PixelFormats.Pbgra32;

        /// <summary>
        /// Indicated weather viewer can add/remove any vertex/edge; ViewerInitializationCompleted event will invoke when the init. process completed.
        /// </summary>
        public bool IsViewerInitialized { get; private set; }

        #region آماده سازی
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public GraphViewer()
            : base()
        {
            // آماده سازی اجزای کنترل برای شروع به کار
            InitializeComponent();
            // مقداردهی اولیه نمونه منطق گراف
            area.SetLogicCore(logicCore);
            // جمع کردن کنترل مدیریت بزرگنمایی گراف جهت شروع به کار کنترل
            GraphX.Controls.ZoomControl.SetViewFinderVisibility(zoomControl, Visibility.Collapsed);
            // فعال‌سازی قابلیت ترکیب خودکار یال‌های همپوشان، بین دو گره
            overlapedEdgesComposition = true;

            IsViewerInitialized = false;
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
            logicCore.DefaultLayoutAlgorithm = GraphX.PCL.Common.Enums.LayoutAlgorithmTypeEnum.LinLog;
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams = logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            (logicCore.DefaultOverlapRemovalAlgorithmParams as OverlapRemovalParameters).HorizontalGap = 150;
            (logicCore.DefaultOverlapRemovalAlgorithmParams as OverlapRemovalParameters).VerticalGap = 150;
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None;
            logicCore.EdgeCurvingEnabled = true;
            logicCore.AsyncAlgorithmCompute = false;

            // برگرفته از مثال "Dynamic Graph"
            area.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromSeconds(0.5));

            // اعمال رخدادگردان ها جهت:

            // تنظیم صدور رخداد خاتمه پویانمایی جابجایی گره ها؛
            // این انتساب قبلا در عملکرد شروع پویانمنایی فراخوانی می شد (که ظاهرا
            // برگرفته از یکی از مثال های نمونه کد گراف ایکس بوده)، ولی
            // به خاطر بروز تداخل در پویانمایی های پشت سر هم یک بار و آن هم
            // اینجا فراخوانی می شود
            area.MoveAnimation.Completed += MoveAnimation_Completed;
            // تغییر بزرگنمایی پس از اعمال چینش جدید به کل گراف
            area.RelayoutFinished += area_RelayoutFinished;
            // افزودن امکان انتخاب با کشیدن و رها کردن
            zoomControl.AreaSelected += zoomControl_AreaSelected;

            zoomControl.PreviewMouseDown += zoomControl_PreviewMouseDown;
            // رفع مشکل جابجایی محتوای گراف با کلیک چپ روی فضای خالی آن و رها نشدن که کاربرپسندی را از بین می برد
            zoomControl.MouseDown += zoomControl_MouseDown;
            // افزودن رخدادگردان رها شدن کلید موس
            zoomControl.PreviewMouseUp += zoomControl_PreviewMouseUp;

            // TODO: فراخوانی این تابع خیلی اتفاق می‌افتد!
            zoomControl.PreviewMouseMove += ZoomControl_PreviewMouseMove;
            // پیکربندی بزرگنمایی دو مرحله ای (رفع مشکل عقب ماندن زوم از تغییرات غیرهمگام)
            zoomControl.ZoomAnimationCompleted += zoomControl_ZoomAnimationCompleted;
            // امکان تغییر آیکن گره‌های نقشه براساس سطح بزرگنمایی، به خاطر تداخل با بزرگنمایی و
            // قفل شدن گراف غیرفعال شد.
            //zoomControl.PropertyChanged += ZoomControl_PropertyChanged;
            // افزودن رخدادگردان های ارتباط منطق و نمایش گراف در
            // حذف/اضافه گره ها و یال ها؛ این رخدادگردان ها تضمین کننده
            // تطابق منطق و نمایش گراف هستند و مشکل گراف ایکس در عدم
            // تطابق منطق و نمایش گراف را پوشش می دهد.
            area.LogicCore.Graph.VertexAdded += GraphLogicCore_VertexAdded;
            area.LogicCore.Graph.EdgeAdded += GraphLogicCore_EdgeAdded;
            area.LogicCore.Graph.VertexRemoved += GraphLogicCore_VertexRemoved;
            area.LogicCore.Graph.EdgeRemoved += GraphLogicCore_EdgeRemoved;
            area.LogicCore.Graph.Cleared += GraphLogicCore_Cleared;
            area.VertexDoubleClick += Area_VertexDoubleClick;
            area.VertexClicked += Area_VertexClicked;
            area.EdgeClicked += Area_EdgeClicked;
            area.MouseUp += Area_MouseUp;

            area.ShowAllEdgesLabels(true);
            area.AlignAllEdgesLabels(true);
            area.EdgeLabelFactory = new DefaultEdgelabelFactory();
            area.InvalidateVisual();
            // آماده سازی کنترل بزرگنمایی
            InitZoomControl();
            InitFlowZoomControl();

            IsViewerInitialized = true;
            OnViewerInitializationCompleted();
        }

        public List<Edge> GetEdgesRelatedTo(object vertex)
        {
            throw new NotImplementedException();
        }

        private void Area_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // از آنجایی که پیاده‌سازی موجود گراف-ایکس، کلیک روی برچسب یال را نادیده
            // می‌گیرد و آن را «هندل» کرده، با شناسایی این وضعیت، صدور رخداد متناظر
            // شبیه‌سازی می‌شود
            if (e.ChangedButton == MouseButton.Right
                && IsDescendantOfAnAttachableEdgeLabelControl(Mouse.DirectlyOver))
            {
                OnEdgeRightClick();
            }
        }

        private void Area_EdgeClicked(object sender, EdgeClickedEventArgs args)
        {
            if (args.MouseArgs.ChangedButton == MouseButton.Right)
            {
                OnEdgeRightClick();
            }
            area.MoveToFront(args.Control);
        }

        private void Area_VertexClicked(object sender, VertexClickedEventArgs args)
        {
            if (args.MouseArgs.ChangedButton == MouseButton.Right)
            {
                OnVertexRightClick();
            }
            area.MoveToFront(args.Control);
        }

        ImageDetails lastReportedImageDetail
            = (ImageDetails)VertexControl.ImageDetailsProperty.DefaultMetadata.DefaultValue;
        double lastAppliedZoom = double.NaN;

        /// <summary></summary>
        /// <remarks>
        /// امکان تغییر آیکن گره‌های نقشه براساس سطح بزرگنمایی، به خاطر تداخل با بزرگنمایی و
        /// قفل شدن گراف غیرفعال شد.
        /// </remarks>
        private void ZoomControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // بخاطر فراوانی فراخوانی این تابع و سربار پردازشی، این شرط کامنت شده
            //if (e.PropertyName.Equals("Zoom"))
            //{

            if (!lastAppliedZoom.Equals(zoomControl.Zoom))
            {
                lastAppliedZoom = zoomControl.Zoom;
                ImageDetails currentZoomNeededImageDetail
                    = (zoomControl.Zoom <= 0.15)
                    ? ImageDetails.DoNotShowImage
                    : ImageDetails.ShowImage;

                if (lastReportedImageDetail != currentZoomNeededImageDetail)
                {
                    lastReportedImageDetail = currentZoomNeededImageDetail;
                    foreach (var vertex in Vertices)
                    {
                        vertex.RelatedVertexControl.ImageDetails = currentZoomNeededImageDetail;
                    }
                    foreach (var edge in Edges)
                    {
                        edge.RelatedEdgeControl.ImageDetails = currentZoomNeededImageDetail;
                    }
                }
            }

            //}
        }

        /// <summary>
        /// آماده سازی کنترل بزرگنمایی
        /// </summary>
        private void InitZoomControl()
        {
            // علارغم تنظیم ویژگی‌های بزرگنمایی (Zoom) و فاکتور بزرگنمایی، در زمان
            // شروع به کار گراف، بزرگنمایی اعمال نمی‌شود؛
            // این مساله می‌تواند برای افزودن اولین شئ به گراف مشکل ایجاد کند.

            // برای دور زدن این مشکل، یک گره موقت به گراف افزوده، بزرگنمایی
            // مطلوب اعمال و نهایتا از گراف حذف می‌شود.
            // بدین ترتیب بزرگنمایی موردنظر برای افزودن اولین گره واقعی به گراف
            // مشکلی پیش نخواهد آمد.
            Vertex initVertex = Vertex.VertexFactory("Init");
            area.AddVertex(initVertex, initVertex.RelatedVertexControl);
            initVertex.RelatedVertexControl.SetPosition(50, 50);
            ZoomToOriginal();
            area.RemoveVertex(initVertex);
        }

        private void GraphLogicCore_VertexAdded(Vertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            // گرهی که به منطق گراف اضافه شده، به نمایش گراف افزوده می شود
            if (!area.VertexList.ContainsKey(vertex))
                area.AddVertex(vertex, vertex.RelatedVertexControl);
        }
        private void GraphLogicCore_EdgeAdded(Edge edge)
        {
            if (edge == null)
                throw new ArgumentNullException("edge");

            // یالی که به منطق گراف اضافه شده، به نمایش گراف افزوده می شود
            if (!area.EdgesList.ContainsKey(edge))
            {
                area.InsertEdge(edge, edge.RelatedEdgeControl, 0, true);

                if (OverlapedEdgesComposition)
                    CombineOverlapedEdgesBetweenVertices(edge.Source, edge.Target);
            }
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
            // صدور رخداد مربوطه
            OnVertexRemoved(vertex);
        }
        private void GraphLogicCore_EdgeRemoved(Edge edge)
        {
            if (edge == null)
                throw new ArgumentNullException("edge");

            // یالی که از منطق گراف حذف شده، از نمایش گراف حذف می شود
            area.RemoveEdge(edge, true);
            //if (area.LogicCore.Graph.VertexCount != area.VertexList.Count || area.LogicCore.Graph.EdgeCount != area.EdgesList.Count)
            //    // TODO: External Bug | Graph Logic & Area may not balanced | Bug-Test: Use iterative removes instead of GraphClear()
            //    throw new Exception("Component internal error; Difference(s) between Graph Logic and Area found");
            // صدور رخداد مربوطه
            OnEdgeRemoved(edge);

            if (OverlapedEdgesComposition)
            {
                var compositionMasterEdge = GetCompoundEdgeMastersEdgeIfCombined(edge);
                if (compositionMasterEdge != null)
                {
                    compositionMasterEdge.InnerEdges.Remove(edge);
                    SeparateCompoundEdge(compositionMasterEdge);
                    CombineOverlapedEdgesBetweenVertices(edge.Source, edge.Target);
                }
            }
        }
        private void GraphLogicCore_Cleared(object sender, EventArgs e)
        {
            // محتوا (گره ها و یال ها)ی نمایش گراف حذف می شود
            area.ClearLayout();
            //if (area.LogicCore.Graph.VertexCount != area.VertexList.Count || area.LogicCore.Graph.EdgeCount != area.EdgesList.Count)
            //    // TODO: External Bug | Graph Logic & Area may not balanced | Bug-Test: Use iterative removes instead of GraphClear()
            //    throw new Exception("Component internal error; Difference(s) between Graph Logic and Area found");
            // صدور رخداد مربوطه
            OnGraphCleared();
        }
        #endregion

        #region ترکیب یال‌های همپوشان
        private bool overlapedEdgesComposition = true;
        /// <summary>
        /// خاصیت ترکیب یال‌ها در صورتی همپوشانی (روی هم افتادگی)؛
        /// اگر بین دو شئ بیش از یک یال وجود داشته باشد، یال‌ها
        /// به صورت ترکیب شده نمایش داده می‌شوند
        /// </summary>
        public bool OverlapedEdgesComposition
        {
            get
            {
                return overlapedEdgesComposition;
            }
        }

        /// <summary>
        /// در صورتی که بین دو گره، بیش از یک یال باشد، یال‌ها با هم ترکیب شده
        /// و به صورت یک یال مرکب نمایش داده می‌شوند
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        private void CombineOverlapedEdgesBetweenVertices(Vertex vertex1, Vertex vertex2)
        {
            var unhiddenEdgesBetweenVertices = GetEdgesBetween(vertex1, vertex2).Where(e => !IsHiddenEdge(e));
            if (unhiddenEdgesBetweenVertices.Take(2).Count() <= 1)
                return;
            foreach (CompoundEdge item in unhiddenEdgesBetweenVertices.Where(e => e is CompoundEdge))
            {
                SeparateCompoundEdge(item);
            }
            // تا اینجا، در صورت وجود یال(های) قبلا ترکیب شده، یال‌های داخلی آن(ها) جدا شده‌اند
            // به عبارت دیگر، تا اینجا، همه‌ی یال‌ها به صورت ساده بین دو گره برقرارند

            var compoundEdge = CompoundEdge.Factory(unhiddenEdgesBetweenVertices);

            foreach (var item in unhiddenEdgesBetweenVertices)
            {
                HideEdge(item);
            }
            AddEdge(compoundEdge);
        }
        /// <summary>
        /// این تابع یال‌های داخلی یک یال ترکیبی را جدا می‌کند؛
        /// این تابع یال‌های جدا شده را روی هم نمایش می‌دهد
        /// </summary>
        /// <param name="compoundEdge"></param>
        private void SeparateCompoundEdge(CompoundEdge compoundEdge)
        {
            foreach (var item in compoundEdge.InnerEdges)
            {
                if (item is CompoundEdge)
                    SeparateCompoundEdge(item as CompoundEdge);
                UnhideEdge(item);
            }

            // TODO: ExternalBug - متاسفانه، گراف ایکس در حذف بعضی یال‌ها درست عمل نمی‌کند و با فراخوانی
            // تابع حذف یال از منطق گراف، یال حذف نمی‌شود؛ برای رفع این مشکل یال پیش از حذف، مخفی می‌شود
            // تا درصورت ناسازگاری گراف ایکس، مشکلی برای عملکرد ترکیب و باز کردن یال‌ها پیش نیاید
            HideEdge(compoundEdge);

            RemoveEdge(compoundEdge);
        }
        /// <summary>
        /// </summary>
        /// <returns>در صورتی که قبلا ترکیب نشده باشد «نال» برمی‌گرداند</returns>
        private CompoundEdge GetCompoundEdgeMastersEdgeIfCombined(Edge edge)
        {
            var compoundEdges = Edges
                .Where(e => e is CompoundEdge)
                .Select(e => e as CompoundEdge);
            foreach (var item in compoundEdges)
            {
                if (item.InnerEdges.Contains(edge))
                    return item;
            }
            return null;
        }

        private IEnumerable<CompoundEdge> GetRelatedCompoundEdges(List<Vertex> vertices)
        {
            var result = new List<CompoundEdge>();
            foreach (var item in vertices)
                result.AddRange
                    (GetRelatedEdges(item)
                    .Where(e => e is CompoundEdge)
                    .Select(e => e as CompoundEdge));
            return result;
        }
        #endregion

        #region خصوصیات و رخدادگردان های «محلی» انتخاب گره ها و مدیریت گراف
        /// <summary>
        /// رخدادگردان انتخاب محدوده ای از گراف با استفاده از گرفتن و کشیدن؛ این رخداد در حالت عادی با گرفتن کلیدهای Control و Alt به همراه کشیدن موس انجام می شود
        /// </summary>
        private void zoomControl_AreaSelected(object sender, GraphX.Controls.AreaSelectedEventArgs args)
        {
            SelectGraphContentBySelectedArea(args.Rectangle);
        }

        private void SelectGraphContentBySelectedArea(Rect selectedAreaRect)
        {
            // منبع کد: مثال نمونه سازنده کنترل گراف
            // این قطعه کد امکان انتخاب چند گره را با گرفتن «کنترل» و «آلت» / «کنترل» (تنها) کشیدن و رها کردن همزمان ارائه می دهد
            DeselectAllEdges();
            List<VertexControl> vertexControlsToSelect = new List<VertexControl>();
            var r = selectedAreaRect;
            foreach (var item in area.VertexList)
            {
                var offset = item.Value.GetPosition();
                var irect = new Rect(offset.X, offset.Y, item.Value.ActualWidth, item.Value.ActualHeight);
                if (irect.IntersectsWith(r) && item.Value.Visibility == Visibility.Visible && !item.Key.RelatedVertexControl.IsFrozen)
                    vertexControlsToSelect.Add((VertexControl)item.Value);
            }
            InvertSelectVertexControls(vertexControlsToSelect);

            List<EdgeControl> edgeControlsToSelect = new List<EdgeControl>();
            // در صورت انتخاب شدن مبدا و مقصد یک یال، آن یال نیز به حالت انتخاب می رود
            foreach (var itemEdge in area.EdgesList)
            {
                if (((VertexControl)itemEdge.Value.Source).IsSelected
                    && ((VertexControl)itemEdge.Value.Target).IsSelected
                    && itemEdge.Value.Visibility == Visibility.Visible)
                    edgeControlsToSelect.Add((EdgeControl)itemEdge.Value);
            }
            SelectEdgeControls(edgeControlsToSelect);
        }

        /// <summary>
        /// خاصیت نگهداری موقعیت مکانی نشانگر موس در زمان آخرین فشرده شدن کلید موس
        /// </summary>
        private Point mouseDownPositionRelatedToViewer;
        /// <summary>
        /// رخدادگردان فشرده شدن کلیدی از موس روی محدوده ی زیرنظر کنترل مدیریت بزرگنمایی گراف
        /// </summary>
        private void zoomControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // موقعیت مکانی نشانگر موس در زمان آخرین فشرده شدن کلید موس  به روز می شود
            mouseDownPositionRelatedToViewer = e.GetPosition(this);
            // برگرفته از کد مبدا کنترل گراف ایکس
            //
            // برای پیاده‌سازی امکان انتخاب گره‌ها با گرفتن کنترل و 
            // کشیدن موس -که توسط گراف ایکس انجام نمی‌شود- و نیز برای
            // جلوگیری از تغییر در کدهای مبدا گراف ایکس، کدهای زیر از
            // گراف ایکس گرفته شده و اینجا افزوده شده‌اند
            if (Keyboard.Modifiers == ModifierKeys.Control
                && e.LeftButton == MouseButtonState.Pressed)
            {
                zoomControl.ModifierMode = GraphX.Controls.ZoomViewModifierMode.ZoomBox;
                startAsAreaSelection = true;
            }

            // TODO: ظاهرا این مشکل در نسخه‌های جدید رفع شده است => در صورت اطمینان کد حذف شود
            // درصورت در حالت فشرده بودن کلیک چپ موس،
            //if (e.LeftButton == MouseButtonState.Pressed)
            // از انتقال رخداد گردشی به کنترل های دیگر جلوگیری می شود؛
            // بدین ترتیب مشکل جابجایی محتوای گراف با کلیک چپ روی فضای خالی
            // آن و رها نشدن که کاربرپسندی را از بین می برد، رفع می شود
            //e.Handled = true;
        }

        private bool startAsAreaSelection;

        // TODO: فراخوانی این تابع خیلی اتفاق می‌افتد!
        private void ZoomControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // برگرفته از کد مبدا کنترل گراف ایکس
            //
            // برای پیاده‌سازی امکان انتخاب گره‌ها با گرفتن کنترل و 
            // کشیدن موس -که توسط گراف ایکس انجام نمی‌شود- و نیز برای
            // جلوگیری از تغییر در کدهای مبدا گراف ایکس، کدهای زیر از
            // گراف ایکس گرفته شده و اینجا افزوده شده‌اند
            if (zoomControl.ModifierMode == GraphX.Controls.ZoomViewModifierMode.ZoomBox
                && startAsAreaSelection)
            {
                var pos = e.GetPosition(this);
                var x = Math.Min(mouseDownPositionRelatedToViewer.X, pos.X);
                var y = Math.Min(mouseDownPositionRelatedToViewer.Y, pos.Y);
                var sizeX = Math.Abs(mouseDownPositionRelatedToViewer.X - pos.X);
                var sizeY = Math.Abs(mouseDownPositionRelatedToViewer.Y - pos.Y);
                zoomControl.ZoomBox = new Rect(x, y, sizeX, sizeY);
            }
        }

        private void zoomControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // پیاده‌سازی موجود کنترل گراف-ایکس رخداد را در
            // ZoomControl.PreviewMouseDown
            // هندل کرده کرده و سلسله فراخوانی‌های این رخداد (در حالت غیر دیباگ!) به
            // AttachableEdgeLabelControl.PreviewMouseDown
            // جلوگیری می‌کند؛ همچنین در پیاده‌سازی موجود کنترل، کلیک روی برچسب را نادیده
            // گرفته و کنترل زوم را به حالت جابجایی محتوای گراف می‌برد.
            //
            // کد زیر با اطمینان از کلیک شدن روی اجزای یک برچسب یال، پیاده‌سازی مشکل‌آفرین
            // را خنثی می‌کند!
            if (IsDescendantOfAnAttachableEdgeLabelControl(Mouse.DirectlyOver))
            {
                zoomControl.ModifierMode = GraphX.Controls.ZoomViewModifierMode.Pan;
            }
        }

        private bool IsDescendantOfAnAttachableEdgeLabelControl(IInputElement element)
        {
            if (element == null)
                return false;
            if (element is Visual)
            {
                foreach (DependencyObject dObj in area.GetChildControls<GraphX.Controls.AttachableEdgeLabelControl>())
                {
                    if ((element as Visual).IsDescendantOf(dObj))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// رخدادگردان رها شدن کلید موس از روی محیط گراف
        /// </summary>
        private void zoomControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // اگر موس با کلیدهای «کنترل» و «آلت» صفحه کلید روی گراف کشیده و رها شود، امکان
            // انتخاب چند گره (و یال) پیاده سازی شده است؛ ولی مشکل زمانی پیش می آید که این کلیدها
            // فشرده شده باشند و موس در همان نقطه فشرده شدن رها شود. در این صورت به خاطر آنکه
            // مستطیل تشکیل شده توسط کشیدن و رها کردن موس، پهنا و درازای صفر دارد، گراف‌ایکس
            // به استثناء بر می خورد.
            // برای رفع این مشکل (عدم تغییر در کد مبدا)، با استفاده از قطعه کد زیر، از
            // به وجود آمدن مربع خالی جلوگیری می شود.

            // اگر موقعیت مکانی موس در زمان فشرده شدن و رها شدن کلید آن جابجا نشده باشد،
            if (mouseDownPositionRelatedToViewer.Equals(e.GetPosition(this)))
            {
                // رفع مشکل زمان کشیدن و انتخاب کردن و عدم جابجایی موس (همراه با نگهداشتن
                // کلید های «آلت» و «کنترل»): شرط بالا و دستورات زیر، برای رفع مشکل مذکور
                // افزوده شده‌اند و در آن شرایط از به‌هم خوردن دید کنونی کاربر جلوگیری می‌کنند
                PreventZoomChange();

                // برای پیاده سازی امکان اینکه با کلیک روی فضای خالی گراف، همه گره ها و یال ها
                // از انتخاب خارج شوند، اگر موس طی فشردن و رها شدن جابجایی موقعیت مکانی نداشته
                // باشد، و کلیدهای ترکیبی کنترل (چپ یا راست) هم همراهی نداشته باشند می بایست
                // تمام گره ها و یال ها از حالت انتخاب خارج شوند

                // در صورت همراهی یکی از کلیدهای «کنترل» (چپ یا راست)، حال
                // انتخاب گره ای که کلیک شود، معکوس می گردد و فرایند پایان می پذیرد
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    // از آنجایی که پیاده‌سازی موجود گراف-ایکس، کلیک روی برچسب یال را نادیده
                    // می‌گیرد، با شناسایی این حالت، از رفتار نامناسب کنترل جلوگیری می‌شود
                    && !IsDescendantOfAnAttachableEdgeLabelControl(Mouse.DirectlyOver))
                {
                    // در غیر اینصورت همه گره ها و یال ها از انتخاب خارج شوند
                    DeselectAllVertices();
                    DeselectAllEdges();
                }
            }
            // برگرفته از کد مبدا کنترل گراف ایکس
            //
            // برای پیاده‌سازی امکان انتخاب گره‌ها با گرفتن کنترل و 
            // کشیدن موس -که توسط گراف ایکس انجام نمی‌شود- و نیز برای
            // جلوگیری از تغییر در کدهای مبدا گراف ایکس، کدهای زیر از
            // گراف ایکس گرفته شده و اینجا افزوده شده‌اند
            else
            {
                var zoomBoxRect = ToContentRectangle(zoomControl.ZoomBox);
                if (startAsAreaSelection)
                {
                    startAsAreaSelection = false;

                    SelectGraphContentBySelectedArea(zoomBoxRect);
                    // به خاطر تداخل بزرگنمایی گراف ایکس، این مقداردهی انجام شده تا
                    // دید کاربر حفظ شود
                    PreventZoomChange();
                }
                else
                {
                    if (zoomBoxRect.Width < 50 && zoomBoxRect.Height < 50)
                    {
                        PreventZoomChange();
                    }
                }
            }
        }

        private void PreventZoomChange()
        {
            // زوم را به محدوده‌ی کنونی برمی‌گرداند
            zoomControl.ModifierMode = GraphX.Controls.ZoomViewModifierMode.ZoomBox;
            zoomControl.ZoomBox = new Rect(0, 0, zoomControl.ActualWidth, zoomControl.ActualHeight);
            // جعبه‌ی زوم را از دید کاربر خارج می‌کند؛
            zoomControl.ModifierMode = GraphX.Controls.ZoomViewModifierMode.ZoomIn;
            zoomControl.ZoomBox = new Rect(0, 0, 0, 0);
        }

        /// <summary>
        /// Converts screen rectangle area to rectangle in content coordinate space according to scale and translation
        /// </summary>
        /// <param name="screenRectangle">Screen rectangle data</param>
        private Rect ToContentRectangle(Rect screenRectangle)
        {
            var tl = TranslatePoint(new Point(screenRectangle.X, screenRectangle.Y), zoomControl.ContentVisual);

            return new Rect(tl.X, tl.Y, screenRectangle.Width / zoomControl.Zoom, screenRectangle.Height / zoomControl.Zoom);
        }

        /// <summary>
        /// رخدادگردان اتمام اعمال بازچینش به کل (گره های) گراف (و نه گره های انتخاب شده)
        /// </summary>
        private void area_RelayoutFinished(object sender, EventArgs e)
        {
            // تلاش برای نمایش محتوای گراف در میانه ی فضای دید کاربر از گراف
            ZoomToCenterContent();
        }

        /// <summary>
        /// لیست گره های انتخاب شده را براساس لیست کنترل های گره انتخاب شده برمی گرداند
        /// </summary>
        public IEnumerable<Vertex> GetSelectedVertices()
        {
            return Vertices.Where(v => v.IsSelected);
        }

        private IEnumerable<VertexControl> GetSelectedVertexControls()
        {
            return GetSelectedVertices()
                .Select(v => v.RelatedVertexControl);
        }

        /// <summary>
        /// لیست یال های انتخاب شده را براساس لیست کنترل های یال انتخاب شده برمی گرداند
        /// </summary>
        public IEnumerable<Edge> GetSelectedEdges()
        {
            if (OverlapedEdgesComposition)
            {
                var innerEdgesOfSelectedCompoundEdges = Edges
                    .Where(e => e is CompoundEdge
                        && IsSelectedEdgeControl(e.RelatedEdgeControl))
                    .SelectMany(ce => (ce as CompoundEdge).InnerEdges);

                var selectedNonCompoundEdges = Edges
                    .Where(e => IsSelectedEdgeControl(e.RelatedEdgeControl)
                        && !(e is CompoundEdge));

                return selectedNonCompoundEdges
                    .Concat(innerEdgesOfSelectedCompoundEdges);
            }
            else
                return
                    Edges.Where(e => IsSelectedEdgeControl(e.RelatedEdgeControl));
        }

        private IEnumerable<EdgeControl> GetSelectedEdgeControls()
        {
            return Edges
                .Select(e => e.RelatedEdgeControl)
                .Where(ec => IsSelectedEdgeControl(ec));
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
        #endregion

        #region عملکردهای «خارجی» و «موروثی» انتخاب گره ها
        /// <summary>
        /// گره(راس)ها را به حالت انتخاب می برد
        /// </summary>
        public void SelectVertices(IEnumerable<Vertex> verticesToSelect)
        {
            if (verticesToSelect == null)
                throw new ArgumentNullException("verticesToSelect");

            SelectVertexControls(verticesToSelect.Select(v => v.RelatedVertexControl));
        }
        /// <summary>
        /// یک کنترل گره را به حالت انتخاب می برد
        /// </summary>
        protected void SelectVertexControls(IEnumerable<VertexControl> vertexControlsToSelect)
        {
            if (vertexControlsToSelect == null)
                throw new ArgumentNullException("vertexControlsToSelect");
            if (!vertexControlsToSelect.Any())
                return;

            foreach (var item in vertexControlsToSelect)
            {
                if (item.Visibility != Visibility.Visible || item.IsFrozen)
                    continue;

                ((Vertex)item.Vertex).IsSelected = true;
                area.MoveToFront(item);
            }
            OnVerticesSelectionChanged();
        }
        /// <summary>
        /// لینک(یال)ها را به حالت انتخاب می برد
        /// </summary>
        public void SelectEdge(IEnumerable<Edge> edgesToSelect)
        {
            if (edgesToSelect == null)
                throw new ArgumentNullException("edgeToSelect");

            SelectEdgeControls
                (edgesToSelect
                    .Where(e => e != null)
                    .Select(e => e.RelatedEdgeControl));
        }
        /// <summary>
        /// کنترل‌های لینک را به حالت انتخاب می برد
        /// </summary>
        protected void SelectEdgeControls(IEnumerable<EdgeControl> edgeControlsToSelect)
        {
            if (edgeControlsToSelect == null)
                throw new ArgumentNullException("edgeControlsToSelect");
            if (!edgeControlsToSelect.Any())
                return;

            foreach (var item in edgeControlsToSelect)
            {
                if (item.Visibility != Visibility.Visible)
                    continue;
                GraphX.Controls.HighlightBehaviour.SetHighlighted(item, true);
                GraphX.Controls.DragBehaviour.SetIsTagged(item, true);
                item.IsSelected = true;

                if (item.LabelControl != null)
                {
                    GraphX.Controls.HighlightBehaviour.SetHighlighted(item.LabelControl, true);
                    GraphX.Controls.DragBehaviour.SetIsTagged(item.LabelControl, true);
                }
                area.MoveToFront(item);
            }
            OnEdgesSelectionChanged();
        }
        /// <summary>
        /// لینکهای خروجی از گره های انتخاب شده را به حالت انتخاب می برد
        /// </summary>
        public void SelectOuterEdgesOfSelectedVertices()
        {
            List<EdgeControl> edgeControlsToSelect = new List<EdgeControl>();
            foreach (Edge e in Edges)
            {
                // از بین یالهای جهت دار، یالهایی یافت می شود که گره انتخابی مبدا آنها باشد
                if (e.Direction == EdgeDirection.FromSourceToTarget)
                {
                    if (e.Source.IsSelected)
                        edgeControlsToSelect.Add(e.RelatedEdgeControl);
                }
                else if (e.Direction == EdgeDirection.FromTargetToSource)
                {
                    if (e.Target.IsSelected)
                        edgeControlsToSelect.Add(e.RelatedEdgeControl);
                }
                // یالهای بدون جهتی که گره مبدا یا مقصد آنهاانتخاب شده باشند انتخاب می شوند
                else
                {
                    if (e.Target.IsSelected || e.Source.IsSelected)
                        edgeControlsToSelect.Add(e.RelatedEdgeControl);
                }
            }
            SelectEdgeControls(edgeControlsToSelect);
        }

        /// <summary>
        /// اشیاء خروجی از گره های انتخاب شده را به حالت انتخاب می برد
        /// </summary>
        public void SelectOuterObjectsOfSelectedVertices()
        {
            List<VertexControl> vertexControlsToSelect = new List<VertexControl>();
            foreach (Edge e in area.EdgesList.Keys)
            {
                if (e.Direction == EdgeDirection.FromSourceToTarget)
                {
                    if (e.Source.IsSelected)
                        vertexControlsToSelect.Add(e.Target.RelatedVertexControl);
                }
                else if (e.Direction == EdgeDirection.FromTargetToSource)
                {
                    if (e.Target.IsSelected)
                        vertexControlsToSelect.Add(e.Source.RelatedVertexControl);
                }
                else
                {
                    if (e.Target.IsSelected)
                        vertexControlsToSelect.Add(e.Source.RelatedVertexControl);
                    else if (e.Source.IsSelected)
                        vertexControlsToSelect.Add(e.Target.RelatedVertexControl);
                }
            }
            SelectVertexControls(vertexControlsToSelect);
        }
        public void SelectInnerEdgesOfSelectedVertices()
        {
            List<EdgeControl> edgeControlsToSelect = new List<EdgeControl>();
            foreach (Edge e in Edges)
            {
                /// از بین یالهای جهت دار، یالهایی یافت می شود که گره انتخابی مقصد آنها باشد
                if (e.Direction == EdgeDirection.FromSourceToTarget)
                {
                    if (e.Target.IsSelected)
                        edgeControlsToSelect.Add(e.RelatedEdgeControl);
                }
                else if (e.Direction == EdgeDirection.FromTargetToSource)
                {
                    if (e.Source.IsSelected)
                        edgeControlsToSelect.Add(e.RelatedEdgeControl);
                }
                /// یالهای بدون جهتی که گره انتخابی مبدا یا مقصد آنها باشند انتخاب می شوند
                else
                {
                    if (e.Target.IsSelected || e.Source.IsSelected)
                        edgeControlsToSelect.Add(e.RelatedEdgeControl);
                }
            }
            SelectEdgeControls(edgeControlsToSelect);
        }

        public void SelectInnerObjectsOfSelectedVertices()
        {
            List<VertexControl> verticesToSelect = new List<VertexControl>();
            foreach (Edge e in area.EdgesList.Keys)
            {
                if (e.Direction == EdgeDirection.FromSourceToTarget)
                {
                    if (e.Target.IsSelected)
                        verticesToSelect.Add(e.Source.RelatedVertexControl);
                }
                else if (e.Direction == EdgeDirection.FromTargetToSource)
                {
                    if (e.Source.IsSelected)
                        verticesToSelect.Add(e.Target.RelatedVertexControl);
                }
                else
                {
                    if (e.Target.IsSelected)
                        verticesToSelect.Add(e.Source.RelatedVertexControl);
                    else if (e.Source.IsSelected)
                        verticesToSelect.Add(e.Target.RelatedVertexControl);
                }
            }
            SelectVertexControls(verticesToSelect);
        }

        public void SelectLinksBetweenSelectedVertex()
        {
            List<EdgeControl> edgeControlsToSelect = new List<EdgeControl>();
            foreach (Edge e in Edges)
                if (e.Source.IsSelected && e.Target.IsSelected)
                    edgeControlsToSelect.Add(e.RelatedEdgeControl);
            SelectEdgeControls(edgeControlsToSelect);
        }

        public void SelectAllEdgesOfSelectedVertices()
        {
            List<EdgeControl> edgeControlsToSelect = new List<EdgeControl>();
            foreach (Vertex v in GetSelectedVertices())
                // تمام یالهایی که گره انتخابی مبدا یا مقصد آنها باشند انتخاب می شوند
                foreach (Edge e in GetRelatedEdges(v))
                    if (!edgeControlsToSelect.Contains(e.RelatedEdgeControl))
                        edgeControlsToSelect.Add(e.RelatedEdgeControl);
            SelectEdgeControls(edgeControlsToSelect);
        }

        public Point GetExpandModeSubGroupPositionRelatedToMasterOfCollapsedGroup(GroupMasterVertex groupMaster, Vertex subGroup)
        {
            Point subgroupPosition;
            if (!groupMaster.RelatedVertexControl
                .CollapsedSubGroupsExpandedModePositionRelatedToMaster
                .TryGetValue(subGroup, out subgroupPosition))
            {
                subgroupPosition = new Point(double.NaN, double.NaN);
            }
            return subgroupPosition;
        }

        public Visibility GetVertexVisiblity(Vertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            return vertex.RelatedVertexControl.Visibility;
        }

        /// <summary>
        /// همه کنترل گره (راس) ها را از حالت انتخاب خارج می کند
        /// </summary>
        public void DeselectAllVertices()
        {
            DeselectVertexControls(GetSelectedVertexControls());
        }
        /// <summary>
        /// گره(راس)های خاصی را از حالت انتخاب خارج می کند
        /// </summary>
        public void DeselectVertices(IEnumerable<Vertex> verticesToDeselect)
        {
            if (verticesToDeselect == null)
                throw new ArgumentNullException("verticesToDeselect");

            DeselectVertexControls(verticesToDeselect.Select(v => v.RelatedVertexControl));
        }
        /// <summary>
        /// کنترل گره (راس) های خاصی را از حالت انتخاب خارج می کند
        /// </summary>
        private void DeselectVertexControls(IEnumerable<VertexControl> vertexControlsToDeselect)
        {
            if (vertexControlsToDeselect == null)
                throw new ArgumentNullException("vertexControlToDeselect");

            foreach (var item in vertexControlsToDeselect)
            {
                ((Vertex)item.Vertex).IsSelected = false;
            }
            OnVerticesSelectionChanged();
        }
        /// <summary>
        /// لینک (یال) های خاصی را از حالت انتخاب خارج می کند
        /// </summary>
        public void DeselectEdges(IEnumerable<Edge> edgesToDeselect)
        {
            if (edgesToDeselect == null)
                throw new ArgumentNullException("edgesToDeselect");

            DeselectEdgeControls(edgesToDeselect.Select(e => e.RelatedEdgeControl));
        }
        /// <summary>
        /// کنترل لینک (یال) های خاصی را از حالت انتخاب خارج می کند
        /// </summary>
        protected void DeselectEdgeControls(IEnumerable<EdgeControl> edgeControlsToDeselect)
        {
            if (edgeControlsToDeselect == null)
                throw new ArgumentNullException("edgeControlToDeselect");

            foreach (var item in edgeControlsToDeselect)
            {
                GraphX.Controls.HighlightBehaviour.SetHighlighted(item, false);
                GraphX.Controls.DragBehaviour.SetIsTagged(item, false);
                item.IsSelected = false;
                if (item.LabelControl != null)
                {
                    GraphX.Controls.HighlightBehaviour.SetHighlighted(item.LabelControl, false);
                    GraphX.Controls.DragBehaviour.SetIsTagged(item.LabelControl, false);
                }
            }
            OnEdgesSelectionChanged();
        }
        /// <summary>
        /// همه گره هایی که هیچ یالی به آنها وصل نیست را انتخاب می کند
        /// </summary>
        public void SelectOrphansVertices()
        {
            DeselectAllVertices();
            Dictionary<Vertex, bool> vertexOrphanmentDictionary = new Dictionary<Foundations.Vertex, bool>(Vertices.Count);
            foreach (var vertex in Vertices)
            {
                vertexOrphanmentDictionary.Add(vertex, true);
            }
            foreach (var edge in Edges)
            {
                vertexOrphanmentDictionary[edge.Source] = false;
                vertexOrphanmentDictionary[edge.Target] = false;
            }
            SelectVertexControls(vertexOrphanmentDictionary.Where(n => n.Value == true).Select(n => n.Key.RelatedVertexControl));
        }
        /// <summary>
        /// گره هایی که انتخاب شده اند را از حالت انتخاب خارج می کند و گره هایی که انتخاب نشده اند را به حالت انتخاب می برد
        /// </summary>        
        public void InvertSelectionVertices()
        {
            InvertSelectVertexControls(Vertices.Select(v => v.RelatedVertexControl));
        }
        /// <summary>
        /// گره مورد نظر را در حالت انتخاب نگه میدارد و تمام گره هایی که به این گره، با هر نوع یالی وصل هستند را به حالت انتخاب می برد
        /// </summary> 
        public void ExpandNodeSelectiontoNextLinkedLevel()
        {
            List<VertexControl> vertexControlsToSelect = new List<VertexControl>();
            foreach (var edgeItem in Edges)
            {
                if (edgeItem.Source.IsSelected)
                    vertexControlsToSelect.Add(edgeItem.Target.RelatedVertexControl);
                if (edgeItem.Target.IsSelected)
                    vertexControlsToSelect.Add(edgeItem.Source.RelatedVertexControl);
            }
            SelectVertexControls(vertexControlsToSelect);
        }
        /// <summary>
        /// گره مورد نظر را از حالت انتخاب خارج می کند و تمام گره هایی که به این گره، با هر نوع یالی وصل هستند را به حالت انتخاب می برد
        /// </summary> 
        public void SetNodeSelectiontoNextLinkedLevel()
        {
            List<VertexControl> vertexControlsToSelect = new List<VertexControl>();
            List<VertexControl> vertexControlsToDeselect = new List<VertexControl>();
            foreach (var edgeItem in Edges)
            {
                if (edgeItem.Source.IsSelected && !edgeItem.Target.IsSelected)
                    vertexControlsToSelect.Add(edgeItem.Target.RelatedVertexControl);
                if (edgeItem.Target.IsSelected && !edgeItem.Source.IsSelected)
                    vertexControlsToSelect.Add(edgeItem.Source.RelatedVertexControl);
            }
            DeselectVertices(GetSelectedVertices());
            SelectVertexControls(vertexControlsToSelect);
        }

        public Point GetVertexPosition(Vertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            return vertex.RelatedVertexControl.GetPosition();
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

        /// <summary>
        /// تمام گره ها را به حالت انتخاب می برد
        /// </summary> 
        public void SelectAllVertices()
        {
            SelectVertexControls
                (Vertices
                    .Where(v => !v.IsSelected)
                    .Select(v => v.RelatedVertexControl));
        }

        /// <summary>
        /// تصویری از وضعیت در حال نمایش گراف می گیرد و برمی گرداند
        /// </summary>
        public RenderTargetBitmap TakeImageOfGraphCurrentShowingArea()
        {
            return TakeImageOfGraphCurrentShowingArea(true, 96d, 100);
        }

        private RenderTargetBitmap TakeImageOfGraphCurrentShowingArea(bool useZoomControlSurface, double imgdpi = DefaultDPI, int imgQuality = 100)
        {
            UIElement uIElement = GetUIElementForSnapshot(useZoomControlSurface);

            var renderBitmap =
                    new RenderTargetBitmap(
                    (int)(uIElement.DesiredSize.Width * (imgdpi / DefaultDPI)),
                    (int)(uIElement.DesiredSize.Height * (imgdpi / DefaultDPI)),
                    imgdpi,
                    imgdpi,
                    pixelFormat);

            //Render the graphlayout onto the bitmap.
            renderBitmap.Render(uIElement);
            return renderBitmap;
        }

        public UIElement GetUIElementForSnapshot(bool useZoomControlSurface)
        {
            UIElement uIElement = area;

            if (useZoomControlSurface)
            {
                if (area.Parent != null && area.Parent is GraphX.Controls.IZoomControl)
                    uIElement = (area.Parent as GraphX.Controls.IZoomControl).PresenterVisual;
                else if (area.Parent != null && area.Parent is FrameworkElement && (area.Parent as FrameworkElement).Parent is GraphX.Controls.IZoomControl)
                    uIElement = ((area.Parent as FrameworkElement).Parent as GraphX.Controls.IZoomControl).PresenterVisual;
            }

            return uIElement;
        }

        /// <summary>
        /// تمام لینک ها را به حالت انتخاب می برد
        /// </summary>
        public void SelectAllEdges()
        {
            SelectEdge(Edges);
        }
        /// <summary>
        /// تمام لینکها را از حالت انتخاب خارج می کند
        /// </summary>
        public void DeselectAllEdges()
        {
            DeselectEdgeControls(GetSelectedEdgeControls());
        }
        /// <summary>
        /// در حالت انتخاب بودن/نبودن مجموعه‌ای از کنترل گره ها را معکوس می کند
        /// </summary>
        protected void InvertSelectVertexControls(IEnumerable<VertexControl> vertexControlsInvertSelect)
        {
            if (vertexControlsInvertSelect == null)
                throw new ArgumentNullException("vertexControlsInvertSelect");
            if (!vertexControlsInvertSelect.Any())
                return;

            foreach (var item in vertexControlsInvertSelect)
            {
                if (item.Visibility != Visibility.Visible || item.IsFrozen)
                    continue;

                ((Vertex)item.Vertex).IsSelected = !((Vertex)item.Vertex).IsSelected;
            }
            OnVerticesSelectionChanged();
        }
        /// <summary>
        /// در حالت انتخاب بودن/نبودن یک لینک (یال) را معکوس می کند
        /// </summary> 
        public void InvertSelectionEdges()
        {
            InvertSelectEdgeControl(Edges.Select(e => e.RelatedEdgeControl));
        }
        /// <summary>
        /// در حالت انتخاب بودن/نبودن یک کنترل لینک (یال) را معکوس می کند
        /// </summary> 
        protected void InvertSelectEdgeControl(IEnumerable<EdgeControl> edgeControlsToInvertSelect)
        {
            if (edgeControlsToInvertSelect == null)
                throw new ArgumentNullException("edgeControlsToInvertSelect");
            if (!edgeControlsToInvertSelect.Any())
                return;

            foreach (var item in edgeControlsToInvertSelect)
            {
                if (item.Visibility != Visibility.Visible)
                    continue;
                GraphX.Controls.HighlightBehaviour.SetHighlighted(item, !GraphX.Controls.HighlightBehaviour.GetHighlighted(item));
                GraphX.Controls.DragBehaviour.SetIsTagged(item, GraphX.Controls.DragBehaviour.GetIsTagged(item));
                item.IsSelected = !item.IsSelected;
                if (item.LabelControl != null)
                {
                    GraphX.Controls.HighlightBehaviour.SetHighlighted(item.LabelControl, !GraphX.Controls.HighlightBehaviour.GetHighlighted(item.LabelControl));
                    GraphX.Controls.DragBehaviour.SetIsTagged(item.LabelControl, !GraphX.Controls.DragBehaviour.GetIsTagged(item.LabelControl));
                }
            }
            OnEdgesSelectionChanged();
        }

        public bool IsSelectedEdge(Edge edgeToSelect)
        {
            if (edgeToSelect == null)
                throw new ArgumentNullException("edgeToSelect");

            if (IsSelectedEdgeControl(edgeToSelect.RelatedEdgeControl))
            {
                return true;
            }
            else
            {
                if (OverlapedEdgesComposition == false)
                {
                    return false;
                }
                else
                {
                    return Edges
                        .Where(e => e is CompoundEdge && IsSelectedEdgeControl(e.RelatedEdgeControl))
                        .Any(ce => (ce as CompoundEdge).InnerEdges.Any(e => e.Equals(edgeToSelect)));
                }
            }
        }

        private bool IsSelectedEdgeControl(EdgeControl edgeControl)
        {
            if (edgeControl == null)
                throw new ArgumentNullException("edgeControl");

            return GraphX.Controls.HighlightBehaviour.GetHighlighted(edgeControl);
        }
        #endregion

        #region عملکردها و ویژگی های مدیریت منطق نمایشی | مدیریت گره ها و یال ها
        /// <summary>
        /// فاصله ثابت افقی بین گره های موجود و گره جدید؛
        /// این مقدار در زمان ایجاد شی جدید، برای نظم گراف استفاده می شود
        /// </summary>
        private const int GapBetweenNewVertexAndExistingVertices = 40;

        /// <summary>
        /// افزودن یک گره (راس) جدید روی گراف؛
        /// در صورت عدم تعیین موقعیت افقی «و» عمودی (هر دو) برای گره، آن را در موقعیتی تصادفی (و بدون روی هم افتادن گره ها) روی گراف قرار می دهد و در عین حال سعی می کند آن را در محدوده در حال نمایش قرار دهد
        /// </summary>
        public void AddVertex(Vertex vertexToAdd, double vertexPositionX = double.NaN, double vertexPositionY = double.NaN)
        {
            if (vertexToAdd == null)
                throw new ArgumentNullException(nameof(vertexToAdd));

            vertexToAdd.RelatedVertexControl.IsSelectedChanged -= VertexControlOnIsSelectedChanged;
            vertexToAdd.RelatedVertexControl.IsSelectedChanged += VertexControlOnIsSelectedChanged;

            Binding binding = new Binding(nameof(Foreground))
            {
                Source = this
            };
            vertexToAdd.RelatedVertexControl.SetBinding(ForegroundProperty, binding);

            if (vertexPositionX.Equals(double.NaN) || vertexPositionY.Equals(double.NaN))
            {
                // تلاش برای تنظیم موقعیت مکانی گره جدید در محلی که در دید کنونی کاربر و ترجیحا وسط آن باشد
                Point areaCenterPosition = GetCurrentViewCenterPosition();

                vertexToAdd.RelatedVertexControl.SetPosition(new Point
                    (areaCenterPosition.X - ((area.VertexList.Count == 1) ? 0 : 50) /*+ (new Random(DateTime.Now.Millisecond)).Next(100)*/
                    , areaCenterPosition.Y - ((area.VertexList.Count == 1) ? 0 : 50) /*+ (new Random(DateTime.Now.Millisecond * DateTime.Now.Second)).Next(100)*/));
                GraphX.GraphAreaBase.SetX(vertexToAdd.RelatedVertexControl, GraphX.GraphAreaBase.GetX(vertexToAdd.RelatedVertexControl) - vertexToAdd.RelatedVertexControl.ActualWidth / 2 - 5);
                GraphX.GraphAreaBase.SetY(vertexToAdd.RelatedVertexControl, GraphX.GraphAreaBase.GetY(vertexToAdd.RelatedVertexControl) - vertexToAdd.RelatedVertexControl.ActualHeight / 2 - 5);
            }
            else
                vertexToAdd.RelatedVertexControl.SetPosition(new Point(vertexPositionX, vertexPositionY));

            // افزودن گره به گراف به منطق گراف؛
            // پس از اضافه شدن این گره به منطق گراف و صدور رخداد مربوط به
            // آن، گره -توسط رخداد گردان- به چیدمان (نمایشی) گراف نیز اضافه خواهد شد
            // بدین ترتیب تا گرهی به منطق گراف اضافه نشود، به محتوای
            // نمایشی گراف اضافه نخواهد شد
            area.LogicCore.Graph.AddVertex(vertexToAdd);

            // تنظیم رخدادگردان های کلیک روی کنترل گره
            vertexToAdd.RelatedVertexControl.PreviewMouseUp += RelatedVertexControl_PreviewMouseUp;
            vertexToAdd.RelatedVertexControl.PreviewMouseDown += RelatedVertexControl_PreviewMouseDown;

            vertexToAdd.RelatedVertexControl.ImageDetails = lastReportedImageDetail;
        }

        private void VertexControlOnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GraphX.Controls.HighlightBehaviour.SetHighlighted((VertexControl)sender, (bool)e.NewValue);
            GraphX.Controls.DragBehaviour.SetIsTagged((VertexControl)sender, (bool)e.NewValue);
        }

        public void AddVertices(IEnumerable<VertexAddMetadata> addMetadatas)
        {
            if (addMetadatas == null)
                throw new ArgumentNullException(nameof(addMetadatas));

            foreach (VertexAddMetadata metadata in addMetadatas)
            {
                if (metadata.PositionX.Equals(double.NaN) || metadata.PositionY.Equals(double.NaN))
                {
                    // تلاش برای تنظیم موقعیت مکانی گره جدید در محلی که در دید کنونی کاربر و ترجیحا وسط آن باشد
                    Point areaCenterPosition = GetCurrentViewCenterPosition();

                    metadata.VertexToAdd.RelatedVertexControl.SetPosition(new Point
                        (areaCenterPosition.X - ((area.VertexList.Count == 1) ? 0 : 50) /*+ (new Random(DateTime.Now.Millisecond)).Next(100)*/
                        , areaCenterPosition.Y - ((area.VertexList.Count == 1) ? 0 : 50) /*+ (new Random(DateTime.Now.Millisecond * DateTime.Now.Second)).Next(100)*/));
                    GraphX.GraphAreaBase.SetX(metadata.VertexToAdd.RelatedVertexControl, GraphX.GraphAreaBase.GetX(metadata.VertexToAdd.RelatedVertexControl) - metadata.VertexToAdd.RelatedVertexControl.ActualWidth / 2 - 5);
                    GraphX.GraphAreaBase.SetY(metadata.VertexToAdd.RelatedVertexControl, GraphX.GraphAreaBase.GetY(metadata.VertexToAdd.RelatedVertexControl) - metadata.VertexToAdd.RelatedVertexControl.ActualHeight / 2 - 5);
                }
                else
                    metadata.VertexToAdd.RelatedVertexControl.SetPosition(new Point(metadata.PositionX, metadata.PositionY));
            }

            // افزودن گره به گراف به منطق گراف؛
            // پس از اضافه شدن این گره به منطق گراف و صدور رخداد مربوط به
            // آن، گره -توسط رخداد گردان- به چیدمان (نمایشی) گراف نیز اضافه خواهد شد
            // بدین ترتیب تا گرهی به منطق گراف اضافه نشود، به محتوای
            // نمایشی گراف اضافه نخواهد شد
            area.LogicCore.Graph.AddVertexRange(addMetadatas.Select(m => m.VertexToAdd));

            // تنظیم رخدادگردان های کلیک روی کنترل گره
            foreach (Vertex vertexToAdd in addMetadatas.Select(m => m.VertexToAdd))
            {
                vertexToAdd.RelatedVertexControl.IsSelectedChanged -= VertexControlOnIsSelectedChanged;
                vertexToAdd.RelatedVertexControl.IsSelectedChanged += VertexControlOnIsSelectedChanged;

                vertexToAdd.RelatedVertexControl.PreviewMouseUp += RelatedVertexControl_PreviewMouseUp;
                vertexToAdd.RelatedVertexControl.PreviewMouseDown += RelatedVertexControl_PreviewMouseDown;

                vertexToAdd.RelatedVertexControl.ImageDetails = lastReportedImageDetail;
            }
        }

        private void Area_VertexDoubleClick(object sender, VertexSelectedEventArgs args)
        {
            OnVertexDoubleClick(args.VertexControl as VertexControl);
            args.MouseArgs.Handled = true;
        }

        public void AnimateVerticesAdd(IEnumerable<Vertex> addedVertices)
        {
            Dictionary<Vertex, GraphX.Measure.Point> vp = new Dictionary<Vertex, GraphX.Measure.Point>();
            foreach (var vertex in addedVertices)
            {
                Point position = vertex.RelatedVertexControl.GetPosition();
                vp.Add(vertex, new GraphX.Measure.Point(position.X, position.Y));
                Point newPosition = new Point(position.X + 5, position.Y + 35);
                vertex.RelatedVertexControl.SetPosition(newPosition);
            }
            AnimateVerticesMove(vp);
        }

        public void SetZoomFocusOnVertex(Vertex vertexToFocus)
        {
            var vertexPosition = vertexToFocus.RelatedVertexControl.GetPosition();
            Rect zoomRect = new Rect(vertexPosition.X + vertexToFocus.RelatedVertexControl.ActualWidth / 2 - zoomControl.Viewport.Width / 2
                , vertexPosition.Y + vertexToFocus.RelatedVertexControl.ActualHeight / 2 - zoomControl.Viewport.Height / 2
                , zoomControl.Viewport.Width
                , zoomControl.Viewport.Height);
            ZoomToContent(zoomRect);
        }

        /// <summary>
        /// بزرگنمایی کنونی را به شکلی تغییر می‌دهد که گره‌های تعیین شده در ورودی در محدوده نمایشی کنونی قرار گیرند؛
        /// این عملکرد سعی می‌کند برای جلوگیری از گیج شدن استفاده کننده، کمترین تغییر را در وضعیت نمایشی کنونی ایجاد کند.
        /// </summary>
        /// <param name="vertexToFocus"></param>
        /// <remarks>
        /// بزرگنمایی این عملکرد، در حال حاضر، بدون پویانمایی انجام می‌شود که این مساله از طرف کنترل پایه مورد استفاده جهت نمایشی گراف است.
        /// </remarks>
        public void ZoomOutToCoverVertices(IEnumerable<Vertex> vertexToFocus)
        {
            if (vertexToFocus == null)
                throw new ArgumentNullException("vertexToFocus");

            if (double.IsInfinity(zoomControl.Viewport.Width) || double.IsInfinity(zoomControl.Viewport.Height))
                return;

            // تفکیک گره‌هایی که در محدوده‌ی نمایشی کنونی گراف قرار ندارند
            IEnumerable<Vertex> outOfZoomViewVertices = vertexToFocus.Where(v => v != null && !IsVertexCoveredByCurrentZoom(v));
            // درصورتی که چنین گره‌هایی بین گره‌های ورودی نباشد، نیاز به هیچ تغییری نیست
            if (!outOfZoomViewVertices.Any())
                return;
            // ابعاد بزرگنمایی کنونی
            Rect currentZoomRect = new Rect
                (zoomControl.TranslatePoint(new Point(0, 0), area).X
                , zoomControl.TranslatePoint(new Point(0, 0), area).Y
                , zoomControl.Viewport.Width
                , zoomControl.Viewport.Height);
            // مقداردهی اولیه ابعاد جدید بزرگنمایی براساس بزرگنمایی کنونی
            Rect newZoomRect = new Rect(currentZoomRect.X, currentZoomRect.Y, currentZoomRect.Width, currentZoomRect.Height);
            // تعداد پیکسل ثابتی به عنوان حاشیه بزرگنمایی در نظر گرفته می‌شود
            // تا پس از تغییر بزرگنمایی گره‌ها کمی از لبه گراف فاصله داشته باشند
            const int postZoomOutMarginPixels = 10;
            // ابعاد جدید بزرگنمایی با هر یک از گره‌های خارج از محدوده مقایسه شده و براساس آن‌ها به‌روز می‌شود
            foreach (var item in outOfZoomViewVertices)
            {
                GraphX.Measure.Rect itemRect = new GraphX.Measure.Rect(GetVertexControlPosition(item.RelatedVertexControl), GetVertexSize(item));
                if (itemRect.X < newZoomRect.X)
                {
                    newZoomRect.Width += newZoomRect.X - itemRect.X + postZoomOutMarginPixels;
                    newZoomRect.X = itemRect.X - postZoomOutMarginPixels;
                }
                if (itemRect.Y < newZoomRect.Y)
                {
                    newZoomRect.Height += newZoomRect.Y - itemRect.Y + postZoomOutMarginPixels;
                    newZoomRect.Y = itemRect.Y - postZoomOutMarginPixels;
                }
                if (itemRect.BottomRight.X > newZoomRect.X + newZoomRect.Width)
                    newZoomRect.Width = itemRect.BottomRight.X - newZoomRect.X + postZoomOutMarginPixels;
                if (itemRect.BottomRight.Y > newZoomRect.Y + newZoomRect.Height)
                    newZoomRect.Height = itemRect.BottomRight.Y - newZoomRect.Y + postZoomOutMarginPixels;
            }

            // در نهایت، بزرگنمایی جدید روی گراف اعمال می‌شود
            ZoomToContent(newZoomRect);
        }

        private void ZoomToContent(Rect newZoomRect, bool usingContentCoordinates = true, bool applyWithAnimation = true)
        {
            // برگرفته از کدهای داخلی
            // GraphX (ver 2.1.7)
            // Assembly:    GraphX.WPF.Controls
            // Class:       GraphX.Conrtols.ZoomControl
            // Method(s):   ZoomToContent (and it's sub-methods)
            //
            // کد مربوطه در کنترل اصلی انیمیشن نداشت و باعث سردر گمی کاربر می‌شد؛
            // در نتیجه با استفاده از کدهای داخلی آن سعی شده این پویانمایی برای
            // این نوع از بزرگنمایی پیاده‌سازی شود


            // translate the region from the coordinate space of the content 
            // to the coordinate space of the content presenter
            var region = usingContentCoordinates ?
                new Rect(
                  zoomControl.ContentVisual.TranslatePoint(newZoomRect.TopLeft, zoomControl.Presenter),
                  zoomControl.ContentVisual.TranslatePoint(newZoomRect.BottomRight, zoomControl.Presenter)) : newZoomRect;

            // calculate actual zoom, which must fit the entire selection 
            // while maintaining a 1:1 ratio
            var aspectX = ActualWidth / region.Width;
            var aspectY = ActualHeight / region.Height;
            var newRelativeScale = aspectX < aspectY ? aspectX : aspectY;
            // ensure that the scale value alls within the valid range
            if (newRelativeScale > zoomControl.MaxZoom)
                newRelativeScale = zoomControl.MaxZoom;
            else if (newRelativeScale < zoomControl.MinZoom)
                newRelativeScale = zoomControl.MinZoom;

            var center = new Point(newZoomRect.X + newZoomRect.Width / 2, newZoomRect.Y + newZoomRect.Height / 2);
            var newRelativePosition = new Point((ActualWidth / 2 - center.X) * zoomControl.Zoom, (ActualHeight / 2 - center.Y) * zoomControl.Zoom);

            // شرط اجرا با پویانمایی توسط ما افروده شده
            if (applyWithAnimation)
            {
                DoZoomAnimation(newRelativeScale, newRelativePosition.X, newRelativePosition.Y);
            }
            else
            {
                // و این کد، کد اصلی منبع می‌باشد که فاقد پویانمایی است
                zoomControl.TranslateX = newRelativePosition.X;
                zoomControl.TranslateY = newRelativePosition.Y;
                zoomControl.Zoom = newRelativeScale;
            }
        }

#if DEBUG
        private void ShowPonit(Rect newZoomRect)
        {
            ShowPonit(newZoomRect.TopLeft);
            ShowPonit(newZoomRect.TopRight);
            ShowPonit(newZoomRect.BottomLeft);
            ShowPonit(newZoomRect.BottomRight);
        }
        private void ShowPonit(Point point)
        {
            AddVertex(Vertex.VertexFactory(point.ToString()), point.X, point.Y);
        }
#endif

        private void DoZoomAnimation(double targetZoom, double transformX, double transformY, bool isZooming = true)
        {
            // برگرفته از کدهای داخلی
            // GraphX (ver 2.3.6)
            // Assembly:    GraphX.WPF.Controls
            // Class:       GraphX.Conrtols.ZoomControl
            // Method(s):   DoZoomAnimation (and it's sub-methods)
            //
            // کد مربوطه در کنترل اصلی انیمیشن نداشت و باعث سردر گمی کاربر می‌شد؛
            // در نتیجه با استفاده از کدهای داخلی آن سعی شده این پویانمایی برای
            // این نوع از بزرگنمایی پیاده‌سازی شود


            if (targetZoom == 0d && double.IsNaN(transformX) && double.IsNaN(transformY)) return;

            //_isZooming = isZooming;
            if (!zoomControl.IsAnimationEnabled)
            {
                zoomControl.SetCurrentValue(GraphX.Controls.ZoomControl.TranslateXProperty, transformX);
                zoomControl.SetCurrentValue(GraphX.Controls.ZoomControl.TranslateYProperty, transformY);
                zoomControl.SetCurrentValue(GraphX.Controls.ZoomControl.ZoomProperty, targetZoom);
                //ZoomCompleted(this, null);
                return;
            }
            var duration = new Duration(zoomControl.AnimationLength);
            var value = (double)zoomControl.GetValue(GraphX.Controls.ZoomControl.TranslateXProperty);
            if (double.IsNaN(value) || double.IsInfinity(value)) zoomControl.SetValue(GraphX.Controls.ZoomControl.TranslateXProperty, 0d);
            value = (double)zoomControl.GetValue(GraphX.Controls.ZoomControl.TranslateYProperty);
            if (double.IsNaN(value) || double.IsInfinity(value)) zoomControl.SetValue(GraphX.Controls.ZoomControl.TranslateYProperty, 0d); ;

            // این بخش از کد نسبت به کد اصلی تغییر یافته؛ اصلی به دلیل نامعلومی
            // در ترکیب سه پویانمایی محور افق، محور عمود و یزرگنمایی، تداخل دارد
            // و نسبت به اعمال این بزرگنمایی در حالت فاقد پویانمایی، رفتاری
            // متفاوت دارد؛ در نتیجه با بررسی شرایط مختلف اعمال این پویانمایی
            // رفتاری صحیح در روش زیر دیده شد که ابتدا پویانمایی محورهای افق و
            // عمود اعمال شوند و پس از پایان آن پویانمایی بزرگنمایی اعمال شود
            //
            // همچنین با توجه به شبیه‌سازی رفتار کننترل از بیرون، برای جلوگیری از
            // احتمال بروز تداخل با کدهای داخلی، در حین اجرای این پویانمایی‌ها، گراف
            // غیرفعال می‌شود

            zoomControl.IsEnabled = false;

            // شروع پویانمایی در محور افق
            DoubleAnimation XAnim = GetNewDoubleAnimation(transformX, duration);
            StartAnimation(GraphX.Controls.ZoomControl.TranslateXProperty, XAnim);
            // شروع پویانمایی در محور عمود
            DoubleAnimation YAnim = GetNewDoubleAnimation(transformY, duration);
            if (double.IsNaN(transformY) || double.IsInfinity(transformY)) transformY = 0;
            YAnim.Completed += (s, arg) =>
            {
                // شروع پویانمایی بزرگنمایی، پس از خاتمه‌ی پویانمایی محورهای افق و عمود
                if (double.IsNaN(targetZoom) || double.IsInfinity(targetZoom)) targetZoom = 1;
                DoubleAnimation ZoomAnim = GetNewDoubleAnimation(targetZoom, duration);
                StartAnimation(GraphX.Controls.ZoomControl.ZoomProperty, ZoomAnim);

                zoomControl.IsEnabled = true;
            };
            StartAnimation(GraphX.Controls.ZoomControl.TranslateYProperty, YAnim);
        }
        private DoubleAnimation GetNewDoubleAnimation(double toValue, Duration duration)
        {
            if (double.IsNaN(toValue) || double.IsInfinity(toValue))
            {
                //if (dp == ZoomProperty)
                //{
                //    _isZooming = false;
                //}
                return null;
            }
            var animation = new DoubleAnimation(toValue, duration);
            Timeline.SetDesiredFrameRate(animation, 30);

            return new DoubleAnimation(toValue, duration);
        }
        private void StartAnimation(DependencyProperty dp, DoubleAnimation animation)
        {
            if (animation == null)
                return;

            //if (dp == ZoomProperty)
            //{
            //    _zoomAnimCount++;
            //    animation.Completed += ZoomCompleted;
            //}
            zoomControl.BeginAnimation(dp, animation, HandoffBehavior.Compose);
        }

        protected bool IsVertexCoveredByCurrentZoom(Vertex vertexToCheck)
        {
            if (vertexToCheck == null)
                throw new ArgumentNullException("vertexToCheck");

            if (double.IsInfinity(zoomControl.Viewport.Width) || double.IsInfinity(zoomControl.Viewport.Height))
                return false;

            GraphX.Measure.Rect vertexRect = new GraphX.Measure.Rect(GetVertexControlPosition(vertexToCheck.RelatedVertexControl), GetVertexSize(vertexToCheck));
            Point point = zoomControl.TranslatePoint(new Point(0, 0), area);
            GraphX.Measure.Rect zoomRect = new GraphX.Measure.Rect(point.X, point.Y, zoomControl.Viewport.Width, zoomControl.Viewport.Height);
            return zoomRect.Contains(vertexRect);
        }

        /// <summary>
        /// آخرین موقعیت آخرین کنترل گره کلیک شده را نگه می دارد
        /// </summary>
        private Point lastClickVertexPosition;
        /// <summary>
        /// نمونه شی آخرین کنترل گره کلیک شده را نگه می دارد
        /// </summary>
        private VertexControl lastClickedVertexControl;
        /// <summary>
        /// کنترل گره کلیک شده و موقعیت کنونی آن را موقتا ذخیره می کند
        /// </summary>
        /// <param name="sender">کنترل گره کلیک شده</param>
        /// <param name="positionToStore">موقعیت نسبت به محیط گراف (گراف اریا) یی که کنترل گره را در برگرفته است</param>
        private void StoreLastClickedVertexPosition(VertexControl sender, Point positionToStore)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (positionToStore == null)
                throw new ArgumentNullException("positionToStore");

            lastClickVertexPosition = positionToStore;
            lastClickedVertexControl = sender;
        }
        /// <summary>
        /// بررسی می کند که آیا کنترل گره، همان کنترل سابقا کلیک شده است و آیا موقعیت مکانی آن روی گراف تغییر کرده یا خیر
        /// </summary>
        /// <param name="sender">کنترل گرهی که مقایسه روی آن انجام خواهد گرفت</param>
        /// <param name="newPositionRelativeToMasterArea">موقعیت کنونی نسبت به محیط گراف (گراف اریا) یی که کنترل گره را در برگرفته است</param>
        /// <returns>درصورت یکی بودن آخرین کنترل گره کلیک شده (که کلیک  موس روی آن فشرده شده) با کنترل گره کنونی و نیز جابجا شدن آن، مقدار «صحیح» و در غیر اینصورت مقدار «غلط» برگردانده می شود</returns>
        private bool IsLastClickedVervtexMoved(VertexControl sender, Point newPositionRelativeToMasterArea)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (newPositionRelativeToMasterArea == null)
                throw new ArgumentNullException("newPositionRelativeToMasterArea");

            return (lastClickVertexPosition != null && lastClickedVertexControl != null) &&
                (lastClickedVertexControl == sender &&
                !lastClickVertexPosition.Equals(newPositionRelativeToMasterArea));
        }

        /// <summary>
        /// رخدادگردان فشرده شدن کلیک موس روی یک کنترل گره
        /// </summary>
        private void RelatedVertexControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // ذخیره سازی موقت کنترل گره کلیک شده و موقعیت کنونی آن
            StoreLastClickedVertexPosition(sender as VertexControl, e.GetPosition(area));
        }
        /// <summary>
        /// رخدادگردان رها شدن کلیک موس از روی یک کنترل گره
        /// </summary>
        private void RelatedVertexControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            VertexControl vertexControl = (sender as VertexControl);
            bool isMouseUppedVertexOneOfSelectedVertices = vertexControl.IsSelected;

            if (!(e.ChangedButton == MouseButton.Right && isMouseUppedVertexOneOfSelectedVertices))
            {
                bool vertexMoved = IsLastClickedVervtexMoved(vertexControl, e.GetPosition(area));
                if (!vertexMoved)
                {
                    // در صورت همراهی یکی از کلیدهای «کنترل» (چپ یا راست) حالت انتخاب گره ای که کلیک شود، معکوس می گردد و فرایند پایان می پذیرد
                    if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                    {
                        // در غیر اینصورت همه گره‌ها و یال‌ها عدم انتخاب می شوند
                        DeselectAllVertices();
                        DeselectAllEdges();
                    }
                    // و در نهایت گرهی که روی آن کلیک شده، در وضعیت عکس وضع انتخاب کنونی اش قرار می گیرد
                    InvertSelectVertexControls(new VertexControl[] { sender as VertexControl });
                }
            }
        }

        /// <summary>
        /// افزودن یک یال جدید بین در گره (راس) روی گراف
        /// این عملگر از نمونه «کنترل یال» داده شده در شی یال استفاده می کند
        /// </summary>
        public void AddEdge(Edge edgeToAdd)
        {
            if (edgeToAdd == null)
                throw new ArgumentNullException(nameof(edgeToAdd));

            AddEdgeRange(new Edge[] { edgeToAdd });
        }

        public void AddEdgeRange(IEnumerable<Edge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException(nameof(edges));

            // افزودن یال به منطق گراف؛
            // پس از اضافه شدن این یال به منطق گراف و صدور رخداد مربوط به
            // آن، یال -توسط رخداد گردان- به چیدمان (نمایشی) گراف نیز اضافه خواهد شد
            // بدین ترتیب تا یالی به منطق گراف اضافه نشود، به محتوای
            // نمایشی گراف اضافه نخواهد شد
            area.LogicCore.Graph.AddEdgeRange(edges);
            foreach (Edge edge in edges)
            {
                edge.RelatedEdgeControl.PreviewMouseUp += EdgeControl_PreviewMouseUp;
                if (edge.RelatedEdgeControl.LabelControl != null)
                {
                    edge.RelatedEdgeControl.LabelControl.PreviewMouseUp += EdgeLabelControl_PreviewMouseUp;
                }
            }
        }

        private void EdgeLabelControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ApplyPreviewMouseUpForEdgeControl((EdgeControl)(((GraphX.Controls.AttachableEdgeLabelControl)sender).AttachNode), e);
        }

        private void EdgeControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ApplyPreviewMouseUpForEdgeControl((EdgeControl)sender, e);
        }

        private void ApplyPreviewMouseUpForEdgeControl(EdgeControl clickedEdgeControl, MouseButtonEventArgs mouseArguments)
        {
            bool isMouseUppedEdgeOneOfSelectedEdges = GetSelectedEdgeControls().Any(ec => ec == clickedEdgeControl);

            // درصورتی که کلیک فشرده شده کلیک راست باشد، نیازی به تغییر وضعیت انتخاب گره‌ها و یال‌ها نیست
            if (!(mouseArguments.ChangedButton == MouseButton.Right && isMouseUppedEdgeOneOfSelectedEdges))
            {
                // در صورت همراهی یکی از کلیدهای «کنترل» (چپ یا راست) حالت انتخاب یالی که کلیک شود، معکوس می گردد و فرایند پایان می پذیرد
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    // در غیر اینصورت همه گره‌ها و یال‌ها عدم انتخاب می شوند
                    DeselectAllVertices();
                    DeselectAllEdges();
                }
                // و در نهایت یالی که روی آن کلیک شده، در وضعیت عکس وضع انتخاب کنونی اش قرار می گیرد
                InvertSelectEdgeControl(new EdgeControl[] { clickedEdgeControl });
            }
        }

        /// <summary>
        /// گره (راس) های در حال نمایش گراف را برمی گرداند
        /// </summary>
        public ICollection<Vertex> Vertices
        { get { return area.VertexList.Keys; } }
        /// <summary>
        /// یال های در حال نمایش گراف را برمی گرداند
        /// </summary>
        public ICollection<Edge> Edges
        { get { return area.EdgesList.Keys; } }

        /// <summary>
        /// یک گره (راس) خاص را از روی گراف حذف می کند
        /// </summary>
        public void RemoveVertex(Vertex vertexToRemove)
        {
            if (vertexToRemove == null)
                throw new ArgumentNullException("vertexToRemove");

            // آماده سازی
            if (vertexToRemove.IsSelected)
            {
                DeselectVertexControls(new VertexControl[] { vertexToRemove.RelatedVertexControl });
            }
            // حذف یال های مرتبط با گره از گراف
            RemoveAllEdgesRelatedWith(vertexToRemove);
            // تنظیم رخدادگردان های کلیک روی کنترل گره
            vertexToRemove.RelatedVertexControl.PreviewMouseUp -= RelatedVertexControl_PreviewMouseUp;
            vertexToRemove.RelatedVertexControl.PreviewMouseDown -= RelatedVertexControl_PreviewMouseDown;
            // حذف گره از منطق گراف؛
            // پس از حذف شدن این گره از منطق گراف و صدور رخداد مربوط به
            // آن، گره -توسط رخداد گردان- از چیدمان (نمایشی) گراف نیز حذف خواهد شد
            // بدین ترتیب تا گرهی از منطق گراف حذف نشود، از محتوای
            // نمایشی گراف حذف نخواهد شد
            area.LogicCore.Graph.RemoveVertex(vertexToRemove);
        }
        /// <summary>
        /// یک یال خاص را از روی گراف حذف می کند
        /// </summary>
        public void RemoveEdge(Edge edgeToRemove)
        {
            if (edgeToRemove == null)
                throw new ArgumentNullException("edgeToRemove");

            // آماده سازی
            DeselectEdgeControls(new EdgeControl[] { edgeToRemove.RelatedEdgeControl });

            edgeToRemove.RelatedEdgeControl.PreviewMouseUp -= EdgeControl_PreviewMouseUp;
            if (edgeToRemove.RelatedEdgeControl.LabelControl != null)
            {
                edgeToRemove.RelatedEdgeControl.LabelControl.PreviewMouseUp -= EdgeLabelControl_PreviewMouseUp;
            }
            // حذف یال از منطق گراف؛
            // پس از حذف شدن این یال از منطق گراف و صدور رخداد مربوط به
            // آن، یال -توسط رخداد گردان- از چیدمان (نمایشی) گراف نیز حذف خواهد شد
            // بدین ترتیب تا یالی از منطق گراف حذف نشود، از محتوای
            // نمایشی گراف حذف نخواهد شد
            area.LogicCore.Graph.RemoveEdge(edgeToRemove);
        }
        /// <summary>
        /// پاک کردن محتوا(گره ها و یال ها)ی گراف
        /// </summary>
        public void ClearGraph()
        {
            // آماده سازی
            DeselectAllEdges();
            DeselectAllVertices();
            // پاک کردن محتوای منطق گراف در حال نمایش؛
            // پس از پاک شدن محتوای منطق گراف و صدور رخداد مربوط
            // به آن، چیدمان (نمایشی) گراف -توسط رخدادگردان- پاک خواهد شد؛
            // بدین ترتیب تا محتوای منطق گراف پاک نشود، محتوای
            // نمایشی گراف پاک نخواهد شد
            area.LogicCore.Graph.Clear();
            ZoomToOriginal();
        }

        public void UnhideEdge(Edge edge)
        {
            SetEdgeVisiblity(edge, Visibility.Visible);
        }
        public void HideEdge(Edge edge)
        {
            SetEdgeVisiblity(edge, Visibility.Hidden);
        }
        public bool IsHiddenEdge(Edge edge)
        {
            return GetEdgeVisiblity(edge) == Visibility.Hidden;
        }

        /// <summary>
        /// تمام یال های مرتبط با یک گره را از روی گراف حذف می کند
        /// </summary>
        protected void RemoveAllEdgesRelatedWith(Vertex vertexToRemove)
        {
            if (vertexToRemove == null)
                throw new ArgumentNullException("vertexToRemove");

            // حذف یال های مرتبط با گره مورد نظر (مبدا یا مقصد یال این گره باشد) از گراف
            List<Edge> relatedEdges = GetRelatedEdges(vertexToRemove);
            for (int i = relatedEdges.Count - 1; i >= 0; i--)
            {
                RemoveEdge(relatedEdges[i]);
            }
        }
        #endregion

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
        public void RelayoutVertices(IEnumerable<Vertex> verticesToRelayout, LayoutAlgorithms.LayoutAlgorithmTypeEnum layoutToApply, ILayoutParameters layoutParameters = null, bool zoomOutToCoverRelayoutedVertices = true, bool applyWithAnimation = true)
        {
            if (verticesToRelayout == null)
                throw new ArgumentNullException(nameof(verticesToRelayout));
            if (!IsViewerInitialized)
                throw new InvalidOperationException("Graph-Viewer is not initialized.");

            if (verticesToRelayout.Take(2).Count() <= 1)
                return;

            // تبدیل مقادیر ورودی به مقادیر فایل استفاده برای الگوریتم
            GraphData gd = new GraphData();
            Dictionary<Vertex, GraphX.Measure.Point> vp = new Dictionary<Vertex, GraphX.Measure.Point>();
            Dictionary<Vertex, GraphX.Measure.Size> vs = new Dictionary<Vertex, GraphX.Measure.Size>();
            foreach (Vertex item in verticesToRelayout)
            {
                if (!vp.ContainsKey(item))
                {
                    gd.AddVertex(item);
                    vp.Add(item, GetVertexControlPosition(item.RelatedVertexControl));
                    vs.Add(item, GetVertexSize(item));
                }
            }
            foreach (Edge checkingEdge in Edges)
                if (verticesToRelayout.Contains(checkingEdge.Source) && verticesToRelayout.Contains(checkingEdge.Target))
                    gd.AddEdge(checkingEdge);

            // آماده سازی الگوریتم چینش
            ILayoutAlgorithm alg = layoutAlgorithmFactory.CreateLayoutAlgorithm(layoutToApply, gd, vp, vs, layoutParameters);
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
        #endregion

        /// <summary>
        /// گروه را جمع می کند؛
        /// جمع/باز کردن عملی نمایشی ست که کاربر را قادر می کند جزئیات عضویت گره ها در یک گروه را نمایان یا مخفی کند
        /// </summary>
        /// <param name="groupVertexToCollapse">گره میزبان گروهی که می خواهیم جمع شود</param>
        /// <remarks>
        /// پس از فراخوانی این عملکرد، زیرگروه های گروه از وضعیت انتخاب شده خارج خواهند شد
        /// </remarks>
        public void CollapseGroup(GroupMasterVertex groupVertexToCollapse, bool collapseWithAnimation = true)
        {
            if (groupVertexToCollapse == null)
                throw new ArgumentNullException("groupVertexToCollapse");

            DeselectVertices(groupVertexToCollapse.SubGroup);
            DeselectEdges(groupVertexToCollapse.RelatedVertexControl.GetGroupInternalEdges(this, groupVertexToCollapse));

            if (OverlapedEdgesComposition)
                // جداسازی تمام یال‌هایی که به گره‌های در حال جمع شدن، متصل باشند و نیز در حالت ترکیب شده هستند
                foreach (var compoundEdgeRelatedToGrandSubGroup in GetRelatedCompoundEdges(GetGrandSubGroups(groupVertexToCollapse)))
                    SeparateCompoundEdge(compoundEdgeRelatedToGrandSubGroup);

            if (collapseWithAnimation)
                groupVertexToCollapse.RelatedVertexControl.CollapseGroupCompleted += RelatedVertexControl_CollapseGroupCompleted;
            groupVertexToCollapse.RelatedVertexControl.CollapseGroup(this, collapseWithAnimation);
        }

        private void RelatedVertexControl_CollapseGroupCompleted(object sender, EventArgs e)
        {
            var vertexControl = sender as VertexControl;
            var groupMasterVertex = vertexControl.Vertex as GroupMasterVertex;

            vertexControl.CollapseGroupCompleted -= RelatedVertexControl_CollapseGroupCompleted;
            OnAnimatingCollapseGroupCompleted(groupMasterVertex);

            if (OverlapedEdgesComposition)
            {
                // ترکیب تمام گره‌های روی هم افتاده‌ی متصل به میزبان گروه، غیر از زیر گروه‌ها
                foreach (var nonsubgroupRelatedVertex in GetRelatedVertices(groupMasterVertex)
                       .Where(v => !groupMasterVertex.SubGroup.Contains(v)))
                {
                    CombineOverlapedEdgesBetweenVertices(groupMasterVertex, nonsubgroupRelatedVertex);
                }
            }
        }

        public Size GetVertexActualSize(Vertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");
            GraphX.Measure.Size size = GetVertexSize(vertex);
            return new Size(size.Width, size.Height);
        }

        public Rect GetVertexActualRect(Vertex vertex)
        {
            return new Rect(GetVertexPosition(vertex), GetVertexActualSize(vertex));
        }

        /// <summary>
        /// گروه را باز می کند؛
        /// جمع/باز کردن عملی نمایشی ست که کاربر را قادر می کند جزئیات عضویت گره ها در یک گروه را نمایان یا مخفی کند
        /// </summary>
        /// <param name="groupVertexToCollapse">گره میزبان گروهی که می خواهیم باز شود</param>
        /// <remarks>
        /// پس از فراخوانی این عملکرد، در صورتی که میزبان گروه در وضعیت انتخاب شده باشد، اعضای آن نیز به وضعیت انتخاب شده خواهند رفت
        /// </remarks>
        public void ExpandGroup(GroupMasterVertex groupVertexToExpand, bool expandWitAnimation = true)
        {
            if (groupVertexToExpand == null)
                throw new ArgumentNullException("groupVertexToExpand");

            if (OverlapedEdgesComposition)
                // جداسازی تمام یال‌هایی که به میزبان گروه متصل باشند و نیز در حالت ترکیب شده هستند
                foreach (CompoundEdge compoundEdgeRelatedToGroupMaster in GetRelatedEdges(groupVertexToExpand).Where(e => e is CompoundEdge))
                    SeparateCompoundEdge(compoundEdgeRelatedToGroupMaster);

            if (expandWitAnimation)
                groupVertexToExpand.RelatedVertexControl.ExpandGroupCompleted += RelatedVertexControl_ExpandGroupCompleted;
            groupVertexToExpand.RelatedVertexControl.ExpandGroup(this, expandWitAnimation);

            if (groupVertexToExpand.IsSelected)
                SelectVertices(groupVertexToExpand.SubGroup);
        }

        private void RelatedVertexControl_ExpandGroupCompleted(object sender, EventArgs e)
        {
            var vertexControl = sender as VertexControl;
            var groupMasterVertex = vertexControl.Vertex as GroupMasterVertex;

            vertexControl.ExpandGroupCompleted -= RelatedVertexControl_ExpandGroupCompleted;
            OnAnimatingExpandGroupCompleted(groupMasterVertex);

            if (OverlapedEdgesComposition)
            {
                foreach (var nonsubgroupRelatedVertex in GetRelatedVertices(groupMasterVertex)
                    .Where(v => !groupMasterVertex.SubGroup.Contains(v)))
                {
                    CombineOverlapedEdgesBetweenVertices(groupMasterVertex, nonsubgroupRelatedVertex);
                }
                // ترکیب تمام گره‌های روی هم افتاده‌ی متصل به (میزبان گروه و) زیرگروه‌ها
                foreach (var subgroup in groupMasterVertex.SubGroup)
                    foreach (var vertexRelatedToSubGroup in GetRelatedVertices(subgroup))
                        CombineOverlapedEdgesBetweenVertices(subgroup, vertexRelatedToSubGroup);
            }
        }
        /// <summary>
        /// باز شدن (عکس جمع شدن) گره های گروه بندی شده به صورت آبشاری (تودرتو) بدون پویانمایی
        /// </summary>
        /// <param name="groupToExpand">گروهی که می خواهیم باز شود</param>
        public void ExpandGroupCascadely(GroupMasterVertex groupToExpand)
        {
            if (groupToExpand == null)
                throw new ArgumentNullException("groupToExpand");

            ExpandGroup(groupToExpand, false);
            foreach (var item in groupToExpand.SubGroup
                .Where(sub => sub is GroupMasterVertex && IsGroupInCollapsedViewing(sub as GroupMasterVertex)))
            {
                ExpandGroupCascadely(item as GroupMasterVertex);
            }
        }

        /// <summary>
        /// باز شدن (عکس جمع شدن) گره های گروه بندی شده به صورت آبشاری (تودرتو) برای رسیدن یکی از زیرگروه‌ها، بدون پویانمایی
        /// </summary>
        public void ExpandGroupCascadelyToExpandVertex(GroupMasterVertex groupToStartExpanding, Vertex goalVertexToStopExpandWith)
        {
            if (groupToStartExpanding == null)
                throw new ArgumentNullException("groupToExpand");
            if (goalVertexToStopExpandWith == null)
                throw new ArgumentNullException("goalVertexOfExpand");

            if (!IsGroupInCollapsedViewing(groupToStartExpanding))
                throw new ArgumentException("Group is not in collapse view", "groupToExpand");
            if (!GetGrandSubGroups(groupToStartExpanding).Contains(goalVertexToStopExpandWith))
                return;

            ExpandGroup(groupToStartExpanding, false);
            foreach (var item in groupToStartExpanding.SubGroup
                .Where(sub => sub is GroupMasterVertex && IsGroupInCollapsedViewing(sub as GroupMasterVertex)))
            {
                ExpandGroupCascadelyToExpandVertex(item as GroupMasterVertex, goalVertexToStopExpandWith);
            }
        }

        protected List<Vertex> GetGrandSubGroups(GroupMasterVertex groupMaster)
        {
            if (groupMaster == null)
                throw new ArgumentNullException("groupMaster");

            List<Vertex> result = new List<Vertex>();
            // افزودن زیرگروه‌های تودرتوی زیرگروه هایی که خودشان میزبان گروه اند، به لیست نتیجه
            foreach (var item in groupMaster.SubGroup
                .Where(sub => sub is GroupMasterVertex))
            {
                result.AddRange(GetGrandSubGroups(item as GroupMasterVertex));
            }
            // افزودن همه زیرگروه ها به لیست نتیجه
            result.AddRange(groupMaster.SubGroup);
            return result;
        }

        /// <summary>
        /// در وضعیت جمع شده بودن یا نبودن یک گره میزبان گروه را برمی گرداند
        /// </summary>
        public bool IsGroupInCollapsedViewing(GroupMasterVertex groupVertexToCheck)
        {
            if (groupVertexToCheck == null)
                throw new ArgumentNullException("groupVertexToCheck");

            return groupVertexToCheck.RelatedVertexControl.CollapseStatus == VertexControl.CollapseState.GroupCollapsed;
        }

        /// <summary>
        /// در وضعیت باز شده بودن یا نبودن یک گره میزبان گروه را برمی گرداند
        /// </summary>
        public bool IsGroupInExpandedViewing(GroupMasterVertex groupVertexToCheck)
        {
            if (groupVertexToCheck == null)
                throw new ArgumentNullException("groupVertexToCheck");

            return groupVertexToCheck.RelatedVertexControl.CollapseStatus == VertexControl.CollapseState.GroupExpanded;
        }

        public bool IsVertexCurrentlyCollapsedByAGroup(Foundations.Vertex checkingVertex, GroupMasterVertex ignoreGroupmMster = null)
        {
            if (checkingVertex == null)
                throw new ArgumentNullException("checkingVertex");

            if (ignoreGroupmMster != null)
            {
                foreach (var item in Vertices
                        .Where(v => v is GroupMasterVertex && IsGroupInCollapsedViewing(v as GroupMasterVertex) && !v.Equals(ignoreGroupmMster))
                        .Select(v => (v as GroupMasterVertex).SubGroup))
                    if (item.Contains(checkingVertex))
                        return true;
            }
            else
            {
                foreach (var item in Vertices
                      .Where(v => v is GroupMasterVertex && IsGroupInCollapsedViewing(v as GroupMasterVertex))
                      .Select(v => (v as GroupMasterVertex).SubGroup))
                    if (item.Contains(checkingVertex))
                        return true;
            }
            return false;
        }
        public GroupMasterVertex GetVertexMostTopCollapsedGroup(Foundations.Vertex checkingVertex)
        {
            if (checkingVertex == null)
                throw new ArgumentNullException("checkingVertex");
            if (!IsVertexCurrentlyCollapsedByAGroup(checkingVertex))
                throw new ArgumentException("Vertex is not collapsed in a group", "checkingVertex");

            Vertex immediateGroupMaster = Vertices
                  .Single(v => v is GroupMasterVertex
                      && IsGroupInCollapsedViewing(v as GroupMasterVertex)
                      && (v as GroupMasterVertex).SubGroup.Contains(checkingVertex));
            if (!IsVertexCurrentlyCollapsedByAGroup(immediateGroupMaster))
                return immediateGroupMaster as GroupMasterVertex;
            else
                return GetVertexMostTopCollapsedGroup(immediateGroupMaster);
        }

        public void AnimateVerticesMove(IDictionary<Vertex, Point> verticesFinalPosition)
        {
            var dic = new Dictionary<Vertex, GraphX.Measure.Point>();
            foreach (var item in verticesFinalPosition)
            {
                dic.Add(item.Key, new GraphX.Measure.Point(item.Value.X, item.Value.Y));
            }
            AnimateVerticesMove(dic);
        }

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
            if (!IsViewerInitialized)
                throw new InvalidOperationException("Graph-Viewer is not initialized.");

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

        public void SetVertexPosition(Vertex vertex, double x, double y)
        {
            vertex.RelatedVertexControl.SetPosition(x, y);
        }

        /// <summary>
        /// رخدادگردان خاتمه اجرای پویانمایی جابجایی گره ها
        /// </summary>
        private void MoveAnimation_Completed(object sender, EventArgs e)
        {
            OnVerticesMoveAnimationCompleted(area.MoveAnimation.VertexStorage.Keys.Select(gc => (gc as VertexControl).Vertex as Vertex).ToList());
        }

        /// <summary>
        /// لیست یال های «در حال نمایش» روی گراف بین دو شی داده شده را برمی گرداند
        /// </summary>
        internal List<Edge> GetEdgesBetween(Vertex vertex1, Vertex vertex2)
        {
            if (vertex1 == null)
                throw new ArgumentNullException("vertex1");
            if (vertex2 == null)
                throw new ArgumentNullException("vertex2");

            List<Edge> result = new List<Edge>();
            foreach (var item in Edges)
            {
                if ((item.Source == vertex1 && item.Target == vertex2)
                    || (item.Source == vertex2 && item.Target == vertex1))
                {
                    result.Add(item);
                }
            }
            return result;
        }
        /// <summary>
        /// لیست یال های مرتبط با یک گره خاص (یال هایی که مبدا یا مقصدشان یک گره خاص باشد) را برمی گرداند
        /// </summary>
        /// <param name="vertexToCheckAppearanceInEdge">گرهی که می خواهیم یال های مرتبطش را بگیریم</param>
        internal List<Edge> GetRelatedEdges(Vertex vertexToCheckAppearanceInEdge)
        {
            if (vertexToCheckAppearanceInEdge == null)
                throw new ArgumentNullException("vertexToCheckAppearanceInEdge");

            List<Edge> result = new List<Edge>();
            foreach (var item in Edges)
            {
                if (item.Source == vertexToCheckAppearanceInEdge || item.Target == vertexToCheckAppearanceInEdge)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public List<Edge> GetEdgesRelatedTo(Vertex vertexToCheckAppearanceInEdge)
        {
            return GetRelatedEdges(vertexToCheckAppearanceInEdge)
                .Where(e => !(e is CompoundEdge))
                .ToList();
        }

        /// <summary>
        /// گره‌هایی که با گره داده شده رابطه داشته باشند (یالی بین آن‌ها باشد) را برمی‌گرداند
        /// </summary>
        /// <param name="vertexToCheckRelationWithOtherVertices"></param>
        /// <returns></returns>
        internal List<Vertex> GetRelatedVertices(Vertex vertexToCheckRelationWithOtherVertices)
        {
            if (vertexToCheckRelationWithOtherVertices == null)
                throw new ArgumentNullException("vertexToCheckRelationWithOtherVertices");

            var result = new List<Vertex>();
            foreach (var item in Edges)
                if (item.Source == vertexToCheckRelationWithOtherVertices && !result.Contains(item.Target))
                    result.Add(item.Target);
                else if (item.Target == vertexToCheckRelationWithOtherVertices && !result.Contains(item.Source))
                    result.Add(item.Source);
            return result;
        }

        public void SetVertexVisiblity(Vertex vertex, Visibility visibility)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            vertex.RelatedVertexControl.Visibility = visibility;
        }

        protected void SetEdgeVisiblity(Edge edge, Visibility visibility)
        {
            if (edge == null)
                throw new ArgumentNullException("edge");

            edge.RelatedEdgeControl.Visibility = visibility;
        }
        protected Visibility GetEdgeVisiblity(Edge edge)
        {
            if (edge == null)
                throw new ArgumentNullException("edge");

            return edge.RelatedEdgeControl.Visibility;
        }

        /// <summary>
        /// گره(راس)ها را به حالت منجمد می‌برد
        /// </summary>
        /// <remarks>در حالت منجمد گره‌ها به صورت محو نمایش داده می‌شوند و امکان انتخاب و جابجایی آن‌ها غیرفعال می‌شود تا زمانی که از انجماد خارج شوند</remarks>
        public void FreezeVertices(IEnumerable<Vertex> verticesToFreeze)
        {
            if (verticesToFreeze == null)
                throw new ArgumentNullException("verticesToFreeze");

            FreezeVertexControls(verticesToFreeze.Select(v => v.RelatedVertexControl));
        }
        /// <summary>
        /// کنترل گره(راس)ها را به حالت منجمد می‌برد
        /// </summary>
        /// <remarks>در حالت منجمد گره‌ها به صورت محو نمایش داده می‌شوند و امکان انتخاب و جابجایی آن‌ها غیرفعال می‌شود تا زمانی که از انجماد خارج شوند</remarks>
        private void FreezeVertexControls(IEnumerable<VertexControl> vertexControlsToFreeze)
        {
            if (vertexControlsToFreeze == null)
                throw new ArgumentNullException("vertexControlsToFreeze");

            DeselectVertexControls(vertexControlsToFreeze);
            foreach (var item in vertexControlsToFreeze)
            {
                item.IsFrozen = true;
            }
        }
        /// <summary>
        /// همه‌ی گره(راس)ها را از حالت منجمد خارج می‌کند
        /// </summary>
        /// <remarks>در حالت منجمد گره‌ها به صورت محو نمایش داده می‌شوند و امکان انتخاب و جابجایی آن‌ها غیرفعال می‌شود تا زمانی که از انجماد خارج شوند</remarks>
        public void DefreezeAllVertices()
        {
            DefreezeVertexControls(Vertices.Select(v => v.RelatedVertexControl));
        }

        /// <summary>
        /// گره(راس)ها را از حالت منجمد خارج می‌کند
        /// </summary>
        /// <remarks>در حالت منجمد گره‌ها به صورت محو نمایش داده می‌شوند و امکان انتخاب و جابجایی آن‌ها غیرفعال می‌شود تا زمانی که از انجماد خارج شوند</remarks>
        public void DefreezeVertices(IEnumerable<Vertex> verticesToDefreeze)
        {
            if (verticesToDefreeze == null)
                throw new ArgumentNullException("verticesToDefreeze");

            DefreezeVertexControls(verticesToDefreeze.Select(v => v.RelatedVertexControl));
        }
        /// <summary>
        /// کنترل گره(راس)ها را به حالت منجمد می‌برد
        /// </summary>
        /// <remarks>در حالت منجمد گره‌ها به صورت محو نمایش داده می‌شوند و امکان انتخاب و جابجایی آن‌ها غیرفعال می‌شود تا زمانی که از انجماد خارج شوند</remarks>
        private void DefreezeVertexControls(IEnumerable<VertexControl> vertexControlsToDefreeze)
        {
            if (vertexControlsToDefreeze == null)
                throw new ArgumentNullException("vertexControlsToDefreeze");

            foreach (var item in vertexControlsToDefreeze)
                item.IsFrozen = false;
        }

        public Point GetCurrentViewCenterPosition()
        {
            return zoomControl.TranslatePoint(new Point(zoomControl.ActualWidth / 2, zoomControl.ActualHeight / 2), area);
        }
    }
}