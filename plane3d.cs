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
            Vector3[] vertices = new Vector3[3];

            for(int i = 0; i < xyz.Length; i++)
            {
                vertices[i] = new Vector3(xyz[i].X * -Render3D.WorldScale, xyz[i].Y * Render3D.WorldScale, xyz[i].Z * Render3D.WorldScale);
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
            bool changed = false;
            bool changed_uvs = false;
            for (int i = 0; i < cached_xyz.Length; i++)
            {
                if (cached_xyz[i] != xyz[i])
                {
                    changed = true;
                }

                if (cached_uv[i] != st[i])
                {
                    changed_uvs = true;
                }
            }

            if (changed_uvs)
            {
                for (int i = 0; i < cached_xyz.Length; i++)
                {
                    cached_uv[i] = st[i];
                }
            }

            if (changed)
            {
                for (int i = 0; i < cached_xyz.Length; i++)
                {
                    cached_xyz[i] = xyz[i];
                }
            }

            Vector4 newParms = new Vector4(visibility, shadeOffset, palette, curbasepal);
            if (_parms == newParms)
                return;

            _parms = newParms;
        }

        public void Build(float visibility, float shadeOffset, float palette, float curbasepa)
        {
            if (indexes == null)
                return;

            cached_xyz = new Vector3[xyz.Length];
            for (int i = 0; i < cached_xyz.Length; i++)
            {
                cached_xyz[i] = xyz[i];
            }

            cached_uv = new Vector2[xyz.Length];
            for (int i = 0; i < cached_uv.Length; i++)
            {
                cached_uv[i] = st[i];
            }
        }
    }
}
