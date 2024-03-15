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
        public int entIndex { get; set; }

        public string playerName { get; set; }
        public bool spotted { get; set; }
        

        public IntPtr address { get; set; }
        public int health { get; set; }
        public int teamNum { get; set; }

    }
}
