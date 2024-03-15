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
        //offsets.cs
        public int dwLocalPlayerPawn = 0x17341B8;
        public int dwEntityList = 0x18BFC08;
        public int dwViewMatrix = 0x1921050;
        public int dwForceAttack = 0x172D030;
        public int dwGameRules = 0x191CB50;
        public int dwGlowManager = 0x191CF70;
        public int dwViewAngles = 0x192D860; // 

        //attributes
        //client.dll.cs offsets
        public int m_vOldOrigin = 0x127C;
        public int m_iTeamNum = 0x3CB;
        public int m_lifeState = 0x338;
        public int m_hPlayerPawn = 0x7E4;
        public int m_vecViewOffset = 0xC58;
        public int m_iIDEntIndex = 0x15A4;
        public int m_iEntIndex = 0x90;
        public int m_flFlashBangTime = 0x14B8;
        public int m_iHealth = 0x334;
        public int m_bBombPlanted = 0x9DD;
        public int m_iszPlayerName = 0x638;
        public int m_entitySpottedState = 0x1698;
        public int m_bSpotted = 0x8;
        public int m_flDetectedByEnemySensorTime = 0x1440;
        public int m_iShotsFired = 0x147C; // 
        public int m_aimPunchAngle = 0x177C; // 

    }
}
