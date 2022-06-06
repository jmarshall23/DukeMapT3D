using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
