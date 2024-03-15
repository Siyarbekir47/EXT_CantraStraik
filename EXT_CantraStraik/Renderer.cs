using ClickableTransparentOverlay;
using ImGuiNET;
using Swed64;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid.OpenGLBinding;
using Vortice.Direct3D11;

namespace EXT_CantraStraik
{
    public class Renderer : Overlay
    {
        //imports and structs

        [DllImport("user32.dll")]

        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool GetAsyncKeyState(int vKey);

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


        ImDrawListPtr drawList;





        Vector4 tColor = new Vector4(0, 0, 1, 1);
        Vector4 eColor = new Vector4(1, 0, 0, 1);
        Vector4 hBarColor = new Vector4(0, 1, 0, 1);
        Vector4 hTextColor = new Vector4(0, 0, 0, 1);
        Vector4 whiteColor = new Vector4(1, 1, 1, 1);
        Vector4 yellowColor = new Vector4(1, 1, 0, 1);
        Vector4 greenColor = new Vector4(0, 1, 0, 1);
        Vector4 redColor = new Vector4(1, 0, 0, 1);


        //screen variables, later update

        Vector2 windowLocation = new Vector2(0, 0);
        public Vector2 windowSize = new Vector2(2560, 1440);
        Vector2 lineOrigin = new Vector2(2560 / 2, 1440);
        Vector2 windowCenter = new Vector2(2560 / 2, 1440 / 2);

        public ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        public Entity localPlayer = new Entity();
        private readonly object entityLock = new object();
        public bool bombPlanted = false;
        public int timeLeft = -1;
        public System.Timers.Timer rcsTimer;
        public Vector3 lastAimPunch = Vector3.Zero;


        //imgui checkbox

        public bool showMenu = true;

        bool enableESP = true;

        bool enableTeamLine = false;
        bool enableTeamBox = false;
        bool enableTeamDot = false;
        bool enableTeamHealthBar = false;
        bool enableTeamHealthText = false;
        bool enableTeamDistance = false;
        bool enableTeamName = false;

        bool enableEnemyLine = true;
        bool enableEnemyBox = false;
        bool enableEnemyDot = true;
        bool enableEnemyHealthBar = true;
        bool enableEnemyHealthText = false;
        bool enableEnemyDistance = false;
        bool enableEnemyName = false;
        //misc
        public bool enableNoFlash = false;
        public bool enableGlow = true;
        public bool enableBombTimer = true;
        public bool enableRadar = true;
        public bool enableCrosshair = true;

        //aim
        public bool enableTriggerBot = false;
        public bool enableTriggerTeam = false;
        public float triggerDelay = 50f;

        public bool enableRCS = false;
        public float compensationFactor = 0.35f;
        public Renderer()
        {
        }

        bool isDrawOverlay = true;
        protected override void Render()
        {
            //only render stuff
            DrawOverlay();

            drawList = ImGui.GetWindowDrawList();


            if (showMenu)
            {
                DrawMenu();
            }
            if (enableESP)
            {
                ESP();
            }
            if(enableCrosshair)
            {
                DrawCrosshair();
            }
            if(enableBombTimer)
            {
                DrawBombTimerWindow();
            }

            ImGui.End();
        }


        void DrawMenu()
        {
            ImGui.Begin("@Siyarbekir's Metin3 Menu");
            ImGui.GetIO().FontGlobalScale = 1.5f;
            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem("Visuals"))
                {
                    ImGui.Checkbox("Enable ESP", ref enableESP);
                    ImGui.Separator();
                    ImGui.Text("Team");
                    ImGui.Checkbox("Enable Team Line", ref enableTeamLine);
                    ImGui.Checkbox("Enable Team Box", ref enableTeamBox);
                    ImGui.Checkbox("Enable Team Dot", ref enableTeamDot);
                    ImGui.Checkbox("Enable Team Health Bar", ref enableTeamHealthBar);
                    ImGui.Checkbox("Enable Team Health Text", ref enableTeamHealthText);
                    ImGui.Checkbox("Eneable Team Name", ref enableTeamName);
                    ImGui.Checkbox("Enable Team Distance", ref enableTeamDistance);
                    ImGui.Separator();
                    ImGui.Text("Enemy");
                    ImGui.Checkbox("Enable Enemy Line", ref enableEnemyLine);
                    ImGui.Checkbox("Enable Enemy Box", ref enableEnemyBox);
                    ImGui.Checkbox("Enable Enemy Dot", ref enableEnemyDot);
                    ImGui.Checkbox("Enable Enemy Health Bar", ref enableEnemyHealthBar);
                    ImGui.Checkbox("Enable Enemy Health Text", ref enableEnemyHealthText);
                    ImGui.Checkbox("Enable Enemy Name", ref enableEnemyName);
                    ImGui.Checkbox("Enable Enemy Distance", ref enableEnemyDistance);


                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Aim"))
                {
                    ImGui.Checkbox("Enable TriggerBot", ref enableTriggerBot);
                    ImGui.Checkbox("    ->Enable Trigger Team", ref enableTriggerTeam);
                    ImGui.SliderFloat("Trigger Delay", ref triggerDelay, 15f, 300f);
                    ImGui.Separator();
                    ImGui.Checkbox("Enable RCS(only use one option below)--NOT WORKING ATM---", ref enableRCS);
                    ImGui.SliderFloat("Compensation Factor", ref compensationFactor, 0.0f, 1.0f);
                    ImGui.EndTabItem();


                }
                if (ImGui.BeginTabItem("Misc"))
                {
                    ImGui.Text("Misc");
                    ImGui.Checkbox("Enable noFlash", ref enableNoFlash);
                    ImGui.Checkbox("Enable Bomb Timer", ref enableBombTimer);
                    ImGui.Separator();
                    ImGui.Checkbox("Enable Radar", ref enableRadar);
                    ImGui.Checkbox("Enable Glow", ref enableGlow);
                    ImGui.Separator();
                    ImGui.Checkbox("Enable Crosshair", ref enableCrosshair);
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Colors"))
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

        private void ESP()
        {
            foreach (var entity in entities)
            {
                
                if (EntityOnScreen(entity))
                {
                    if (entity.teamNum != localPlayer.teamNum)
                    {
                        if (enableEnemyBox) DrawBox(entity);
                        if (enableEnemyLine) DrawLine(entity);
                        if (enableEnemyDot) DrawDot(entity);
                        if (enableEnemyHealthBar) DrawHealthBar(entity);
                        if (enableEnemyHealthText) DrawHealthText(entity);
                        if (enableEnemyName) DrawName(entity);
                    }
                    else  // Teammates
                    {
                        if(entity.playerName != localPlayer.playerName)
                        {
                            if (enableTeamBox) DrawBox(entity);
                            if (enableTeamLine) DrawLine(entity);
                            if (enableTeamDot) DrawDot(entity);
                            if (enableTeamHealthBar) DrawHealthBar(entity);
                            if (enableTeamHealthText) DrawHealthText(entity);
                            if (enableTeamName) DrawName(entity);
                        }

                    }
                }
            }
        }

        private void DrawName(Entity entity)
        {
            Vector2 textLocation = new Vector2(entity.viewPosition2D.X - 40, entity.viewPosition2D.Y - 50);
            drawList.AddText(textLocation, ImGui.GetColorU32(whiteColor), entity.playerName);
        }
        private void DrawLine(Entity entity)
        {
            Vector4 lineColor = localPlayer.teamNum == entity.teamNum ? tColor : eColor;
            drawList.AddLine(lineOrigin, entity.position2D, ImGui.GetColorU32(lineColor), 3);
        }
        void DrawCrosshair()
        {
            Vector2 crosshairSize = new Vector2(10, 10);
            Vector2 crosshairCenter = new Vector2(windowSize.X / 2, windowSize.Y / 2);
            Vector2 crosshairTop = new Vector2(crosshairCenter.X, crosshairCenter.Y - crosshairSize.Y);
            Vector2 crosshairBottom = new Vector2(crosshairCenter.X, crosshairCenter.Y + crosshairSize.Y);
            Vector2 crosshairLeft = new Vector2(crosshairCenter.X - crosshairSize.X, crosshairCenter.Y);
            Vector2 crosshairRight = new Vector2(crosshairCenter.X + crosshairSize.X, crosshairCenter.Y);
            drawList.AddLine(crosshairTop, crosshairBottom, ImGui.GetColorU32(whiteColor), 2);
            drawList.AddLine(crosshairLeft, crosshairRight, ImGui.GetColorU32(whiteColor), 2);
        }

        private void DrawDot(Entity entity)
        {

            Vector4 dotColor = new Vector4(0, 0, 0, 1);

            if (entity.health <= 40)
            {
                dotColor = redColor;
            }
            else if (entity.health > 40 && entity.health <= 70)
            {
                dotColor = yellowColor;
            }
            else
            {
                dotColor = greenColor;
            }

            drawList.AddCircleFilled(entity.position2D, 8, ImGui.GetColorU32(dotColor));




        }
        private void DrawHealthText(Entity entity)
        {
            Vector2 textLocation = new Vector2(entity.viewPosition2D.X - 40, entity.viewPosition2D.Y - 70);


                if (entity.health <= 40)
                {
                    drawList.AddText(textLocation, ImGui.GetColorU32(eColor), entity.health.ToString());

                }
                else if (entity.health > 40 && entity.health <= 70)
                {
                    drawList.AddText(textLocation, ImGui.GetColorU32(yellowColor), entity.health.ToString());
                }
                else
                {
                    drawList.AddText(textLocation, ImGui.GetColorU32(greenColor), entity.health.ToString());
                }
            
        }
        private void DrawHealthBar(Entity entity)
        {

            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;
            //get box location
            float boxLeft = entity.viewPosition2D.X - entityHeight / 3;
            float boxRight = entity.position2D.X + entityHeight / 3;
            //calculate health width
            float barPerecentWidth = 0.15f;
            float barPixelWidth = barPerecentWidth * (boxRight - boxLeft);

            //calculate bar height after health
            float barHeight = entityHeight * (entity.health / 100f);

            //calculate bar rectangle, two vectors
            Vector2 barTop = new Vector2(boxLeft - barPixelWidth, entity.position2D.Y - barHeight);
            Vector2 barBottom = new Vector2(boxLeft, entity.position2D.Y);

            if (entity.health <= 40)
            {
                hBarColor = redColor;
            }
            else if (entity.health > 40 && entity.health <= 70)
            {
                hBarColor = yellowColor;
            }
            else
            {
                hBarColor = greenColor;
            }
            drawList.AddRectFilled(barTop, barBottom, ImGui.GetColorU32(hBarColor));



        }
        private void DrawBox(Entity entity)
        {
            //calculate box height
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, (entity.viewPosition2D.Y));

            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);

            Vector4 boxColor = localPlayer.teamNum == entity.teamNum ? tColor : eColor;

            drawList.AddRect(rectTop, rectBottom, ImGui.GetColorU32(boxColor));

        }

        void DrawBombTimerWindow()
        {

            if (!enableBombTimer)
            {
                return; // Skip rendering if the checkbox is not checked
            }
            ImGui.SetNextWindowSize(new Vector2(200, 100));
            ImGui.Begin("Bomb Timer", ref enableBombTimer);
            if(bombPlanted)
            {
                ImGui.TextColored(eColor, "Bomb Planted");
                ImGui.TextColored(eColor, "Time Left: " + timeLeft);
                
            }
            else
            {
                ImGui.TextColored(hBarColor, "No Bomb Planted");
            }



            ImGui.End();
        }


        //check position
        bool EntityOnScreen(Entity entity)
        {
            if (entity.position2D.X > 0 && entity.position2D.X < windowSize.X && entity.position2D.Y > 0 && entity.position2D.Y < windowSize.Y) 
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public Entity GetLocalPlayer() //get local player
        {
            lock (entityLock)
            {
                return localPlayer;
            }
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }
        internal void UpdateEntities(List<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }




        void DrawOverlay()
        {
            ImGui.SetNextWindowSize(windowSize);
            ImGui.SetNextWindowPos(windowLocation);
            ImGui.Begin("Metin3", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                | ImGuiWindowFlags.NoTitleBar

                );
        }


    }
}
