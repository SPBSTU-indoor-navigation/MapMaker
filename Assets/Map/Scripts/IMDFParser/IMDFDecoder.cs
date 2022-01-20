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

            public Properties(IMDF.Venue venue)
            {
                name = venue.localizedName.getFeature();
                alt_name = venue.altName.getFeature();
                category = venue.category;
                restriction = venue.restriction;

                hours = venue.hours;
                phone = venue.phone;
                website = venue.website;

                address_id = venue.address.guid;
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
            public GeoJSONGeometry display_point;
            public Guid? address_id;

            public Properties(IMDF.Building building)
            {
                name = building.localizedName.getFeature();
                alt_name = building.altName.getFeature();
                category = building.category;

                restriction = building.restriction;
                display_point = RefferencePointEditor.GetGeoJSONPoint(building);
                address_id = building.addressId?.guid;
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
            auditorium,
            brick,
            classroom,
            column,
            concrete,
            conferenceroom,
            drywall,
            elevator,
            escalator,
            fieldofplay,
            firstaid,
            fitnessroom,
            foodservice,
            footbridge,
            glass,
            huddleroom,
            kitchen,
            laboratory,
            library,
            lobby,
            lounge,
            mailroom,
            mothersroom,
            movietheater,
            movingwalkway,
            nonpublic,
            office,
            opentobelow,
            parking,
            phoneroom,
            platform,
            privatelounge,
            ramp,
            recreation,
            restroom,
            [EnumMember(Value = "restroom.family")] restroomFamily,
            [EnumMember(Value = "restroom.female")] restroomFemale,
            [EnumMember(Value = "restroom.female.wheelchair")] restroomFemaleWheelchair,
            [EnumMember(Value = "restroom.male")] restroomMale,
            [EnumMember(Value = "restroom.male.wheelchair")] restroomMaleWheelchair,
            [EnumMember(Value = "restroom.transgender")] restroomTransgender,
            [EnumMember(Value = "restroom.transgender.wheelchair")] restroomTransgenderWheelchair,
            [EnumMember(Value = "restroom.unisex")] restroomUnisex,
            [EnumMember(Value = "restroom.unisex.wheelchair")] restroomUnisexWheelchair,
            [EnumMember(Value = "restroom.wheelchair")] restroomWheelchair,
            road,
            room,
            serverroom,
            shower,
            smokingarea,
            stairs,
            steps,
            storage,
            structure,
            terrace,
            theater,
            unenclosedarea,
            unspecified,
            vegetation,
            waitingroom,
            walkway,
            [EnumMember(Value = "walkway.island")] walkwayIsland,
            wood,
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
            geometry = new GeoJSONLineString { points = opening.GetPoints() };
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
            atm, //Банкомат
            eatingdrinking,
            escalator,
            entry, //вход
            faregate, //пропускной вход
            information,
            library,
            restroom,
            [EnumMember(Value = "restroom.female")] restroomFemale,
            [EnumMember(Value = "restroom.male")] restroomMale,
            seat,
            security,
            stairs,
            [EnumMember(Value = "security.checkpoint")] securityCheckpoint,
            smokingarea,
            studentservices,
            swimmingpool,
            vendingmachine,
            unspecified
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
                unit_ids = amenity.units.Select(t => t.guid).ToArray();

                detailLevel = amenity.detailLevel;
                hours = amenity.hours.OrNull();
                phone = amenity.phone.OrNull();
                website = amenity.website.OrNull();
                address_id = amenity.address ? amenity.address.guid : null;

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

    [JsonConverter(typeof(EnviromentUnit))]
    public class EnviromentUnit : Feature<EnviromentUnit.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            [EnumMember(Value = "road.main")] roadMain,
            [EnumMember(Value = "road.dirt")] roadDirt,
            [EnumMember(Value = "road.pedestrian.main")] roadPedestrianMain,
            grass,
            tree,
            forest,
            [EnumMember(Value = "fence.main")] fenceMain,
            [EnumMember(Value = "fence.second")] fenceSecond,
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
            [EnumMember(Value = "parking.car")] parkingCar,
            [EnumMember(Value = "parking.bicycle")] parkingBicycle,
            banch
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
                detailLevel = amenity.detailLevel;
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

    [JsonConverter(typeof(EnviromentDetail))]
    public class EnviromentDetail : Feature<EnviromentDetail.Properties>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Category
        {
            crosswalk,
            [EnumMember(Value = "road.marking.main")] roadMarkingMain,
            [EnumMember(Value = "parking.marking")] parkingMarking,
            [EnumMember(Value = "parking.big")] parkingBig,
            [EnumMember(Value = "fence.main")] fenceMain
        }

        public class Properties
        {
            public Category category;

            public Properties() { }

            public Properties(IMDF.Crosswalk crosswalk)
            {
                category = Category.crosswalk;
            }

            public Properties(IMDF.DetailLine detailLine)
            {
                category = detailLine.category;
            }
        }

        public EnviromentDetail() { }

        public EnviromentDetail(IMDF.Crosswalk crosswalk)
        {
            identifier = crosswalk.guid;
            geometry = new GeoJSONMultiLineString(crosswalk.Lines());
            properties = new Properties(crosswalk);
        }

        public EnviromentDetail(IMDF.DetailLine detailLine)
        {
            identifier = detailLine.guid;
            geometry = new GeoJSONMultiLineString(detailLine.Lines());
            properties = new Properties(detailLine);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            base.WriteJson(writer, value, serializer, "enviroment_detail");
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
        public static void Ser()
        {
            var features = GameObject.FindObjectsOfType<FeatureMB>(true);
            foreach (var feature in features) feature.GenerateGUID();

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
                    .Select(t => new Address(t.address))
                    .Distinct().ToArray(), "address"),
                (GameObject.FindObjectsOfType<IMDF.EnviromentUnit>(true).Select(t => new EnviromentUnit(t)).ToArray(), "enviroment"),
                (GameObject.FindObjectsOfType<IMDF.Attraction>(true).Select(t => new Attraction(t)).ToArray(), "attraction"),
                (GameObject.FindObjectsOfType<IMDF.Crosswalk>(true).Select(t => new EnviromentDetail(t))
                    .Concat(GameObject.FindObjectsOfType<IMDF.DetailLine>(true).Select(t => new EnviromentDetail(t)))
                    .ToArray(), "enviromentDetail"),
            };

            var path = EditorUtility.SaveFolderPanel("Save IMDF achive", PlayerPrefs.GetString("IMDF_PATH") ?? "", "IMDF");
            PlayerPrefs.SetString("IMDF_PATH", path);


            foreach (var item in featuresType)
            {
                FeatureCollection openingsFC = new FeatureCollection(item.Item1, item.Item2);
                File.WriteAllText(Path.Combine(path, item.Item2 + ".geojson"), ToJSON(openingsFC));
            }


            File.WriteAllText(Path.Combine(path, "manifest.json"), JsonConvert.SerializeObject(Manifest.CreateManifest()));

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
