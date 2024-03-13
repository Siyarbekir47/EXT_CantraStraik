using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EXT_CantraStraik
{
    public class Offsets
    {
        //bases

        public int localPlayer = 0x1730118;
        public int entityList = 0x173DBC0;
        public int viewMatrix = 0x191CF30;
        public int forceAttack = 0x1728F90;


        //attributes
        public int teamNum = 0x3CB;
        public int jumpFlag = 0x3D4;
        public int health = 0x0334;
        public int origin = 0x0D60;
        public int flasbangTime = 0x14B8;
        public int iIDEntIndex = 0x15A4;
        public int hPlayerPawn = 0x7E4;
        public int flDetectedByEnemySensorTime = 0x1440;
    }
}
