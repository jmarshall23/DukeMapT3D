using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DukeMapT3D
{
    internal class Sector3D
    {
        public float floorz;
        public float ceilingz;
        public float floorheinum;
        public float ceilingheinum;
        public float floorstat;
        public float ceilingstat;
        public float floorxpanning;
        public float ceilingxpanning;
        public float floorypanning;
        public float ceilingypanning;
        public float floorshade;
        public float floorpal;
        public float floorpicnum;
        public float ceilingpicnum;

        public int indicescount;

        public Plane3D floor;
        public Plane3D ceil;
    }
}
