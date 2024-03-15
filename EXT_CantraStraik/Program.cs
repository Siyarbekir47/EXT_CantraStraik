using ClickableTransparentOverlay;
using ImGuiNET;
using Swed64;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using EXT_CantraStraik;

//init swed
Swed swed = new Swed("cs2");

//get client module
IntPtr clientDll = swed.GetModuleBase("client.dll");

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);


Thread hotkeyThread = new Thread(HotkeyListenerLoop) {IsBackground = true };
hotkeyThread.Start();


//get init renderer
Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();



//get screensize from renderer

Vector2 screenSize = renderer.windowSize;

//store entities
List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

Offsets offsets = new Offsets();
//loop

Thread triggerBotLogic = new Thread(TriggerBotLogic) { IsBackground = true };
triggerBotLogic.Start();

Thread bombTimerLogic = new Thread(BombTimerLogic) { IsBackground = true };
bombTimerLogic.Start();
bool isF10KeyPressed = false;


bool glowStatus = false;
string spottedStatus = "";

while (true)
{
    entities.Clear();
    //get entity list
    IntPtr entityList = swed.ReadPointer(clientDll, offsets.dwEntityList);
    //make entry
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);
    IntPtr localPlayerPawn = swed.ReadPointer(clientDll, offsets.dwLocalPlayerPawn);
    //get local player
    IntPtr currentLPController = swed.ReadPointer(listEntry, 0x78);

    //get team, so we can compare
    localPlayer.address = localPlayerPawn;
    localPlayer.teamNum = swed.ReadInt(localPlayer.address + offsets.m_iTeamNum);
    localPlayer.playerName = swed.ReadString(currentLPController + offsets.m_iszPlayerName, 16).Split("\0")[0];
    localPlayer.health = swed.ReadInt(localPlayer.address + offsets.m_iHealth);
    localPlayer.position = swed.ReadVec(localPlayer.address + offsets.m_vOldOrigin);
    localPlayer.viewOffset = swed.ReadVec(localPlayer.address + offsets.m_vecViewOffset);
    localPlayer.entIndex = swed.ReadInt(localPlayer.address, offsets.m_iIDEntIndex);


    for (int i = 0; i < 64; i++)
    {
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
        if (currentController == IntPtr.Zero)
            continue;
        int pawnHandle = swed.ReadInt(currentController + offsets.m_hPlayerPawn);
        if (pawnHandle == 0)
            continue;

        //get current pawn
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        if (listEntry2 == IntPtr.Zero)
            continue;

        //get current pawn 
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        if (currentPawn == IntPtr.Zero)
            continue;
        //check lifeState
        int lifeState = swed.ReadInt(currentPawn + offsets.m_lifeState);
        if (lifeState != 256)
            continue;

        //get matrix
        float[] viewMatrix = swed.ReadMatrix(clientDll + offsets.dwViewMatrix);

        //populate entity
        Entity entity = new Entity();
        entity.teamNum = swed.ReadInt(currentPawn + offsets.m_iTeamNum);
        entity.position = swed.ReadVec(currentPawn + offsets.m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn + offsets.m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);
        entity.health = swed.ReadInt(currentPawn + offsets.m_iHealth);
        entity.address = currentPawn;
        entity.playerName = swed.ReadString(currentController + offsets.m_iszPlayerName, 16).Split("\0")[0];
        entity.spotted = swed.ReadBool(currentPawn, offsets.m_entitySpottedState + offsets.m_bSpotted);
        if(renderer.enableRadar)
        {
            swed.WriteBool(currentPawn, offsets.m_entitySpottedState + offsets.m_bSpotted, true);

        }
        if(renderer.enableGlow)
        {
            swed.WriteFloat(currentPawn + offsets.m_flDetectedByEnemySensorTime, 86400);

        }
        glowStatus = renderer.enableGlow;
        if (glowStatus == false)
        {
            swed.WriteFloat(currentPawn + offsets.m_flDetectedByEnemySensorTime, 0);
        }


        spottedStatus = entity.spotted == true ? "Spotted" : " "; //string depending on if entity is spotted   
        entities.Add(entity);

    }


    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);

}


Vector3 Normalize(Vector3 angel)
{
    while (angel.Y < -180) angel.Y += 360;
    while (angel.Y > 180) angel.Y -= 360;
    if (angel.X < -89) angel.X = -89;
    if (angel.X > 89) angel.X = 89;
    return angel;
}




void HotkeyListenerLoop()
{
    while (true)
    {
        isF10KeyPressed = GetAsyncKeyState(0x2D) < 0;

        if (isF10KeyPressed)
        {
            renderer.showMenu = !renderer.showMenu; // Toggle the menu
            Thread.Sleep(100); // Debounce
        }

        Thread.Sleep(10); // Adjust polling interval as needed
    }
}


void TriggerBotLogic()
{
    Vector3 oPunch = new Vector3(0, 0, 0);

    while (true)
    {





        if (renderer.enableTriggerBot)
        {

            if (GetAsyncKeyState(0x6) < 0)
            {
                IntPtr entityList = swed.ReadPointer(clientDll, offsets.dwEntityList);
                //int entIndex = swed.ReadInt(localPlayer.address, offsets.m_iIDEntIndex);


                                        
            if (localPlayer.entIndex != -1) 
            {
                IntPtr listEntry = swed.ReadPointer(entityList, 0x8 * ((localPlayer.entIndex & 0x7FFF) >> 9) + 0x10);
                IntPtr currentPawn = swed.ReadPointer(listEntry, 0x78 * (localPlayer.entIndex & 0x1FF));

                int entityTeam = swed.ReadInt(currentPawn + offsets.m_iTeamNum);

                if(entityTeam != localPlayer.teamNum || renderer.enableTriggerTeam)
                {

                        if (localPlayer.entIndex > 0)
                        {
                            Thread.Sleep((int)renderer.triggerDelay);
                            swed.WriteInt(clientDll + offsets.dwForceAttack, 65537);
                            Thread.Sleep(1);
                            swed.WriteInt(clientDll + offsets.dwForceAttack, 256);

                        }

                }
            }

            }

        }









        if (renderer.enableNoFlash)
        {
            float flashDuration = swed.ReadFloat(localPlayer.address, offsets.m_flFlashBangTime);
            if (flashDuration > 0)
            {

                swed.WriteFloat(localPlayer.address, offsets.m_flFlashBangTime, 0f);

            }
        }


        //read gameRUles






        if (renderer.enableRCS)
        {
            Vector3 viewAngles = swed.ReadVec(clientDll + offsets.dwViewAngles);
            int shotsFired = swed.ReadInt(localPlayer.address, offsets.m_iShotsFired);
            Vector3 aimPunch = swed.ReadVec(localPlayer.address, offsets.m_aimPunchAngle);

            if (shotsFired > 1)
            {
                Vector3 punchAngle = aimPunch * 2 * renderer.compensationFactor;
                Vector3 newAngle = viewAngles + oPunch - punchAngle;
                newAngle = Normalize(newAngle);
                swed.WriteVec(clientDll + offsets.dwViewAngles, newAngle);
                Thread.Sleep(1);

            }

            oPunch = aimPunch * 2 * renderer.compensationFactor; // Retain some of the recoil for the next shot
        }
   

    }

    
}

void BombTimerLogic()
{
    bool bombPlanted = false;

    IntPtr gameRules = swed.ReadPointer(clientDll, offsets.dwGameRules);

    while (true)
    {
        if (gameRules == IntPtr.Zero)
            continue;
        if (renderer.enableBombTimer)
        {
            bombPlanted = swed.ReadBool(gameRules, offsets.m_bBombPlanted);
            if (bombPlanted)
            {
                for (int i = 0; i < 40; i++)
                {
                    bombPlanted = swed.ReadBool(gameRules, offsets.m_bBombPlanted);
                    if (!bombPlanted)//
                    {
                        break;
                    }
                    //calculate time left
                    int timeLeft = 40 - i;
                    renderer.timeLeft = timeLeft;
                    renderer.bombPlanted = true;
                    Thread.Sleep(1000);
                }
            }
            else
            {
                renderer.timeLeft = -1;
                renderer.bombPlanted = false;
            }
        }

    }

}