using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;

namespace IMDF.Feature
{
    public class RestrictionCategoryStringEnumConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.Equals(RestrictionCategory.nullable)) value = null;
            base.WriteJson(writer, value, serializer);
        }
    }

    [JsonConverter(typeof(RestrictionCategoryStringEnumConverter))]
    public enum RestrictionCategory
    {
        employeesonly,
        restricted,
        nullable
    }

    public abstract class Serializable : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == this.GetType();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotImplementedException();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

    }

    public abstract class GeoJSONObject : Serializable { }

    public abstract class GeoJSONGeometry : GeoJSONObject { }

    [JsonConverter(typeof(Polygon))]
    public class Polygon : Serializable
    {
        public Point[] points;

        public Polygon() { }

        public Polygon(Point[] points)
        {
            this.points = points;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Polygon polygon = value as Polygon;
            serializer.Serialize(writer, polygon.points);
        }
    }

    [JsonConverter(typeof(GeoPolygon))]
    public class GeoPolygon : Serializable
    {
        public Polygon[] polygons;

        public GeoPolygon() { }

        public GeoPolygon(Polygon[] polygons)
        {
            this.polygons = polygons;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            GeoPolygon polygon = value as GeoPolygon;
            serializer.Serialize(writer, polygon.polygons);
        }
    }

    [JsonConverter(typeof(Point))]
    public class Point : Serializable
    {
        public double x;
        public double y;

        public Point() { }

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(Vector2 point)
        {
            this.x = point.x;
            this.y = point.y;
        }

        public Point(IRefferencePoint point)
        {
            this.x = point.displayPoint.x;
            this.y = point.displayPoint.y;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Point point = value as Point;
            writer.WriteStartArray();

            writer.WriteValue(point.x);
            writer.WriteValue(point.y);

            writer.WriteEndArray();
        }
    }

    [JsonConverter(typeof(LocalizedName))]
    public class LocalizedName : Serializable
    {
        public Dictionary<string, string> localizations;

        public LocalizedName() { }

        public LocalizedName(Dictionary<string, string> localizations)
        {
            this.localizations = localizations;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var localizedName = value as LocalizedName;
            serializer.Serialize(writer, localizedName.localizations);
        }
    }

    [JsonConverter(typeof(GeoJSONPoint))]
    public class GeoJSONPoint : GeoJSONGeometry
    {
        public Point point;

        public GeoJSONPoint() { }

        public GeoJSONPoint(Point point)
        {
            this.point = point;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            GeoJSONPoint point = value as GeoJSONPoint;
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("Point");

            writer.WritePropertyName("coordinates");
            serializer.Serialize(writer, point.point);
            writer.WriteEndObject();
        }

    }

    [JsonConverter(typeof(GeoJSONPolygon))]
    public class GeoJSONPolygon : GeoJSONGeometry
    {
        public GeoPolygon[] polygons;

        public GeoJSONPolygon() { }

        public GeoJSONPolygon(GeoPolygon[] polygons)
        {
            this.polygons = polygons;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            GeoJSONPolygon polygon = value as GeoJSONPolygon;

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(polygon.polygons.Length == 1 ? "Polygon" : "MultiPolygon");

            writer.WritePropertyName("coordinates");
            serializer.Serialize(writer, polygon.polygons.Length == 1 ? polygon.polygons[0] : polygon.polygons);
            writer.WriteEndObject();
        }
    }

    [JsonConverter(typeof(GeoJSONLineString))]
    public class GeoJSONLineString : GeoJSONGeometry
    {
        public Point[] points;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            GeoJSONLineString line = value as GeoJSONLineString;
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("LineString");

            writer.WritePropertyName("coordinates");
            serializer.Serialize(writer, line.points);
            writer.WriteEndObject();
        }
    }

    [JsonConverter(typeof(GeoJSONMultiLineString))]
    public class GeoJSONMultiLineString : GeoJSONGeometry
    {
        public List<Point[]> lines;

        public GeoJSONMultiLineString() { }

        public GeoJSONMultiLineString(List<Point[]> lines)
        {
            this.lines = lines;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            GeoJSONMultiLineString line = value as GeoJSONMultiLineString;
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue("MultiLineString");

            writer.WritePropertyName("coordinates");
            serializer.Serialize(writer, line.lines);
            writer.WriteEndObject();
        }
    }

    [JsonConverter(typeof(FeatureCollection))]
    public class FeatureCollection : GeoJSONObject
    {
        public GeoJSONObject[] features;
        public string collectionName = "1234";

        public FeatureCollection() { }
        public FeatureCollection(GeoJSONObject[] features, string collectionName)
        {
            this.features = features;
            this.collectionName = collectionName;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var collection = value as FeatureCollection;

            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue("FeatureCollection");

            writer.WritePropertyName("name");
            writer.WriteValue(collection.collectionName);

            writer.WritePropertyName("features");
            serializer.Serialize(writer, collection.features);

            writer.WriteEndObject();
        }

    }

    public class Feature<T> : GeoJSONObject
    {
        public Guid identifier;
        public GeoJSONGeometry geometry;
        public T properties;

        public void WriteJson(JsonWriter writer, object value, JsonSerializer serializer, string featureType)
        {
            var feature = value as Feature<T>;

            writer.WriteStartObject();

            writer.WritePropertyName("id");
            writer.WriteValue(feature.identifier);

            writer.WritePropertyName("type");
            writer.WriteValue("Feature");

            writer.WritePropertyName("feature_type");
            writer.WriteValue(featureType);

            writer.WritePropertyName("geometry");
            serializer.Serialize(writer, feature.geometry);

            writer.WritePropertyName("properties");
            serializer.Serialize(writer, feature.properties);

            writer.WriteEndObject();
        }
    }

    [JsonConverter(typeof(Address))]
    public class Address : Feature<Address.Properties>
    {
        public class Properties
        {
            public string address;
            public string unit;
            public string locality;
            public string province;
            public string country;
            public string postal_code;
            public string postal_code_ext;
            public string postal_code_vanity;

            public Properties(IMDF.Address address)
            {
                this.address = address.address;
                unit = address.unit;
                locality = address.locality;
                province = address.province;
                country = address.country;
                postal_code = address.postal_code.OrNull();
                postal_code_ext = address.postal_code_ext.OrNull();
                postal_code_vanity = address.postal_code_vanity.OrNull();
            }

        }

        public Address() { }

        public Address(IMDF.Address address)
        {
            identifier = address.guid;
            geometry = null;
            properties = new Properties(address);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "address");
        }
    }

    [JsonConverter(typeof(Venue))]
    public class Venue : Feature<Venue.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            airport,
            aquarium,
            businesscampus,
            casino,
            communitycenter,
            conventioncenter,
            governmentfacility,
            healthcarefacility,
            hotel,
            museum,
            parkingfacility,
            resort,
            retailstore,
            shoppingcenter,
            stadium,
            stripmall,
            theater,
            themepark,
            trainstation,
            transitstation,
            university,
        }

        public class Properties
        {
            public LocalizedName name;
            public LocalizedName alt_name;
            public Category category;
            public RestrictionCategory restriction;

            public string hours;
            public string phone;
            public string website;

            public Guid address_id;
            public Guid? navpath_begin_id;

            public Properties(IMDF.Venue venue)
            {
                name = venue.localizedName.getFeature();
                alt_name = venue.altName.getFeature();
                category = venue.category;
                restriction = venue.restriction;

                hours = venue.hours;
                phone = venue.phone;
                website = venue.website;

                address_id = venue.address.address.guid;
                navpath_begin_id = venue.defaultPathBegin?.guid;
            }
        }

        public Venue() { }

        public Venue(IMDF.Venue venue)
        {
            identifier = venue.guid;
            geometry = venue.GetGeoJSONGeometry();
            properties = new Properties(venue);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "venue");
        }
    }

    [JsonConverter(typeof(Building))]
    public class Building : Feature<Building.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            parking,
            transit,
            transitBus,
            transitTrain,
            unspecified,
        }

        public class Properties
        {
            public LocalizedName name;
            public LocalizedName alt_name;
            public Category category;
            public RestrictionCategory restriction;
            public float rotation;
            public GeoJSONGeometry display_point;
            public Guid? address_id;

            public Properties(IMDF.Building building)
            {
                name = building.localizedName.getFeature();
                alt_name = building.altName.getFeature();
                category = building.category;

                restriction = building.restriction;
                display_point = RefferencePointEditor.GetGeoJSONPoint(building);
                address_id = building.addressId?.address.guid;
                rotation = building.rotation;
            }
        }

        public Building() { }

        public Building(IMDF.Building building)
        {
            identifier = building.guid;
            geometry = building.GetGeoJSONGeometry();
            properties = new Properties(building);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "building");
        }
    }

    [JsonConverter(typeof(Footprint))]
    public class Footprint : Feature<Footprint.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            aerial,
            ground,
            subterranean
        }

        public class Properties
        {
            public Category category;
            public string name;
            public Guid[] building_ids;

            public Properties(IMDF.Building building, Category category)
            {
                this.category = category;
                name = null;
                building_ids = new Guid[] { building.guid };
            }
        }

        public Footprint() { }

        public Footprint(IMDF.Building building, Category category)
        {
            identifier = Guid.NewGuid();

            if (category == Category.ground ||
                (category == Category.aerial && building.aerial == null) ||
                (category == Category.subterranean && building.subterranean == null))
            {
                geometry = building.GetGeoJSONGeometry();
            }
            else if (category == Category.aerial)
            {
                geometry = GeometryPolygon.GetGeoJSONGeometry(building.aerial.GetComponents<PolygonCollider2D>());
            }
            else
            {
                geometry = GeometryPolygon.GetGeoJSONGeometry(building.subterranean.GetComponents<PolygonCollider2D>());
            }

            properties = new Properties(building, category);
        }

        public static Footprint[] FootprintFromBuilding(IMDF.Building building)
        {
            Footprint ground = new Footprint(building, Category.ground);
            Footprint aerial = new Footprint(building, Category.aerial);
            Footprint subterranean = new Footprint(building, Category.subterranean);

            return new Footprint[] { ground, aerial, subterranean };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "footprint");
        }

    }

    [JsonConverter(typeof(Level))]
    public class Level : Feature<Level.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            arrivals,
            arrivalsDomestic,
            arrivalsIntl,
            departures,
            departuresDomestic,
            departuresIntl,
            parking,
            transit,
            unspecified,
        }

        public class Properties
        {
            public LocalizedName name;
            public LocalizedName short_name;
            public int ordinal;
            public bool outdoor;
            public Guid? address_id;
            public Guid[] building_ids;
            public GeoJSONGeometry display_point;

            public Category category;
            public RestrictionCategory restriction;

            public Properties() { }

            public Properties(IMDF.Level level)
            {
                name = level.localizedName.getFeature();
                short_name = level.shortName.getFeature() ?? name;
                ordinal = level.ordinal;
                outdoor = level.outdoor;
                address_id = null;
                building_ids = level.buildings.Select(t => t.guid).ToArray();
                display_point = RefferencePointEditor.GetGeoJSONPoint(level);


                if (name == null || short_name == null) throw new ArgumentNullException("name", "name and short name MUST BE not null (Level)");
            }
        }

        public Level() { }

        public Level(IMDF.Level level)
        {
            identifier = level.guid;
            geometry = level.GetGeoJSONGeometry();
            properties = new Properties(level);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "level");
        }
    }

    [JsonConverter(typeof(Unit))]
    public class Unit : Feature<Unit.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            auditorium = 0,
            administration = 63,
            brick = 1,
            classroom = 2,
            column = 3,
            concrete = 4,
            conferenceroom = 5,
            drywall = 6,
            elevator = 7,
            escalator = 8,
            fieldofplay = 9,
            firstaid = 10,
            fitnessroom = 11,
            foodservice = 12,
            [EnumMember(Value = "foodservice.coffee")] foodserviceCoffee = 67,
            footbridge = 13,
            glass = 14,
            huddleroom = 15,
            kitchen = 16,
            laboratory = 17,
            library = 18,
            lobby = 19,
            lounge = 20,
            mailroom = 21,
            mothersroom = 22,
            movietheater = 23,
            movingwalkway = 24,
            nonpublic = 25,
            office = 26,
            opentobelow = 27,
            parking = 28,
            phoneroom = 29,
            platform = 30,
            privatelounge = 31,
            ramp = 32,
            recreation = 33,
            restroom = 34,
            [EnumMember(Value = "restroom.family")] restroomFamily = 35,
            [EnumMember(Value = "restroom.female")] restroomFemale = 36,
            [EnumMember(Value = "restroom.female.wheelchair")] restroomFemaleWheelchair = 37,
            [EnumMember(Value = "restroom.male")] restroomMale = 38,
            [EnumMember(Value = "restroom.male.wheelchair")] restroomMaleWheelchair = 39,
            [EnumMember(Value = "restroom.transgender")] restroomTransgender = 40,
            [EnumMember(Value = "restroom.transgender.wheelchair")] restroomTransgenderWheelchair = 41,
            [EnumMember(Value = "restroom.unisex")] restroomUnisex = 42,
            [EnumMember(Value = "restroom.unisex.wheelchair")] restroomUnisexWheelchair = 43,
            [EnumMember(Value = "restroom.wheelchair")] restroomWheelchair = 44,
            road = 45,
            room = 46,
            serverroom = 47,
            shower = 48,
            smokingarea = 49,
            security = 64,
            shop = 66,
            stairs = 50,
            steps = 51,
            storage = 52,
            structure = 53,
            terrace = 54,
            theater = 55,
            unenclosedarea = 56,
            unspecified = 57,
            vegetation = 58,
            waitingroom = 59,
            wardrobe = 65,
            walkway = 60,
            [EnumMember(Value = "walkway.island")] walkwayIsland = 61,
            wood = 62,
        }

        public class Properties
        {
            public Category category;

            public LocalizedName name;
            public LocalizedName alt_name;

            public RestrictionCategory restriction;
            public int? accessibility = null;

            public Guid level_id;
            public GeoJSONPoint display_point;

            public Properties(IMDF.Unit unit)
            {
                category = unit.category;
                name = unit.localizedName.getFeature();
                alt_name = unit.altName.getFeature();

                restriction = unit.restriction;

                level_id = unit.GetComponentInParent<IMDF.Level>(true).guid;
                display_point = RefferencePointEditor.GetGeoJSONPoint(unit);
            }
        }

        public Unit() { }

        public Unit(IMDF.Unit unit)
        {
            identifier = unit.guid;
            geometry = unit.GetGeoJSONGeometry();
            properties = new Properties(unit);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "unit");
        }

    }

    [JsonConverter(typeof(Opening))]
    public class Opening : Feature<Opening.Properties>
    {

        public class Door
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Type
            {
                movablepartition,
                open,
                revolving,
                shutter,
                sliding,
                swinging,
                turnstile,
                [EnumMember(Value = "turnstile.fullheight")] turnstileFullheight,
                [EnumMember(Value = "turnstile.waistheight")] turnstileWaistheight,
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum Material
            {
                wood,
                glass,
                metal,
                gate
            }

            public Type type;
            public Material material;
            public bool automatic;

            public Door() { }

            public Door(Type type, Material material, bool automatic)
            {
                this.type = type;
                this.material = material;
                this.automatic = automatic;
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            automobile,
            bicycle,
            emergencyexit,
            pedestrian,
            [EnumMember(Value = "pedestrian.principal")] pedestrianPrincipal,
            [EnumMember(Value = "pedestrian.transit")] pedestrianTransit,
            service
        }

        public class Properties
        {
            public Category category;
            public Unit.Category? unit_categoty;
            public RestrictionCategory? unit_restriction;
            public int? accessibility = null;
            public int? access_control = null;
            public Door door;
            public LocalizedName name;
            public LocalizedName alt_name;
            public GeoJSONPoint display_point;
            public Guid level_id;

            public Properties(IMDF.Opening opening)
            {
                category = opening.category;
                unit_categoty = opening.GetComponentInParent<IMDF.Unit>(true)?.category;
                unit_restriction = opening.GetComponentInParent<IMDF.Unit>(true)?.restriction;
                name = opening.localizedName.getFeature();
                alt_name = opening.altName.getFeature();

                display_point = RefferencePointEditor.GetGeoJSONPoint(opening);
                door = new Door(opening.type, opening.material, opening.automatic);
                level_id = opening.GetComponentInParent<IMDF.Level>(true).guid;
            }
        }

        public Opening() { }

        public Opening(IMDF.Opening opening)
        {
            identifier = opening.guid;
            geometry = new GeoJSONLineString { points = opening.GetGeoPoints() };
            properties = new Properties(opening);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "opening");
        }
    }

    [JsonConverter(typeof(Amenity))]
    public class Amenity : Feature<Amenity.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CategoryMin
        {
            atm = 0, //Банкомат
            eatingdrinking = 1,
            escalator = 2,
            entry = 3, //вход
            faregate = 4, //пропускной вход
            information = 5,
            library = 6,
            restroom = 7,
            [EnumMember(Value = "restroom.female")] restroomFemale = 8,
            [EnumMember(Value = "restroom.male")] restroomMale = 9,
            seat = 10,
            security = 11,
            stairs = 12,
            elevator = 18,
            [EnumMember(Value = "security.checkpoint")] securityCheckpoint = 13,
            smokingarea = 14,
            studentservices = 15,
            swimmingpool = 16,
            vendingmachine = 17,
            unspecified = 10000
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            amphitheater,
            animalreliefarea,
            arrivalgate,
            atm,
            babychanging,
            baggagecarousel,
            [EnumMember(Value = "baggagecarousel.intl")] baggagecarouselIntl,
            baggagecarts,
            baggageclaim,
            [EnumMember(Value = "baggageclaim.oversize")] baggageclaimOversize,
            baggagerecheck,
            baggagestorage,
            boardinggate,
            [EnumMember(Value = "boardinggate.aircraft")] boardinggateAircraft,
            [EnumMember(Value = "boardinggate.bus")] boardinggateBus,
            [EnumMember(Value = "boardinggate.ferry")] boardinggateFerry,
            [EnumMember(Value = "boardinggate.train")] boardinggateTrain,
            bus,
            [EnumMember(Value = "bus.muni")] busMuni,
            [EnumMember(Value = "bus.national")] busNational,
            businesscenter,
            cabin,
            caregiver,
            carrental,
            cashier,
            changemachine,
            checkin,
            [EnumMember(Value = "checkin.desk")] checkinDesk,
            [EnumMember(Value = "checkin.desk.oversizebaggage")] checkinDeskOversizebaggage,
            [EnumMember(Value = "checkin.desk.transfer")] checkinDeskTransfer,
            [EnumMember(Value = "checkin.selfservice")] checkinSelfservice,
            childplayarea,
            coinlocker,
            copymachine,
            defibrillator,
            drinkingfountain,
            eatingdrinking,
            elevator,
            emergencyshelter,
            entry,
            escalator,
            exhibit,
            faregate,
            [EnumMember(Value = "faregate.oversized")] faregateOversized,
            fieldofplay,
            [EnumMember(Value = "fieldofplay.americanfootball")] fieldofplayAmericanfootball,
            [EnumMember(Value = "fieldofplay.baseball")] fieldofplayBaseball,
            [EnumMember(Value = "fieldofplay.basketball")] fieldofplayBasketball,
            [EnumMember(Value = "fieldofplay.fieldhockey")] fieldofplayFieldhockey,
            [EnumMember(Value = "fieldofplay.icehockey")] fieldofplayIcehockey,
            [EnumMember(Value = "fieldofplay.rugby")] fieldofplayRugby,
            [EnumMember(Value = "fieldofplay.soccer")] fieldofplaySoccer,
            [EnumMember(Value = "fieldofplay.softball")] fieldofplaySoftball,
            [EnumMember(Value = "fieldofplay.tennis")] fieldofplayTennis,
            [EnumMember(Value = "fieldofplay.trackfield")] fieldofplayTrackfield,
            [EnumMember(Value = "fieldofplay.volleyball")] fieldofplayVolleyball,
            firealarmpullstation,
            fireextinguisher,
            firstaid,
            fittingroom,
            foodservice,
            gatearea,
            groundtransportation,
            guestservices,
            handsanitizerstation,
            healthscreening,
            hoteling,
            immigration,
            information,
            [EnumMember(Value = "information.bid")] informationBid,
            [EnumMember(Value = "information.carrental")] informationCarrental,
            [EnumMember(Value = "information.hotel")] informationHotel,
            [EnumMember(Value = "information.mufid")] informationMufid,
            [EnumMember(Value = "information.mufid.arrivals")] informationMufidArrivals,
            [EnumMember(Value = "information.mufid.departures")] informationMufidDepartures,
            [EnumMember(Value = "information.transit")] informationTransit,
            landmark,
            library,
            limo,
            lostandfound,
            mailbox,
            meditation,
            meetingpoint,
            mobilityrescue,
            mothersroom,
            movingwalkway,
            paidarea,
            parkandride,
            parking,
            [EnumMember(Value = "parking.bicycle")] parkingBicycle,
            [EnumMember(Value = "parking.compact")] parkingCompact,
            [EnumMember(Value = "parking.ev")] parkingEv,
            [EnumMember(Value = "parking.longterm")] parkingLongterm,
            [EnumMember(Value = "parking.motorcycle")] parkingMotorcycle,
            [EnumMember(Value = "parking.shortterm")] parkingShortterm,
            [EnumMember(Value = "parking.waitingarea")] parkingWaitingarea,
            payphone,
            pedestriancrossing,
            peoplemover,
            phone,
            [EnumMember(Value = "phone.emergency")] phoneEmergency,
            photobooth,
            platform,
            police,
            powerchargingstation,
            prayerroom,
            [EnumMember(Value = "prayerroom.buddhism")] prayerroomBuddhism,
            [EnumMember(Value = "prayerroom.christianity")] prayerroomChristianity,
            [EnumMember(Value = "prayerroom.hinduism")] prayerroomHinduism,
            [EnumMember(Value = "prayerroom.islam")] prayerroomIslam,
            [EnumMember(Value = "prayerroom.islam.female")] prayerroomIslamFemale,
            [EnumMember(Value = "prayerroom.islam.male")] prayerroomIslamMale,
            [EnumMember(Value = "prayerroom.judaism")] prayerroomJudaism,
            [EnumMember(Value = "prayerroom.shintoism")] prayerroomShintoism,
            [EnumMember(Value = "prayerroom.sikh")] prayerroomSikh,
            [EnumMember(Value = "prayerroom.taoic")] prayerroomTaoic,
            privatelounge,
            productreturn,
            [EnumMember(Value = "rail.muni")] railMuni,
            [EnumMember(Value = "rail.national")] railNational,
            ramp,
            [EnumMember(Value = "reception.desk")] receptionDesk,
            recreation,
            restroom,
            [EnumMember(Value = "restroom.family")] restroomFamily,
            [EnumMember(Value = "restroom.female")] restroomFemale,
            [EnumMember(Value = "restroom.female.wheelchair")] restroomFemaleWheelchair,
            [EnumMember(Value = "restroom.male")] restroomMale,
            [EnumMember(Value = "restroom.male.wheelchair")] restroomMaleWheelchair,
            [EnumMember(Value = "restroom.transgender")] restroomYransgender,
            [EnumMember(Value = "restroom.transgender.wheelchair")] restroomYransgenderWheelchair,
            [EnumMember(Value = "restroom.unisex")] restroomUnisex,
            [EnumMember(Value = "restroom.unisex.wheelchair")] restroomUnisexWheelchair,
            [EnumMember(Value = "restroom.wheelchair")] restroomWheelchair,
            rideshare,
            seat,
            seating,
            security,
            [EnumMember(Value = "security.checkpoint")] securityCheckpoint,
            [EnumMember(Value = "security.inspection")] securityInspection,
            service,
            shower,
            shuttle,
            sleepbox,
            smokingarea,
            stairs,
            storage,
            strollerrental,
            studentadmissions,
            studentservices,
            swimmingpool,
            [EnumMember(Value = "swimmingpool.children")] swimmingpoolChildren,
            [EnumMember(Value = "swimmingpool.family")] swimmingpoolFamily,
            taxi,
            ticketing,
            [EnumMember(Value = "ticketing.airline")] ticketingAirline,
            [EnumMember(Value = "ticketing.bus")] ticketingBus,
            [EnumMember(Value = "ticketing.bus.muni")] ticketingBusMuni,
            [EnumMember(Value = "ticketing.bus.national")] ticketingBusNational,
            [EnumMember(Value = "ticketing.rail")] ticketingRail,
            [EnumMember(Value = "ticketing.rail.muni")] ticketingRailMuni,
            [EnumMember(Value = "ticketing.rail.national")] ticketingRailNational,
            [EnumMember(Value = "ticketing.shuttle")] ticketingShuttle,
            traintrack,
            transit,
            unspecified,
            valet,
            vendingmachine,
            [EnumMember(Value = "vendingmachine.trainticket")] vendingmachineTrainticket,
            wheelchairassist,
            wifi,
            yoga,

        }

        public class Properties
        {
            public CategoryMin category;
            public LocalizedName name;
            public LocalizedName alt_name;
            public Guid[] unit_ids;

            public string hours;
            public string phone;
            public string website;
            public int detailLevel;
            public Guid? address_id;

            public Properties() { }

            public Properties(IMDF.Amenity amenity)
            {
                category = amenity.category;
                name = amenity.localizedName.IsEmpty() ? amenity.GetComponentInParent<IMDF.Unit>(true).localizedName.getFeature() : amenity.localizedName.getFeature();
                alt_name = amenity.altName.IsEmpty() ? amenity.GetComponentInParent<IMDF.Unit>(true).altName.getFeature() : amenity.altName.getFeature();

                if (amenity.units.Length == 0)
                {
                    unit_ids = new Guid[] { amenity.GetComponentInParent<IMDF.Unit>(true).guid };
                }
                else
                {
                    unit_ids = amenity.units.Select(t => t.guid).ToArray();
                }

                detailLevel = (int)amenity.detailLevel;
                hours = amenity.hours.OrNull();
                phone = amenity.phone.OrNull();
                website = amenity.website.OrNull();
                address_id = amenity.address ? amenity.address.address.guid : null;

            }
        }

        public Amenity() { }

        public Amenity(IMDF.Amenity amenity)
        {
            identifier = amenity.guid;
            geometry = new GeoJSONPoint(amenity.GetPoint());
            properties = new Properties(amenity);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "amenity");
        }
    }

    [JsonConverter(typeof(Anchor))]
    public class Anchor : Feature<Anchor.Properties>
    {
        public class Properties
        {
            public Guid unit_id;
            public Guid? address_id;

            public Properties() { }

            public Properties(Guid unit_id, Guid? address_id)
            {
                this.unit_id = unit_id;
                this.address_id = address_id;
            }
        }

        public Anchor() { }

        public Anchor(IMDF.Unit unit)
        {
            identifier = unit.anchorGuid;
            geometry = RefferencePointEditor.GetGeoJSONPoint(unit);
            properties = new Properties(unit.guid, (unit as IAddress).address?.guid);
        }

        public Anchor(IMDF.Occupant occupant)
        {
            identifier = occupant.anchorGuid;
            geometry = new IMDF.Feature.GeoJSONPoint(GeoMap.CalculateGeo(occupant.transform.position).GetPoint());
            properties = new Properties(occupant.GetComponentInParent<IMDF.Unit>(true).guid, (occupant as IAddress).address?.guid);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "anchor");
        }
    }

    [JsonConverter(typeof(Occupant))]
    public class Occupant : Feature<Occupant.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            auditorium = 0,
            administration = 1,
            classroom = 2,
            laboratory = 3,
            library = 4,
            souvenirs = 5,
            [EnumMember(Value = "foodservice.coffee")] foodserviceCoffee = 6,
            foodservice = 18,
            security = 7,
            wardrobe = 8,
            restroom = 9,
            [EnumMember(Value = "restroom.female")] restroomFemale = 10,
            [EnumMember(Value = "restroom.male")] restroomMale = 11,
            ticket = 12,
            museum = 13,
            [EnumMember(Value = "concert.hall")] concertHall = 14,
            archive = 15,
            [EnumMember(Value = "reading.room")] readingRoom = 16,
            [EnumMember(Value = "academic.council")] academicCouncil = 17,
            information = 18,
            photobooth = 19,
            firstaid = 20,

            unspecified = 10000,
        }

        public class Properties
        {
            public LocalizedName name;
            public LocalizedName shortName;
            public Category category;
            public Guid anchor_id;
            public string hours = null;
            public string phone = null;
            public string email = null;
            public string website = null;
            public string description = null;
            public string iconUrl = null;
            public Guid? correlation_id;

            public Properties() { }

            const string BASE_ICON_URL = "https://dev.image.umap.space/api/";
            public Properties(IMDF.Occupant occupant)
            {
                name = occupant.fullName.getFeature();
                shortName = occupant.shortName.getFeature();
                anchor_id = (occupant as IAnchor).anchor.identifier;
                category = occupant.category;
                hours = occupant.hours.OrNull();
                phone = occupant.phone.OrNull();
                website = occupant.website.OrNull();
                email = occupant.email.OrNull();
                iconUrl = string.IsNullOrWhiteSpace(occupant.iconUrl) ? null : BASE_ICON_URL + occupant.iconUrl + ".png";
                description = occupant.description.OrNull();
            }

            public Properties(IMDF.Unit unit)
            {
                name = unit.localizedName.getFeature();
                shortName = unit.altName.getFeature();
                anchor_id = (unit as IAnchor).anchor.identifier;
                category = unit.occupantCategory;
                iconUrl = string.IsNullOrWhiteSpace(unit.iconUrl) ? null : BASE_ICON_URL + unit.iconUrl + ".png";
                description = unit.description.OrNull();
                website = unit.website.OrNull();

            }
        }

        public Occupant() { }

        public Occupant(IMDF.Occupant occupant)
        {
            identifier = occupant.guid;
            geometry = null;
            properties = new Properties(occupant);
        }

        public Occupant(IMDF.Unit unit)
        {
            identifier = unit.occupantGuid;
            geometry = null;
            properties = new Properties(unit);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "occupant");
        }
    }

    [JsonConverter(typeof(EnviromentUnit))]
    public class EnviromentUnit : Feature<EnviromentUnit.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            [EnumMember(Value = "road.main")] roadMain = 0,
            [EnumMember(Value = "road.dirt")] roadDirt = 1,
            [EnumMember(Value = "road.pedestrian.main")] roadPedestrianMain = 2,
            [EnumMember(Value = "road.pedestrian.second")] roadPedestrianSecond = 8,
            [EnumMember(Value = "road.pedestrian.treadmill")] roadPedestrianTreadmill = 9,
            grass = 3,
            [EnumMember(Value = "grass.stadion")] grassStadion = 10,
            tree = 4,
            forest = 5,
            sand = 11,
            water = 12,
            [EnumMember(Value = "fence.main")] fenceMain = 6,
            [EnumMember(Value = "fence.second")] fenceSecond = 7,
        }

        public class Properties
        {
            public Category category;

            public LocalizedName name;
            public LocalizedName alt_name;

            public GeoJSONPoint display_point;

            public Properties(IMDF.EnviromentUnit unit)
            {
                category = unit.category;
                name = unit.localizedName.getFeature();
                alt_name = unit.altName.getFeature();

                display_point = RefferencePointEditor.GetGeoJSONPoint(unit);
            }
        }

        public EnviromentUnit() { }

        public EnviromentUnit(IMDF.EnviromentUnit unit)
        {
            identifier = unit.guid;
            geometry = unit.GetGeoJSONGeometry();
            properties = new Properties(unit);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "enviroment_unit");
        }

    }

    [JsonConverter(typeof(Attraction))]
    public class Attraction : Feature<Attraction.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            building
        }

        public class Properties
        {
            public Category category;

            public LocalizedName name;
            public LocalizedName alt_name;
            public LocalizedName short_name;
            public string image;

            public Guid building_id;

            public Properties(IMDF.Attraction attraction)
            {
                category = attraction.category;
                name = attraction.localizedName.getFeature();
                alt_name = attraction.altName.getFeature();
                short_name = attraction.shortName.getFeature();
                building_id = attraction.building.guid;
                image = string.IsNullOrWhiteSpace(attraction.image) ? null : attraction.image;
            }
        }

        public Attraction() { }

        public Attraction(IMDF.Attraction attraction)
        {
            identifier = attraction.guid;
            geometry = new GeoJSONPoint(attraction.GetPoint());
            properties = new Properties(attraction);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "attraction");
        }

    }

    [JsonConverter(typeof(EnviromentAmenity))]
    public class EnviromentAmenity : Feature<EnviromentAmenity.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            [EnumMember(Value = "parking.car")] parkingCar = 0,
            [EnumMember(Value = "parking.bicycle")] parkingBicycle = 1,
            banch = 2,
            metro = 3,
            [EnumMember(Value = "stadium")] stadium = 9,
            [EnumMember(Value = "stadium.football")] stadiumFootball = 4,
            [EnumMember(Value = "stadium.basketball")] stadiumBasketbal = 5,
            [EnumMember(Value = "stadium.volleyball")] stadiumVolleybal = 6,
            entrance = 7,
            playground = 8
        }

        public class Properties
        {
            public Category category;

            public LocalizedName name;
            public LocalizedName alt_name;
            public int detailLevel;

            public Properties(IMDF.EnviromentAmenity amenity)
            {
                category = amenity.category;
                name = amenity.localizedName.getFeature();
                alt_name = amenity.altName.getFeature();
                detailLevel = (int)amenity.detailLevel;
            }
        }

        public EnviromentAmenity() { }

        public EnviromentAmenity(IMDF.EnviromentAmenity amenity)
        {
            identifier = amenity.guid;
            geometry = new GeoJSONPoint(amenity.GetPoint());
            properties = new Properties(amenity);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "enviroment_amenity");
        }
    }

    [JsonConverter(typeof(Detail))]
    public class Detail : Feature<Detail.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            crosswalk = 0,
            [EnumMember(Value = "road.marking.main")] roadMarkingMain = 1,
            [EnumMember(Value = "parking.marking")] parkingMarking = 2,
            [EnumMember(Value = "parking.big")] parkingBig = 3,
            [EnumMember(Value = "stadion.grass.marking")] stadionGrassMarking = 9,
            [EnumMember(Value = "treadmill.marking")] treadmillMarking = 10,
            [EnumMember(Value = "fence.main")] fenceMain = 4,
            [EnumMember(Value = "fence.heigth")] fenceHeight = 5,
            steps = 6,
            [EnumMember(Value = "indoor.steps")] indoorSteps = 7,
            [EnumMember(Value = "indoor.stairs")] indoorStairs = 8
        }

        public class Properties
        {
            public Category category;
            public Guid? level_id;

            public Properties(FeatureMB feature)
            {
                if (feature.GetComponentInParent<IMDF.Level>(true))
                {
                    level_id = feature.GetComponentInParent<IMDF.Level>(true).guid;
                }
            }

            public Properties(IMDF.Crosswalk crosswalk) : this(crosswalk as FeatureMB)
            {
                category = crosswalk.category;
            }

            public Properties(IMDF.DetailLine detailLine) : this(detailLine as FeatureMB)
            {
                category = detailLine.category;


            }
        }

        public Detail() { }

        public Detail(IMDF.Crosswalk crosswalk)
        {
            identifier = crosswalk.guid;
            geometry = new GeoJSONMultiLineString(crosswalk.Lines());
            properties = new Properties(crosswalk);
        }

        public Detail(IMDF.DetailLine detailLine)
        {
            identifier = detailLine.guid;
            geometry = new GeoJSONMultiLineString(detailLine.Lines());
            properties = new Properties(detailLine);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "detail");
        }
    }

    [JsonConverter(typeof(NavPath))]
    public class NavPath : Feature<NavPath.Properties>
    {
        public class Properties
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum Tag
            {
                dirt = 1,
                service = 2,
            }

            public Guid? level_id;
            public Guid? builing_id;
            public Guid[] neighbours;
            public float weight;
            public Tag[] tags;

            public Properties(PathNode node)
            {
                this.neighbours = node.neighbors.Select(t => t.guid).ToArray();
                level_id = node.GetComponentInParent<IMDF.Level>(true)?.guid;
                builing_id = node.GetComponentInParent<IMDF.Building>(true)?.guid;
                weight = node.weight;

                List<Tag> tags = new List<Tag>();
                if (node.isDirt) tags.Add(Tag.dirt);
                if (node.isService) tags.Add(Tag.service);

                this.tags = tags.ToArray();
            }
        }

        public NavPath() { }

        public NavPath(PathNode node)
        {
            identifier = node.guid;
            geometry = new GeoJSONPoint(node.GetPoint());
            properties = new Properties(node);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "navPath");
        }

    }

    [JsonConverter(typeof(NavPathAssocieted))]
    public class NavPathAssocieted : Feature<NavPathAssocieted.Properties>
    {
        public class Properties
        {
            public Guid pathNode_id;
            public Guid associeted_id;

            public Properties(Guid node, Guid associeted)
            {
                this.pathNode_id = node;
                this.associeted_id = associeted;
            }
        }

        public NavPathAssocieted() { }

        public NavPathAssocieted(Guid associeted, PathNode node)
        {
            identifier = Guid.NewGuid();
            geometry = null;
            properties = new Properties(node.guid, associeted);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "navPathAssocieted");
        }
    }


    public class Manifest
    {
        public string version;
        public DateTime created;
        public string generated_by;
        public string language;

        public Manifest(string version, DateTime created, string language = "en")
        {
            this.version = version;
            this.created = created;
            this.generated_by = $"Unity_{Application.unityVersion} IMDF_{Application.version}";
            this.language = language;
        }

        public static Manifest CreateManifest()
        {
            int version = PlayerPrefs.GetInt("Manifest_Version");
            PlayerPrefs.SetInt("Manifest_Version", ++version);


            return new Manifest("1.0.0", DateTime.Now);
        }
    }

    public class IMDFDecoder
    {
        public static Dictionary<FeatureMB, Occupant> iOccupants = new Dictionary<FeatureMB, Occupant>();
        public static Dictionary<FeatureMB, IAnnotation> iAnnotations = new Dictionary<FeatureMB, IAnnotation>();

        static (GeoJSONObject[], string)[] Convert()
        {

            GameObject.FindObjectsOfType<PolygonGeometry>(true).Where(t => t.GetComponent<IMDF.Building>() || t.GetComponent<IMDF.Venue>()).ToList().ForEach(t =>
            {
                t.GetComponents<PolygonCollider2D>().ToList().ForEach(c =>
                {
                    for (int i = 0; i < c.pathCount; i++)
                    {
                        var points = c.GetPath(i);
                        if (points.Length <= 2) continue;

                        var sum = 0f;
                        for (int j = 0; j < points.Length; j++)
                        {
                            var p1 = points[j];
                            var p2 = points[(j + 1) % points.Length];
                            sum += (p2.x - p1.x) * (p2.y + p1.y);
                        }

                        if (i == 0 && sum < 0 ||
                            i != 0 && sum > 0)
                        {
                            c.SetPath(i, points.Reverse().ToArray());
                        }
                    }
                });
            });

            UUIDStorage.shared.Load();
            var features = GameObject.FindObjectsOfType<FeatureMB>(true);
            foreach (var feature in features) feature.GenerateGUID();
            UUIDStorage.shared.Save();

            var pathNodes = GameObject.FindObjectsOfType<PathNode>(true);
            foreach (var node in pathNodes) node.Fix();

            iOccupants = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IOccupant>()
                                    .ToDictionary(t => t as FeatureMB, t => t.occupant)
                                    .Where(t => t.Value != null)
                                    .ToDictionary(t => t.Key, t => t.Value);

            iAnnotations = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IAnnotation>()
                                        .ToDictionary(t => t as FeatureMB, t => t)
                                        .Where(t => t.Value != null)
                                        .ToDictionary(t => t.Key, t => t.Value);


            List<GeoJSONObject> associetedPath = new List<GeoJSONObject>();
            foreach (var node in pathNodes)
            {
                foreach (var item in node.associatedFeatures)
                {
                    if (iAnnotations.TryGetValue(item, out IAnnotation annotation) && annotation.identifier.HasValue)
                    {
                        associetedPath.Add(new NavPathAssocieted(annotation.identifier.Value, node));
                    }
                }
            }

            var featuresType = new (GeoJSONObject[], string)[] {
                (GameObject.FindObjectsOfType<IMDF.Opening>(true).Select(t => new Opening(t)).ToArray(), "opening"),
                (GameObject.FindObjectsOfType<IMDF.Unit>(true).Select(t => new Unit(t)).ToArray(), "unit"),
                (GameObject.FindObjectsOfType<IMDF.Level>(true).Select(t => new Level(t)).ToArray(), "level"),
                (GameObject.FindObjectsOfType<IMDF.Building>(true).Select(t => new Building(t)).ToArray(), "building"),
                (GameObject.FindObjectsOfType<IMDF.Building>(true).Select(t => Footprint.FootprintFromBuilding(t)).SelectMany(t => t).ToArray(), "footprint"),
                (GameObject.FindObjectsOfType<IMDF.Venue>(true).Select(t => new Venue(t)).ToArray(), "venue"),
                (GameObject.FindObjectsOfType<IMDF.Amenity>(true).Select(t => new Amenity(t)).ToArray(), "amenity"),
                (GameObject.FindObjectsOfType<IMDF.EnviromentAmenity>(true).Select(t => new EnviromentAmenity(t)) .ToArray(), "enviromentAmenity"),
                (GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IAddress>()
                    .Where(t => t.address != null)
                    .Select(t => t.address)
                    .Distinct()
                    .Select(t => new Address(t))
                    .ToArray(), "address"),
                (GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IAnchor>().Select(t => t.anchor).ToArray(), "anchor"),
                (iOccupants.Values.ToArray(), "occupant"),
                (GameObject.FindObjectsOfType<IMDF.EnviromentUnit>(true).Select(t => new EnviromentUnit(t)).ToArray(), "enviroment"),
                (GameObject.FindObjectsOfType<IMDF.Attraction>(true).Select(t => new Attraction(t)).ToArray(), "attraction"),
                (GameObject.FindObjectsOfType<IMDF.Crosswalk>(true).Select(t => new Detail(t))
                    .Concat(GameObject.FindObjectsOfType<IMDF.DetailLine>(true).Select(t => new Detail(t)))
                    .ToArray(), "detail"),
                (pathNodes.Select(t => new NavPath(t)).ToArray(), "navPath"),
                (associetedPath.ToArray(), "navPathAssocieted"),
            };

            return featuresType;

        }

        public static void Ser()
        {
            var featuresType = Convert();

            var path = EditorUtility.SaveFolderPanel("Save IMDF achive", PlayerPrefs.GetString("IMDF_PATH") ?? "", "IMDF");
            PlayerPrefs.SetString("IMDF_PATH", path);

            foreach (var item in featuresType)
            {
                FeatureCollection openingsFC = new FeatureCollection(item.Item1, item.Item2);
                File.WriteAllText(Path.Combine(path, item.Item2 + ".geojson"), ToJSON(openingsFC));
            }


            File.WriteAllText(Path.Combine(path, "manifest.json"), JsonConvert.SerializeObject(Manifest.CreateManifest()));
        }

        public static void Send(string id, string password, UnityEngine.Object obj)
        {
            var featuresType = Convert();

            UnityWebRequest request = UnityWebRequest.Post("https://dev.mapstorage.polymap.ru/api/saveMap/" + id, "");

            var dict = featuresType.ToDictionary(t => t.Item2, t => t.Item1);

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dict));
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.SetRequestHeader("authorization", "Bearer " + password);

            EditorCoroutineUtility.StartCoroutine(Send(request), obj);
        }

        static IEnumerator Send(UnityWebRequest request)
        {
            Debug.Log("Sending...");
            yield return request.SendWebRequest();
            Debug.Log(request.result);
            Debug.Log(request.responseCode);
            // Debug.Log("DONE");
        }

        public static string ToJSON(GeoJSONObject obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }


    public static class Extensions
    {
        public static string OrNull(this String str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }
    }

}
