using GPAS.Workspace.Logic;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.GraphTests
{
    [TestClass]
    public class GraphRepositoryManagerTests
    {
        private bool isInitialized = false;

        [TestInitialize]
        public async Task Init()
        {
            if (!isInitialized)
            {
                var authentication = new UserAccountControlProvider();
                bool result = await authentication.AuthenticateAsync("admin", "admin");
                await Workspace.Logic.System.InitializationAsync();
                isInitialized = true;
            }
        }


        [TestCategory("ذخیره و بازیابی گراف")]
        [ExpectedException(typeof(ArgumentNullException), "انتظار می‌رود در صورت قرار دادن نال به عنوان ورودی تابع انتشار گراف، استثنا متناظر صادر شود")]
        [TestMethod()]
        public async Task PublishGraph_NullTitleInput_ThrowsException()
        {
            await GraphRepositoryManager.PublishGraphAsync(null, string.Empty, new GraphArrangment(), new byte[] { });
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [ExpectedException(typeof(ArgumentException), "انتظار می‌رود در صورت قرار دادن رشته خالی به عنوان ورودی تابع انتشار گراف، استثنا متناظر صادر شود")]
        [TestMethod()]
        public async Task PublishGraph_EmptyTitleInput_ThrowsException()
        {
            await GraphRepositoryManager.PublishGraphAsync(string.Empty, string.Empty, new GraphArrangment(), new byte[] { });
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [ExpectedException(typeof(ArgumentNullException), "انتظار می‌رود در صورت قرار دادن نال به عنوان ورودی تابع انتشار گراف، استثنا متناظر صادر شود")]
        [TestMethod()]
        public async Task PublishGraph_NullDescriptionInput_ThrowsException()
        {
            await GraphRepositoryManager.PublishGraphAsync("123", null, new GraphArrangment(), new byte[] { });
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [ExpectedException(typeof(ArgumentNullException), "انتظار می‌رود در صورت قرار دادن نال به عنوان ورودی تابع انتشار گراف، استثنا متناظر صادر شود")]
        [TestMethod()]
        public async Task PublishGraph_NullImageBytesInput_ThrowsException()
        {
            await GraphRepositoryManager.PublishGraphAsync("123", string.Empty, new GraphArrangment(), null);
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [ExpectedException(typeof(ArgumentException), "انتظار می‌رود در صورت قرار دادن مقدار منفی به عنوان شناسه‌ی گراف ذخیره شده، استثنا متناظر صادر شود")]
        [TestMethod()]
        public async Task RetrieveGraph_NegativeInput_ThrowsException()
        {
            await GraphRepositoryManager.RetrieveGraphArrangmentAsync(-1);
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [ExpectedException(typeof(ArgumentException), "انتظار می‌رود در صورت قرار دادن مقدار منفی به عنوان شناسه‌ی گراف ذخیره شده، استثنا متناظر صادر شود")]
        [TestMethod()]
        public async Task RetrieveGraphImage_NegativeInput_ThrowsException()
        {
            await GraphRepositoryManager.RetrieveGraphImageAsync(-1);
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [TestMethod()]
        public async Task PublishAndRetriveEmptyGraph()
        {
            using (ShimsContext.Create())
            {
                // Fake Definitions
                byte[] publishedArrangmentByteArray = null;
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.PublishGraphAsyncInt64StringStringByteArrayByteArrayInt32StringInt64
                    = async (id, a, b, c, d, arrangmentByteArray, f, g, h) =>
                    {
                        await Task.Delay(0);
                        publishedArrangmentByteArray = arrangmentByteArray;
                        return new Workspace.ServiceAccess.RemoteService.KGraphArrangement();
                    };
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.GetPublishedGraphAsyncInt32
                    = async (a, b) =>
                    {
                        await Task.Delay(0);
                        return publishedArrangmentByteArray;
                    };

                // Arrange
                GraphArrangment inputArrangment = new GraphArrangment();
                GraphArrangment outputArrangment;
                // Act
                await GraphRepositoryManager.PublishGraphAsync("۱۲۳", "۱۲۳", inputArrangment, new byte[] { });
                outputArrangment = await GraphRepositoryManager.RetrieveGraphArrangmentAsync(1);
                //Assert
                Assert.IsTrue(outputArrangment.Objects.Count == 0);
                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions.Count == 0);
                Assert.IsTrue(outputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count == 0);
                Assert.IsTrue(outputArrangment.EventBasedLinks.Count == 0);
                Assert.IsTrue(outputArrangment.PropertyBasedLinks.Count == 0);
            }
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [TestMethod()]
        public async Task PublishAndRetriveGraphWithSingleObject()
        {
            using (ShimsContext.Create())
            {
                // Fake Definitions
                byte[] publishedArrangmentByteArray = null;
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.PublishGraphAsyncInt64StringStringByteArrayByteArrayInt32StringInt64 = async (id, a, b, c, d, arrangmentByteArray, f, g, h) =>
                {
                    await Task.Delay(0);
                    publishedArrangmentByteArray = arrangmentByteArray;
                    return new Workspace.ServiceAccess.RemoteService.KGraphArrangement();
                };
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.GetPublishedGraphAsyncInt32 = async (a, b) =>
                {
                    await Task.Delay(0);
                    return publishedArrangmentByteArray;
                };

                // Arrange
                var inputObjectNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 123,
                    Position = new GraphArrangment.Point()
                    {
                        X = 10,
                        Y = 20
                    },
                    IsVisible = true,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };

                GraphArrangment inputArrangment = new GraphArrangment();
                inputArrangment.Objects.Add(inputObjectNode);
                GraphArrangment outputArrangment;
                // Act
                await GraphRepositoryManager.PublishGraphAsync("۱۲۳", "۱۲۳", inputArrangment, new byte[] { });
                outputArrangment = await GraphRepositoryManager.RetrieveGraphArrangmentAsync(1);
                //Assert
                Assert.IsTrue(outputArrangment.Objects.Count == inputArrangment.Objects.Count);
                var outputObjectNode = outputArrangment.Objects[0];
                Assert.IsTrue(outputObjectNode.NotResolvedObjectId == inputObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputObjectNode.Position.X == inputObjectNode.Position.X);
                Assert.IsTrue(outputObjectNode.Position.Y == inputObjectNode.Position.Y);
                Assert.IsTrue(outputObjectNode.IsVisible == inputObjectNode.IsVisible);
                Assert.IsTrue(outputObjectNode.IsMasterOfGroup == inputObjectNode.IsMasterOfGroup);
                Assert.IsTrue(outputObjectNode.IsMasterOfACollapsedGroup == inputObjectNode.IsMasterOfACollapsedGroup);

                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions.Count == inputArrangment.CollapsedGroupsRelativePoistions.Count);
                Assert.IsTrue(outputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count == inputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count);
                Assert.IsTrue(outputArrangment.EventBasedLinks.Count == inputArrangment.EventBasedLinks.Count);
                Assert.IsTrue(outputArrangment.PropertyBasedLinks.Count == inputArrangment.PropertyBasedLinks.Count);
            }
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [TestMethod()]
        public async Task PublishAndRetriveGraphWithOneCollapsedGroup()
        {
            using (ShimsContext.Create())
            {
                // Fake Definitions
                byte[] publishedArrangmentByteArray = null;
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.PublishGraphAsyncInt64StringStringByteArrayByteArrayInt32StringInt64 = async (id, a, b, c, d, arrangmentByteArray, f, g, h) =>
                {
                    await Task.Delay(0);
                    publishedArrangmentByteArray = arrangmentByteArray;
                    return new Workspace.ServiceAccess.RemoteService.KGraphArrangement();
                };
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.GetPublishedGraphAsyncInt32 = async (a, b) =>
                {
                    await Task.Delay(0);
                    return publishedArrangmentByteArray;
                };
                Workspace.Logic.Fakes.ShimGraphRepositoryManager.GetArrangmentRelationshipIDsAsyncGraphArrangment = async (a) =>
                {
                    await Task.Delay(0);
                    return new List<long>();
                };
                Workspace.Logic.Fakes.ShimGraphRepositoryManager.HasGraphAnyUnpublishedConceptsListOfInt64ListOfInt64 = (a, b) =>
                {
                    return false;
                };

                // Arrange
                var inputGroupMasterObjectNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 100,
                    Position = new GraphArrangment.Point() { X = 10, Y = 20 },
                    IsVisible = true,
                    IsMasterOfGroup = true,
                    IsMasterOfACollapsedGroup = true
                };

                var inputCollapsedGroupSubNode1 = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 101,
                    Position = new GraphArrangment.Point() { X = 1110, Y = 1120 },
                    IsVisible = false,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };
                var inputCollapsedGroupSubNode2 = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 102,
                    Position = new GraphArrangment.Point() { X = 1210, Y = 1220 },
                    IsVisible = false,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };

                GraphArrangment inputArrangment = new GraphArrangment();
                inputArrangment.Objects.Add(inputGroupMasterObjectNode);
                inputArrangment.Objects.Add(inputCollapsedGroupSubNode1);
                inputArrangment.Objects.Add(inputCollapsedGroupSubNode2);
                inputArrangment.CollapsedGroupsRelativePoistions = new List<GraphArrangment.CollapsedGroupMembersPositionRelaterdToMasterInExpandedMode>();
                inputArrangment.CollapsedGroupsRelativePoistions.Add(new GraphArrangment.CollapsedGroupMembersPositionRelaterdToMasterInExpandedMode()
                {
                    NotResolvedGroupMasterObjectId = inputGroupMasterObjectNode.NotResolvedObjectId,
                    GroupMembersRelativePositionsByObjectId = new Dictionary<long, GraphArrangment.Point>()
                });
                inputArrangment.CollapsedGroupsRelativePoistions[0].GroupMembersRelativePositionsByObjectId.Add
                    (inputCollapsedGroupSubNode1.NotResolvedObjectId
                    , new GraphArrangment.Point() { X = 150, Y = 250 });
                inputArrangment.CollapsedGroupsRelativePoistions[0].GroupMembersRelativePositionsByObjectId.Add
                    (inputCollapsedGroupSubNode2.NotResolvedObjectId
                    , new GraphArrangment.Point() { X = 200, Y = 300 });
                inputArrangment.RelationshipBasedLinksExceptGroupInnerLinks = new List<GraphArrangment.RelationshipBasedLink>();
                inputArrangment.EventBasedLinks = new List<GraphArrangment.EventBasedLink>();
                inputArrangment.PropertyBasedLinks = new List<GraphArrangment.PropertyBasedLink>();
                GraphArrangment outputArrangment;
                // Act
                await GraphRepositoryManager.PublishGraphAsync("۱۲۳", "۱۲۳", inputArrangment, new byte[] { });
                outputArrangment = await GraphRepositoryManager.RetrieveGraphArrangmentAsync(1);
                //Assert
                Assert.IsTrue(outputArrangment.Objects.Count == inputArrangment.Objects.Count);

                var outputGroupMasterObjectNode = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputGroupMasterObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputGroupMasterObjectNode.NotResolvedObjectId == inputGroupMasterObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputGroupMasterObjectNode.Position.X == inputGroupMasterObjectNode.Position.X);
                Assert.IsTrue(outputGroupMasterObjectNode.Position.Y == inputGroupMasterObjectNode.Position.Y);
                Assert.IsTrue(outputGroupMasterObjectNode.IsVisible == inputGroupMasterObjectNode.IsVisible);
                Assert.IsTrue(outputGroupMasterObjectNode.IsMasterOfGroup == inputGroupMasterObjectNode.IsMasterOfGroup);
                Assert.IsTrue(outputGroupMasterObjectNode.IsMasterOfACollapsedGroup == inputGroupMasterObjectNode.IsMasterOfACollapsedGroup);

                var outputCollapsedGroupSubNode1 = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputCollapsedGroupSubNode1.NotResolvedObjectId);
                Assert.IsTrue(outputCollapsedGroupSubNode1.NotResolvedObjectId == inputCollapsedGroupSubNode1.NotResolvedObjectId);
                Assert.IsTrue(outputCollapsedGroupSubNode1.Position.X == inputCollapsedGroupSubNode1.Position.X);
                Assert.IsTrue(outputCollapsedGroupSubNode1.Position.Y == inputCollapsedGroupSubNode1.Position.Y);
                Assert.IsTrue(outputCollapsedGroupSubNode1.IsVisible == inputCollapsedGroupSubNode1.IsVisible);
                Assert.IsTrue(outputCollapsedGroupSubNode1.IsMasterOfGroup == inputCollapsedGroupSubNode1.IsMasterOfGroup);
                Assert.IsTrue(outputCollapsedGroupSubNode1.IsMasterOfACollapsedGroup == inputCollapsedGroupSubNode1.IsMasterOfACollapsedGroup);

                var outputCollapsedGroupSubNode2 = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputCollapsedGroupSubNode2.NotResolvedObjectId);
                Assert.IsTrue(outputCollapsedGroupSubNode2.NotResolvedObjectId == inputCollapsedGroupSubNode2.NotResolvedObjectId);
                Assert.IsTrue(outputCollapsedGroupSubNode2.Position.X == inputCollapsedGroupSubNode2.Position.X);
                Assert.IsTrue(outputCollapsedGroupSubNode2.Position.Y == inputCollapsedGroupSubNode2.Position.Y);
                Assert.IsTrue(outputCollapsedGroupSubNode2.IsVisible == inputCollapsedGroupSubNode2.IsVisible);
                Assert.IsTrue(outputCollapsedGroupSubNode2.IsMasterOfGroup == inputCollapsedGroupSubNode2.IsMasterOfGroup);
                Assert.IsTrue(outputCollapsedGroupSubNode2.IsMasterOfACollapsedGroup == inputCollapsedGroupSubNode2.IsMasterOfACollapsedGroup);

                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions.Count == inputArrangment.CollapsedGroupsRelativePoistions.Count);
                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions[0].NotResolvedGroupMasterObjectId
                    == inputArrangment.CollapsedGroupsRelativePoistions[0].NotResolvedGroupMasterObjectId);

                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions[0].GroupMembersRelativePositionsByObjectId
                    .ContainsKey(inputCollapsedGroupSubNode1.NotResolvedObjectId));
                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions[0]
                        .GroupMembersRelativePositionsByObjectId[inputCollapsedGroupSubNode1.NotResolvedObjectId].X ==
                    inputArrangment.CollapsedGroupsRelativePoistions[0]
                        .GroupMembersRelativePositionsByObjectId[inputCollapsedGroupSubNode1.NotResolvedObjectId].X);
                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions[0]
                        .GroupMembersRelativePositionsByObjectId[inputCollapsedGroupSubNode1.NotResolvedObjectId].Y ==
                    inputArrangment.CollapsedGroupsRelativePoistions[0]
                        .GroupMembersRelativePositionsByObjectId[inputCollapsedGroupSubNode1.NotResolvedObjectId].Y);

                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions[0].GroupMembersRelativePositionsByObjectId
                    .ContainsKey(inputCollapsedGroupSubNode2.NotResolvedObjectId));
                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions[0]
                        .GroupMembersRelativePositionsByObjectId[inputCollapsedGroupSubNode2.NotResolvedObjectId].X ==
                    inputArrangment.CollapsedGroupsRelativePoistions[0]
                        .GroupMembersRelativePositionsByObjectId[inputCollapsedGroupSubNode2.NotResolvedObjectId].X);
                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions[0]
                        .GroupMembersRelativePositionsByObjectId[inputCollapsedGroupSubNode2.NotResolvedObjectId].Y ==
                    inputArrangment.CollapsedGroupsRelativePoistions[0]
                        .GroupMembersRelativePositionsByObjectId[inputCollapsedGroupSubNode2.NotResolvedObjectId].Y);

                Assert.IsTrue(outputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count == inputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count);
                Assert.IsTrue(outputArrangment.EventBasedLinks.Count == inputArrangment.EventBasedLinks.Count);
                Assert.IsTrue(outputArrangment.PropertyBasedLinks.Count == inputArrangment.PropertyBasedLinks.Count);
            }
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [TestMethod()]
        public async Task PublishAndRetriveGraphWithOneRelationshipBaseLink()
        {
            using (ShimsContext.Create())
            {
                // Fake Definitions
                byte[] publishedArrangmentByteArray = null;
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.PublishGraphAsyncInt64StringStringByteArrayByteArrayInt32StringInt64 = async (id, a, b, c, d, arrangmentByteArray, f, g, h) =>
                {
                    await Task.Delay(0);
                    publishedArrangmentByteArray = arrangmentByteArray;
                    return new Workspace.ServiceAccess.RemoteService.KGraphArrangement();
                };
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.GetPublishedGraphAsyncInt32 = async (a, b) =>
                {
                    await Task.Delay(0);
                    return publishedArrangmentByteArray;
                };

                // Arrange
                var inputSourceObjectNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 100,
                    Position = new GraphArrangment.Point() { X = 10, Y = 20 },
                    IsVisible = true,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };
                var inputTargetObjectNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 102,
                    Position = new GraphArrangment.Point() { X = 1210, Y = 1220 },
                    IsVisible = true,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };

                var inputRelationshipBasedLink = new GraphArrangment.RelationshipBasedLink()
                {
                    SourceObjectId = inputSourceObjectNode.NotResolvedObjectId,
                    TargetObjectId = inputTargetObjectNode.NotResolvedObjectId,
                    RelationshipId = 201
                };

                GraphArrangment inputArrangment = new GraphArrangment();
                inputArrangment.Objects.Add(inputSourceObjectNode);
                inputArrangment.Objects.Add(inputTargetObjectNode);
                inputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Add(inputRelationshipBasedLink);
                GraphArrangment outputArrangment;
                // Act
                await GraphRepositoryManager.PublishGraphAsync("۱۲۳", "۱۲۳", inputArrangment, new byte[] { });
                outputArrangment = await GraphRepositoryManager.RetrieveGraphArrangmentAsync(1);
                //Assert
                Assert.IsTrue(outputArrangment.Objects.Count == inputArrangment.Objects.Count);

                var outputSourceObjectNode = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputSourceObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputSourceObjectNode.NotResolvedObjectId == inputSourceObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputSourceObjectNode.Position.X == inputSourceObjectNode.Position.X);
                Assert.IsTrue(outputSourceObjectNode.Position.Y == inputSourceObjectNode.Position.Y);
                Assert.IsTrue(outputSourceObjectNode.IsVisible == inputSourceObjectNode.IsVisible);
                Assert.IsTrue(outputSourceObjectNode.IsMasterOfGroup == inputSourceObjectNode.IsMasterOfGroup);
                Assert.IsTrue(outputSourceObjectNode.IsMasterOfACollapsedGroup == inputSourceObjectNode.IsMasterOfACollapsedGroup);

                var outputTargetObjectNode = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputTargetObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputTargetObjectNode.NotResolvedObjectId == inputTargetObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputTargetObjectNode.Position.X == inputTargetObjectNode.Position.X);
                Assert.IsTrue(outputTargetObjectNode.Position.Y == inputTargetObjectNode.Position.Y);
                Assert.IsTrue(outputTargetObjectNode.IsVisible == inputTargetObjectNode.IsVisible);
                Assert.IsTrue(outputTargetObjectNode.IsMasterOfGroup == inputTargetObjectNode.IsMasterOfGroup);
                Assert.IsTrue(outputTargetObjectNode.IsMasterOfACollapsedGroup == inputTargetObjectNode.IsMasterOfACollapsedGroup);

                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions.Count == inputArrangment.CollapsedGroupsRelativePoistions.Count);
                Assert.IsTrue(outputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count == inputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count);
                var outputRelationshipBaseLink = outputArrangment.RelationshipBasedLinksExceptGroupInnerLinks[0];
                Assert.IsTrue(outputRelationshipBaseLink.SourceObjectId == inputRelationshipBasedLink.SourceObjectId);
                Assert.IsTrue(outputRelationshipBaseLink.TargetObjectId == inputRelationshipBasedLink.TargetObjectId);
                Assert.IsTrue(outputRelationshipBaseLink.RelationshipId == inputRelationshipBasedLink.RelationshipId);

                Assert.IsTrue(outputArrangment.EventBasedLinks.Count == inputArrangment.EventBasedLinks.Count);
                Assert.IsTrue(outputArrangment.PropertyBasedLinks.Count == inputArrangment.PropertyBasedLinks.Count);
            }
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [TestMethod()]
        public async Task PublishAndRetriveGraphWithOneEventBaseLink()
        {
            using (ShimsContext.Create())
            {
                // Fake Definitions
                byte[] publishedArrangmentByteArray = null;
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.PublishGraphAsyncInt64StringStringByteArrayByteArrayInt32StringInt64 = async (id, a, b, c, d, arrangmentByteArray, f, g, h) =>
                {
                    await Task.Delay(0);
                    publishedArrangmentByteArray = arrangmentByteArray;
                    return new Workspace.ServiceAccess.RemoteService.KGraphArrangement();
                };
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.GetPublishedGraphAsyncInt32 = async (a, b) =>
                {
                    await Task.Delay(0);
                    return publishedArrangmentByteArray;
                };

                // Arrange
                var inputSourceObjectNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 100,
                    Position = new GraphArrangment.Point() { X = 10, Y = 20 },
                    IsVisible = true,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };
                var inputIntermediaryEventNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 101,
                    Position = new GraphArrangment.Point() { X = 1110, Y = 1120 },
                    IsVisible = false,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };
                var inputTargetObjectNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 102,
                    Position = new GraphArrangment.Point() { X = 1210, Y = 1220 },
                    IsVisible = true,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };

                var inputEventBasedLink = new GraphArrangment.EventBasedLink()
                {
                    SourceObjectId = inputSourceObjectNode.NotResolvedObjectId,
                    intermediaryEventId = inputIntermediaryEventNode.NotResolvedObjectId,
                    TargetObjectId = inputTargetObjectNode.NotResolvedObjectId,
                    SecondRelationshipId = 201,
                    FirstRelationshipId = 202
                };

                GraphArrangment inputArrangment = new GraphArrangment();
                inputArrangment.Objects.Add(inputSourceObjectNode);
                inputArrangment.Objects.Add(inputIntermediaryEventNode);
                inputArrangment.Objects.Add(inputTargetObjectNode);
                inputArrangment.EventBasedLinks.Add(inputEventBasedLink);
                GraphArrangment outputArrangment;
                // Act
                await GraphRepositoryManager.PublishGraphAsync("۱۲۳", "۱۲۳", inputArrangment, new byte[] { });
                outputArrangment = await GraphRepositoryManager.RetrieveGraphArrangmentAsync(1);
                //Assert
                Assert.IsTrue(outputArrangment.Objects.Count == inputArrangment.Objects.Count);

                var outputSourceObjectNode = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputSourceObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputSourceObjectNode.NotResolvedObjectId == inputSourceObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputSourceObjectNode.Position.X == inputSourceObjectNode.Position.X);
                Assert.IsTrue(outputSourceObjectNode.Position.Y == inputSourceObjectNode.Position.Y);
                Assert.IsTrue(outputSourceObjectNode.IsVisible == inputSourceObjectNode.IsVisible);
                Assert.IsTrue(outputSourceObjectNode.IsMasterOfGroup == inputSourceObjectNode.IsMasterOfGroup);
                Assert.IsTrue(outputSourceObjectNode.IsMasterOfACollapsedGroup == inputSourceObjectNode.IsMasterOfACollapsedGroup);

                var outputIntermediaryEventNode = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputIntermediaryEventNode.NotResolvedObjectId);
                Assert.IsTrue(outputIntermediaryEventNode.NotResolvedObjectId == inputIntermediaryEventNode.NotResolvedObjectId);
                Assert.IsTrue(outputIntermediaryEventNode.Position.X == inputIntermediaryEventNode.Position.X);
                Assert.IsTrue(outputIntermediaryEventNode.Position.Y == inputIntermediaryEventNode.Position.Y);
                Assert.IsTrue(outputIntermediaryEventNode.IsVisible == inputIntermediaryEventNode.IsVisible);
                Assert.IsTrue(outputIntermediaryEventNode.IsMasterOfGroup == inputIntermediaryEventNode.IsMasterOfGroup);
                Assert.IsTrue(outputIntermediaryEventNode.IsMasterOfACollapsedGroup == inputIntermediaryEventNode.IsMasterOfACollapsedGroup);

                var outputTargetObjectNode = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputTargetObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputTargetObjectNode.NotResolvedObjectId == inputTargetObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputTargetObjectNode.Position.X == inputTargetObjectNode.Position.X);
                Assert.IsTrue(outputTargetObjectNode.Position.Y == inputTargetObjectNode.Position.Y);
                Assert.IsTrue(outputTargetObjectNode.IsVisible == inputTargetObjectNode.IsVisible);
                Assert.IsTrue(outputTargetObjectNode.IsMasterOfGroup == inputTargetObjectNode.IsMasterOfGroup);
                Assert.IsTrue(outputTargetObjectNode.IsMasterOfACollapsedGroup == inputTargetObjectNode.IsMasterOfACollapsedGroup);

                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions.Count == inputArrangment.CollapsedGroupsRelativePoistions.Count);
                Assert.IsTrue(outputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count == inputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count);
                Assert.IsTrue(outputArrangment.EventBasedLinks.Count == inputArrangment.EventBasedLinks.Count);
                var outputEventBaseLink = outputArrangment.EventBasedLinks[0];
                Assert.IsTrue(outputEventBaseLink.SourceObjectId == inputEventBasedLink.SourceObjectId);
                Assert.IsTrue(outputEventBaseLink.FirstRelationshipId == inputEventBasedLink.FirstRelationshipId);
                Assert.IsTrue(outputEventBaseLink.intermediaryEventId == inputEventBasedLink.intermediaryEventId);
                Assert.IsTrue(outputEventBaseLink.SecondRelationshipId == inputEventBasedLink.SecondRelationshipId);
                Assert.IsTrue(outputEventBaseLink.TargetObjectId == inputEventBasedLink.TargetObjectId);

                Assert.IsTrue(outputArrangment.PropertyBasedLinks.Count == inputArrangment.PropertyBasedLinks.Count);
            }
        }

        [TestCategory("ذخیره و بازیابی گراف")]
        [TestMethod()]
        public async Task PublishAndRetriveGraphWithOnePropertyBaseLink()
        {
            using (ShimsContext.Create())
            {
                // Fake Definitions
                byte[] publishedArrangmentByteArray = null;
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.PublishGraphAsyncInt64StringStringByteArrayByteArrayInt32StringInt64 = async (id, a, b, c, d, arrangmentByteArray, f, g, h) =>
                {
                    await Task.Delay(0);
                    publishedArrangmentByteArray = arrangmentByteArray;
                    return new Workspace.ServiceAccess.RemoteService.KGraphArrangement();
                };
                Workspace.ServiceAccess.RemoteService.Fakes.ShimWorkspaceServiceClient.AllInstances.GetPublishedGraphAsyncInt32 = async (a, b) =>
                {
                    await Task.Delay(0);
                    return publishedArrangmentByteArray;
                };

                // Arrange
                var inputSourceObjectNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 100,
                    Position = new GraphArrangment.Point() { X = 10, Y = 20 },
                    IsVisible = true,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };
                var inputTargetObjectNode = new GraphArrangment.ObjectNode()
                {
                    NotResolvedObjectId = 102,
                    Position = new GraphArrangment.Point() { X = 1210, Y = 1220 },
                    IsVisible = true,
                    IsMasterOfGroup = false,
                    IsMasterOfACollapsedGroup = false
                };

                var inputProeprtyBasedLink = new GraphArrangment.PropertyBasedLink()
                {
                    SourceObjectId = inputSourceObjectNode.NotResolvedObjectId,
                    TargetObjectId = inputTargetObjectNode.NotResolvedObjectId,
                    SamePropertyTypeUri = "201",
                    SamePropertyValue = "202"
                };

                GraphArrangment inputArrangment = new GraphArrangment();
                inputArrangment.Objects.Add(inputSourceObjectNode);
                inputArrangment.Objects.Add(inputTargetObjectNode);
                inputArrangment.PropertyBasedLinks.Add(inputProeprtyBasedLink);
                GraphArrangment outputArrangment;
                // Act
                await GraphRepositoryManager.PublishGraphAsync("۱۲۳", "۱۲۳", inputArrangment, new byte[] { });
                outputArrangment = await GraphRepositoryManager.RetrieveGraphArrangmentAsync(1);
                //Assert
                Assert.IsTrue(outputArrangment.Objects.Count == inputArrangment.Objects.Count);

                var outputSourceObjectNode = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputSourceObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputSourceObjectNode.NotResolvedObjectId == inputSourceObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputSourceObjectNode.Position.X == inputSourceObjectNode.Position.X);
                Assert.IsTrue(outputSourceObjectNode.Position.Y == inputSourceObjectNode.Position.Y);
                Assert.IsTrue(outputSourceObjectNode.IsVisible == inputSourceObjectNode.IsVisible);
                Assert.IsTrue(outputSourceObjectNode.IsMasterOfGroup == inputSourceObjectNode.IsMasterOfGroup);
                Assert.IsTrue(outputSourceObjectNode.IsMasterOfACollapsedGroup == inputSourceObjectNode.IsMasterOfACollapsedGroup);

                var outputTargetObjectNode = outputArrangment.Objects
                    .Single(o => o.NotResolvedObjectId == inputTargetObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputTargetObjectNode.NotResolvedObjectId == inputTargetObjectNode.NotResolvedObjectId);
                Assert.IsTrue(outputTargetObjectNode.Position.X == inputTargetObjectNode.Position.X);
                Assert.IsTrue(outputTargetObjectNode.Position.Y == inputTargetObjectNode.Position.Y);
                Assert.IsTrue(outputTargetObjectNode.IsVisible == inputTargetObjectNode.IsVisible);
                Assert.IsTrue(outputTargetObjectNode.IsMasterOfGroup == inputTargetObjectNode.IsMasterOfGroup);
                Assert.IsTrue(outputTargetObjectNode.IsMasterOfACollapsedGroup == inputTargetObjectNode.IsMasterOfACollapsedGroup);

                Assert.IsTrue(outputArrangment.CollapsedGroupsRelativePoistions.Count == inputArrangment.CollapsedGroupsRelativePoistions.Count);
                Assert.IsTrue(outputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count == inputArrangment.RelationshipBasedLinksExceptGroupInnerLinks.Count);
                Assert.IsTrue(outputArrangment.EventBasedLinks.Count == inputArrangment.EventBasedLinks.Count);
                Assert.IsTrue(outputArrangment.PropertyBasedLinks.Count == inputArrangment.PropertyBasedLinks.Count);
                var outputPropertyBaseLink = outputArrangment.PropertyBasedLinks[0];
                Assert.IsTrue(outputPropertyBaseLink.SourceObjectId == inputProeprtyBasedLink.SourceObjectId);
                Assert.IsTrue(outputPropertyBaseLink.TargetObjectId == inputProeprtyBasedLink.TargetObjectId);
                Assert.IsTrue(outputPropertyBaseLink.SamePropertyTypeUri == inputProeprtyBasedLink.SamePropertyTypeUri);
                Assert.IsTrue(outputPropertyBaseLink.SamePropertyValue == inputProeprtyBasedLink.SamePropertyValue);
            }
        }
    }
}
