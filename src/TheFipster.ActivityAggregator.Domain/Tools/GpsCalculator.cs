using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheFipster.ActivityAggregator.Domain.Tools
{
    public class GpsCalculator
    {
        /// <summary>
        /// Uses a WGS84 geodic projection to calculate line length
        /// </summary>
        public double GetLength(IEnumerable<GpsPoint> points)
        {
            double totalMeters = 0;
            GpsPoint lastPoint = null;
            foreach (var point in points)
            {
                if (lastPoint != null)
                    totalMeters += HaversineDistance(lastPoint, point);

                lastPoint = point;
            }

            return totalMeters;
        }

        public double HaversineDistance(GpsPoint p1, GpsPoint p2)
        {
            const double R = 6371000; // Earth radius in meters
            double lat1 = DegreesToRadians(p1.Latitude);
            double lat2 = DegreesToRadians(p2.Latitude);
            double dLat = lat2 - lat1;
            double dLon = DegreesToRadians(p2.Longitude - p1.Longitude);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        public double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
