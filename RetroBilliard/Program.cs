using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;

namespace RetroBilliard
{
    enum GameState { Menu, NewGameModeSelect, InGame, About, Setting, Exit }
    enum PlayMode { PvP, PvC }

    class Program
    {
        const int ScreenWidth = 1024;
        const int ScreenHeight = 768;

        static GameState currentState = GameState.Menu;
        static PlayMode currentMode = PlayMode.PvP;
        static Camera3D camera;
        static List<Ball> balls = new List<Ball>();
        static Vector3[] pockets;
        
        static float cueAngle = 0.0f;
        static float cuePower = 0.0f;
        static bool isAiming = true;
        static bool isMoving = false;
        
        const float TableSurfaceY = 0.0f;
        const float BallRadius = 0.4f;
        const float BallY = TableSurfaceY + BallRadius;

        static float cameraDistance = 18.0f;
        static float cameraAngle = 0.5f; 
        static float cameraHeight = 12.0f;

        static float aiTimer = 0;
        static bool isAiTurn = false;

        static void Main(string[] args)
        {
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "RetroBilliard - by Jacky the Code Bender");
            
            // PENTING: Matikan fungsi ESC untuk menutup aplikasi secara otomatis
            Raylib.SetExitKey(KeyboardKey.Null); 
            
            Raylib.ToggleFullscreen();
            Raylib.InitAudioDevice();
            Raylib.SetTargetFPS(60);

            camera = new Camera3D();
            camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
            camera.FovY = 45.0f;
            camera.Projection = CameraProjection.Perspective;

            pockets = new Vector3[] {
                new Vector3(-4.8f, TableSurfaceY, -4.8f), new Vector3(4.0f, TableSurfaceY, -4.8f), new Vector3(12.8f, TableSurfaceY, -4.8f),
                new Vector3(-4.8f, TableSurfaceY, 4.8f), new Vector3(4.0f, TableSurfaceY, 4.8f), new Vector3(12.8f, TableSurfaceY, 4.8f)
            };

            ResetGame();

            while (!Raylib.WindowShouldClose() && currentState != GameState.Exit)
            {
                Update();
                Draw();
            }

            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
        }

        static void ResetGame()
        {
            balls.Clear();
            balls.Add(new Ball(new Vector3(-1, BallY, 0), Color.White, 0));
            
            int id = 1;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    float x = 4.0f + (i * 0.82f);
                    float z = (j * 0.85f) - (i * 0.425f);
                    Color col = (id % 2 == 0) ? Color.Red : Color.Yellow;
                    if (id == 8) col = Color.Black;
                    balls.Add(new Ball(new Vector3(x, BallY, z), col, id++));
                }
            }
            isAiming = true;
            isMoving = false;
            isAiTurn = false;
        }

        static void Update()
        {
            switch (currentState)
            {
                case GameState.Menu:
                    if (Raylib.IsKeyPressed(KeyboardKey.N)) currentState = GameState.NewGameModeSelect;
                    if (Raylib.IsKeyPressed(KeyboardKey.A)) currentState = GameState.About;
                    if (Raylib.IsKeyPressed(KeyboardKey.S)) currentState = GameState.Setting;
                    if (Raylib.IsKeyPressed(KeyboardKey.E)) currentState = GameState.Exit;
                    // Jika di menu tekan ESC, kita bisa abaikan atau beri pilihan exit. Di sini kita biarkan saja.
                    break;
                case GameState.NewGameModeSelect:
                    if (Raylib.IsKeyPressed(KeyboardKey.One)) { currentMode = PlayMode.PvP; currentState = GameState.InGame; ResetGame(); }
                    if (Raylib.IsKeyPressed(KeyboardKey.Two)) { currentMode = PlayMode.PvC; currentState = GameState.InGame; ResetGame(); }
                    if (Raylib.IsKeyPressed(KeyboardKey.Escape)) currentState = GameState.Menu;
                    break;
                case GameState.InGame:
                    UpdateCamera();
                    UpdateInGame();
                    break;
                default: // About, Setting
                    if (Raylib.IsKeyPressed(KeyboardKey.Escape)) currentState = GameState.Menu;
                    break;
            }
        }

        static void UpdateCamera()
        {
            if (Raylib.IsKeyDown(KeyboardKey.A)) cameraAngle -= 0.03f;
            if (Raylib.IsKeyDown(KeyboardKey.D)) cameraAngle += 0.03f;
            if (Raylib.IsKeyDown(KeyboardKey.W)) cameraDistance -= 0.2f;
            if (Raylib.IsKeyDown(KeyboardKey.S)) cameraDistance += 0.2f;
            
            cameraDistance = Math.Clamp(cameraDistance, 5.0f, 30.0f);

            camera.Position.X = 4.0f + MathF.Cos(cameraAngle) * cameraDistance;
            camera.Position.Z = MathF.Sin(cameraAngle) * cameraDistance;
            camera.Position.Y = cameraHeight;
            camera.Target = new Vector3(4.0f, 0.0f, 0.0f);
        }

        static void UpdateInGame()
        {
            isMoving = false;
            foreach (var b in balls) if (b.Velocity.Length() > 0.01f) isMoving = true;

            if (!isMoving)
            {
                if (currentMode == PlayMode.PvC && isAiTurn)
                {
                    aiTimer += Raylib.GetFrameTime();
                    if (aiTimer > 1.2f)
                    {
                        if (balls.Count > 1)
                        {
                            Vector3 diff = balls[1].Position - balls[0].Position;
                            float targetAngle = MathF.Atan2(diff.Z, diff.X);
                            Vector3 hitDir = new Vector3(MathF.Cos(targetAngle), 0, MathF.Sin(targetAngle));
                            balls[0].Velocity = hitDir * 1.9f;
                        }
                        isAiTurn = false;
                        aiTimer = 0;
                    }
                }
                else
                {
                    isAiming = true;
                    if (Raylib.IsKeyDown(KeyboardKey.Left)) cueAngle += 0.04f;
                    if (Raylib.IsKeyDown(KeyboardKey.Right)) cueAngle -= 0.04f;
                    
                    if (Raylib.IsKeyDown(KeyboardKey.Space))
                    {
                        cuePower += 0.5f;
                        if (cuePower > 25.0f) cuePower = 25.0f;
                    }
                    else if (cuePower > 0)
                    {
                        Vector3 hitDir = new Vector3(MathF.Cos(cueAngle), 0, MathF.Sin(cueAngle));
                        balls[0].Velocity = hitDir * cuePower * 0.12f;
                        cuePower = 0;
                        isAiming = false;
                        if (currentMode == PlayMode.PvC) isAiTurn = true;
                    }
                }
            }

            for (int i = balls.Count - 1; i >= 0; i--)
            {
                balls[i].Update();
                foreach (var pocket in pockets)
                {
                    if (Vector2.Distance(new Vector2(balls[i].Position.X, balls[i].Position.Z), new Vector2(pocket.X, pocket.Z)) < 0.7f)
                    {
                        if (balls[i].Id == 0) 
                        {
                            balls[i].Position = new Vector3(-1, BallY, 0);
                            balls[i].Velocity = Vector3.Zero;
                        }
                        else
                        {
                            balls.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            
            for (int i = 0; i < balls.Count; i++)
            {
                for (int j = i + 1; j < balls.Count; j++)
                {
                    balls[i].CheckCollision(balls[j]);
                }
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.R)) ResetGame();
            
            // ESC sekarang kembali ke menu, bukan keluar app
            if (Raylib.IsKeyPressed(KeyboardKey.Escape)) currentState = GameState.Menu;
        }

        static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(15, 15, 30, 255));

            int sw = Raylib.GetScreenWidth();
            int sh = Raylib.GetScreenHeight();

            if (currentState == GameState.InGame)
            {
                Raylib.BeginMode3D(camera);
                Raylib.DrawCube(new Vector3(4, -0.5f, 0), 18.0f, 1.0f, 10.0f, Color.DarkGreen);
                Raylib.DrawCubeWires(new Vector3(4, -0.5f, 0), 18.2f, 1.1f, 10.2f, Color.Brown);

                foreach (var p in pockets)
                {
                    Raylib.DrawCylinder(p, 0.6f, 0.6f, 0.05f, 16, Color.Black);
                }

                foreach (var b in balls) Raylib.DrawSphere(b.Position, BallRadius, b.Color);

                if (isAiming && !isMoving && !isAiTurn)
                {
                    Vector3 stickDir = new Vector3(MathF.Cos(cueAngle), 0, MathF.Sin(cueAngle));
                    Raylib.DrawLine3D(balls[0].Position - stickDir * 0.5f, balls[0].Position - stickDir * 6.0f, Color.Gold);
                }
                Raylib.EndMode3D();

                foreach (var b in balls)
                {
                    if (b.Id == 0) continue;
                    Vector2 screenPos = Raylib.GetWorldToScreen(b.Position, camera);
                    if (screenPos.X > 0 && screenPos.X < sw && screenPos.Y > 0 && screenPos.Y < sh)
                    {
                        string txt = b.Id.ToString();
                        int fontSize = 12;
                        int textWidth = Raylib.MeasureText(txt, fontSize);
                        Raylib.DrawCircle((int)screenPos.X, (int)screenPos.Y, 8, Color.White);
                        Raylib.DrawText(txt, (int)screenPos.X - textWidth/2, (int)screenPos.Y - 5, fontSize, Color.Black);
                    }
                }

                Raylib.DrawRectangle(0, 0, sw, 40, Raylib.Fade(Color.Black, 0.6f));
                Raylib.DrawText($"MODE: {currentMode} | BALLS LEFT: {balls.Count - 1}", 20, 10, 20, Color.White);
                
                if (isAiming && !isMoving && !isAiTurn) 
                {
                    int barWidth = 250;
                    int barHeight = 25;
                    int barPosX = 20;
                    int barPosY = 60;
                    Raylib.DrawRectangle(barPosX, barPosY, barWidth, barHeight, Color.DarkGray);
                    Raylib.DrawRectangle(barPosX, barPosY, (int)(cuePower * (barWidth / 25.0f)), barHeight, Color.Red);
                    Raylib.DrawRectangleLines(barPosX, barPosY, barWidth, barHeight, Color.White);
                    Raylib.DrawText("POWER", barPosX + 5, barPosY + 5, 15, Color.White);
                }

                if (isAiTurn && !isMoving) Raylib.DrawText("COMPUTER THINKING...", sw/2 - 120, 60, 25, Color.Red);
                
                Raylib.DrawRectangle(0, sh - 50, sw, 50, Raylib.Fade(Color.Black, 0.6f));
                Raylib.DrawText("WASD: Camera | ARROWS: Aim | SPACE (Hold): Set Power | R: Reset | ESC: Menu", 20, sh - 35, 18, Color.Yellow);
            }
            else
            {
                DrawUI();
            }

            Raylib.EndDrawing();
        }

        static void DrawUI()
        {
            int sw = Raylib.GetScreenWidth();
            int sh = Raylib.GetScreenHeight();
            switch (currentState)
            {
                case GameState.Menu:
                    Raylib.DrawText("RETRO BILLIARD PRO", sw / 2 - 250, sh/4, 60, Color.Yellow);
                    Raylib.DrawText("[N] New Game", sw / 2 - 100, sh/2 - 40, 30, Color.White);
                    Raylib.DrawText("[A] About", sw / 2 - 100, sh/2 + 10, 30, Color.White);
                    Raylib.DrawText("[S] Setting", sw / 2 - 100, sh/2 + 60, 30, Color.White);
                    Raylib.DrawText("[E] Exit", sw / 2 - 100, sh/2 + 110, 30, Color.White);
                    break;
                case GameState.NewGameModeSelect:
                    Raylib.DrawText("SELECT MODE", sw / 2 - 150, sh/3, 45, Color.Yellow);
                    Raylib.DrawText("[1] Player vs Player", sw / 2 - 180, sh/2, 30, Color.White);
                    Raylib.DrawText("[2] Player vs Computer", sw / 2 - 180, sh/2 + 50, 30, Color.White);
                    Raylib.DrawText("Press ESC to Back", sw / 2 - 120, sh - 100, 20, Color.Gray);
                    break;
                case GameState.About:
                    Raylib.DrawText("ABOUT", sw / 2 - 80, sh/3, 45, Color.Yellow);
                    Raylib.DrawText("A highly polished Retro 3D Billiard Game.", sw / 2 - 250, sh/2, 25, Color.White);
                    Raylib.DrawText("Developed by Jacky @ Gravicode Studios", sw / 2 - 240, sh/2 + 40, 25, Color.Green);
                    Raylib.DrawText("Press ESC to Back", sw / 2 - 120, sh - 100, 20, Color.Gray);
                    break;
                case GameState.Setting:
                    Raylib.DrawText("SETTINGS", sw / 2 - 100, sh/3, 45, Color.Yellow);
                    Raylib.DrawText("ESC No Longer Exits App", sw / 2 - 180, sh/2, 25, Color.White);
                    Raylib.DrawText("Press ESC to Back", sw / 2 - 120, sh - 100, 20, Color.Gray);
                    break;
            }
        }
    }

    class Ball
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Color Color;
        public int Id;
        public float Radius = 0.4f;
        private float friction = 0.985f;

        public Ball(Vector3 pos, Color col, int id)
        {
            Position = pos;
            Color = col;
            Id = id;
            Velocity = Vector3.Zero;
        }

        public void Update()
        {
            Position += Velocity;
            Velocity *= friction;
            if (Velocity.Length() < 0.005f) Velocity = Vector3.Zero;

            if (Position.X < -4.3f || Position.X > 12.3f) { Velocity.X *= -0.85f; Position.X = Math.Clamp(Position.X, -4.3f, 12.3f); }
            if (Position.Z < -4.3f || Position.Z > 4.3f) { Velocity.Z *= -0.85f; Position.Z = Math.Clamp(Position.Z, -4.3f, 4.3f); }
        }

        public void CheckCollision(Ball other)
        {
            float dist = Vector3.Distance(this.Position, other.Position);
            if (dist < Radius * 2)
            {
                Vector3 normal = Vector3.Normalize(this.Position - other.Position);
                Vector3 relVel = this.Velocity - other.Velocity;
                float velAlongNormal = Vector3.Dot(relVel, normal);
                if (velAlongNormal > 0) return;

                float j = -(1.8f) * velAlongNormal;
                j /= 2;
                Vector3 impulse = j * normal;
                this.Velocity += impulse;
                other.Velocity -= impulse;

                float overlap = (Radius * 2) - dist;
                this.Position += normal * (overlap * 0.51f);
                other.Position -= normal * (overlap * 0.51f);
            }
        }
    }
}
