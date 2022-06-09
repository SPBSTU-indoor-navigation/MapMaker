using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
public class UUIDStorage : MonoBehaviour
{

    [System.Serializable]
    public struct Item
    {
        public string id;
        public System.Guid identifier => new System.Guid(id);
        public IMDF.FeatureMB featureMB;
    }

    [System.Serializable]
    public class ItemCollection
    {
        public List<Item> items = new List<Item>();

        public void Add(Item item)
        {
            items.Add(item);
        }

    }

    public List<ItemCollection> items = new List<ItemCollection>();

    public void Add(IMDF.FeatureMB featureMB, System.Guid id, int order)
    {
        dict[order].Add(featureMB, id);
        items[order].Add(new Item() { id = id.ToString(), featureMB = featureMB });
    }

    static UUIDStorage _shared;

    public static UUIDStorage shared
    {
        get
        {
            if (!_shared)
            {
                _shared = GameObject.FindObjectOfType<UUIDStorage>(true);
            }
            return _shared;
        }
    }

    private void Start()
    {
        _shared = this;
    }

    Dictionary<IMDF.FeatureMB, System.Guid>[] dict = new Dictionary<IMDF.FeatureMB, System.Guid>[0];

    public void Load()
    {
        dict = new Dictionary<IMDF.FeatureMB, System.Guid>[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            dict[i] = items[i].items.ToDictionary(t => t.featureMB, t => System.Guid.Parse(t.id));
        }
    }

    public void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public System.Guid GetAddressGUID(IMDF.Address address)
    {
        return System.Guid.NewGuid();
    }

    public System.Guid GetGUID(IMDF.FeatureMB featureMB, int order = 0)
    {
        if (dict[order].ContainsKey(featureMB))
            return dict[order][featureMB];

        var id = System.Guid.NewGuid();
        Add(featureMB, id, order);

        return id;
    }
}
