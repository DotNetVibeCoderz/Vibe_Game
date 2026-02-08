using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ZombieNet
{
    public class WaveManager
    {
        public List<Enemy> Enemies { get; private set; }
        public int CurrentWave { get; private set; }
        public bool WaveActive { get; private set; }
        public float SpawnTimer { get; private set; }
        public float TimeBetweenWaves { get; private set; }
        public float WaveStartTimer { get; private set; }

        private List<Vector2> _path;
        private Texture2D _zombieTexture;
        private int _enemiesToSpawn;
        private int _enemiesSpawned;
        private float _spawnRate;

        public WaveManager(List<Vector2> path, Texture2D zombieTexture)
        {
            Enemies = new List<Enemy>();
            _path = path;
            _zombieTexture = zombieTexture;
            CurrentWave = 0;
            WaveActive = false;
            TimeBetweenWaves = 5.0f;
            WaveStartTimer = 0;
        }

        public void StartNextWave()
        {
            CurrentWave++;
            _enemiesToSpawn = 5 + (CurrentWave * 2); // Increase enemies
            _enemiesSpawned = 0;
            _spawnRate = 1.0f - (CurrentWave * 0.05f); // Faster spawning
            if (_spawnRate < 0.2f) _spawnRate = 0.2f;
            WaveActive = true;
            SpawnTimer = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (WaveActive)
            {
                if (_enemiesSpawned < _enemiesToSpawn)
                {
                    SpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (SpawnTimer >= _spawnRate)
                    {
                        SpawnEnemy();
                        SpawnTimer = 0;
                    }
                }
                else if (Enemies.Count == 0)
                {
                    WaveActive = false; // Wave finished
                    WaveStartTimer = 0;
                }
            }
            else
            {
                WaveStartTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (WaveStartTimer >= TimeBetweenWaves)
                {
                    StartNextWave();
                }
            }

            // Update enemies movement only
            foreach (var enemy in Enemies)
            {
                enemy.Update(gameTime);
            }
        }

        private void SpawnEnemy()
        {
            int health = 10 + (CurrentWave * 5);
            // Speed logic
            Enemies.Add(new Enemy(_zombieTexture, _path, health, 1.0f, 10));
            _enemiesSpawned++;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var enemy in Enemies)
            {
                enemy.Draw(spriteBatch);
            }
        }
    }
}
