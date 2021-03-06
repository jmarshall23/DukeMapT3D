using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DukeMapT3D
{
    public struct bPalLookup
    {
        public byte[] palookup;
    }
    public class Palette
    {
        public short numpalookups;
        public byte[] scratchpal = new byte[768];
        public byte[] palette = new byte[768];
        public bPalLookup[] _palookup = new bPalLookup[256];
        public byte[] palookup = null;
        public byte[] transluc = new byte[65536];
        public int globalpalwritten;
        public int globalpal;
        private int _currentpal = 0;

        public int currentpal
        {
            set
            {
                /*
                if (_palookup[value].palookup == null)
                {
                    _currentpal = 0;
                }
                else
                {
                    _currentpal = value;
                }
                */
                palookup = _palookup[_currentpal].palookup;
            }
        }

        public int[] _palettebuffer = new int[256];

        public const int FASTPALGRIDSIZ = 8;
        public int[] rdist = new int[129];
        public int[] gdist = new int[129];
        public int[] bdist = new int[129];
        public byte[] colhere = new byte[(((FASTPALGRIDSIZ + 2) * (FASTPALGRIDSIZ + 2) * (FASTPALGRIDSIZ + 2)) >> 3)];
        public byte[] colhead = new byte[((FASTPALGRIDSIZ + 2) * (FASTPALGRIDSIZ + 2) * (FASTPALGRIDSIZ + 2))];
        public int[] colnext = new int[256];
        public byte[] coldist = new byte[] { 0, 1, 2, 3, 4, 3, 2, 1 };
        public int[] colscan = new int[27];

        private bool needPaletteUpdate = false;

        const int MAXPALOOKUPS = 256;

        public int getpalookup(int davis, int dashade)
        {
            return (int)(Math.Min(Math.Max(dashade + (davis >> 8), 0), numpalookups - 1));
        }

        public Palette()
        {
            kFile fil;
            int i, j, k, dist;

            Console.WriteLine("Loading Palette.dat...");
            if ((fil = Program.fileSystem.kopen4load("palette.dat")) == null)
            {
                throw new Exception("palette.dat not found");
            }

            fil.kread<byte>(ref palette, 0, 768);
            fil.kreadshort(out numpalookups);

            globalpalwritten = 0; globalpal = 0;

            _palookup[globalpal].palookup = new byte[numpalookups << 8];


            //A.setpalookupaddress(_palookup[globalpal].palookup, globalpalwritten);
            A.fixtransluscence(transluc, 0);

            fil.kread<byte>(ref _palookup[globalpal].palookup, globalpal, numpalookups << 8);
            fil.kread<byte>(ref transluc, 0, 65536);
            fil.Close();

            initfastcolorlookup(30, 59, 11);
            UpdatePalette();
        }

        private void ColorShiftLightingBytes(int r, int g, int b, out byte rr, out byte rg, out byte rb )
        {
            // normalize by color instead of saturating to white
            if ((r | g | b) > 255)
            {
                int max;

                max = r > g ? r : g;
                max = max > b ? max : b;
                r = r * 255 / max;
                g = g * 255 / max;
                b = b * 255 / max;
            }

            rr = (byte)r;
            rg = (byte)g;
            rb = (byte)b;
        }

        public byte[] RasterizeTexture(int dabrightness, int width, int height, byte[] tileData)
        {
            byte[] rgbaTextureData = new byte[width * height * 4];


            int curbrightness = Math.Min(Math.Max((int)dabrightness, 0), 15);

            for (int i = 0; i < palette.Length; i++)
            {
                scratchpal[i] = Program.table.britable[curbrightness].data[palette[i]];
            }

            for (int i = 0; i < width * height; i++)
            {
                int offset = tileData[i];

                float r = scratchpal[(offset * 3) + 0] * 3.5f;
                float g = scratchpal[(offset * 3) + 1] * 3.5f;
                float b = scratchpal[(offset * 3) + 2] * 3.5f;

                ColorShiftLightingBytes((int)r, (int)g, (int)b, out rgbaTextureData[(i * 4) + 0], out rgbaTextureData[(i * 4) + 1], out rgbaTextureData[(i * 4) + 2]);

                if (offset == 255)
                {
                    rgbaTextureData[(i * 4) + 3] = 0;
                }
                else
                {
                    rgbaTextureData[(i * 4) + 3] = 255;
                }
            }

            return rgbaTextureData;
        }

        public void UpdatePalette()
        {
            initfastcolorlookup2();
            needPaletteUpdate = true;
        }

        public void UpdatePaletteMainThread()
        {
#if false
            if (!needPaletteUpdate)
                return;

            if (paletteTexture == null)
            {
                paletteTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false);
                paletteTexture.filterMode = FilterMode.Point;
            }

            byte[] tempbuffer = new byte[256 * 4];
            for (int i = 0; i < 256; i++)
            {
                tempbuffer[(i * 4) + 0] = palette[(i * 3) + 0];
                tempbuffer[(i * 4) + 1] = palette[(i * 3) + 1];
                tempbuffer[(i * 4) + 2] = palette[(i * 3) + 2];
                tempbuffer[(i * 4) + 3] = 255;
            }

            paletteTexture.LoadRawTextureData(tempbuffer);
            paletteTexture.Apply();

            byte[] palookuptable = new byte[MAXPALOOKUPS * (256 * (32))];
            int x = 0;
            for (int p = 0; p < MAXPALOOKUPS; p++)
            {
                for (int i = 0; i < 256 * 32; ++i, x++)
                {
                    if (_palookup[p].palookup != null)
                    {
                        palookuptable[x] = _palookup[p].palookup[i];
                    }
                }
            }

            palookupTexture = new Texture2D(256, 8192, TextureFormat.R8, false);
            palookupTexture.filterMode = FilterMode.Point;
            palookupTexture.LoadRawTextureData(palookuptable);
            palookupTexture.Apply();

            needPaletteUpdate = false;
#endif
        }

        //
        // getclosestcol (internal)
        //
        private int getclosestcol(int r, int g, int b)
        {
            int i, j, k, dist, mindist, retcol;
            int pal1;

            j = (r >> 3) * FASTPALGRIDSIZ * FASTPALGRIDSIZ + (g >> 3) * FASTPALGRIDSIZ + (b >> 3) + FASTPALGRIDSIZ * FASTPALGRIDSIZ + FASTPALGRIDSIZ + 1;
            mindist = Math.Min(rdist[coldist[r & 7] + 64 + 8], gdist[coldist[g & 7] + 64 + 8]);
            mindist = Math.Min(mindist, bdist[coldist[b & 7] + 64 + 8]);
            mindist++;

            r = 64 - r; g = 64 - g; b = 64 - b;

            retcol = -1;
            for (k = 26; k >= 0; k--)
            {
                i = colscan[k] + j; if ((colhere[i >> 3] & Board.pow2char[i & 7]) == 0) continue;
                i = colhead[i];
                do
                {
                    //pal1 = (char*)&palette[i * 3];
                    pal1 = i * 3;
                    dist = gdist[palette[pal1 + 1] + g];
                    if (dist < mindist)
                    {
                        dist += rdist[palette[pal1 + 0] + r];
                        if (dist < mindist)
                        {
                            dist += bdist[palette[pal1 + 2] + b];
                            if (dist < mindist) { mindist = dist; retcol = i; }
                        }
                    }
                    i = colnext[i];
                } while (i >= 0);
            }
            if (retcol >= 0) return (retcol);

            mindist = 0x7fffffff;
            //pal1 = (char*)&palette[768 - 3];
            pal1 = 768 - 3;
            for (i = 255; i >= 0; i--, pal1 -= 3)
            {
                dist = gdist[palette[pal1 + 1] + g]; if (dist >= mindist) continue;
                dist += rdist[palette[pal1 + 0] + r]; if (dist >= mindist) continue;
                dist += bdist[palette[pal1 + 2] + b]; if (dist >= mindist) continue;
                mindist = dist; retcol = i;
            }
            return (retcol);
        }

        public void makepalookup(int palnum, byte[] remapbuf, byte r, byte g, byte b, bool dastat)
        {
            int i, j, palscale;
            int ptr, ptr2;

            if (_palookup[palnum].palookup == null)
            {
                //Allocate palookup buffer
                _palookup[palnum].palookup = new byte[numpalookups << 8];
            }

            if (dastat == false) return;
            if ((r | g | b | 63) != 63) return;

            if ((r | g | b) == 0)
            {
                for (i = 0; i < 256; i++)
                {
                    // ptr = (char*)(FP_OFF(palookup[0]) + remapbuf[i]);
                    //ptr2 = (char*)(FP_OFF(palookup[palnum]) + i);
                    ptr = remapbuf[i];
                    ptr2 = i;
                    for (j = 0; j < numpalookups; j++)
                    {
                        //*ptr2 = *ptr; 
                        _palookup[palnum].palookup[ptr2] = _palookup[0].palookup[ptr];
                        ptr += 256;
                        ptr2 += 256;
                    }
                }
            }
            else
            {
                ptr2 = palookup[palnum];
                for (i = 0; i < numpalookups; i++)
                {
                    palscale = pragmas.divscale16(i, numpalookups);
                    for (j = 0; j < 256; j++)
                    {
                        ptr = remapbuf[j] * 3; // (char*)&palette[remapbuf[j] * 3];

                        // jv - casted to byte
                        _palookup[palnum].palookup[ptr2] = (byte)getclosestcol((int)palette[ptr + 0] + pragmas.mulscale16(r - palette[ptr + 0], palscale),
                                                                           (int)palette[ptr + 1] + pragmas.mulscale16(g - palette[ptr + 1], palscale),
                                                                           (int)palette[ptr + 2] + pragmas.mulscale16(b - palette[ptr + 2], palscale));

                        // jv end
                    }
                }
            }
        }

        //
        // initfastcolorlookup2
        //
        public void initfastcolorlookup2()
        {
            for (int i = 0, a = 0; i < palette.Length; i += 3, a++)
            {
                _palettebuffer[a] = (255 << 24) | ((byte)(palette[i + 0] * 4) << 16) | (byte)(palette[i + 1] * 4) << 8 | (byte)(palette[i + 2] * 4);
            }
        }

        private void initfastcolorlookup(int rscale, int gscale, int bscale)
        {
            int i, j, x, y, z;
            int pal1;

            j = 0;
            for (i = 64; i >= 0; i--)
            {
                //j = (i-64)*(i-64);
                rdist[i] = rdist[128 - i] = j * rscale;
                gdist[i] = gdist[128 - i] = j * gscale;
                bdist[i] = bdist[128 - i] = j * bscale;
                j += 129 - (i << 1);
            }

            //clearbufbyte(FP_OFF(colhere),sizeof(colhere),0L);
            //clearbufbyte(FP_OFF(colhead),sizeof(colhead),0L);

            pal1 = 768 - 3;
            for (i = 255; i >= 0; i--, pal1 -= 3)
            {
                j = (palette[pal1] >> 3) * FASTPALGRIDSIZ * FASTPALGRIDSIZ + (palette[pal1 + 1] >> 3) * FASTPALGRIDSIZ + (palette[pal1 + 2] >> 3) + FASTPALGRIDSIZ * FASTPALGRIDSIZ + FASTPALGRIDSIZ + 1;
                if ((colhere[j >> 3] & Board.pow2char[j & 7]) != 0)
                    colnext[i] = colhead[j];
                else
                    colnext[i] = -1;

                colhead[j] = (byte)i;
                colhere[j >> 3] |= Board.pow2char[j & 7];
            }

            i = 0;
            for (x = -FASTPALGRIDSIZ * FASTPALGRIDSIZ; x <= FASTPALGRIDSIZ * FASTPALGRIDSIZ; x += FASTPALGRIDSIZ * FASTPALGRIDSIZ)
                for (y = -FASTPALGRIDSIZ; y <= FASTPALGRIDSIZ; y += FASTPALGRIDSIZ)
                    for (z = -1; z <= 1; z++)
                        colscan[i++] = x + y + z;
            i = colscan[13]; colscan[13] = colscan[26]; colscan[26] = i;
        }
    }
}
