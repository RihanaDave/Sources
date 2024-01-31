namespace GPAS.Workspace.Presentation.Controls.TagCloud
{
    public class KeyPhraseModel : BaseModel
    {
        private string key;
        public string Key
        {
            get => key;
            set
            {
                SetValue(ref key, value);
            }
        }

        private float weight;
        public float Weight
        {
            get => weight;
            set
            {
                SetValue(ref weight, value);
            }
        }
    }
}
