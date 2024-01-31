using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Convertor
{
    public class PropertyTypeURIToValidateForChartConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if (value == null)
                return false;

            string typeURI = value.ToString();
            var ontology = Logic.OntologyProvider.GetOntology();
            var typeBase = ontology.GetBaseDataTypeOfProperty(typeURI);

            if (typeBase == Ontology.BaseDataTypes.Double || typeBase == Ontology.BaseDataTypes.Int || typeBase == Ontology.BaseDataTypes.Long)
                return true;
            else return false;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
