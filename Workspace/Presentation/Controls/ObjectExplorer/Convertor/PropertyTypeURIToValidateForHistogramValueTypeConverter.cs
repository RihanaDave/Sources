using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Convertor
{
    public class PropertyTypeURIToValidateForHistogramValueTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if (value == null)
                return true;

            string typeURI = value.ToString();
            var ontology = Logic.OntologyProvider.GetOntology();

            if (ontology.GetBaseDataTypeOfProperty(typeURI) == Ontology.BaseDataTypes.GeoTime ||
                ontology.GetBaseDataTypeOfProperty(typeURI) == Ontology.BaseDataTypes.GeoPoint ||
                ontology.GetBaseDataTypeOfProperty(typeURI) == Ontology.BaseDataTypes.DateTime)
                return false;
            else return true;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
