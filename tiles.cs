using System;
using System.Collections.Generic;
using System.IO;

namespace DukeMapT3D
{
    internal class Tiles
    {
        public const int MAXTILES = 9216;

        public short[] tilesizx = new short[MAXTILES];
        public short[] tilesizy = new short[MAXTILES];
        public int[] picanm = new int[MAXTILES];

        public short[] tilefilenum = new short[MAXTILES];
        private int[] tilefileoffs = new int[MAXTILES];
        private int artsize = 0;
        private short numtilefiles = 0;
        public byte[] picsiz = new byte[MAXTILES];

        private kFile artfil;
        private int artfilplc = -1;
        private int artfilnum = -1;
        private string artfilename;

        public class tilecontainer
        {
            public byte[] memory;
        }

        public tilecontainer[] waloff = new tilecontainer[MAXTILES];

        public class HdTile
        {
            public int tileNum;
            public int width;
            public int height;
            public byte[] buffer;
            public int listId;
        }

        private List<HdTile> hdTiles = new List<HdTile>();

        /*
        ================
        WriteTGA
        ================
        */
        private void WriteTGA( string filename, byte[] data, int width, int height, bool flipVertical ) {
	        byte[]	buffer;
	        int		i;
	        int		bufferSize = width*height*4 + 18;
	        int     imgStart = 18;

            buffer = new byte[bufferSize];
	        buffer[2] = 2;		// uncompressed type
	        buffer[12] = (byte)(width&255);
	        buffer[13] = (byte)(width >>8);
	        buffer[14] = (byte)(height &255);
	        buffer[15] = (byte)(height >>8);
	        buffer[16] = 32;	// pixel siz e
	        if ( !flipVertical ) {
		        buffer[17] = (1<<5);	// flip bit, for normal top to bottom raster order
	        }

	        // swap rgb to bgr
	        for ( i=imgStart ; i<bufferSize ; i+=4 ) {
		        buffer[i] = data[i-imgStart+2];		// blue
		        buffer[i+1] = data[i-imgStart+1];		// green
		        buffer[i+2] = data[i-imgStart+0];		// red
		        buffer[i+3] = data[i-imgStart+3];		// alpha
	        }

            File.WriteAllBytes(filename, buffer);
        }

        private void SafeCreateMapFolder(string folderName)
        {
            bool exists = System.IO.Directory.Exists(folderName);

            if (!exists)
                System.IO.Directory.CreateDirectory(folderName);
        }

        public void WriteAllTextureForMap(string mapName)
        {
            mapName = Path.GetFileNameWithoutExtension(mapName);
            SafeCreateMapFolder(mapName);

            foreach(HdTile tile in hdTiles)
            {
                string textureName = mapName + "/tile" + tile.listId + ".tga";
                WriteTGA(textureName, tile.buffer, tile.width, tile.height, false);
            }
        }

        public HdTile LoadHdTile(int tileNum)
        {
            for(int d = 0; d < hdTiles.Count; d++)
            {
                if (hdTiles[d].tileNum == tileNum)
                    return hdTiles[d];
            }

            if (waloff[tileNum] == null)
                loadtile((short)tileNum);

            if (waloff[tileNum] == null)
                return null;

            byte[] tilebuffer = waloff[tileNum].memory;
            if (tilebuffer == null)
                return null;

            byte[] tempbuffer = new byte[tilesizx[tileNum] * tilesizy[tileNum]];
            int i, j, k;

            i = k = 0;
            while (i < tilesizy[tileNum])
            {
                j = 0;
                while (j < tilesizx[tileNum])
                {
                    tempbuffer[k] = tilebuffer[(j * tilesizy[tileNum]) + i];
                    k++;
                    j++;
                }
                i++;
            }

            HdTile hdTile = new HdTile();   
            hdTile.tileNum = tileNum;
            hdTile.width = tilesizx[tileNum];
            hdTile.height = tilesizy[tileNum];
            hdTile.buffer = Program.dukePalette.RasterizeTexture(0, hdTile.width, hdTile.height, tempbuffer);
            hdTile.listId = hdTiles.Count;

            hdTiles.Add(hdTile);
            return hdTile;
        }

        public int loadpics(string filename)
        {
            int offscount, siz, localtilestart, localtileend, dasiz;
            int i, j, k;
            kFile fil;
            int artversion, mapversion;
            int numtiles;

            for (i = 0; i < MAXTILES; i++)
            {
                tilesizx[i] = 0;
                tilesizy[i] = 0;
                picanm[i] = 0;
            }

            artsize = 0;

            numtilefiles = 0;
            do
            {
                artfilename = filename;
                k = numtilefiles;

                artfilename = artfilename.Remove(5, 3);
                artfilename = artfilename.Insert(5, "" + (char)(((k / 100) % 10) + 48) + "" + (char)(((k / 10) % 10) + 48) + "" + (char)((k % 10) + 48));

                if ((fil = Program.fileSystem.kopen4load(artfilename)) != null)
                {
                    fil.kreadint(out artversion);
                    if (artversion != 1) return (-1);
                    fil.kreadint(out numtiles);
                    fil.kreadint(out localtilestart);
                    fil.kreadint(out localtileend);
                    fil.kread<short>(ref tilesizx, localtilestart, (localtileend - localtilestart + 1) << 1);
                    fil.kread<short>(ref tilesizy, localtilestart, (localtileend - localtilestart + 1) << 1);
                    fil.kread<int, int>(ref picanm, localtilestart, (localtileend - localtilestart + 1) << 2);

                    offscount = 4 + 4 + 4 + 4 + ((localtileend - localtilestart + 1) << 3);
                    for (i = localtilestart; i <= localtileend; i++)
                    {
                        tilefilenum[i] = (short)k;
                        tilefileoffs[i] = offscount;
                        dasiz = (int)((int)tilesizx[i] * (int)tilesizy[i]);
                        offscount += dasiz;
                        artsize += (int)(((dasiz + 15) & 0xfffffff0));
                    }
                    fil.Close();

                    numtilefiles++;
                }
            }
            while (k != numtilefiles);

            for (i = 0; i < MAXTILES; i++)
            {
                j = 15;
                while ((j > 1) && (Board.pow2long[j] > tilesizx[i])) j--;
                picsiz[i] = ((byte)j);
                j = 15;
                while ((j > 1) && (Board.pow2long[j] > tilesizy[i])) j--;
                picsiz[i] += ((byte)(j << 4));
            }

            return (0);
        }

        private void loadtile(short tilenume)
        {
            //PointerObject<byte> ptr;
            int i, dasiz;

            if (tilenume >= MAXTILES)
                return;

            if (waloff[tilenume] != null)
                return;

            dasiz = tilesizx[tilenume] * tilesizy[tilenume];
            if (dasiz <= 0) return;

            i = tilefilenum[tilenume];
            if (i != artfilnum)
            {
                if (artfil != null)
                    artfil.Close();

                artfilnum = i;
                artfilplc = 0;

                artfilename = artfilename.Remove(5, 3);
                artfilename = artfilename.Insert(5, "" + (char)(((i / 100) % 10) + 48) + "" + (char)(((i / 10) % 10) + 48) + "" + (char)((i % 10) + 48));


                artfil = Program.fileSystem.kopen4load(artfilename);
                //faketimerhandler();
            }

            if (artfilplc != tilefileoffs[tilenume])
            {
                artfil.SetPosition((int)(tilefileoffs[tilenume] - artfilplc));
                //faketimerhandler();
            }
            waloff[tilenume] = new tilecontainer();
            byte[] tempbuf = artfil.kread((int)dasiz);

            waloff[tilenume].memory = new byte[tempbuf.Length * 2];
            Array.Copy(tempbuf, 0, waloff[tilenume].memory, 0, tempbuf.Length);
            Array.Copy(tempbuf, 0, waloff[tilenume].memory, tempbuf.Length, tempbuf.Length);

            //faketimerhandler();
            artfilplc = tilefileoffs[tilenume] + dasiz;
        }
    }
}
