using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    /// <summary>
    /// کلاس نشاندهنده یک گروه جمع شده روی گراف؛
    /// این کلاس برای استفاده داخلی در زمان بازیابی یک گراف طراحی شده است
    /// </summary>
    internal class CollapsedGroupNode
    {
        internal CollapsedGroupNode(GroupMasterKWObject notResolvedGroupMaster)
        {
            if (notResolvedGroupMaster == null)
                throw new ArgumentNullException("groupMaster");

            NotResolvedGroupMasterObject = notResolvedGroupMaster;
        }
        /// <summary>
        /// شئ میزبان گروهی که این نمونه از کلاس نشاندهنده آن است
        /// </summary>
        internal GroupMasterKWObject NotResolvedGroupMasterObject;
        /// <summary>
        /// موقعیت اصلی گره میزبان این گروه؛
        /// در واقع موقعیت میزبان گروه در زمانی ست که تمام گروه ها در وضعیت باز شده باشند؛
        /// از این موقعیت برای تعیین موقعیت نسبی زیرگروه های گروه استفاده می شود
        /// </summary>
        internal Point GroupMasterMostlyExpandedPosition;
        /// <summary>
        /// لیست زیرگروه هایی از گروه، که طبق تعاریف چینش، در وضعیت جمع شده قرار دارند
        /// </summary>
        internal List<CollapsedGroupNode> CollapsedGroupSubNodes = new List<CollapsedGroupNode>();
    }
}
