using System;
using System.Collections.Generic;
using GPAS.Workspace.Entities;
using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Utility;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// کلاس مدیریت گره های کنترل گراف
    /// </summary>
    internal class GraphControlVertexManager
    {
        /// <summary>
        /// سازنده کلاس؛ که به خاطر جلوگیری از ایجاد این شی توسط استفاده کننده بیرونی از دسترس خارج (محلی) شده است
        /// </summary>
        private GraphControlVertexManager()
        { }

        /// <summary>
        /// عملکرد ایجاد گره جدید برای استفاده در گراف کنترل
        /// </summary>
        /// <param name="relatedObject">شئی که می خواهیم گره متناظر با آن را بسازیم</param>
        /// <param name="masterGraphControl">گراف کنترلی که می خواهیم گره روی آن نمایش داده شود</param>
        public static Vertex VertexFactory(KWObject relatedObject, GraphControl masterGraphControl)
        {
            if (relatedObject == null)
                throw new ArgumentNullException("relatedObject");
            if (masterGraphControl == null)
                throw new ArgumentNullException("masterGraphControl");

            // ایجاد گره جدید براساس ویژگی های شی داده شده
            Vertex vrtxNew;
            // میزبان یک گروه بودن گره ورودی (طبق تعریف آن در سمت محیط کاربری) در نحوه
            // تعریف گره (با تعریف آن در نمایشگر گراف) موثر است
            if (relatedObject is GroupMasterKWObject)
            {
                List<Vertex> subGroupVertices = new List<Vertex>();
                foreach (var subObj in ((GroupMasterKWObject)relatedObject).GetSubGroupObjects())
                {
                    // در صورتی که زیرمجموعه قبلا روی گراف اضافه شده باشد، به عنوان زیر گروه
                    // در نظر گرفته می شود برای نمایش در نظر گرفته نمی شود
                    Vertex subGroupVertex = masterGraphControl.GetRelatedVertex(subObj);
                    if (subGroupVertex != null)
                    {
                        subGroupVertices.Add(subGroupVertex);
                    }
                }
                // ایجاد یک نمونه گره جدید برای یک شی میزبان گروه
                vrtxNew = Vertex.VertexFactory(relatedObject.GetObjectLabel(), subGroupVertices, null);
            }
            else
            {
                VertexType vertexType = VertexType.Normal;
                if (relatedObject.IsMaster)
                    vertexType = VertexType.Master;
                if (relatedObject.IsSlave)
                    vertexType = VertexType.Slave;

                // ایجاد یک نمونه گره جدید برای یک شی عادی
                vrtxNew = Vertex.VertexFactory(relatedObject.GetObjectLabel(), null, vertexType);
            }
            vrtxNew.SetIconUri(ObjectManager.GetIconPath(relatedObject).ToString());
            vrtxNew.Tag = relatedObject;
            // برگرداندن گره جدید به فراخواننده
            return vrtxNew;
        }
    }
}
