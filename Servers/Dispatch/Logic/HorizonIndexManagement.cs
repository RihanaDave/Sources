using GPAS.Dispatch.Entities;
using System;
using System.Collections.Generic;
using Horizon = GPAS.Dispatch.ServiceAccess.HorizonService;

namespace GPAS.Dispatch.Logic
{
    public class HorizonIndexManagement
    {
        public IEnumerable<HorizonIndex> GetAllIndexes()
        {
            Horizon.ServiceClient serviceClient = new Horizon.ServiceClient();
            return ConverterHorizonIndexes(serviceClient.GetAllIndexes());
        }

        public void CreateIndex(HorizonIndex indexModel)
        {
            if (indexModel == null)
                throw new ArgumentNullException(nameof(indexModel));

            Horizon.IndexModel newIndex = new Horizon.IndexModel()
            {
                NodeType = indexModel.NodeType,
                PropertiesType = indexModel.PropertiesType
            };

            Horizon.ServiceClient serviceClient = new Horizon.ServiceClient();
            serviceClient.CreateIndex(newIndex);
        }

        public void DeleteIndex(HorizonIndex indexModel)
        {
            if (indexModel == null)
                throw new ArgumentNullException(nameof(indexModel));

            Horizon.IndexModel indexToDelete = new Horizon.IndexModel()
            {
                NodeType = indexModel.NodeType,
                PropertiesType = indexModel.PropertiesType
            };

            Horizon.ServiceClient serviceClient = new Horizon.ServiceClient();
            serviceClient.DeleteIndex(indexToDelete);
        }

        public void EditIndex(HorizonIndex oldIndexModel, HorizonIndex newIndexModel)
        {
            if (oldIndexModel == null)
                throw new ArgumentNullException(nameof(oldIndexModel));

            if (newIndexModel == null)
                throw new ArgumentNullException(nameof(newIndexModel));

            Horizon.IndexModel oldIndex = new Horizon.IndexModel()
            {
                NodeType = oldIndexModel.NodeType,
                PropertiesType = oldIndexModel.PropertiesType
            };

            Horizon.IndexModel newIndex = new Horizon.IndexModel()
            {
                NodeType = newIndexModel.NodeType,
                PropertiesType = newIndexModel.PropertiesType
            };

            Horizon.ServiceClient serviceClient = new Horizon.ServiceClient();
            serviceClient.EditIndex(oldIndex, newIndex);
        }

        public void DeleteAllIndexes()
        {
            Horizon.ServiceClient serviceClient = new Horizon.ServiceClient();
            serviceClient.DeleteAllIndexes();
        }

        private IEnumerable<HorizonIndex> ConverterHorizonIndexes(Horizon.IndexModel[] indexes)
        {
            List<HorizonIndex> result = new List<HorizonIndex>();

            foreach (var index in indexes)
            {
                result.Add(new HorizonIndex()
                {
                    NodeType = index.NodeType,
                    PropertiesType = index.PropertiesType
                });
            }

            return result;
        }
    }
}
