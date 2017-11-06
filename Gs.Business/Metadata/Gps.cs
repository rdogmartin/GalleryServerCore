using System;
using System.Collections.Generic;
using System.Globalization;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.Metadata
{

    /// <summary>
    /// Represents a geographical location on earth.
    /// </summary>
    public class GpsLocation
    {
        private string _version;
        private double? _altitude;
        private GpsDistance _latitude;
        private GpsDistance _longitude;
        private GpsDistance _destLatitude;
        private GpsDistance _destLongitude;
        private readonly Dictionary<RawMetadataItemName, MetadataItem> _rawMetadata;

        public GpsLocation(Dictionary<RawMetadataItemName, MetadataItem> rawMetadata)
        {
            this._rawMetadata = rawMetadata;

            Version = GetVersion();
            Latitude = GetLatitude();
            Longitude = GetLongitude();
            Altitude = GetAltitude();
            DestLatitude = GetDestLatitude();
            DestLongitude = GetDestLongitude();
        }

        /// <summary>
        /// The version of the GPS information. Example: "2.2.0.0"
        /// </summary>
        /// <value>The version of the GPS information.</value>
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// The altitude, in meters, of the media object. Will be negative for values below sea level.
        /// </summary>
        /// <value>The altitude.</value>
        public double? Altitude
        {
            get { return _altitude; }
            set { _altitude = value; }
        }

        /// <summary>
        /// Gets or sets the latitude portion of the current instance.
        /// </summary>
        /// <value>The latitude.</value>
        public GpsDistance Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }

        /// <summary>
        /// Gets or sets the longitude portion of the current instance.
        /// </summary>
        /// <value>The longitude.</value>
        public GpsDistance Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }

        /// <summary>
        /// Gets or sets the destination latitude portion of the current instance.
        /// </summary>
        /// <value>The latitude.</value>
        public GpsDistance DestLatitude
        {
            get { return _destLatitude; }
            set { _destLatitude = value; }
        }

        /// <summary>
        /// Gets or sets the destination longitude portion of the current instance.
        /// </summary>
        /// <value>The longitude.</value>
        public GpsDistance DestLongitude
        {
            get { return _destLongitude; }
            set { _destLongitude = value; }
        }

        /// <summary>
        /// Generates a decimal-based version of the GPS coordinates. Ex: "46.5925° N 88.9882° W"
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public string ToLatitudeLongitudeDecimalString()
        {
            return String.Concat(Latitude.ToDoubleString(), " ", Longitude.ToDoubleString());
        }

        /// <summary>
        /// Expresses the value of the GPS coordinates in terms of degrees, minutes, and seconds. Ex: "46°32'15.24" N 88°53'25.82" W"
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public string ToLatitudeLongitudeDegreeMinuteSecondString()
        {
            return String.Concat(Latitude.ToDegreeMinuteSecondString(), " ", Longitude.ToDegreeMinuteSecondString());
        }

        /// <summary>
        /// Generates a decimal-based version of the destination GPS coordinates. Ex: "46.5925° N 88.9882° W"
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public string ToDestLatitudeLongitudeDecimalString()
        {
            return String.Concat(DestLatitude.ToDoubleString(), " ", DestLongitude.ToDoubleString());
        }

        /// <summary>
        /// Expresses the value of the destination GPS coordinates in terms of degrees, minutes, and seconds. Ex: "46°32'15.24" N 88°53'25.82" W"
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public string ToDestLatitudeLongitudeDegreeMinuteSecondString()
        {
            return String.Concat(DestLatitude.ToDegreeMinuteSecondString(), " ", DestLongitude.ToDegreeMinuteSecondString());
        }

        private string GetVersion()
        {
            if (_rawMetadata.TryGetValue(RawMetadataItemName.GPSVersionID, out var rawMdi))
            {
                if (rawMdi.ExtractedValueType == ExtractedValueType.String)
                {
                    return rawMdi.Value.ToString();
                }
            }

            return null;
        }

        private GpsDistance GetLatitude()
        {
            if (_rawMetadata.TryGetValue(RawMetadataItemName.GPSLatitude, out var rawMdiLat))
            {
                if (rawMdiLat.ExtractedValueType == ExtractedValueType.FractionArray && _rawMetadata.TryGetValue(RawMetadataItemName.GPSLatitudeRef, out var rawMdiLatRef))
                {
                    var gspLat = (Fraction[])rawMdiLat.Value;

                    return new GpsDistance(rawMdiLatRef.Value.ToString(), gspLat[0].ToSingle(), gspLat[1].ToSingle(), gspLat[2].ToSingle());
                }
            }

            return null;
        }

        private GpsDistance GetLongitude()
        {
            if (_rawMetadata.TryGetValue(RawMetadataItemName.GPSLongitude, out var rawMdiLong))
            {
                if (rawMdiLong.ExtractedValueType == ExtractedValueType.FractionArray && _rawMetadata.TryGetValue(RawMetadataItemName.GPSLongitudeRef, out var rawMdiLongRef))
                {
                    var gspLong = (Fraction[])rawMdiLong.Value;

                    return new GpsDistance(rawMdiLongRef.Value.ToString(), gspLong[0].ToSingle(), gspLong[1].ToSingle(), gspLong[2].ToSingle());
                }
            }

            return null;
        }

        private double? GetAltitude()
        {
            if (_rawMetadata.TryGetValue(RawMetadataItemName.GPSAltitude, out var rawMdiAlt))
            {
                if (rawMdiAlt.ExtractedValueType == ExtractedValueType.Fraction)
                {
                    var gspAlt = (Fraction)rawMdiAlt.Value;

                    return (IsBelowSeaLevel() ? gspAlt.ToSingle() * (-1) : gspAlt.ToSingle());
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the GPS altitude is above or below sea level. Returns <c>false</c> if the metadata is not present.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if the GPS position is below sea level; otherwise, <c>false</c>.
        /// </returns>
        private bool IsBelowSeaLevel()
        {
            var isBelowSeaLevel = false;

            if (_rawMetadata.TryGetValue(RawMetadataItemName.GPSAltitudeRef, out var rawMdi))
            {
                isBelowSeaLevel = rawMdi.Value.ToString().Trim().Equals("1"); // 0 = above sea level; 1 = below sea level
            }

            return isBelowSeaLevel;
        }

        private GpsDistance GetDestLatitude()
        {
            if (_rawMetadata.TryGetValue(RawMetadataItemName.GPSDestLatitude, out var rawMdiLat))
            {
                if (rawMdiLat.ExtractedValueType == ExtractedValueType.FractionArray && _rawMetadata.TryGetValue(RawMetadataItemName.GPSDestLatitudeRef, out var rawMdiLatRef))
                {
                    var gspLat = (Fraction[])rawMdiLat.Value;

                    return new GpsDistance(rawMdiLatRef.Value.ToString(), gspLat[0].ToSingle(), gspLat[1].ToSingle(), gspLat[2].ToSingle());
                }
            }

            return null;
        }

        private GpsDistance GetDestLongitude()
        {
            if (_rawMetadata.TryGetValue(RawMetadataItemName.GPSDestLongitude, out var rawMdiLong))
            {
                if (rawMdiLong.ExtractedValueType == ExtractedValueType.FractionArray && _rawMetadata.TryGetValue(RawMetadataItemName.GPSDestLongitudeRef, out var rawMdiLongRef))
                {
                    var gspLong = (Fraction[])rawMdiLong.Value;

                    return new GpsDistance(rawMdiLongRef.Value.ToString(), gspLong[0].ToSingle(), gspLong[1].ToSingle(), gspLong[2].ToSingle());
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Represents a measure of angular distance. Can be used to store the latitude or longitude component of GPS coordinates.
    /// </summary>
    public class GpsDistance
    {
        readonly string[] _directionValues = new[] { "N", "S", "W", "E" };
        readonly string[] _negativeDirectionValues = new[] { "S", "W" };

        private readonly string _direction; // "N", "S", "W", "E"
        private readonly float _degrees;
        private readonly float _minutes;
        private readonly float _seconds;

        /// <summary>
        /// Gets the bearing of this instance. Returns "N", "S", "W", "E".
        /// </summary>
        /// <value>A <see cref="System.String"/>.</value>
        public string Direction
        {
            get { return _direction; }
        }

        /// <summary>
        /// Gets the degrees component of the current instance.
        /// </summary>
        /// <value>The degrees.</value>
        public float Degrees
        {
            get { return _degrees; }
        }

        /// <summary>
        /// Gets the minutes component of the current instance.
        /// </summary>
        /// <value>The minutes.</value>
        public float Minutes
        {
            get { return _minutes; }
        }

        /// <summary>
        /// Gets the seconds component of the current instance.
        /// </summary>
        /// <value>The seconds.</value>
        public float Seconds
        {
            get { return _seconds; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GpsDistance"/> class.
        /// </summary>
        /// <param name="direction">The bearing of the direction. Specify "N", "S", "W", or "E".</param>
        /// <param name="degrees">The degrees.</param>
        /// <param name="minutes">The minutes.</param>
        /// <param name="seconds">The seconds.</param>
        public GpsDistance(string direction, float degrees, float minutes, float seconds)
        {
            if (Array.IndexOf(_directionValues, direction) >= 0)
            {
                _direction = direction;
            }

            _degrees = degrees;
            _minutes = minutes;
            _seconds = seconds;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="GpsDistance"/> to <see cref="System.Double"/>.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator double(GpsDistance obj)
        {
            if (obj == null)
                return 0;

            return obj.ToDouble();
        }

        /// <summary>
        /// Generates an integer representation of the current instance. Will be negative for values west of the Prime Meridian
        /// and south of the equator. Ex: "46.5925", "-88.9882"
        /// </summary>
        /// <returns>A <see cref="System.Double"/> that represents this instance.</returns>
        public double ToDouble()
        {
            double distance = Degrees + Minutes / 60.0d + Seconds / 3600.0d;

            if (Array.IndexOf(_negativeDirectionValues, Direction) >= 0)
            {
                distance = distance * -1;
            }

            return distance;
        }

        /// <summary>
        /// Generates a decimal representation of the current instance, including the north/south/east/west indicator.
        /// Ex: "46.5925° N", "88.9882° W"
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public string ToDoubleString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0:F6}° {1}", Math.Abs(ToDouble()), Direction);
        }

        /// <summary>
        /// Generates a string containing the degrees, minutes, and seconds of the current instance. Includes the north/south/east/west indicator.
        /// Ex: "46°32'15.24" N"
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public string ToDegreeMinuteSecondString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0:F0}°{1:F0}'{2:F2}\" {3}", (float)Degrees, (float)Minutes, (float)Seconds, Direction);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance. Internally, this function calls <see cref="ToDegreeMinuteSecondString" />.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return ToDegreeMinuteSecondString();
        }
    }
}
