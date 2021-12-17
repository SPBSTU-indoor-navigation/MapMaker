using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IMDF
{
    [CreateAssetMenu(fileName = "Address", menuName = "IMDF/Address", order = 0)]
    public class Address : ScriptableObject
    {
        public Guid guid = Guid.NewGuid();
        public string address; //Formatted postal address, excluding suite/unit identifier, i.e. "123 E. Main Street"
        public string unit; //Qualifying official or proprietary unit/suite designation, i.e. "2A"
        public string locality = "city"; //Official locality (e.g. city, town) component of the postal address
        public string province = "RU-SPE"; //Province (e.g. state, territory) component of the postal address
        public string country = "RU"; //Country component of the postal address
        public string postal_code; //Mail sorting code associated with the postal address
        public string postal_code_ext; //Mail sorting code extension associated with the postal code
        public string postal_code_vanity; // Mail sorting vanity code that is recognized by the postal delivery service
    }
}

// "address": "123 E. Main Street",
// "unit": "1A",
// "locality": "Anytown",
// "province": "US-CA",
// "country": "US",
// "postal_code": "12345",
// "postal_code_ext": "1111"
