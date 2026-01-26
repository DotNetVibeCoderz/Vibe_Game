using System;
using System.Text.Json;
using System.IO;

namespace Tamagochi
{
    public enum PetType
    {
        Dino,
        Cat,
        Robot
    }

    public enum LifecycleStage
    {
        Egg,
        Baby,
        Adult,
        Dead
    }

    public enum Emotion
    {
        Happy,
        Normal,
        Sad,
        Angry,
        Sick
    }

    public class PetModel
    {
        public string Name { get; set; } = "MyPet";
        public PetType Type { get; set; } = PetType.Dino;
        public LifecycleStage Stage { get; set; } = LifecycleStage.Egg;
        
        // Stats (0-100)
        public int Hunger { get; set; } = 50; // 0 = Full, 100 = Starving
        public int Hygiene { get; set; } = 100; // 100 = Clean, 0 = Dirty
        public int Energy { get; set; } = 100; // 100 = Full, 0 = Tired
        public int Happiness { get; set; } = 50; // 100 = Happy, 0 = Sad
        public int Health { get; set; } = 100; // 100 = Healthy, 0 = Sick

        public DateTime BirthDate { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        
        // Internal tracking
        public bool IsSick => Health < 40;
        public bool IsDirty => Hygiene < 30;
        public bool IsHungry => Hunger > 70;
        public bool IsTired => Energy < 20;

        public Emotion CurrentEmotion
        {
            get
            {
                if (Stage == LifecycleStage.Dead) return Emotion.Sad;
                if (IsSick) return Emotion.Sick;
                if (IsHungry || IsDirty) return Emotion.Angry;
                if (Happiness < 30) return Emotion.Sad;
                if (Happiness > 70) return Emotion.Happy;
                return Emotion.Normal;
            }
        }

        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText("pet_save.json", jsonString);
        }

        public static PetModel Load()
        {
            if (File.Exists("pet_save.json"))
            {
                string jsonString = File.ReadAllText("pet_save.json");
                try
                {
                    var loaded = JsonSerializer.Deserialize<PetModel>(jsonString);
                    loaded.ApplyOfflineProgress();
                    return loaded;
                }
                catch
                {
                    return new PetModel();
                }
            }
            return new PetModel();
        }

        public void ApplyOfflineProgress()
        {
            if (Stage == LifecycleStage.Dead) return;

            TimeSpan timeDiff = DateTime.Now - LastUpdate;
            double minutes = timeDiff.TotalMinutes;

            // Simple decay: 1 point every 10 minutes offline
            int decay = (int)(minutes / 10);
            
            if (decay > 0)
            {
                Hunger = Math.Min(100, Hunger + decay);
                Hygiene = Math.Max(0, Hygiene - decay);
                Energy = Math.Min(100, Energy + decay); // Energy recovers while offline (sleeping)
                Happiness = Math.Max(0, Happiness - decay);
                
                // If neglected too long
                if (Hunger >= 100 || Hygiene <= 0)
                {
                    Health = Math.Max(0, Health - decay);
                }
                
                CheckLifecycle();
            }
            LastUpdate = DateTime.Now;
        }

        public void Tick()
        {
            if (Stage == LifecycleStage.Dead) return;

            // Natural decay per tick (every second or few seconds in game loop)
            // But here we'll assume this method is called frequently, so we use small probabilities or float logic.
            // Simplified: This method is called by the main timer every 2 seconds.
            
            if (Stage == LifecycleStage.Egg)
            {
                if ((DateTime.Now - BirthDate).TotalSeconds > 10) // Hatch quickly for demo
                {
                    Stage = LifecycleStage.Baby;
                    Happiness = 100;
                    Hunger = 0;
                }
                return;
            }

            Hunger = Math.Min(100, Hunger + 1);
            if (Hunger > 80) Happiness = Math.Max(0, Happiness - 1);
            
            Hygiene = Math.Max(0, Hygiene - 1);
            if (Hygiene < 20) Health = Math.Max(0, Health - 1);

            Energy = Math.Max(0, Energy - 1);
            if (Energy < 10) Happiness = Math.Max(0, Happiness - 1);

            if (IsSick) Health = Math.Max(0, Health - 1);

            CheckLifecycle();
            LastUpdate = DateTime.Now;
        }

        private void CheckLifecycle()
        {
            if (Health <= 0)
            {
                Stage = LifecycleStage.Dead;
                return;
            }

            if (Stage == LifecycleStage.Baby && (DateTime.Now - BirthDate).TotalMinutes > 2) // Grow up fast for demo
            {
                Stage = LifecycleStage.Adult;
            }
        }

        // Actions
        public void Feed()
        {
            if (Stage == LifecycleStage.Dead || Stage == LifecycleStage.Egg) return;
            Hunger = Math.Max(0, Hunger - 20);
            Health = Math.Min(100, Health + 5);
            Energy = Math.Max(0, Energy - 5); // Eating takes effort?
        }

        public void Clean()
        {
            if (Stage == LifecycleStage.Dead || Stage == LifecycleStage.Egg) return;
            Hygiene = 100;
            Happiness = Math.Min(100, Happiness + 10);
        }

        public void Play()
        {
            if (Stage == LifecycleStage.Dead || Stage == LifecycleStage.Egg) return;
            if (Energy < 20) return; // Too tired
            Happiness = Math.Min(100, Happiness + 20);
            Hunger = Math.Min(100, Hunger + 10);
            Energy = Math.Max(0, Energy - 20);
            Hygiene = Math.Max(0, Hygiene - 10);
        }

        public void Medicate()
        {
            if (Stage == LifecycleStage.Dead || Stage == LifecycleStage.Egg) return;
            Health = Math.Min(100, Health + 30);
            Happiness = Math.Max(0, Happiness - 10); // Medicine tastes bad
        }
        
        public void Sleep()
        {
             if (Stage == LifecycleStage.Dead || Stage == LifecycleStage.Egg) return;
             Energy = 100;
             Hunger = Math.Min(100, Hunger + 10);
        }

        public void Reset()
        {
            Name = "NewPet";
            Stage = LifecycleStage.Egg;
            Hunger = 50;
            Hygiene = 100;
            Energy = 100;
            Happiness = 50;
            Health = 100;
            BirthDate = DateTime.Now;
            LastUpdate = DateTime.Now;
        }
    }
}
