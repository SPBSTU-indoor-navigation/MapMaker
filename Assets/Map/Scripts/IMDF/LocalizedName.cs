using System.Collections.Generic;

namespace IMDF
{
    [System.Serializable]
    public class LocalizedName
    {
        public string ru, en;

        public Feature.LocalizedName getFeature()
        {
            if (string.IsNullOrWhiteSpace(ru) && string.IsNullOrWhiteSpace(en))
            {
                return null;
            }

            Dictionary<string, string> local = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(ru))
                local.Add("ru", ru);

            if (!string.IsNullOrWhiteSpace(en))
                local.Add("en", en);

            return new Feature.LocalizedName(local);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(ru) && string.IsNullOrWhiteSpace(en);
        }
    }

}
