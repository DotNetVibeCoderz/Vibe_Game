using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DiggerNet.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DiggerNet.Views
{
    public class GameControl : Control
    {
        private GameState? _gameState;
        private Dictionary<string, Bitmap> _textures;
        
        public GameControl()
        {
            _textures = new Dictionary<string, Bitmap>();
            try { LoadAssets(); } catch { } 
        }

        private void LoadAssets()
        {
            _textures["Digger"] = LoadBitmap("Digger.png");
            _textures["Nobbin"] = LoadBitmap("Nobbin.png");
            _textures["Hobbin"] = LoadBitmap("Hobbin.png");
            _textures["Emerald"] = LoadBitmap("Emerald.png");
            _textures["GoldBag"] = LoadBitmap("GoldBag.png");
            _textures["Dirt"] = LoadBitmap("Dirt.png");
            _textures["Wall"] = LoadBitmap("Wall.png");
        }

        private Bitmap LoadBitmap(string name)
        {
            var uri = new Uri($"avares://DiggerNet/Assets/{name}");
            return new Bitmap(AssetLoader.Open(uri));
        }

        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
            if (_gameState != null)
            {
                _gameState.RequestRender += (s, e) => 
                {
                    Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
                };
            }
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));

            if (_gameState == null) return;

            double mapW = _gameState.Width * _gameState.TileSize;
            double mapH = _gameState.Height * _gameState.TileSize;
            
            if (mapW == 0 || mapH == 0) return;

            double scaleX = Bounds.Width / mapW;
            double scaleY = Bounds.Height / mapH;
            double scale = Math.Min(scaleX, scaleY);
            
            double offsetX = (Bounds.Width - (mapW * scale)) / 2;
            double offsetY = (Bounds.Height - (mapH * scale)) / 2;

            var state = context.PushTransform(Matrix.CreateTranslation(offsetX, offsetY) * Matrix.CreateScale(scale, scale));

            for (int x = 0; x < _gameState.Width; x++)
            {
                for (int y = 0; y < _gameState.Height; y++)
                {
                    var tile = _gameState.Map[x, y];
                    Rect rect = new Rect(x * _gameState.TileSize, y * _gameState.TileSize, _gameState.TileSize, _gameState.TileSize);

                    if (tile == TileType.Dirt && _textures.ContainsKey("Dirt")) context.DrawImage(_textures["Dirt"], rect);
                    else if (tile == TileType.Wall && _textures.ContainsKey("Wall")) context.DrawImage(_textures["Wall"], rect);
                    else if (tile == TileType.Emerald && _textures.ContainsKey("Emerald")) context.DrawImage(_textures["Emerald"], rect);
                    else if (tile == TileType.GoldBag && _textures.ContainsKey("GoldBag")) context.DrawImage(_textures["GoldBag"], rect);
                }
            }

            if (_gameState.Player.IsAlive && _textures.ContainsKey("Digger"))
            {
                Rect r = new Rect(_gameState.Player.X * _gameState.TileSize, _gameState.Player.Y * _gameState.TileSize, _gameState.TileSize, _gameState.TileSize);
                context.DrawImage(_textures["Digger"], r);
            }

            foreach(var enemy in _gameState.Enemies)
            {
                 if (!enemy.IsAlive) continue;
                 Rect r = new Rect(enemy.X * _gameState.TileSize, enemy.Y * _gameState.TileSize, _gameState.TileSize, _gameState.TileSize);
                 string key = enemy.SubType == Enemy.EnemyType.Nobbin ? "Nobbin" : "Hobbin";
                 if (_textures.ContainsKey(key)) context.DrawImage(_textures[key], r);
            }

            foreach(var p in _gameState.Projectiles)
            {
                if (!p.IsAlive) continue;
                context.DrawEllipse(Brushes.Orange, null, new Point((p.X * _gameState.TileSize) + 16, (p.Y * _gameState.TileSize) + 16), 4, 4);
            }
            
            state.Dispose();

            var text = new FormattedText(
                $"Score: {_gameState.Score}   Lives: {_gameState.Lives}   Level: {_gameState.Level}",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily.Default),
                24,
                Brushes.White
            );
            context.DrawText(text, new Point(10, 10));

            if (_gameState.IsGameOver)
            {
                 var goText = new FormattedText(
                    "GAME OVER",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold),
                    60,
                    Brushes.Red
                );
                context.DrawText(goText, new Point((Bounds.Width - goText.Width) / 2, (Bounds.Height - goText.Height) / 2));
            }
        }
    }
}