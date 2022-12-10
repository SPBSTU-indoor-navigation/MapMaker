using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    public class Occupant : GeometryPoint, IAnchor, IAddress, IOccupant, IAnnotation
    {
        public LocalizedName fullName;
        public LocalizedName shortName;
        public Feature.Occupant.Category category = Feature.Occupant.Category.unspecified;
        public string description;
        public string iconUrl;

        public AddressContainer address;
        public string hours;
        public string phone;
        public string email;
        public string website;

        Feature.Anchor IAnchor.anchor => new Feature.Anchor(this);

        Address IAddress.address => address ? address.address : (GetComponentInParent<Unit>(true) as IAddress).address;

        public bool hasOccupant => true;

        Feature.Occupant IOccupant.occupant => new Feature.Occupant(this);

        Guid? IAnnotation.identifier => guid;

        [HideInInspector] public System.Guid anchorGuid;

        public override void GenerateGUID()
        {
            base.GenerateGUID();
            address?.GenerateGUID();
            anchorGuid = UUIDStorage.shared.GetGUID(this, 2);
        }
    }
}
