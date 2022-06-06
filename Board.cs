using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DukeMapT3D
{
    internal class Board
    {
        private int mapversion;

        public static byte[] pow2char = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };
        public static int[] pow2long = new int[]
        {
            1,2,4,8,
            16,32,64,128,
            256,512,1024,2048,
            4096,8192,16384,32768,
            65536,131072,262144,524288,
            1048576,2097152,4194304,8388608,
            16777216,33554432,67108864,134217728,
            268435456,536870912,1073741824,2147483647,
        };

        public const int MAXSECTORS = 1024;
        public const int MAXWALLS = 8192;
        public const int MAXSPRITES = 4096;

        public const int MAXTILES = 9216;
        public const int MAXSTATUS = 1024;

        public const int MAXWALLSB = 2048;
        public const int MAXSPRITESONSCREEN = 1024;

        public int numsectors = 0;
        public int numwalls = 0;
        public int numsprites = 0;

        public sectortype[] sector = new sectortype[MAXSECTORS * 2 + 1];
        public walltype[] wall = new walltype[MAXWALLS];
        public spritetype[] sprite = new spritetype[MAXSPRITES];

        public static int getangle(int xvect, int yvect)
        {
            int xvectshift = 0, yvectshift = 0;
            int index;

            if (xvect < 0)
                xvectshift = 1 << 10;

            if (yvect < 0)
                yvectshift = 1 << 10;

            if ((xvect | yvect) == 0) return (0);
            if (xvect == 0) return (512 + (yvectshift));
            if (yvect == 0) return ((xvectshift));
            if (xvect == yvect) return (256 + (xvectshift));
            if (xvect == -yvect) return (768 + (xvectshift));
            if (pragmas.klabs(xvect) > pragmas.klabs(yvect))
            {
                index = 640 + (int)pragmas.scale(160, yvect, xvect);
                return (((Program.table.radarang[index] >> 6) + (xvectshift)) & 2047);
            }

            index = 640 - (int)pragmas.scale(160, xvect, yvect);
            return (((Program.table.radarang[index] >> 6) + 512 + (yvectshift)) & 2047);
        }

        public int sectorofwall(short theline)
        {
            int i, j, gap;

            if ((theline < 0) || (theline >= numwalls)) return (-1);
            i = wall[theline].nextwall; if (i >= 0) return (wall[i].nextsector);

            gap = (numsectors >> 1); i = gap;
            while (gap > 1)
            {
                gap >>= 1;
                if (sector[i].wallptr < theline) i += gap; else i -= gap;
            }
            while (sector[i].wallptr > theline) i--;
            while (sector[i].wallptr + sector[i].wallnum <= theline) i++;
            return (i);
        }

        //
        // inside
        //
        public int inside(int x, int y, short sectnum)
        {
            walltype wal;
            int i;
            int x1, y1, x2, y2;
            uint cnt;
            int wallnum = 0;

            if ((sectnum < 0) || (sectnum >= numsectors))
                return (-1);

            cnt = 0;
            i = sector[sectnum].wallnum;
            do
            {
                wal = wall[sector[sectnum].wallptr + wallnum];
                y1 = (wal.y - y); y2 = (wall[wal.point2].y - y);
                if ((y1 ^ y2) < 0)
                {
                    x1 = (wal.x - x); x2 = (wall[wal.point2].x - x);
                    if ((x1 ^ x2) >= 0)
                    {
                        cnt ^= (uint)x1;
                    }
                    else
                    {
                        cnt ^= (uint)((x1 * y2 - x2 * y1) ^ y2);
                    }
                }
                wallnum++; i--;
            } while (i != 0);

            return (int)(cnt >> 31);
        }

        public void getzsofslope(short sectnum, int dax, int day, ref int ceilz, ref int florz)
        {
            int dx, dy, i, j;
            walltype wal, wal2;
            sectortype sec;

            sec = sector[sectnum];
            ceilz = sec.ceilingz;
            florz = sec.floorz;
            if (((sec.ceilingstat | sec.floorstat) & 2) != 0)
            {
                wal = wall[sec.wallptr];
                wal2 = wall[wal.point2];
                dx = wal2.x - wal.x; dy = wal2.y - wal.y;
                i = ((int)pragmas.nsqrtasm((uint)(dx * dx + dy * dy)) << 5); if (i == 0) return;
                j = pragmas.dmulscale3(dx, day - wal.y, -dy, dax - wal.x);
                if ((sec.ceilingstat & 2) != 0) ceilz = (ceilz) + pragmas.scale(sec.ceilingheinum, j, i);
                if ((sec.floorstat & 2) != 0) florz = (florz) + pragmas.scale(sec.floorheinum, j, i);
            }
        }

        public void updatesector(int x, int y, ref short sectnum)
        {
            walltype wal;
            int j, wallnum;
            short i;

            if (inside(x, y, sectnum) == 1) return;

            if ((sectnum >= 0) && (sectnum < numsectors))
            {
                wallnum = sector[sectnum].wallptr;
                j = sector[sectnum].wallnum;
                do
                {
                    wal = wall[wallnum];
                    i = wal.nextsector;
                    if (i >= 0)
                    {
                        if (inside(x, y, i) == 1)
                        {
                            sectnum = i;
                            return;
                        }
                    }
                    wallnum++;
                    j--;
                } while (j != 0);
            }

            for (i = (short)(numsectors - 1); i >= 0; i--)
                if (inside(x, y, i) == 1)
                {
                    sectnum = i;
                    return;
                }

            sectnum = -1;
        }

        public int loadboard(kFile fil, ref int daposx, ref int daposy, ref int daposz, ref short daang, ref short dacursectnum, bool editorMode)
        {
            short numsprites;

            fil.kreadint(out mapversion);

            Console.WriteLine("...Using mapversion " + mapversion);

            daposx = fil.kreadint();
            daposy = fil.kreadint();
            daposz = fil.kreadint();
            daang = fil.kreadshort();
            dacursectnum = fil.kreadshort();

            // read in the map sectors
            numsectors = fil.kreadshort();
            for (int i = 0; i < numsectors; i++)
            {
                sector[i] = new sectortype(ref fil);
            }
            sector[MAXSECTORS] = new sectortype();

            // read in the wall sectors
            numwalls = fil.kreadshort();
            for (int i = 0; i < numwalls; i++)
            {
                wall[i] = new walltype(ref fil);
            }

            // read in the sprite sectors
            //numsprites = fil.kreadshort();
            //for (int i = 0; i < numsprites; i++)
            //{
            //    sprite[i].Read(ref fil);
            //    // sprite[i].sectnum = MAXSECTORS;
            //    // sprite[i].statnum = MAXSTATUS;
            //}

           // for (int i = 0; i < numsprites; i++)
           //     insertsprite(sprite[i].sectnum, sprite[i].statnum);

            //Must be after loading sectors, etc!
            updatesector(daposx, daposy, ref dacursectnum);

            fil.Close();

            return (0);
        }

        //ceilingstat/floorstat:
        //   bit 0: 1 = parallaxing, 0 = not                                 "P"
        //   bit 1: 1 = groudraw, 0 = not
        //   bit 2: 1 = swap x&y, 0 = not                                    "F"
        //   bit 3: 1 = double smooshiness                                   "E"
        //   bit 4: 1 = x-flip                                               "F"
        //   bit 5: 1 = y-flip                                               "F"
        //   bit 6: 1 = Align texture to first wall of sector                "R"
        //   bits 7-8:                                                       "T"
        //          00 = normal floors
        //          01 = masked floors
        //          10 = transluscent masked floors
        //          11 = reverse transluscent masked floors
        //   bits 9-15: reserved

        //40 bytes
        [Serializable]
        public class sectortype
        {
            public short wallptr, wallnum;
            public int _ceilingz, _floorz;
            public short ceilingstat, floorstat;
            public short ceilingpicnum, ceilingheinum;
            public sbyte ceilingshade;
            public byte ceilingpal, ceilingxpanning, ceilingypanning;
            public short floorpicnum, floorheinum;
            public sbyte floorshade;
            public byte floorpal, floorxpanning, floorypanning;
            public byte visibility, filler;
            public short lotag, hitag, extra;
            public bool changed;
            public bool neighborChanged;


            public int ceilingz
            {
                get
                {
                    return _ceilingz;
                }
                set
                {
                    if (_ceilingz != value)
                        changed = true;

                    _ceilingz = value;

                }
            }

            public int floorz
            {
                get
                {
                    return _floorz;
                }
                set
                {
                    if (_floorz != value)
                        changed = true;
                    _floorz = value;
                }
            }

            public sectortype()
            {

            }

            public bool IsCeilParalaxed() { return (ceilingstat & 1) != 0; }
            public bool IsFloorParalaxed() { return (floorstat & 1) != 0; }

            public void copyto(ref sectortype sector)
            {
                if (sector == null)
                    sector = new sectortype();

                sector.wallptr = wallptr;
                sector.wallnum = wallnum;

                sector.ceilingz = ceilingz;
                sector.floorz = floorz;

                sector.ceilingstat = ceilingstat;
                sector.floorstat = floorstat;

                sector.ceilingpicnum = ceilingpicnum;
                sector.ceilingheinum = ceilingheinum;

                sector.ceilingshade = ceilingshade;

                sector.ceilingpal = ceilingpal;
                sector.ceilingxpanning = ceilingxpanning;
                sector.ceilingypanning = ceilingypanning;

                sector.floorpicnum = floorpicnum;
                sector.floorheinum = floorheinum;

                sector.floorshade = floorshade;

                sector.floorpal = floorpal;
                sector.floorxpanning = floorxpanning;
                sector.floorypanning = floorypanning;

                sector.visibility = visibility;
                sector.filler = filler;

                sector.lotag = lotag;
                sector.hitag = hitag;
                sector.extra = extra;
            }

            public void Write(ref System.IO.BinaryWriter writer)
            {
                writer.Write(wallptr);
                writer.Write(wallnum);

                writer.Write(ceilingz);
                writer.Write(floorz);

                writer.Write(ceilingstat);
                writer.Write(floorstat);

                writer.Write(ceilingpicnum);
                writer.Write(ceilingheinum);

                writer.Write(ceilingshade);

                writer.Write(ceilingpal);
                writer.Write(ceilingxpanning);
                writer.Write(ceilingypanning);

                writer.Write(floorpicnum);
                writer.Write(floorheinum);

                writer.Write(floorshade);

                writer.Write(floorpal);
                writer.Write(floorxpanning);
                writer.Write(floorypanning);

                writer.Write(visibility);
                writer.Write(filler);

                writer.Write(lotag);
                writer.Write(hitag);
                writer.Write(extra);
            }

            public sectortype(ref kFile file)
            {
                wallptr = file.kreadshort();
                wallnum = file.kreadshort();

                ceilingz = file.kreadint();
                floorz = file.kreadint();

                ceilingstat = file.kreadshort();
                floorstat = file.kreadshort();

                ceilingpicnum = file.kreadshort();
                ceilingheinum = file.kreadshort();

                ceilingshade = file.kreadsbyte();

                ceilingpal = file.kreadbyte();
                ceilingxpanning = file.kreadbyte();
                ceilingypanning = file.kreadbyte();

                floorpicnum = file.kreadshort();
                floorheinum = file.kreadshort();

                floorshade = file.kreadsbyte();

                floorpal = file.kreadbyte();
                floorxpanning = file.kreadbyte();
                floorypanning = file.kreadbyte();

                visibility = file.kreadbyte();
                filler = file.kreadbyte();

                lotag = file.kreadshort();
                hitag = file.kreadshort();
                extra = file.kreadshort();
            }
        };

        //cstat:
        //   bit 0: 1 = Blocking wall (use with clipmove, getzrange)         "B"
        //   bit 1: 1 = bottoms of invisible walls swapped, 0 = not          "2"
        //   bit 2: 1 = align picture on bottom (for doors), 0 = top         "O"
        //   bit 3: 1 = x-flipped, 0 = normal                                "F"
        //   bit 4: 1 = masking wall, 0 = not                                "M"
        //   bit 5: 1 = 1-way wall, 0 = not                                  "1"
        //   bit 6: 1 = Blocking wall (use with hitscan / cliptype 1)        "H"
        //   bit 7: 1 = Transluscence, 0 = not                               "T"
        //   bit 8: 1 = y-flipped, 0 = normal                                "F"
        //   bit 9: 1 = Transluscence reversing, 0 = normal                  "T"
        //   bits 10-15: reserved

        //32 bytes
        [Serializable]
        public class walltype
        {
            public int _x, _y;
            public short point2, nextwall, nextsector, cstat;
            public short picnum, overpicnum;
            public sbyte shade;
            public byte pal, xrepeat, yrepeat, xpanning, ypanning;
            public short lotag, hitag, extra;
            public bool changed;
            public walltype()
            {
                changed = false;
            }

            public int x
            {
                get
                {
                    return _x;
                }
                set
                {
                    if (_x != value)
                        changed = true;
                    _x = value;
                }
            }

            public int y
            {
                get
                {
                    return _y;
                }
                set
                {
                    if (_y != value)
                        changed = true;

                    _y = value;
                }
            }

            public void copyto(ref walltype wall)
            {
                if (wall == null)
                    wall = new walltype();

                wall.x = x;
                wall.y = y;

                wall.point2 = point2;
                wall.nextwall = nextwall;
                wall.nextsector = nextsector;
                wall.cstat = cstat;

                wall.picnum = picnum;
                wall.overpicnum = overpicnum;

                wall.shade = shade;

                wall.pal = pal;
                wall.xrepeat = xrepeat;
                wall.yrepeat = yrepeat;
                wall.xpanning = xpanning;
                wall.ypanning = ypanning;

                wall.lotag = lotag;
                wall.hitag = hitag;
                wall.extra = extra;
            }

            public void Write(ref System.IO.BinaryWriter writer)
            {
                writer.Write(x);
                writer.Write(y);

                writer.Write(point2);
                writer.Write(nextwall);
                writer.Write(nextsector);
                writer.Write(cstat);

                writer.Write(picnum);
                writer.Write(overpicnum);

                writer.Write(shade);

                writer.Write(pal);
                writer.Write(xrepeat);
                writer.Write(yrepeat);
                writer.Write(xpanning);
                writer.Write(ypanning);

                writer.Write(lotag);
                writer.Write(hitag);
                writer.Write(extra);
            }

            public walltype(ref kFile file)
            {
                x = file.kreadint();
                y = file.kreadint();

                point2 = file.kreadshort();
                nextwall = file.kreadshort();
                nextsector = file.kreadshort();
                cstat = file.kreadshort();

                picnum = file.kreadshort();
                overpicnum = file.kreadshort();

                shade = file.kreadsbyte();

                pal = file.kreadbyte();
                xrepeat = file.kreadbyte();
                yrepeat = file.kreadbyte();
                xpanning = file.kreadbyte();
                ypanning = file.kreadbyte();

                lotag = file.kreadshort();
                hitag = file.kreadshort();
                extra = file.kreadshort();
            }
        };

        //cstat:
        //   bit 0: 1 = Blocking sprite (use with clipmove, getzrange)       "B"
        //   bit 1: 1 = transluscence, 0 = normal                            "T"
        //   bit 2: 1 = x-flipped, 0 = normal                                "F"
        //   bit 3: 1 = y-flipped, 0 = normal                                "F"
        //   bits 5-4: 00 = FACE sprite (default)                            "R"
        //             01 = WALL sprite (like masked walls)
        //             10 = FLOOR sprite (parallel to ceilings&floors)
        //   bit 6: 1 = 1-sided sprite, 0 = normal                           "1"
        //   bit 7: 1 = Real centered centering, 0 = foot center             "C"
        //   bit 8: 1 = Blocking sprite (use with hitscan / cliptype 1)      "H"
        //   bit 9: 1 = Transluscence reversing, 0 = normal                  "T"
        //   bits 10-14: reserved
        //   bit 15: 1 = Invisible sprite, 0 = not invisible

        //44 bytes
        [Serializable]
        public class spritetype
        {
            public int x, y, z;
            public short cstat, picnum;
            public sbyte shade;
            public byte pal, clipdist, filler;
            public byte xrepeat, yrepeat;
            public sbyte xoffset, yoffset;
            public short sectnum, statnum;
            public short ang, owner, xvel, yvel, zvel;
            public short lotag, hitag, extra;

            // jv
            public object obj;
            // jv end

            public spritetype()
            {

            }

            public void Copy(spritetype t)
            {
                x = t.x;
                y = t.y;
                z = t.z;
                cstat = t.cstat;
                picnum = t.picnum;
                shade = t.shade;
                pal = t.pal;
                clipdist = t.clipdist;
                filler = t.filler;
                xrepeat = t.xrepeat;
                yrepeat = t.yrepeat;
                xoffset = t.xoffset;
                yoffset = t.yoffset;
                sectnum = t.sectnum;
                statnum = t.statnum;
                ang = t.ang;
                owner = t.owner;
                xvel = t.xvel;
                yvel = t.yvel;
                zvel = t.zvel;
                lotag = t.lotag;
                hitag = t.hitag;
                extra = t.extra;
            }

            public void Write(ref System.IO.BinaryWriter writer)
            {
                writer.Write(x);
                writer.Write(y);
                writer.Write(z);

                writer.Write(cstat);
                writer.Write(picnum);
                writer.Write(shade);
                writer.Write(pal);
                writer.Write(clipdist);
                writer.Write(filler);
                writer.Write(xrepeat);
                writer.Write(yrepeat);
                writer.Write(xoffset);
                writer.Write(yoffset);
                writer.Write(sectnum);
                writer.Write(statnum);
                writer.Write(ang);
                writer.Write(owner);
                writer.Write(xvel);
                writer.Write(yvel);
                writer.Write(zvel);
                writer.Write(lotag);
                writer.Write(hitag);
                writer.Write(extra);
            }

            public void Read(ref kFile file)
            {
                x = file.kreadint();
                y = file.kreadint();
                z = file.kreadint();

                cstat = file.kreadshort();
                picnum = file.kreadshort();
                shade = file.kreadsbyte();
                pal = file.kreadbyte();
                clipdist = file.kreadbyte();
                filler = file.kreadbyte();
                xrepeat = file.kreadbyte();
                yrepeat = file.kreadbyte();
                xoffset = file.kreadsbyte();
                yoffset = file.kreadsbyte();
                sectnum = file.kreadshort();
                statnum = file.kreadshort();
                ang = file.kreadshort();
                owner = file.kreadshort();
                xvel = file.kreadshort();
                yvel = file.kreadshort();
                zvel = file.kreadshort();
                lotag = file.kreadshort();
                hitag = file.kreadshort();
                extra = file.kreadshort();
            }
        };
        [Serializable]
        public struct spritetype2
        {
            public int x, y, z;
            public short cstat, picnum;
            public sbyte shade;
            public byte pal, clipdist, filler;
            public byte xrepeat, yrepeat;
            public sbyte xoffset, yoffset;
            public short sectnum, statnum;
            public short ang, owner, xvel, yvel, zvel;
            public short lotag, hitag, extra;

            public void Copy(spritetype t)
            {
                x = t.x;
                y = t.y;
                z = t.z;
                cstat = t.cstat;
                picnum = t.picnum;
                shade = t.shade;
                pal = t.pal;
                clipdist = t.clipdist;
                filler = t.filler;
                xrepeat = t.xrepeat;
                yrepeat = t.yrepeat;
                xoffset = t.xoffset;
                yoffset = t.yoffset;
                sectnum = t.sectnum;
                statnum = t.statnum;
                ang = t.ang;
                owner = t.owner;
                xvel = t.xvel;
                yvel = t.yvel;
                zvel = t.zvel;
                lotag = t.lotag;
                hitag = t.hitag;
                extra = t.extra;
            }

            public void Copy(spritetype2 t)
            {
                x = t.x;
                y = t.y;
                z = t.z;
                cstat = t.cstat;
                picnum = t.picnum;
                shade = t.shade;
                pal = t.pal;
                clipdist = t.clipdist;
                filler = t.filler;
                xrepeat = t.xrepeat;
                yrepeat = t.yrepeat;
                xoffset = t.xoffset;
                yoffset = t.yoffset;
                sectnum = t.sectnum;
                statnum = t.statnum;
                ang = t.ang;
                owner = t.owner;
                xvel = t.xvel;
                yvel = t.yvel;
                zvel = t.zvel;
                lotag = t.lotag;
                hitag = t.hitag;
                extra = t.extra;
            }
        };
    }
}
