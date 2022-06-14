using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Numerics;

namespace DukeMapT3D
{
    internal class Program
    {
        public static kFileSystem fileSystem = new kFileSystem();
        public static bTable table;
        public static Palette dukePalette;
        public static Tiles tiles;

        static Board board;
        static int daposx, daposy, daposz;
        static short daang, dacursectnum;
        static Render3D render;
        

        static void LoadBoard(string fileName)
        {
            kFile fil = fileSystem.kopen4load(fileName);

            if (fil == null)
            {
                throw new Exception("Failed to open board " + fileName);
            }

            board = new Board();

            if (board.loadboard(fil, ref daposx, ref daposy, ref daposz, ref daang, ref dacursectnum, false) == -1)
            {
                throw new Exception("loadboard: failed to load board " + fileName);
            }

            render = new Render3D();
            render.LoadBoard(board, false);
        }

        static string StupidFloatWriter(float f)
        {
            string s = String.Format("{0:00000.000000}", f);

            if (f > 0)
                return "+" + s;

            return s;
        }

        static Vector3 CrossProduct(Vector3 v1, Vector3 v2)
        {
            float x, y, z;
            x = v1.Y * v2.Z - v2.Y * v1.Z;
            y = (v1.X * v2.Z - v2.X * v1.Z) * -1;
            z = v1.X * v2.Y - v2.X * v1.Y;

            var rtnvector = new Vector3(x, y, z);
            return rtnvector;
        }

        static Vector3 Normalize(Vector3 v1)
        {
            float magnitude = (float)Math.Sqrt((v1.X * v1.X) + (v1.Y * v1.Y) + (v1.Z * v1.Z));

            // find the inverse of the vectors magnitude
            float inverse = 1 / magnitude;

            return new Vector3(
                // multiply each component by the inverse of the magnitude
                v1.X * inverse,
                v1.Y * inverse,
                v1.Z * inverse);
        }

        static void WriteGenericFormat(string fileName)
        {
            StreamWriter file = new StreamWriter(fileName);

            // Write out each plane as a brush
            for (int i = 0; i < render.planes.Count; i++)
            {
                Plane3D plane = render.planes[i];
                Vector3[] xyz = plane.GetVertexes();

                if (plane.indexes == null)
                {
                    //   Console.WriteLine("WARNING: Plane Indexes null.");
                    continue;
                }

                file.WriteLine("Plane " + plane.hdTile.filename);
                file.WriteLine("{");

                for (int d = 0; d < plane.indexes.Length; d += 3)
                {
                    Vector3 a = xyz[plane.indexes[d + 0]];
                    Vector3 b = xyz[plane.indexes[d + 1]];
                    Vector3 c = xyz[plane.indexes[d + 2]];

                    Vector3 v = c - b;
                    Vector3 v1 = a - b;

                    Vector3 normal = Normalize(CrossProduct(v, v1));

                    for (int t = 0; t < 3; t++)
                    {
                        Vector3 pt = xyz[plane.indexes[d + t]];
                        file.WriteLine("\txyz " + pt.X + " " + pt.Y + " " + pt.Z);
                        file.WriteLine("\tnormal " + normal.X + " " + normal.Y + " " + normal.Z);
                    }
                }

                file.WriteLine("}");               
            }
        }

        static void WriteT3D(string fileName)
        {
            StreamWriter file = new StreamWriter(fileName);

            file.WriteLine("Begin Map");
            file.WriteLine("Begin Actor Class=LevelInfo Name=LevelInfo___0");
            file.WriteLine("\tTimeSeconds = 164.771011");
            file.WriteLine("\tTimeDeltaSeconds = 0.005000");
            file.WriteLine("\tAIProfile(0) = 32019773");
            file.WriteLine("\tRegion = (Zone = LevelInfo'MyLevel.LevelInfo___0',iLeaf = -1)");
            file.WriteLine("\tLevel = LevelInfo'MyLevel.LevelInfo___0'");
            file.WriteLine("\tTag = LevelInfo");
            file.WriteLine("\tName = LevelInfo___0");
            file.WriteLine("End Actor");

            file.WriteLine("Begin Actor Class = Brush Name = Brush___0");
            file.WriteLine("\tMainScale=(SheerAxis = SHEER_ZX)");
            file.WriteLine("\tPostScale=(SheerAxis = SHEER_ZX)");
            file.WriteLine("\tRegion=(Zone = LevelInfo'MyLevel.LevelInfo___0',iLeaf = -1");
            file.WriteLine("\tLevel=LevelInfo'MyLevel.LevelInfo___0'");
            file.WriteLine("\tGroup=Sheet");
            file.WriteLine("\tTag=Brush");
            file.WriteLine("\tBegin Brush Name=Brush");
            file.WriteLine("\t\tBegin PolyList");
            file.WriteLine("\t\t\tBegin Polygon Item=Sheet Flags=264");
            file.WriteLine("\t\t\t\tOrigin +00128.000000,+00128.000000,+00000.000000");
            file.WriteLine("\t\t\t\tNormal +00000.000000,+00000.000000,-00001.000000");
            file.WriteLine("\t\t\t\tTextureU -00001.000000,+00000.000000,+00000.000000");
            file.WriteLine("\t\t\t\tTextureV +00000.000000,+00001.000000,+00000.000000");
            file.WriteLine("\t\t\t\tVertex +00128.000000,+00128.000000,+00000.000000");
            file.WriteLine("\t\t\t\tVertex +00128.000000,-00128.000000,+00000.000000");
            file.WriteLine("\t\t\t\tVertex -00128.000000,-00128.000000,+00000.000000");
            file.WriteLine("\t\t\t\tVertex -00128.000000,+00128.000000,+00000.000000");
            file.WriteLine("\t\t\tEnd Polygon");
            file.WriteLine("\t\tEnd PolyList");
            file.WriteLine("\tEnd Brush");
            file.WriteLine("\tBrush = Model'MyLevel.Brush'");
            file.WriteLine("\tName = Brush___0");
            file.WriteLine("End Actor");

            // Write out each plane as a brush
            for (int i = 0; i < render.planes.Count; i++)
            {
                Plane3D plane = render.planes[i];
                Vector3[] xyz = plane.GetVertexes();

                if (plane.indexes == null)
                {
                 //   Console.WriteLine("WARNING: Plane Indexes null.");
                    continue;
                }

                file.WriteLine("Begin Actor Class = Brush Name = Brush___" + (i + 1));
                file.WriteLine("\tMainScale=(SheerAxis = SHEER_ZX)");
                file.WriteLine("\tPostScale=(SheerAxis = SHEER_ZX)");
                file.WriteLine("\tbDynamicLight=True");
                file.WriteLine("\tCsgOper=CSG_Subtract");
                file.WriteLine("\tRegion=(Zone=LevelInfo'MyLevel.LevelInfo___0',iLeaf = -1)");
                file.WriteLine("\tLevel=LevelInfo'MyLevel.LevelInfo___0'");
                file.WriteLine("\tTag=Brush");

                file.WriteLine("\tBegin Brush Name=Brush");
                file.WriteLine("\t\tBegin PolyList");

                for (int d = 0; d < plane.indexes.Length; d+=3)
                {
                    Vector3 a = xyz[plane.indexes[d + 0]];
                    Vector3 b = xyz[plane.indexes[d + 1]];
                    Vector3 c = xyz[plane.indexes[d + 2]];

                    Vector3 v = c - b;
                    Vector3 v1 = a - b;

                    Vector3 normal = Normalize(CrossProduct(v, v1));
                   // normal.Normalize();

                    file.WriteLine("\t\t\tBegin Polygon TEXTURE=" + plane.hdTile.filename);
                    file.WriteLine("\t\t\t\tOrigin   +00000.000000,+00000.000000,+00000.000000");
                    file.WriteLine("\t\t\t\tNormal   " + StupidFloatWriter(normal.X) + "," + StupidFloatWriter(normal.Y) + "," + StupidFloatWriter(normal.Z));
                    file.WriteLine("\t\t\t\tTextureU -00001.000000,+00000.000000,+00000.000000");
                    file.WriteLine("\t\t\t\tTextureV +00000.000000,+00001.000000,+00000.000000");

                    for (int t = 0; t < 3; t++)
                    {
                        Vector3 pt = xyz[plane.indexes[d + t]];
                        file.WriteLine("\t\t\t\tVertex   " + StupidFloatWriter(pt.X) + "," + StupidFloatWriter(pt.Y) + "," + StupidFloatWriter(pt.Z));
                    }
                    file.WriteLine("\t\t\tEnd Polygon");
                }
                
                
                file.WriteLine("\t\tEnd PolyList");
                file.WriteLine("\tEnd Brush");
                file.WriteLine("\tModel'MyLevel.Brush'");
                file.WriteLine("\tName = Brush___" + (i + 1));
                file.WriteLine("End Actor");
            }

            file.WriteLine("End Map");
            file.Flush();
        }

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Usage: DukeMapT3D.exe mapFile.map");
                return;
            }

            string fileName = args[0];            

            table = new bTable();
            dukePalette = new Palette();
            tiles = new Tiles();

            pragmas.InitPragmas();

            tiles.loadpics("tiles000.art");

            LoadBoard(fileName);

            tiles.WriteAllTextureForMap(fileName);

            WriteGenericFormat(Path.ChangeExtension(fileName, ".planes"));
        }
    }
}
