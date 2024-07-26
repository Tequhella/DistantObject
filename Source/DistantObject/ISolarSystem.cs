using Kopernicus.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistantObject
{
    internal interface ISolarSystem
    {
        Vector3d GetSunPosition();
    }

    // Default Solar System implementation
    class DefaultSolarSystem : ISolarSystem
    {
        public Vector3d GetSunPosition()
        {
            return FlightGlobals.Bodies[0].position;
        }
    }

    // Kopernicus Solar System implementation
    class KopernicusSolarSystem : ISolarSystem
    {
        public Vector3d GetSunPosition()
        {
            return KopernicusStar.Current.sun.position;
        }
    }
}
