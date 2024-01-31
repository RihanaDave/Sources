using GPAS.HistogramViewer;
using GPAS.Workspace.Entities;
using System.Collections.Generic;
using System;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public class ObjectTypesHistogramTypeCountPair : IHistogramFillingValueCountPair
    {
        public ObjectTypesHistogramTypeCountPair(string uriOfType, List<KWObject> objectsWithTheType, string pairTitle = "", bool isTopPairInCategory = false)
        {
            typeUri = uriOfType;
            ObjectsWithType = objectsWithTheType;
            title = pairTitle;
            isCategoryTopPair = isTopPairInCategory;
        }

        public List<KWObject> ObjectsWithType
        {
            get;
            private set;
        }
        public int HistogramCountForValue
        {
            get { return ObjectsWithType.Count; }
        }

        private string typeUri;
        private string title;
        public string HistogramValue
        {
            get
            {
                return
                  (string.IsNullOrWhiteSpace(title))
                  ? Logic.OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri)
                  : title;
            }
        }

        private bool isCategoryTopPair;
        public bool IsTopValueCountPairInHistogramCategory
        {
            get { return isCategoryTopPair; }
        }
    }
}