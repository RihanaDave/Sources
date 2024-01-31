using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [Serializable]
    public class ValueMapModel : MapElement
    {
        #region Properties

        string id;
        public string Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        DataSourceFieldModel field;
        public DataSourceFieldModel Field
        {
            get => field;
            set
            {
                if (SetValue(ref field, value))
                {
                    if (Field != null)
                    {
                        Field.SampleValueChanged -= Field_SampleValueChanged;
                        Field.SampleValueChanged += Field_SampleValueChanged;

                        SetSampleValueFromField();
                        OnScenarioChanged();
                    }
                }
            }
        }

        bool hasRegularExpression;
        public bool HasRegularExpression
        {
            get => hasRegularExpression;
            set
            {
                if (SetValue(ref hasRegularExpression, value))
                {
                    SetSampleValueFromField();
                    OnScenarioChanged();
                }
            }
        }

        string regularExpressionPattern = string.Empty;
        public string RegularExpressionPattern
        {
            get => regularExpressionPattern;
            set
            {
                if (SetValue(ref regularExpressionPattern, value))
                {
                    SetSampleValueFromField();
                    OnScenarioChanged();
                }
            }
        }

        string sampleValue = string.Empty;
        public string SampleValue
        {
            get => sampleValue;
            set
            {
                if (SetValue(ref sampleValue, SetRegularExpression(value)))
                {
                    OnSampleValueChanged();
                }
            }
        }

        PropertyMapModel ownerProperty;
        [XmlIgnore]
        public PropertyMapModel OwnerProperty
        {
            get => ownerProperty;
            set
            {
                if (SetValue(ref ownerProperty, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        #endregion

        #region Methods

        public ValueMapModel()
        {
            Id = Guid.NewGuid().ToString();
        }

        private void SetSampleValueFromField()
        {
            SampleValue = Field.SampleValue;
        }

        private void Field_SampleValueChanged(object sender, EventArgs e)
        {
            SetSampleValueFromField();
        }

        private string SetRegularExpression(string value)
        {
            if (HasRegularExpression)
            {
                try
                {
                    MatchCollection matches = Regex.Matches(value, RegularExpressionPattern);
                    string result = string.Empty;
                    foreach (Match match in matches)
                    {
                        result += match.Value;
                    }
                    return result;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
            else
            {
                return value;
            }
        }

        public bool Equals(ValueMapModel otherValue)
        {
            return Id.Equals(otherValue.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

        #region Events

        public event EventHandler SampleValueChanged;
        protected void OnSampleValueChanged()
        {
            SampleValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
