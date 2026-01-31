using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossyGame.Models
{
    public enum LaneType
    {
        Grass,
        Road,
        Water,
        Rail
    }

    public class GameObject
    {
        public double X { get; set; }
        public double Y { get; set; } // Y represents the Lane Index usually
        public double Width { get; set; } = 1.0;
        public double Height { get; set; } = 1.0;
        public string Color { get; set; } = "White";
    }

    public class Player : GameObject
    {
        public int GridX { get; set; }
        public int GridY { get; set; } // Progress forward
        public bool IsDead { get; set; }
        public bool IsOnLog { get; set; }

        public Player(int startX)
        {
            GridX = startX;
            GridY = 0;
            Color = "Blue";
        }
    }

    public class Obstacle : GameObject
    {
        public double Speed { get; set; } // Units per second
        public double Direction { get; set; } // 1 for right, -1 for left
        public bool IsLog { get; set; } // If true, player can ride it. If false (Car/Train), it kills.
        
        public Obstacle(double x, double y, double speed, double direction, bool isLog, string color, double width)
        {
            X = x;
            Y = y;
            Speed = speed;
            Direction = direction;
            IsLog = isLog;
            Color = color;
            Width = width;
        }

        public void Update(double deltaTime, int mapWidth)
        {
            X += Speed * Direction * deltaTime;

            // Wrap around
            if (Direction > 0 && X > mapWidth + 2) X = -Width - 2;
            if (Direction < 0 && X < -Width - 2) X = mapWidth + 2;
        }
    }

    public class Lane
    {
        public int YIndex { get; set; }
        public LaneType Type { get; set; }
        public List<Obstacle> Obstacles { get; set; } = new List<Obstacle>();
        public double SpeedMultiplier { get; set; } = 1.0;
        
        public bool IsTrainComing { get; set; } 
        public double TrainTimer { get; set; }

        public Lane(int index, LaneType type)
        {
            YIndex = index;
            Type = type;
        }
    }

    public class GameState
    {
        public Player Player { get; set; }
        public List<Lane> Lanes { get; set; } = new List<Lane>();
        public int Score { get; set; }
        public int HighScore { get; set; }
        public bool IsGameOver { get; set; }
        
        public const int MapWidth = 15; // Grid cells wide
        public const int VisibleLanes = 20;

        private Random _rnd = new Random();

        public GameState()
        {
            // Initial dummy assignment to satisfy non-nullable requirement before Reset()
            Player = new Player(0); 
            Reset();
        }

        public void Reset()
        {
            Player = new Player(MapWidth / 2);
            Lanes.Clear();
            Score = 0;
            IsGameOver = false;

            // Initialize starting lanes (mostly grass)
            for (int i = 0; i < 10; i++)
            {
                GenerateLane(i, LaneType.Grass);
            }
            // Generate some initial mix
            for (int i = 10; i < VisibleLanes + 5; i++)
            {
                GenerateNextLane(i);
            }
        }

        private void GenerateNextLane(int index)
        {
            LaneType type = LaneType.Grass;
            int r = _rnd.Next(100);

            if (r < 40) type = LaneType.Road;
            else if (r < 70) type = LaneType.Water;
            else if (r < 80) type = LaneType.Rail;
            else type = LaneType.Grass;

            if (index > 0)
            {
                var prev = Lanes.FirstOrDefault(l => l.YIndex == index - 1);
                if (prev != null && prev.Type == LaneType.Water && type == LaneType.Water)
                {
                    if (Lanes.Count(l => l.YIndex >= index - 2 && l.Type == LaneType.Water) >= 2)
                        type = LaneType.Grass;
                }
            }

            GenerateLane(index, type);
        }

        private void GenerateLane(int index, LaneType type)
        {
            var lane = new Lane(index, type);
            Lanes.Add(lane);

            if (type == LaneType.Road)
            {
                double speed = 2.0 + (_rnd.NextDouble() * 3.0);
                int direction = _rnd.Next(2) == 0 ? -1 : 1;
                int count = _rnd.Next(1, 4);
                double spacing = MapWidth / (double)count;
                
                for(int i=0; i<count; i++)
                {
                    lane.Obstacles.Add(new Obstacle(
                        (i * spacing) + _rnd.NextDouble(), 
                        index, 
                        speed, 
                        direction, 
                        false, 
                        "Red", 
                        1.5 
                    ));
                }
            }
            else if (type == LaneType.Water)
            {
                double speed = 1.5 + (_rnd.NextDouble() * 2.0);
                int direction = _rnd.Next(2) == 0 ? -1 : 1;
                int count = _rnd.Next(2, 4);
                double spacing = MapWidth / (double)count;

                for (int i = 0; i < count; i++)
                {
                    lane.Obstacles.Add(new Obstacle(
                        (i * spacing) + _rnd.NextDouble(),
                        index, 
                        speed, 
                        direction, 
                        true, 
                        "SaddleBrown",
                        2.5 
                    ));
                }
            }
        }

        public void MovePlayer(int dx, int dy)
        {
            if (IsGameOver) return;

            int targetX = Player.GridX + dx;
            int targetY = Player.GridY + dy;

            if (targetX < 0 || targetX >= MapWidth) return; 
            if (targetY < 0) return;

            Player.GridX = targetX;
            Player.GridY = targetY;

            if (Player.GridY > Score)
            {
                Score = Player.GridY;
                while (Lanes.Max(l => l.YIndex) < Player.GridY + VisibleLanes)
                {
                    GenerateNextLane(Lanes.Max(l => l.YIndex) + 1);
                }
                Lanes.RemoveAll(l => l.YIndex < Player.GridY - 5);
            }
        }

        public void Update(double deltaTime)
        {
            if (IsGameOver) return;

            Player.IsOnLog = false; 

            foreach (var lane in Lanes)
            {
                if (lane.Type == LaneType.Rail)
                {
                    lane.TrainTimer -= deltaTime;
                    if (lane.TrainTimer <= 0 && !lane.IsTrainComing)
                    {
                        lane.IsTrainComing = true; 
                        lane.TrainTimer = 2.0; 
                    }
                    else if (lane.TrainTimer <= 0 && lane.IsTrainComing)
                    {
                        lane.IsTrainComing = false;
                        lane.TrainTimer = 5.0 + _rnd.NextDouble() * 5.0; 
                        lane.Obstacles.Add(new Obstacle(
                             -20, lane.YIndex, 30.0, 1, false, "DarkGray", 20.0 
                        )); 
                    }
                }

                for (int i = lane.Obstacles.Count - 1; i >= 0; i--)
                {
                    var obs = lane.Obstacles[i];
                    obs.Update(deltaTime, MapWidth);

                    if (Player.GridY == lane.YIndex)
                    {
                        double playerLeft = Player.GridX;
                        double playerRight = Player.GridX + 0.8; 
                        double obsLeft = obs.X;
                        double obsRight = obs.X + obs.Width;

                        bool overlap = (playerLeft < obsRight && playerRight > obsLeft);

                        if (overlap)
                        {
                            if (obs.IsLog)
                            {
                                Player.IsOnLog = true;
                                Player.X += obs.Speed * obs.Direction * deltaTime; 
                            }
                            else
                            {
                                IsGameOver = true;
                            }
                        }
                    }

                    if (lane.Type == LaneType.Rail && obs.Speed > 10 && obs.X > MapWidth + 50)
                    {
                        lane.Obstacles.RemoveAt(i);
                    }
                }

                if (Player.GridY == lane.YIndex && lane.Type == LaneType.Water)
                {
                    if (!Player.IsOnLog)
                    {
                        IsGameOver = true; 
                    }
                }
            }
        }
    }
}