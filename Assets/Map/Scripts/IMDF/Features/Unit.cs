using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IMDF
{
    [RequireComponent(typeof(RefferencePointEditor))]
    public class Unit : GeometryPolygon, IRefferencePoint, IAddress, IAnchor, IOccupant, IAnnotation
    {

        public LocalizedName localizedName;
        public LocalizedName altName;
        public Feature.Unit.Category category;
        public Feature.RestrictionCategory restriction = Feature.RestrictionCategory.nullable;

        public bool showDisplayPoint = true;
        public AddressContainer address;

        [Space]
        [Header("Occupant")]
        public bool generateOccupant = true;
        public string iconUrl;
        [TextArea(1, 6)]
        public string description;
        public string website;
        public Feature.Occupant.Category occupantCategory = Feature.Occupant.Category.unspecified;

        [HideInInspector, SerializeField]
        Vector2 displayPoint_ = Vector2.zero;

        public Vector2 displayPoint
        {
            get
            {
                return displayPoint_;
            }
            set
            {
                displayPoint_ = value;
            }
        }

        bool IRefferencePoint.showDisplayPoint => showDisplayPoint;


        Address IAddress.address
        {
            get
            {
                if (address) return address.address;

                if (GetComponentInParent<Building>(true).addressId)
                {
                    var address = new Address(addresGuid, GetComponentInParent<Building>(true).addressId.address);
                    address.unit = localizedName.ru;
                    return address;
                }

                return null;
            }
        }

        public bool hasOccupant => altName.getFeature() != null && generateOccupant;

        Feature.Anchor IAnchor.anchor => new Feature.Anchor(this);

        Feature.Occupant IOccupant.occupant
        {
            get
            {
                if (hasOccupant)
                {
                    return new Feature.Occupant(this);
                }
                return null;
            }
        }

        System.Guid? IAnnotation.identifier => ((IOccupant)this).occupant?.identifier;

        public System.Guid anchorGuid, addresGuid, occupantGuid;


        public void NewObj()
        {
            GetComponent<PolygonGeometry>().color = new Color(Random.Range(0.17f, 0.4f), 0.17f, 0.4f);
            displayPoint = Vector2.zero;
        }

        public void OnValidate()
        {
            gameObject.name = category.ToString() + "_" + altName.ru;
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI2;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI2;
        }

        private GUIStyle guiStyle = new GUIStyle();
        void OnSceneGUI2(SceneView sceneView)
        {
            Handles.BeginGUI();
            float dist = Vector3.Distance(sceneView.camera.transform.position, transform.position);
            if (dist < 1000)
            {
                guiStyle.fontSize = (int)Mathf.Max(10, Mathf.Min(20, 20 - dist / 10));
                guiStyle.normal.textColor = Color.white;
                guiStyle.alignment = TextAnchor.UpperLeft;

                Handles.Label(transform.position + (showDisplayPoint ? new Vector3(displayPoint.x, displayPoint.y, 0) : Vector3.zero), string.IsNullOrWhiteSpace(altName.ru) ? "-" : altName.ru, guiStyle);
            }
            Handles.EndGUI();

        }

        public override void GenerateGUID()
        {
            base.GenerateGUID();
            anchorGuid = UUIDStorage.shared.GetGUID(this, 1);
            addresGuid = UUIDStorage.shared.GetGUID(this, 2);
            occupantGuid = UUIDStorage.shared.GetGUID(this, 3);
        }


    }
}
