using Neo4j.Driver;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j
{
    public static class NodeOperation
    {
        public static bool HasProperty(INode node, string propertyName)
        {
            return node.Properties.ContainsKey(propertyName);
        }

        public static bool TryGetPropertyValue(INode node, string propertyName, out object propertyValues)
        {
            if (HasProperty(node, propertyName))
            {
                propertyValues = node.Properties[propertyName];
                return true;
            }
            else
            {
                propertyValues = null;
                return false;
            }
        }
    }
}
