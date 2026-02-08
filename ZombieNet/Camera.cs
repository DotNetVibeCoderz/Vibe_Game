using Microsoft.Xna.Framework;

namespace ZombieNet
{
    public class Camera
    {
        public Matrix Transform { get; private set; }
        private Vector2 _position;
        private int _viewportWidth;
        private int _viewportHeight;
        private int _worldWidth;
        private int _worldHeight;

        public Camera(int viewportWidth, int viewportHeight, int worldWidth, int worldHeight)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
            _position = Vector2.Zero;
        }

        public void Move(Vector2 delta)
        {
            _position += delta;
            ClampPosition();
            UpdateMatrix();
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
            ClampPosition();
            UpdateMatrix();
        }

        private void ClampPosition()
        {
            if (_position.X < 0) _position.X = 0;
            if (_position.Y < 0) _position.Y = 0;
            if (_position.X > _worldWidth - _viewportWidth) _position.X = _worldWidth - _viewportWidth;
            if (_position.Y > _worldHeight - _viewportHeight) _position.Y = _worldHeight - _viewportHeight;
        }

        private void UpdateMatrix()
        {
            Transform = Matrix.CreateTranslation(new Vector3(-_position.X, -_position.Y, 0));
        }
        
        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            return screenPos + _position;
        }
    }
}
