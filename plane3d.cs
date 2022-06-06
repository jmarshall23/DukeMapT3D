using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DukeMapT3D
{
    internal class Plane3D
    {
        public Vector3[] xyz;
        public Vector2[] st;
        public int[] indexes;
        private Vector4 _parms;
        private Vector3[] cached_xyz;
        private Vector2[] cached_uv;
        private bool isVisible;
        public int displayFrameId;
        private Render3D parent;

        public Plane3D(Render3D parent)
        {
            this.parent = parent;
            isVisible = true;
            displayFrameId = 0;
            parent.planes.Add(this);
        }

        public Vector3[] GetVertexes()
        {
            Vector3[] vertices = new Vector3[xyz.Length];

            for(int i = 0; i < xyz.Length; i++)
            {
                vertices[i] = new Vector3(xyz[i].X * -Render3D.WorldScale, xyz[i].Z * Render3D.WorldScale, xyz[i].Y * Render3D.WorldScale);
            }

            return vertices;
        }

        public void InitTexture(int tileNum)
        {

        }

        public void Init(int vertexcount)
        {
            xyz = new Vector3[vertexcount];
            st = new Vector2[vertexcount];
        }


        public void Update(float visibility, float shadeOffset, float palette, float curbasepal)
        {
           
        }

        public void Build(float visibility, float shadeOffset, float palette, float curbasepa)
        {
            
        }
    }
}
