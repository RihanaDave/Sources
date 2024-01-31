using GraphX.Measure;
using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Graph.GraphViewer.LayoutAlgorithms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Graph.GraphViewerTest
{
    [TestClass]
    public class SortedLinerLayoutAlgorithmTest
    {
        private const LayoutAlgorithmTypeEnum algorithmEnum = LayoutAlgorithmTypeEnum.SortedLiner;

        [TestCategory("الگوریتم‌های چینش گراف")]
        [TestMethod]
        public void AvoidNullInputs()
        {
            // Assign
            GraphData graph = new GraphData();
            Dictionary<Vertex, Point> verticesPositions = new Dictionary<Vertex, Point>();
            Dictionary<Vertex, Size> verticesSizes = new Dictionary<Vertex, Size>();
            AlgorithmFactory af = new AlgorithmFactory();
            SortedLinerLayoutAlgorithm alg = null;

            // Act
            // بررسی عملکرد سازنده (فکتوری) الگوریتم
            try
            {
                alg = (SortedLinerLayoutAlgorithm)af.CreateLayoutAlgorithm(algorithmEnum, null, null, null, null);
            }
            catch (ArgumentNullException)
            { }
            try
            {
                alg = (SortedLinerLayoutAlgorithm)af.CreateLayoutAlgorithm(algorithmEnum, graph, null, null, null);
            }
            catch (ArgumentNullException)
            { }
            try
            {
                alg = (SortedLinerLayoutAlgorithm)af.CreateLayoutAlgorithm(algorithmEnum, graph, verticesPositions, null, null);
            }
            catch (ArgumentNullException)
            { }
            try
            {
                alg = (SortedLinerLayoutAlgorithm)af.CreateLayoutAlgorithm(algorithmEnum, graph, verticesPositions, verticesSizes, null);
            }
            catch (ArgumentNullException)
            { }
            // Assert
            //  در صورت عدم بروز استثنائی غیر موارد رخدادگردانی شده بالا، تست پاس شده می‌باشد
        }

        [TestCategory("الگوریتم‌های چینش گراف")]
        [TestMethod]
        public void ComputeWithZeroVertices()
        {
            // Assign
            GraphData graph = new GraphData();
            Dictionary<Vertex, Point> verticesPositions = new Dictionary<Vertex, Point>();
            Dictionary<Vertex, Size> verticesSizes = new Dictionary<Vertex, Size>();
            AlgorithmFactory af = new AlgorithmFactory();
            SortedLinerLayoutAlgorithm alg = null;
            // Act
            alg = (SortedLinerLayoutAlgorithm)af.CreateLayoutAlgorithm(algorithmEnum, graph, verticesPositions, verticesSizes, af.CreateLayoutParameters(algorithmEnum));
            alg.Compute(new System.Threading.CancellationToken());
            // Assert
            Assert.IsNotNull(alg, "عملکرد سازنده الگوریتم (فکتوری) قادر به ایجاد الگوریتم نیست");
            Assert.IsNotNull(alg.VertexPositions, "دیکشنری موقعیت گره‌ها نال است");
            Assert.IsNotNull(alg.VertexSizes, "دیکشنری اندازه گره‌ها نال است");
            Assert.IsTrue(alg.VertexPositions.Count == 0, "دیکشنری موقعیت گره‌ها مانند ورودی خالی نیست");
            Assert.IsTrue(alg.VertexSizes.Count == 0, "دیکشنری اندازه گره‌ها مانند ورودی خالی نیست");
        }

        [TestCategory("الگوریتم‌های چینش گراف")]
        [TestMethod]
        public void ComputeWithOneVertices()
        {
            // Assign
            GraphData graph = new GraphData();
            Dictionary<Vertex, Point> verticesPositions = new Dictionary<Vertex, Point>();
            Dictionary<Vertex, Size> verticesSizes = new Dictionary<Vertex, Size>();
            AlgorithmFactory af = new AlgorithmFactory();
            SortedLinerLayoutAlgorithm alg = null;
            Vertex v1 = Vertex.VertexFactory("v1");
            // Act
            verticesPositions.Add(v1, new Point(50, 50));
            verticesSizes.Add(v1, new Size(35, 35));
            graph.AddVertex(v1);
            alg = (SortedLinerLayoutAlgorithm)af.CreateLayoutAlgorithm(algorithmEnum, graph, verticesPositions, verticesSizes, af.CreateLayoutParameters(algorithmEnum));
            alg.Compute(new System.Threading.CancellationToken());
            // Assert
            Assert.IsNotNull(alg, "عملکرد سازنده الگوریتم (فکتوری) قادر به ایجاد الگوریتم نیست");
            Assert.IsNotNull(alg.VertexPositions, "دیکشنری موقعیت گره‌ها نال است");
            Assert.IsNotNull(alg.VertexSizes, "دیکشنری اندازه گره‌ها نال است");
            Assert.IsTrue(alg.VertexPositions.Count == verticesPositions.Count(), "تعداد عناصر دیکشنری موقعیت گره‌ها با دیکشنری ورودی تطابق ندارد");
            Assert.IsTrue(alg.VertexSizes.Count == verticesSizes.Count(), "تعداد عناصر دیکشنری اندازه گره‌ها با دیکشنری ورودی تطابق ندارد");
            Assert.AreEqual(verticesPositions[v1].X, alg.VertexPositions[v1].X, "طول/عرض تنها گره داده شده به الگوریتم، پس از پردازش آن تغییر می‌کند");
            Assert.AreEqual(verticesPositions[v1].Y, alg.VertexPositions[v1].Y, "طول/عرض تنها گره داده شده به الگوریتم، پس از پردازش آن تغییر می‌کند");
            Assert.IsFalse(double.IsNaN(alg.VertexPositions[v1].X), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsNaN(alg.VertexPositions[v1].Y), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsInfinity(alg.VertexPositions[v1].X), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsInfinity(alg.VertexPositions[v1].Y), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
        }

        [TestCategory("الگوریتم‌های چینش گراف")]
        [TestMethod]
        public void ComputeWithTwoFullOverlapedVertices()
        {
            // Assign
            GraphData graph = new GraphData();
            Dictionary<Vertex, Point> verticesPositions = new Dictionary<Vertex, Point>();
            Dictionary<Vertex, Size> verticesSizes = new Dictionary<Vertex, Size>();
            AlgorithmFactory af = new AlgorithmFactory();
            SortedLinerLayoutAlgorithm alg = null;
            Vertex v1 = Vertex.VertexFactory("v1");
            Vertex v2 = Vertex.VertexFactory("v2");
            // Act
            verticesPositions.Add(v1, new Point(50, 50));
            verticesSizes.Add(v1, new Size(35, 35));
            graph.AddVertex(v1);
            verticesPositions.Add(v2, new Point(50, 50));
            verticesSizes.Add(v2, new Size(35, 35));
            graph.AddVertex(v2);
            alg = (SortedLinerLayoutAlgorithm)af.CreateLayoutAlgorithm(algorithmEnum, graph, verticesPositions, verticesSizes, af.CreateLayoutParameters(algorithmEnum));
            alg.Compute(new System.Threading.CancellationToken());
            // Assert
            Assert.IsNotNull(alg, "عملکرد سازنده الگوریتم (فکتوری) قادر به ایجاد الگوریتم نیست");
            Assert.IsNotNull(alg.VertexPositions, "دیکشنری موقعیت گره‌ها نال است");
            Assert.IsNotNull(alg.VertexSizes, "دیکشنری اندازه گره‌ها نال است");
            Assert.IsTrue(alg.VertexPositions.Count == verticesPositions.Count(), "تعداد عناصر دیکشنری موقعیت گره‌ها با دیکشنری ورودی تطابق ندارد");
            Assert.IsTrue(alg.VertexSizes.Count == verticesSizes.Count(), "تعداد عناصر دیکشنری اندازه گره‌ها با دیکشنری ورودی تطابق ندارد");

            Assert.IsFalse(double.IsNaN(alg.VertexPositions[v1].X), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsNaN(alg.VertexPositions[v1].Y), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsInfinity(alg.VertexPositions[v1].X), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsInfinity(alg.VertexPositions[v1].Y), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");

            Assert.IsFalse(double.IsNaN(alg.VertexPositions[v2].X), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsNaN(alg.VertexPositions[v2].Y), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsInfinity(alg.VertexPositions[v2].X), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            Assert.IsFalse(double.IsInfinity(alg.VertexPositions[v2].Y), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");

            Assert.IsFalse(alg.VertexPositions[v1].Equals(alg.VertexPositions[v2]), "دو گره روی هم افتاده در ورودی، می‌بایست در خروجی روی هم نباشند");
        }

        [TestCategory("الگوریتم‌های چینش گراف")]
        [TestMethod]
        public void ComputeWithRandomContentAndParameters()
        {
            // Assign
            GraphData graph = new GraphData();
            Dictionary<Vertex, Point> verticesPositions = new Dictionary<Vertex, Point>();
            Dictionary<Vertex, Size> verticesSizes = new Dictionary<Vertex, Size>();
            AlgorithmFactory af = new AlgorithmFactory();
            SortedLinerLayoutAlgorithm alg = null;
            const int verticesCount = 100;
            Random randomValueGenerator = new Random(DateTime.Now.Millisecond);
            // Act
            // افزودن مجموعه‌ای از گره‌ها -با موقعیت و اندازه‌های تصادفی- برای تست الگوریتم
            for (int i = 0; i < verticesCount; i++)
            {
                Vertex v = Vertex.VertexFactory("v" + i.ToString());
                verticesPositions.Add(v, new Point(randomValueGenerator.NextDouble() * 10000, randomValueGenerator.NextDouble() * 10000));
                verticesSizes.Add(v, new Size(randomValueGenerator.NextDouble() * 1000, randomValueGenerator.NextDouble() * 1000));
                graph.AddVertex(v);
                // افزودن مجموعه‌ای از یال‌ها -بین گر‌های تصادفی و با جهت‌های مختلف- برای تست الگوریتم
                if (graph.Vertices.Count() > 5)
                {
                    graph.AddEdge(new Edge(v, graph.Vertices.ElementAt(randomValueGenerator.Next(graph.Vertices.Count() - 1)), EdgeDirection.Bidirectional, "e" + graph.Edges.Count() + 1));
                    graph.AddEdge(new Edge(v, graph.Vertices.ElementAt(randomValueGenerator.Next(graph.Vertices.Count() - 1)), EdgeDirection.FromSourceToTarget, "e" + graph.Edges.Count() + 1));
                    graph.AddEdge(new Edge(graph.Vertices.ElementAt(randomValueGenerator.Next(graph.Vertices.Count() - 1)), v, EdgeDirection.FromSourceToTarget, "e" + graph.Edges.Count() + 1));
                    graph.AddEdge(new Edge(graph.Vertices.ElementAt(randomValueGenerator.Next(graph.Vertices.Count() - 1)), v, EdgeDirection.FromSourceToTarget, "e" + graph.Edges.Count() + 1));
                    graph.AddEdge(new Edge(v, graph.Vertices.ElementAt(randomValueGenerator.Next(graph.Vertices.Count() - 1)), EdgeDirection.FromSourceToTarget, "e" + graph.Edges.Count() + 1));
                }
            }
            // آماده‌سازی و اجرای الگوریتم با داده‌های تصادفی
            SortedLinerLayoutParameters parameters = (af.CreateLayoutParameters(algorithmEnum) as SortedLinerLayoutParameters);
            parameters.GapBetweenVertices = randomValueGenerator.NextDouble() * 1000;
            alg = (SortedLinerLayoutAlgorithm)af.CreateLayoutAlgorithm(algorithmEnum, graph, verticesPositions, verticesSizes, parameters);
            alg.Compute(new System.Threading.CancellationToken());
            // Assert
            Assert.IsNotNull(alg, "عملکرد سازنده الگوریتم (فکتوری) قادر به ایجاد الگوریتم نیست");
            Assert.IsNotNull(alg.VertexPositions, "دیکشنری موقعیت گره‌ها نال است");
            Assert.IsNotNull(alg.VertexSizes, "دیکشنری اندازه گره‌ها نال است");
            Assert.IsTrue(alg.VertexPositions.Count == verticesCount, "دیکشنری موقعیت گره‌ها مانند ورودی دارای یک عنصر نیست");
            Assert.IsTrue(alg.VertexSizes.Count == verticesCount, "دیکشنری اندازه گره‌ها مانند ورودی دارای یک عنصر نیست");
            foreach (var item in alg.VertexPositions.Keys)
            {
                Assert.IsFalse(double.IsNaN(alg.VertexPositions[item].X), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
                Assert.IsFalse(double.IsNaN(alg.VertexPositions[item].Y), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
                Assert.IsFalse(double.IsInfinity(alg.VertexPositions[item].X), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
                Assert.IsFalse(double.IsInfinity(alg.VertexPositions[item].Y), "طول/عرض گره‌های خروجی الگوریتم، می‌بایست عدد مشخصی باشد");
            }
        }
    }
}
