using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.Workspace.ServiceAccess.RemoteService;
using GPAS.Workspace.Entities;
using System.Globalization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using GPAS.DataImport.Publish;
using GPAS.Workspace.ServiceAccess;
using System.ComponentModel;
using System.Security;
using System.Xml.Linq;

namespace GPAS.Workspace.Logic
{
    public class GraphRepositoryManager
    {
        private static Dictionary<string, string> xmlInvalidCharsToStringMapping = null;
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private GraphRepositoryManager()
        {
        }

        private static void GenerateXMLInvalidCharsToStringMapping()
        {
            xmlInvalidCharsToStringMapping = new Dictionary<string, string>();
            xmlInvalidCharsToStringMapping.Add("<", "&lt;");
            xmlInvalidCharsToStringMapping.Add(">", "&gt;");
            xmlInvalidCharsToStringMapping.Add("&", "&amp;");
            xmlInvalidCharsToStringMapping.Add("\"", "&quot;");
            xmlInvalidCharsToStringMapping.Add("\'", "&apos;");
        }

        private static string CleanUpFromInvalidChars(string rowString)
        {
            string cleanedString = rowString;

            foreach (var currentInvalidChar in xmlInvalidCharsToStringMapping.Keys)
            {
                if (cleanedString.Contains(currentInvalidChar))
                {
                    cleanedString = cleanedString.Replace(currentInvalidChar, xmlInvalidCharsToStringMapping[currentInvalidChar].ToString());
                }
            }

            return cleanedString;
        }

        private static string AddInvalidXMLCharsToCleanedString(string cleanedString)
        {
            string replacedString = cleanedString;
            foreach (var currentInvalidChar in xmlInvalidCharsToStringMapping.Keys)
            {
                if (replacedString.Contains(xmlInvalidCharsToStringMapping[currentInvalidChar]))
                {
                    replacedString = replacedString.Replace(xmlInvalidCharsToStringMapping[currentInvalidChar], currentInvalidChar);
                }
            }
            return replacedString;
        }

        static string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
        }

        /// <summary>
        /// ایجاد یک گراف جدید در مخزن داده ها
        /// </summary>
        public async static Task PublishGraphAsync(string graphTitle, string graphDescription, GraphArrangment graphArrangement, byte[] graphImage)
        {
            if (graphTitle == null)
                throw new ArgumentNullException("graphTitle");
            if (graphTitle == string.Empty)
                throw new ArgumentException("Graph title can not remains empty", "graphTitle");
            if (graphDescription == null)
                throw new ArgumentNullException("graphDescription");
            if (graphImage == null)
                throw new ArgumentNullException("graphImageArrangement");

            List<long> arrangmentObjectIDs = GetArrangmentObjectIDs(graphArrangement);
            List<long> arrangmentRelationshipIDs = await GetArrangmentRelationshipIDsAsync(graphArrangement);

            if (!HasGraphAnyUnpublishedConcepts(arrangmentObjectIDs, arrangmentRelationshipIDs))
            {
                string graphArrangmentString = GetXmlStringForGraphArrangmentAsync(graphArrangement, arrangmentObjectIDs, arrangmentRelationshipIDs);
                byte[] graphArrangmentByteArray = Encoding.UTF8.GetBytes(graphArrangmentString);
                int arrangmentNodesCount = graphArrangement.GetNodesCount();

                DateTime saveGraphTime = DateTime.Now;
                string saveGraphTimeString = saveGraphTime.ToString(CultureInfo.InvariantCulture);
                WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
                try
                {
                    long id = await sc.GetNewGraphIdAsync();
                    DataSourceMetadata dataSource = new DataSourceMetadata()
                    {
                        Acl = Logic.UserAccountControlProvider.PublishGraphACL,
                        Content = null,
                        Description = graphDescription,
                        Type = GPAS.AccessControl.DataSourceType.Graph,
                        Name = graphTitle
                    };

                    PublishAdaptor publishAdaptor = new Logic.DataImport.PublishAdaptor();
                    DataSourceRegisterationProvider dataSourceRegisterationProvider = new DataSourceRegisterationProvider(dataSource, publishAdaptor);
                    dataSourceRegisterationProvider.Register();
                    long dataSourceID = dataSourceRegisterationProvider.DataSourceID;
                    await sc.PublishGraphAsync
                        (id,
                        graphTitle
                        , graphDescription
                        , graphImage
                        , graphArrangmentByteArray
                        , arrangmentNodesCount
                        , saveGraphTimeString
                        , dataSourceID
                        );
                }
                finally
                {
                    sc.Close();
                }
            }
            else
            {
                throw new Exception("Your graph has unpublished concepts, please publish these concepts before you want to publish your graph.");
            }
        }

        public static bool HasGraphAnyUnpublishedConcepts(List<long> arrangmentObjectIDs, List<long> arrangmentRelationshipIDs)
        {
            foreach (var item in arrangmentObjectIDs)
                if (DataAccessManager.ObjectManager.IsUnpublishedObject(item))
                {
                    return true;
                }

            foreach (var item in arrangmentRelationshipIDs)
                if (DataAccessManager.LinkManager.IsUnpublishedRelationship(item))
                {
                    return true;
                }

            return false;
        }

        /// <summary>
        /// دریافت رشته ایکس.ام.ال نشاندهنده چینش اشیا و لینک ها روی گراف
        /// </summary>
        /// <remarks>
        /// قالب ایکس.ام.ال خروجی این عملکرد به این صورت می باشد:
        /// ---------------------------------------
        /// <GraphArrangment>
        ///    <Vertices>
        ///        <Object id="" x="" y="" isVisible=""/>
        ///        <GroupMasterObject id="" x="" y="" isVisible="" isCollapsed="">
        ///            <CollapsedSubGroupsExpandedModePositionRelatedToMaster>
        ///                <RelativePosition subGroupId="" relatedX="" relatedY=""/>
        ///                ...
        ///            </CollapsedSubGroupsExpandedModePositionRelatedToMaster>
        ///        </GroupMasterObject>
        ///        ...
        ///    </Vertices>
        ///    <Edges>
        ///        <RelationshipBasedLink id="" sourceObjectId="" targetObjectId=""/>
        ///        <EventBasedLink intermediaryEventId="" sourceObjectId="" targetObjectId="" firstRelationshipId="" secondRelationshipId=""/>
        ///        <PropertyBasedLink sourceObjectId="" sourcePropertyId="" targetObjectId="" targetPropertyId=""/>
        ///        ...
        ///    </Edges>
        /// </GraphArrangment>
        /// ---------------------------------------
        /// </remarks>
        public static string GetXmlStringForGraphArrangmentAsync(GraphArrangment arrangment, List<long> arrangmentObjectIDs, List<long> arrangmentRelationshipIDs)
        {
            // این عملکرد تگ های فایل ایکس.ام.ال را به صورت دستی و یکی پس از دیگری قرار می دهد
            // و در نهایت رشته ایکس.ام.ال نشاندهنده وضعیت کنونی گراف را برمی گرداند
            // قالب ایکس.ام.ال در ملاحضات (remark) عملکرد آمده است            

            GenerateXMLInvalidCharsToStringMapping();

            string result = "<GraphArrangment>";
            result += "<Vertices>";

            foreach (var item in arrangment.Objects)
            {
                if (item.IsMasterOfGroup)
                {
                    result += string.Format("<GroupMasterObject id=\"{0}\" x=\"{1}\" y=\"{2}\" isVisible=\"{3}\" isCollapsed=\"{4}\">"
                        , item.NotResolvedObjectId.ToString(CultureInfo.InvariantCulture)
                        , item.Position.X.ToString(CultureInfo.InvariantCulture)
                        , item.Position.Y.ToString(CultureInfo.InvariantCulture)
                        , item.IsVisible.ToString(CultureInfo.InvariantCulture)
                        , item.IsMasterOfACollapsedGroup.ToString(CultureInfo.InvariantCulture));
                    if (item.IsMasterOfACollapsedGroup)
                    {
                        result += "<CollapsedSubGroupsExpandedModePositionRelatedToMaster>";
                        foreach (var subGroupRelativePosition in arrangment.CollapsedGroupsRelativePoistions
                            .Single(p => p.NotResolvedGroupMasterObjectId == item.NotResolvedObjectId)
                            .GroupMembersRelativePositionsByObjectId)
                        {
                            result += string.Format("<RelativePosition subGroupId=\"{0}\" relatedX=\"{1}\" relatedY=\"{2}\"/>"
                                , subGroupRelativePosition.Key.ToString(CultureInfo.InvariantCulture)
                                , subGroupRelativePosition.Value.X.ToString(CultureInfo.InvariantCulture)
                                , subGroupRelativePosition.Value.Y.ToString(CultureInfo.InvariantCulture));
                        }
                        result += "</CollapsedSubGroupsExpandedModePositionRelatedToMaster>";
                    }
                    result += "</GroupMasterObject>";
                }
                else
                    result += string.Format("<Object id=\"{0}\" x=\"{1}\" y=\"{2}\" isVisible=\"{3}\"/>"
                        , item.NotResolvedObjectId.ToString(CultureInfo.InvariantCulture)
                        , item.Position.X.ToString(CultureInfo.InvariantCulture)
                        , item.Position.Y.ToString(CultureInfo.InvariantCulture)
                        , item.IsVisible.ToString(CultureInfo.InvariantCulture));
            }
            result += "</Vertices>";
            result += "<Edges>";
            foreach (var item in arrangment.RelationshipBasedLinksExceptGroupInnerLinks)
                result += string.Format("<RelationshipBasedLink id=\"{0}\" sourceObjectId=\"{1}\" targetObjectId=\"{2}\"/>"
                    , item.RelationshipId.ToString(CultureInfo.InvariantCulture)
                    , item.SourceObjectId.ToString(CultureInfo.InvariantCulture)
                    , item.TargetObjectId.ToString(CultureInfo.InvariantCulture));
            foreach (var item in arrangment.EventBasedLinks)
                result += string.Format("<EventBasedLink intermediaryEventId=\"{0}\" sourceObjectId=\"{1}\" targetObjectId=\"{2}\" firstRelationshipId=\"{3}\" secondRelationshipId=\"{4}\"/>"
                    , item.intermediaryEventId.ToString(CultureInfo.InvariantCulture)
                    , item.SourceObjectId.ToString(CultureInfo.InvariantCulture)
                    , item.TargetObjectId.ToString(CultureInfo.InvariantCulture)
                    , item.FirstRelationshipId.ToString(CultureInfo.InvariantCulture)
                    , item.SecondRelationshipId.ToString(CultureInfo.InvariantCulture));
            foreach (var item in arrangment.PropertyBasedLinks)
                result += string.Format("<PropertyBasedLink sourceObjectId=\"{0}\" targetObjectId=\"{1}\" samePropertyTypeUri=\"{2}\" samePropertyValue=\"{3}\"/>"
                    , item.SourceObjectId.ToString(CultureInfo.InvariantCulture)
                    , item.TargetObjectId.ToString(CultureInfo.InvariantCulture)
                    , CleanUpFromInvalidChars(item.SamePropertyTypeUri)
                    , CleanUpFromInvalidChars(item.SamePropertyValue));
            foreach (var item in arrangment.NotLoadedRelationshipBasedLinks)
            {
                result += string.Format("<NotLoadedRelationshipBasedLink sourceObjectId=\"{0}\" targetObjectId=\"{1}\">"
                    , item.SourceObjectId.ToString(CultureInfo.InvariantCulture)
                    , item.TargetObjectId.ToString(CultureInfo.InvariantCulture));
                foreach (var innerRelID in item.RelationshipIDs)
                {
                    result += string.Format("<InnerRel id=\"{0}\"/>", innerRelID.ToString(CultureInfo.InvariantCulture));
                }
                result += "</NotLoadedRelationshipBasedLink>";
            }
            foreach (var item in arrangment.NotLoadedEventBasedLinks)
            {
                result += string.Format("<NotLoadedEventBasedLink sourceObjectId=\"{0}\" targetObjectId=\"{1}\">"
                    , item.SourceObjectId.ToString(CultureInfo.InvariantCulture)
                    , item.TargetObjectId.ToString(CultureInfo.InvariantCulture));
                foreach (var innerRelPair in item.RelationshipIdPairs)
                {
                    result += string.Format("<InnerRelPair Rel1Id=\"{0}\" Rel2Id=\"{1}\"/>"
                        , innerRelPair.FirstRelID.ToString(CultureInfo.InvariantCulture)
                        , innerRelPair.SecondRelID.ToString(CultureInfo.InvariantCulture));
                }
                result += "</NotLoadedEventBasedLink>";
            }
            result += "</Edges>";
            result += "</GraphArrangment>";

            return result;
        }

        public static List<long> GetArrangmentObjectIDs(GraphArrangment arrangment)
        {
            List<long> objectIDs = new List<long>();
            foreach (var item in arrangment.Objects)
            {
                objectIDs.Add(item.NotResolvedObjectId);
            }
            foreach (var item in arrangment.EventBasedLinks)
            {
                objectIDs.Add(item.intermediaryEventId);
            }
            return objectIDs.Distinct().ToList();
        }

        public static async Task<List<long>> GetArrangmentRelationshipIDsAsync(GraphArrangment arrangment)
        {
            List<long> relationshipIDs = new List<long>();
            foreach (var groupMasterId in arrangment.CollapsedGroupsRelativePoistions)
            {
                var groupMaster = (GroupMasterKWObject)(await DataAccessManager.ObjectManager.GetObjectByIdAsync(groupMasterId.NotResolvedGroupMasterObjectId));
                foreach (var subgroupId in groupMasterId.GroupMembersRelativePositionsByObjectId.Keys)
                {
                    var relationshipWithSubGroup = groupMaster.GroupLinks.Single(l => l.Source.ID == subgroupId);
                    relationshipIDs.Add(relationshipWithSubGroup.Relationship.ID);
                }
            }
            foreach (var item in arrangment.RelationshipBasedLinksExceptGroupInnerLinks)
            {
                relationshipIDs.Add(item.RelationshipId);
            }
            foreach (var item in arrangment.EventBasedLinks)
            {
                relationshipIDs.Add(item.FirstRelationshipId);
                relationshipIDs.Add(item.SecondRelationshipId);
            }
            return relationshipIDs.Distinct().ToList();
        }

        /// <summary>
        /// تمام اطلاعات گراف های ذخیره شده را برمی گرداند و براساس زمان دسته بندی می کند.
        /// </summary>
        public async static Task<List<PublishedGraph>> RetrieveGraphsListAsync()
        {
            List<PublishedGraph> graphDictionary = new List<PublishedGraph>();

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            DateTime todayTime = DateTime.Now;
            List<KGraphArrangement> allGraphsList = null;
            try
            {
                allGraphsList = (await sc.GetPublishedGraphsAsync()).ToList();
                todayTime = DateTime.Parse(await sc.GetDispatchCurrentDateTimeAsync());
            }
            finally
            {
                sc.Close();
            }

            if (allGraphsList == null)
                throw new NullReferenceException("Invalid server response");

            allGraphsList.Reverse();

            DateTime yesterdayTime = todayTime.AddDays(-1);
            DateTime thisWeek = todayTime.AddDays(-7);
            DateTime lastWeekTime = todayTime.AddDays(-14);
            DateTime twoWeeksAgoTime = todayTime.AddDays(-21);
            DateTime threeWeeksTime = todayTime.AddDays(-28);

            foreach (var item in allGraphsList)
            {
                DateTime graphCreatedTime = DateTime.Parse(item.TimeCreated, CultureInfo.InvariantCulture);
                PublishedGraph newGraphItem = new PublishedGraph();
                newGraphItem.ID = (int)item.Id;
                newGraphItem.GraphTitle = item.Title;
                newGraphItem.GraphDescription = item.Description;
                newGraphItem.NodesCount = item.NodesCount;
                newGraphItem.CreatedTime = graphCreatedTime;

                if (graphCreatedTime.Date.CompareTo(todayTime.Date) == 0)
                {
                    newGraphItem.GroupCategory = "Today";
                }
                else if (graphCreatedTime.Date.CompareTo(yesterdayTime.Date) == 0)
                {
                    newGraphItem.GroupCategory = "Yesterday";
                }
                else if (graphCreatedTime.Date.CompareTo(yesterdayTime.Date) < 0 && graphCreatedTime.Date.CompareTo(thisWeek.Date) >= 0)
                {
                    newGraphItem.GroupCategory = "This Week";
                }
                else if (graphCreatedTime.Date.CompareTo(thisWeek.Date) < 0 && graphCreatedTime.Date.CompareTo(lastWeekTime.Date) >= 0)
                {
                    newGraphItem.GroupCategory = "Last Week";
                }
                else if (graphCreatedTime.Date.CompareTo(lastWeekTime.Date) < 0 && graphCreatedTime.Date.CompareTo(twoWeeksAgoTime.Date) >= 0)
                {
                    newGraphItem.GroupCategory = "Two Weeks Ago";
                }
                else if (graphCreatedTime.Date.CompareTo(twoWeeksAgoTime.Date) < 0 && graphCreatedTime.Date.CompareTo(threeWeeksTime.Date) >= 0)
                {
                    newGraphItem.GroupCategory = "Three Weeks Ago";
                }
                else
                {
                    newGraphItem.GroupCategory = "Others";
                }
                graphDictionary.Add(newGraphItem);
            }
            return graphDictionary;
        }

        /// <summary>
        /// عکس گراف ذخیره شده را  به صورت آرایه ای از بایت ها برمی گرداند.
        /// </summary>
        public async static Task<byte[]> RetrieveGraphImageAsync(int graphID)
        {
            if (graphID < 0)
                throw new ArgumentException("graphID");

            byte[] graphImageBytes = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                graphImageBytes = await sc.GetPublishedGraphImageAsync(graphID);
            }
            finally
            {
                sc.Close();
            }

            if (graphImageBytes == null)
                throw new NullReferenceException("Invalid Server Response");

            return graphImageBytes;
        }

        /// <summary>
        /// گراف ذخیره شده را به صورت یک Stream برمی گرداند.
        /// </summary>
        public async static Task<GraphArrangment> RetrieveGraphArrangmentAsync(int graphID)
        {
            if (graphID < 0)
                throw new ArgumentException("graphID");

            byte[] graphBytes = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                graphBytes = await sc.GetPublishedGraphAsync(graphID);
            }
            finally
            {
                sc.Close();
            }

            if (graphBytes == null)
                throw new NullReferenceException("Invalid Server Response");

            Stream graphStream = new MemoryStream(graphBytes);
            // دریافت چینش گراف براساس جریان ایکس.ام.ال. بازیابی شده
            GraphArrangment graphArrangment = await GetGraphArrangmentFromXmlStream(graphStream);
            return graphArrangment;
        }

        /// <summary>
        /// ساختار چینش گراف را براساس جریان ایکس.ام.ال ورودی برمی گرداند؛
        /// ایکس.ام.ال. ورودی این عملکرد می بایست براساس خروجی عملکرد GetGraphArrangmentXmlString باشد
        /// </summary>
        public static async Task<GraphArrangment> GetGraphArrangmentFromXmlStream(Stream graphArrangmentXmlStringStreamToShowGraph)
        {
            if (graphArrangmentXmlStringStreamToShowGraph == null)
                throw new ArgumentNullException("graphArrangmentXmlStringStreamToShowGraph");

            GenerateXMLInvalidCharsToStringMapping();

            //Utility.StreamUtility streamUtility = new Utility.StreamUtility();
            //string st = streamUtility.GetStringFromStream(streamUtility.ConvertStreamToMemoryStream(graphArrangmentXmlStringStreamToShowGraph));

            // آماده سازی چینش بازگشتی
            GraphArrangment arrangment = new GraphArrangment();
            arrangment.NotLoadedEventBasedLinks = new List<GraphArrangment.NotLoadedEventBasedLink>();
            arrangment.NotLoadedRelationshipBasedLinks = new List<GraphArrangment.NotLoadedRelationshipBasedLink>();
            // اعتبارسنجی و استخراج محتوای رشته چیدمان گراف
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.Async = true;
            XmlReader reader = XmlReader.Create(graphArrangmentXmlStringStreamToShowGraph, readerSettings);
            while (await reader.ReadAsync())
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "GraphArrangment")
                    break;
            // بازیابی گره های چینش گراف
            if (!(await reader.ReadAsync() && (reader.NodeType == XmlNodeType.Element && reader.Name == "Vertices")))
                throw new InvalidDataException(string.Format("Invalid Graph arrangemt XML"));
            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Vertices"))
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Object")
                    // بازیابی شئ غیرمیزبان گروه
                    arrangment.Objects.Add(new GraphArrangment.ObjectNode()
                    {
                        NotResolvedObjectId = long.Parse(reader.GetAttribute("id"), CultureInfo.InvariantCulture),
                        Position = new GraphArrangment.Point()
                        {
                            X = double.Parse(reader.GetAttribute("x"), CultureInfo.InvariantCulture),
                            Y = double.Parse(reader.GetAttribute("y"), CultureInfo.InvariantCulture)
                        },
                        IsMasterOfGroup = false,
                        IsMasterOfACollapsedGroup = false,
                        IsVisible = bool.Parse(reader.GetAttribute("isVisible"))
                    });
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "GroupMasterObject")
                {
                    // بازیابی شئ میزبان گروه
                    GraphArrangment.ObjectNode masterArrangmentObject
                        = new GraphArrangment.ObjectNode()
                        {
                            NotResolvedObjectId = long.Parse(reader.GetAttribute("id"), CultureInfo.InvariantCulture),
                            Position = new GraphArrangment.Point()
                            {
                                X = double.Parse(reader.GetAttribute("x"), CultureInfo.InvariantCulture),
                                Y = double.Parse(reader.GetAttribute("y"), CultureInfo.InvariantCulture)
                            },
                            IsMasterOfGroup = true,
                            IsMasterOfACollapsedGroup = bool.Parse(reader.GetAttribute("isCollapsed")),
                            IsVisible = bool.Parse(reader.GetAttribute("isVisible"))
                        };
                    arrangment.Objects.Add(masterArrangmentObject);
                    // در صورتی که این شئ میزبان یک گروه بسته شده باشد،
                    // موقعیت زیرگروه های آن نسبت به گره میزبان (این گره) در حالتی که گروه باز شود، بازیابی می شود
                    if (masterArrangmentObject.IsMasterOfACollapsedGroup)
                    {
                        GraphArrangment.CollapsedGroupMembersPositionRelaterdToMasterInExpandedMode masterMemberRelativePositions
                            = new GraphArrangment.CollapsedGroupMembersPositionRelaterdToMasterInExpandedMode()
                            {
                                NotResolvedGroupMasterObjectId = masterArrangmentObject.NotResolvedObjectId,
                                GroupMembersRelativePositionsByObjectId = new Dictionary<long, GraphArrangment.Point>()
                            };
                        arrangment.CollapsedGroupsRelativePoistions.Add(masterMemberRelativePositions);
                        while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "GroupMasterObject"))
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "RelativePosition")
                                masterMemberRelativePositions.GroupMembersRelativePositionsByObjectId.Add
                                    (long.Parse(reader.GetAttribute("subGroupId"), CultureInfo.InvariantCulture)
                                    , new GraphArrangment.Point()
                                    {
                                        X = double.Parse(reader.GetAttribute("relatedX"), CultureInfo.InvariantCulture),
                                        Y = double.Parse(reader.GetAttribute("relatedY"), CultureInfo.InvariantCulture)
                                    });
                    }
                    else
                        while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "GroupMasterObject")) ;
                }
                else
                    throw new InvalidDataException(string.Format("Invalid Graph arrangemt XML"));
            // بازیابی یال های چینش گراف
            if (!(await reader.ReadAsync() && (reader.NodeType == XmlNodeType.Element && reader.Name == "Edges")))
                throw new InvalidDataException(string.Format("Invalid Graph arrangemt XML"));

            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Edges"))
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "RelationshipBasedLink")
                    // بازیابی لینک مبتنی بر رابطه
                    arrangment.RelationshipBasedLinksExceptGroupInnerLinks.Add(new GraphArrangment.RelationshipBasedLink()
                    {
                        RelationshipId = long.Parse(reader.GetAttribute("id"), CultureInfo.InvariantCulture),
                        SourceObjectId = long.Parse(reader.GetAttribute("sourceObjectId"), CultureInfo.InvariantCulture),
                        TargetObjectId = long.Parse(reader.GetAttribute("targetObjectId"), CultureInfo.InvariantCulture)
                    });
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "EventBasedLink")
                    // بازیابی لینک مبتنی بر رخداد
                    arrangment.EventBasedLinks.Add(new GraphArrangment.EventBasedLink()
                    {
                        intermediaryEventId = long.Parse(reader.GetAttribute("intermediaryEventId"), CultureInfo.InvariantCulture),
                        SourceObjectId = long.Parse(reader.GetAttribute("sourceObjectId"), CultureInfo.InvariantCulture),
                        TargetObjectId = long.Parse(reader.GetAttribute("targetObjectId"), CultureInfo.InvariantCulture),
                        FirstRelationshipId = long.Parse(reader.GetAttribute("firstRelationshipId"), CultureInfo.InvariantCulture),
                        SecondRelationshipId = long.Parse(reader.GetAttribute("secondRelationshipId"), CultureInfo.InvariantCulture)
                    });
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "PropertyBasedLink")
                {
                    arrangment.PropertyBasedLinks.Add(new GraphArrangment.PropertyBasedLink()
                    {
                        SourceObjectId = long.Parse(reader.GetAttribute("sourceObjectId"), CultureInfo.InvariantCulture),
                        TargetObjectId = long.Parse(reader.GetAttribute("targetObjectId"), CultureInfo.InvariantCulture),
                        SamePropertyTypeUri = AddInvalidXMLCharsToCleanedString(reader.GetAttribute("samePropertyTypeUri")),
                        SamePropertyValue = AddInvalidXMLCharsToCleanedString(reader.GetAttribute("samePropertyValue"))
                    });
                }
                // بازیابی لینک مبتنی بر ویژگی هم مقدار                        

                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "NotLoadedRelationshipBasedLink")
                {
                    GraphArrangment.NotLoadedRelationshipBasedLink newLink = new GraphArrangment.NotLoadedRelationshipBasedLink()
                    {
                        SourceObjectId = long.Parse(reader.GetAttribute("sourceObjectId"), CultureInfo.InvariantCulture),
                        TargetObjectId = long.Parse(reader.GetAttribute("targetObjectId"), CultureInfo.InvariantCulture)
                    };
                    List<long> innerRelIDs = new List<long>();
                    while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "NotLoadedRelationshipBasedLink"))
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "InnerRel")
                            innerRelIDs.Add(long.Parse(reader.GetAttribute("id"), CultureInfo.InvariantCulture));
                    }
                    newLink.RelationshipIDs = innerRelIDs.ToArray();
                    arrangment.NotLoadedRelationshipBasedLinks.Add(newLink);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "NotLoadedEventBasedLink")
                {
                    GraphArrangment.NotLoadedEventBasedLink newLink = new GraphArrangment.NotLoadedEventBasedLink()
                    {
                        SourceObjectId = long.Parse(reader.GetAttribute("sourceObjectId"), CultureInfo.InvariantCulture),
                        TargetObjectId = long.Parse(reader.GetAttribute("targetObjectId"), CultureInfo.InvariantCulture)
                    };
                    var innerRelIdPairs = new List<GraphArrangment.NotLoadedEventBasedLinkInnerRelIdPair>();
                    while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "NotLoadedEventBasedLink"))
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "InnerRelPair")
                        {
                            innerRelIdPairs.Add(new GraphArrangment.NotLoadedEventBasedLinkInnerRelIdPair()
                            {
                                FirstRelID = long.Parse(reader.GetAttribute("Rel1Id"), CultureInfo.InvariantCulture),
                                SecondRelID = long.Parse(reader.GetAttribute("Rel2Id"), CultureInfo.InvariantCulture)
                            });
                        }
                    }
                    newLink.RelationshipIdPairs = innerRelIdPairs.ToArray();
                    arrangment.NotLoadedEventBasedLinks.Add(newLink);
                }
                else
                    throw new InvalidDataException(string.Format("Invalid Graph arrangemt XML"));


            // پایان بازیابی گراف
            if (!(await reader.ReadAsync() && (reader.NodeType == XmlNodeType.EndElement && reader.Name == "GraphArrangment")))
                throw new InvalidDataException(string.Format("Invalid Graph arrangemt XML"));
            // برگرداندن نتیجه
            return arrangment;
        }
    }
}
