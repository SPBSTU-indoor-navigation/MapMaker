using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{

    public interface ILocalizedName
    {
        public Feature.LocalizedName getFeature();
        public bool IsEmpty();
    }

    public abstract class LocalizedNameBase : ILocalizedName
    {
        protected Feature.LocalizedName getFeature(string ru, string en)
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

        public bool IsEmpty(string ru, string en)
        {
            return string.IsNullOrWhiteSpace(ru) && string.IsNullOrWhiteSpace(en);
        }

        public abstract Feature.LocalizedName getFeature();
        public abstract bool IsEmpty();
    }


    [System.Serializable]
    public class LocalizedName : LocalizedNameBase
    {
        public string ru, en;

        public override Feature.LocalizedName getFeature()
        {
            return getFeature(ru, en);
        }

        public override bool IsEmpty()
        {
            return IsEmpty(ru, en);
        }
    }

    [System.Serializable]
    public class LocalizedNameMultiline : LocalizedNameBase
    {
        [TextArea(1, 10)]
        public string ru, en;

        public override Feature.LocalizedName getFeature()
        {
            return getFeature(ru, en);
        }

        public override bool IsEmpty()
        {
            return IsEmpty(ru, en);
        }
    }

}
