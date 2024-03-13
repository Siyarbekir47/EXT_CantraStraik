using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace EXT_CantraStraik
{
    public class Entity
    {
        //entity list for Csgo2


        public Vector3 viewOffset { get; set; }
        public Vector3 position { get; set; }
        public Vector2 position2D { get; set; }
        public Vector2 viewPosition2D { get; set; }



        public IntPtr address { get; set; }
        public int health { get; set; }
        public Vector3 origin { get; set; }

        public Vector3 abs { get; set; }

        public Vector2 originScreenPosition { get; set; }
        public Vector2 absScreenPosition { get; set; }
        public int teamNum { get; set; }
        public int jumpFlag { get; set; }

    }
}
