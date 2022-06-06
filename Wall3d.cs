using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DukeMapT3D
{
    internal class Wall3D
    {
        public int picnum_anim;
        public int overpicnum_anim;
        public int nwallpicnum;
        public int nwallxpanning;
        public int nwallypanning;
        public int nwallcstat;
        public int nwallshade;
        public int underover;


        public Plane3D wall;
        public Plane3D over;
        public Plane3D mask;
    }
}
