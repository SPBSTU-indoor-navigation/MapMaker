using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    [CreateAssetMenu(fileName = "Address", menuName = "IMDF/Address", order = 0)]
    public class AddressContainer : ScriptableObject
    {
        public Address address;
        public void GenerateGUID()
        {
            address.guid = UUIDStorage.shared.GetAddressGUID(address);
        }
    }

    [System.Serializable]
    public class Address
    {
        public Guid guid;
        public string address; //Formatted postal address, excluding suite/unit identifier, i.e. "123 E. Main Street"
        public string unit; //Qualifying official or proprietary unit/suite designation, i.e. "2A"
        public string locality = "city"; //Official locality (e.g. city, town) component of the postal address
        public string province = "RU-SPE"; //Province (e.g. state, territory) component of the postal address
        public string country = "RU"; //Country component of the postal address
        public string postal_code; //Mail sorting code associated with the postal address
        public string postal_code_ext; //Mail sorting code extension associated with the postal code
        public string postal_code_vanity; // Mail sorting vanity code that is recognized by the postal delivery service

        public Address(Guid guid, Address address)
        {
            this.guid = guid;
            this.address = address.address;
            unit = address.unit;
            locality = address.locality;
            province = address.province;
            country = address.country;
            postal_code = address.postal_code;
            postal_code_ext = address.postal_code_ext;
            postal_code_vanity = address.postal_code_vanity;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return guid == (obj as Address).guid;
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }
    }
}

// "address": "123 E. Main Street",
// "unit": "1A",
// "locality": "Anytown",
// "province": "US-CA",
// "country": "US",
// "postal_code": ".12345",
// "postal_code_ext": "1111"
