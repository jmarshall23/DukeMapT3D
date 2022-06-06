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
                    Console.WriteLine("WARNING: Plane Indexes null.");
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
                    file.WriteLine("\t\t\tBegin Polygon Item=sky");
                    file.WriteLine("\t\t\t\tOrigin   +00000.000000,+00000.000000,+00000.000000");
                    file.WriteLine("\t\t\t\tNormal   +00000.000000,+00000.000000,-00001.000000");
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

            pragmas.InitPragmas();

            LoadBoard(fileName);

            WriteT3D(Path.ChangeExtension(fileName, ".t3d"));
        }
    }
}
