using ClickableTransparentOverlay;
using ImGuiNET;
using Swed64;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace EXT_CantraStraik
{
    class Program : Overlay
    {

        //imports and structs

        [DllImport("user32.dll")]

        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        static extern bool GetAsyncKeyState(int vKey); //handle hotkey

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public RECT GetWindowRect(IntPtr hWnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hWnd, out rect);
            return rect;
        }

        Swed swed = new Swed("cs2");

        Offsets offsets = new Offsets();
        ImDrawListPtr drawList;




        //List<Entity> entities = new List<Entity>();

        List<Entity> enemyTeam = new List<Entity>();
        List<Entity> playerTeam = new List<Entity>();




        IntPtr clientDll;



        Vector4 tColor = new Vector4(0, 0, 1, 1);
        Vector4 eColor = new Vector4(1, 0, 0, 1);
        Vector4 hBarColor = new Vector4(0, 1, 0, 1);
        Vector4 hTextColor = new Vector4(0, 0, 0, 1);


        //screen variables, later update

        Vector2 windowLocation = new Vector2(0, 0);
        Vector2 windowSize = new Vector2(2560, 1440);
        Vector2 lineOrigin = new Vector2(2560 / 2, 1440);
        Vector2 windowCenter = new Vector2(2560 / 2, 1440 / 2);

        public ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        //imgui checkbox

        bool showMenu = true;

        bool enableESP = true;

        bool enableTeamLine = true;
        bool enableTeamBox = true;
        bool enableTeamDot = false;
        bool enableTeamHealthBar = true;
        bool enableTeamDistance = true;

        bool enableEnemyLine = true;
        bool enableEnemyBox = true;
        bool enableEnemyDot = false;
        bool enableEnemyHealthBar = true;
        bool enableEnemyDistance = true;

        //misc
        bool enableNoFlash = false;
        bool enableBunnyHop = false;
        bool enableGlow = false;

        //aim
        bool enableTriggerBot = false;
        bool enableTriggerTeam = false;
        float triggerDelay = 50f;


        protected override void Render()
        {
            throw new NotImplementedException();
            //only render stuff
            DrawMenu();
            DrawOverlay();
            Esp();
            ImGui.End();
        }



        public Entity GetLocalPlayer() //get local player
        {
            lock(entityLock)
            {
               return localPlayer;
            }
        }


        void Esp()
        {
            drawList = ImGui.GetWindowDrawList(); // get overlay

            if(enableESP)
            {
                try
                {
                    foreach(var entity in entities)
                    {
                        if(entity.address == IntPtr.Zero)
                            continue;
                        if(entity.teamNum==localPlayer.teamNum)
                        {
                            DrawVisuals(entity, tColor, enableTeamLine, enableTeamBox, enableTeamDot, enableTeamHealthBar, enableTeamDistance);
                        }
                        else
                        {
                            DrawVisuals(entity, eColor, enableEnemyLine, enableEnemyBox, enableEnemyDot, enableEnemyHealthBar, enableEnemyDistance);
                        }
                    }
                }
                catch { }
            }
        }
        void DrawVisuals(Entity entity, Vector4 color, bool line, bool box, bool dot, bool healthBar, bool distance)
        {
            if(IsPixelInScreen(entity.originScreenPosition))
            {
                //convert our colors to uints

                uint uintColor = ImGui.ColorConvertFloat4ToU32(color);
                uint uintHBarColor = ImGui.ColorConvertFloat4ToU32(hBarColor);
                uint uintHTextColor = ImGui.ColorConvertFloat4ToU32(hTextColor);

                Vector2 boxWidth = new Vector2((entity.originScreenPosition.Y - entity.absScreenPosition.Y) / 2 , 0f); //width of the box
                Vector2 boxStart = Vector2.Subtract(entity.absScreenPosition, boxWidth); //start of the box
                Vector2 boxEnd = Vector2.Add(entity.originScreenPosition, boxWidth); //end of the box

                //calculate healthbar stuff

                float barPercentage = entity.health / 100f; //
                Vector2 barHeight= new Vector2(0, barPercentage*(entity.originScreenPosition.Y - entity.absScreenPosition.Y));
                Vector2 barStart = Vector2.Subtract(Vector2.Subtract(entity.originScreenPosition, boxWidth), barHeight);
                Vector2 barEnd = Vector2.Subtract(entity.originScreenPosition, Vector2.Add(boxWidth, new Vector2(-4, 0)));
            
            

                if(line)
                {
                    drawList.AddLine(lineOrigin, entity.originScreenPosition, uintColor, 3);
                }
                if(box)
                {
                    drawList.AddRect(boxStart, boxEnd, uintColor, 5);
                }
                if(dot)
                {
                    drawList.AddCircleFilled(entity.originScreenPosition, 5, uintColor);
                }
                if(healthBar)
                {
                    drawList.AddText(entity.originScreenPosition, uintHTextColor, $"HP: {entity.health}");
                    drawList.AddRectFilled(barStart, barEnd, uintHBarColor);
                }

            }
        }


        bool IsPixelInScreen(Vector2 pixel)
        {
            return pixel.X > windowLocation.X && pixel.X < windowLocation.X + windowSize.X && pixel.Y > windowLocation.Y && pixel.Y < windowSize.Y + windowLocation.Y; //check if the pixel is in the screen
        }




        void DrawMenu()
        {
            ImGui.Begin("@Siyarbekir's Metin3 Menu");
         
            if(ImGui.BeginTabBar("Tabs"))
            {
                if(ImGui.BeginTabItem("General"))
                {
                    ImGui.Checkbox("Enable ESP", ref enableESP);
                    ImGui.Separator();
                    ImGui.Text("Team");
                    ImGui.Checkbox("Enable Team Line", ref enableTeamLine);
                    ImGui.Checkbox("Enable Team Box", ref enableTeamBox);
                    ImGui.Checkbox("Enable Team Dot", ref enableTeamDot);
                    ImGui.Checkbox("Enable Team Health Bar", ref enableTeamHealthBar);
                    ImGui.Checkbox("Enable Team Distance", ref enableTeamDistance);
                    ImGui.Separator();
                    ImGui.Text("Enemy");
                    ImGui.Checkbox("Enable Enemy Line", ref enableEnemyLine);
                    ImGui.Checkbox("Enable Enemy Box", ref enableEnemyBox);
                    ImGui.Checkbox("Enable Enemy Dot", ref enableEnemyDot);
                    ImGui.Checkbox("Enable Enemy Health Bar", ref enableEnemyHealthBar);
                    ImGui.Checkbox("Enable Enemy Distance", ref enableEnemyDistance);


                    ImGui.EndTabItem();
                }
                if(ImGui.BeginTabItem("Aim"))
                {
                    ImGui.Checkbox("Enable TriggerBot", ref enableTriggerBot);
                    ImGui.Checkbox("    ->Enable Trigger Team", ref enableTriggerTeam);
                    ImGui.SliderFloat("Trigger Delay", ref triggerDelay, 15f, 300f);

                }
                if(ImGui.BeginTabItem("Misc"))
                {
                    ImGui.Text("Misc");
                    ImGui.Checkbox("Enable noFlash", ref enableNoFlash);
                    ImGui.Separator();
                    ImGui.Checkbox("Enable Glow -NOT WORKING ATM-", ref enableGlow);
                    ImGui.EndTabItem();
                }
                if(ImGui.BeginTabItem("Colors"))
                {
                    ImGui.ColorEdit4("Team Color", ref tColor);
                    ImGui.ColorEdit4("Enemy Color", ref eColor);
                    ImGui.ColorEdit4("Health Bar Color", ref hBarColor);
                    ImGui.ColorEdit4("Health Text Color", ref hTextColor);
                    ImGui.EndTabItem();
                }
            }

            ImGui.End();
        }

        void DrawOverlay()
        {
            ImGui.SetNextWindowSize(windowSize);
            ImGui.SetNextWindowPos(windowLocation);
            ImGui.Begin("Overlay",  ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }

        ViewMatrix ReadMatrix(IntPtr matrixAddress)
        {
            var viewMatrix = new ViewMatrix();
            var floatMatrix = swed.ReadMatrix(matrixAddress);


            viewMatrix.m11 = floatMatrix[0];
            viewMatrix.m12 = floatMatrix[1];
            viewMatrix.m13 = floatMatrix[2];
            viewMatrix.m14 = floatMatrix[3];

            viewMatrix.m21 = floatMatrix[4];
            viewMatrix.m22 = floatMatrix[5];
            viewMatrix.m23 = floatMatrix[6];
            viewMatrix.m24 = floatMatrix[7];

            viewMatrix.m31 = floatMatrix[8];
            viewMatrix.m32 = floatMatrix[9];
            viewMatrix.m33 = floatMatrix[10];
            viewMatrix.m34 = floatMatrix[11];

            viewMatrix.m41 = floatMatrix[12];
            viewMatrix.m42 = floatMatrix[13];
            viewMatrix.m43 = floatMatrix[14];
            viewMatrix.m44 = floatMatrix[15];

            return viewMatrix;
        }

        Vector2 WorldToScreen(ViewMatrix matrix, Vector3 pos, int width, int height )
        {

            Vector2 screenCoordinates = new Vector2();


            float screenW = (matrix.m41 * pos.X) + (matrix.m42 * pos.Y) + (matrix.m43 * pos.Z) + matrix.m44;

            if(screenW > 0.001f)
            {
                float screenX = (matrix.m11 * pos.X) + (matrix.m12 * pos.Y) + (matrix.m13 * pos.Z) + matrix.m14;

                float screenY = (matrix.m21 * pos.X) + (matrix.m22 * pos.Y) + (matrix.m23 * pos.Z) + matrix.m24;


                float camX = width / 2;
                float camY = height / 2;

                //perform the perspective divide
                float x = camX + (camX * screenX / screenW);
                float y = camY - (camY * screenY / screenW);

                screenCoordinates.X = x;
                screenCoordinates.Y = y;

                return screenCoordinates;

            }
            else
            {
                return new Vector2(-99,-99);
            }
        }
        void MainLogic()
        {


            var window = GetWindowRect(swed.GetProcess().MainWindowHandle);
            windowLocation = new Vector2(window.Left, window.Top);
            windowSize = Vector2.Subtract(new Vector2(window.Right, window.Bottom), windowLocation);
            lineOrigin = new Vector2(windowLocation.X + windowSize.X/2, window.Bottom);
            windowCenter = new Vector2(lineOrigin.X, window.Bottom - windowSize.Y/2);

            clientDll = swed.GetModuleBase("client.dll");

            while (true)
            {

                ReloadEntities();
                if(enableNoFlash)
                {
                    float flashDuration = swed.ReadFloat(localPlayer.address, offsets.flasbangTime);
                    if (flashDuration > 0)
                    {

                        swed.WriteFloat(localPlayer.address, offsets.flasbangTime, 0f);

                    }
                }

                
                if(enableGlow)
                {
                    
                }

                Thread.Sleep(3);



            }
        }
        void ReloadEntities()
        {
            entities.Clear();
            playerTeam.Clear();
            enemyTeam.Clear();

            localPlayer.address = swed.ReadPointer(clientDll, offsets.localPlayer);
            UpdateEntity(localPlayer);

            UpdateEntities();
        }
        void UpdateEntities()
        {
            for (int i = 0; i < 64; i++)
            {
                IntPtr entityAddress = swed.ReadPointer(clientDll, offsets.entityList + i * 0x08);

                if (entityAddress == IntPtr.Zero)
                    continue;


                Entity entity = new Entity();
                entity.address = entityAddress;


                UpdateEntity(entity);

                if(entity.health < 1 || entity.health > 100)
                    continue;

                
                if(!entities.Any(element => element.origin.X == entity.origin.X))
                {
                    entities.Add(entity);

                    if(entity.teamNum == localPlayer.teamNum)
                    {
                        playerTeam.Add(entity);
                    }
                    else
                    {
                        
                        enemyTeam.Add(entity);
                    }
                }



            }
        }
        void UpdateEntity(Entity entity)
        {
            entity.health = swed.ReadInt(entity.address, offsets.health);
            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.teamNum = swed.ReadInt(entity.address, offsets.teamNum);



            //3d

            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.viewOffset = new Vector3(0, 0, 65);
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);

            //2d

            var currentViewmatrix = ReadMatrix(clientDll + offsets.viewMatrix);
            entity.originScreenPosition = Vector2.Add(WorldToScreen(currentViewmatrix, entity.origin, (int)windowSize.X, (int)windowSize.Y), windowLocation);
            entity.absScreenPosition = Vector2.Add(WorldToScreen(currentViewmatrix, entity.abs, (int)windowSize.X, (int)windowSize.Y), windowLocation);



        }

        void TriggerBotLogic()
        {
            while(true)
            {
                if (enableTriggerBot)
                {

                    int entIndex = swed.ReadInt(localPlayer.address, offsets.iIDEntIndex);
                    if (entIndex < 0)
                        continue;

                    if (GetAsyncKeyState(0x6))
                    {
                        if (entIndex > 0)
                        {
                            Thread.Sleep((int)triggerDelay);
                            swed.WriteInt(clientDll + offsets.forceAttack, 65537);
                            Thread.Sleep(1);
                            swed.WriteInt(clientDll + offsets.forceAttack, 256);

                        }
                    }
                }
            }
            
        }
        static void Main(string[] args)
        {
            //logic here

            Program program = new Program();
            program.Start().Wait();

            Thread mainLogicThread = new Thread(program.MainLogic) { IsBackground = true };
            mainLogicThread.Start();

            
            Thread triggerBotLogicThread = new Thread(program.TriggerBotLogic) { IsBackground = true };
            triggerBotLogicThread.Start();
        }
    }
}