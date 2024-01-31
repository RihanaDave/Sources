using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class SourceSetVertex : Vertex
    {
        protected internal SourceSetVertex(string vertexTitle, IEnumerable<Vertex> setOfVertices, VertexControl relatedVertexControl = null)
            : base(vertexTitle, relatedVertexControl, false)
        {
            if (vertexTitle == null)
                throw new ArgumentNullException("vertexTitle");
            if (setOfVertices == null)
                throw new ArgumentNullException("set Of Vertices");
            if (string.IsNullOrWhiteSpace(vertexTitle))
                throw new ArgumentException("Invalid argument", "vertexTitle");

            SetOfVertices = setOfVertices.ToList();
            ResetSetOfVerticesTitles();
            RelatedVertexControl = (relatedVertexControl == null) ? new VertexControl(this) : relatedVertexControl;
        }

        /// <summary>
        /// گره هایی که زیرمجموعه گروه این گره می باشند و روی گراف نمایش داده می شوند، را برمی گرداند
        /// </summary>
        public List<Vertex> SetOfVertices
        {
            get;
            private set;
        }
        
        /// <summary>
        /// مقایسه دو گره با هم
        /// </summary>
        /// <remarks>پیاده سازی اینترفیس قابل مقایسه بودن کلاس</remarks>
        public new int CompareTo(Vertex other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return base.CompareTo(other);
        }

        /// <summary>
        /// عناوین زیرگروه های گروهی که این گره میزبان آن است را برمی گرداند؛
        /// این عناوین شامل عناوین گره های زیرمجموعه ایست که از آستانه تعیین شده برای نمایش عناوین کمتر باشند و در صورت اضافه بودن تنها تعداد اضافه ها نمایش داده خواهد شد
        /// </summary>
        public string SetOfVerticesTitles
        {
            get;
            protected set;
        }

        /// <summary>
        /// عناوین زیرگروه های گروهی که این گره میزبان آن است را بازنشانی می کند؛
        /// </summary>
        /// <remarks>
        /// این عناوین شامل عناوین گره های زیرمجموعه ایست که از آستانه تعیین شده برای
        /// نمایش عناوین کمتر باشند و در صورت اضافه بودن تنها «تعداد» اضافه ها نمایش
        /// داده خواهد شد
        /// </remarks>
        protected virtual void ResetSetOfVerticesTitles()
        {
            string setOfVerticesTitles = "";

            Dictionary<string, int> setOfVerticesGroupedByType = GroupbySetOfVertices();
            
            if (SetOfVertices.Count == 0)
            {
                setOfVerticesTitles = "(No member)";
            }
            else                
            {
                if (setOfVerticesGroupedByType.Count <= ShowingSetOfVerticesTitlesTreshould)
                {
                    foreach (var item in setOfVerticesGroupedByType)
                    {
                        setOfVerticesTitles += string.Format("{0}: {1} \n", item.Key ,  item.Value);
                    }
                }
                else
                {
                    for (int i = 0; i < ShowingSetOfVerticesTitlesTreshould - 1; i++)
                    {
                        setOfVerticesTitles += string.Format("{0}: {1} \n", setOfVerticesGroupedByType.ElementAt(i).Key,
                            setOfVerticesGroupedByType.ElementAt(i).Value);
                    }

                    setOfVerticesTitles += string.Format("And {0} more object types", setOfVerticesGroupedByType.Count - ShowingSetOfVerticesTitlesTreshould);
                }
            }
            SetOfVerticesTitles = setOfVerticesTitles;
        }

        private Dictionary<string, int> GroupbySetOfVertices()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (var currentVertex in SetOfVertices)
            {
                if (result.ContainsKey(currentVertex.Text))
                {
                    result[currentVertex.Text]++;
                }
                else
                {
                    result.Add(currentVertex.Text, 1);
                }
            }
            return result;
        }

        /// <summary>
        /// آستانه تعیین شده برای تعداد عناوین زیرگروه هایی که برای عنوان گروه نمایش داده می شوند را نگهداری می کند
        /// </summary>
        private static int showingSetOfVerticesTitlesTreshould = 6;

        /// <summary>
        /// آستانه تعیین شده برای تعداد عناوین زیرگروه هایی که برای عنوان گروه نمایش داده می شوند را مقداردهی می کند و برمی گرداند
        /// </summary>
        public static int ShowingSetOfVerticesTitlesTreshould
        {
            get { return showingSetOfVerticesTitlesTreshould; }
            set { showingSetOfVerticesTitlesTreshould = value; }
        }
    }
}
