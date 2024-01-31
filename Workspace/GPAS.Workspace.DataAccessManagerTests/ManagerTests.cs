using GPAS.Workspace.ServiceAccess.RemoteService;

namespace GPAS.Workspace.DataAccessManager.Tests
{
    public class DamTests
    {
        internal static long FakeStoredObjectIdCounter = 1;
        internal static long FakeStoredPropertyIdCounter = 1;
        internal static long FakeStoredRelationshipIdCounter = 1;
        internal static long FakeStoredMediaIdCounter = 1;

        protected static long GenerateSupposalStoredObjectId()
        {
            return FakeStoredObjectIdCounter++;
        }
        protected static long GenerateSupposalStoredPropertyId()
        {
            return FakeStoredPropertyIdCounter++;
        }
        protected static long GenerateSupposalStoredRelationshipId()
        {
            return FakeStoredRelationshipIdCounter++;
        }
        protected static long GenerateSupposalStoredMediaId()
        {
            return FakeStoredMediaIdCounter++;
        }

        protected static long[] GenerateSupposalStoredObjectIdRange(int count)
        {
            long[] result = new long[count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GenerateSupposalStoredObjectId();
            }
            return result;
        }
        protected static long[] GenerateSupposalStoredPropertyIdRange(int count)
        {
            long[] result = new long[count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GenerateSupposalStoredPropertyId();
            }
            return result;
        }
        protected static long[] GenerateSupposalStoredRelationshipId(int count)
        {
            long[] result = new long[count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GenerateSupposalStoredObjectId();
            }
            return result;
        }

        protected void ShimRemoteServiceClientCreateAndClose()
        {
            global::System.ServiceModel.Fakes.ShimClientBase<IWorkspaceService>.AllInstances.Close = (cb) => { };
            ServiceAccess.Fakes.ShimRemoteServiceClientFactory.GetNewClient = () => { return new WorkspaceServiceClient(); };
        }
    }
}
