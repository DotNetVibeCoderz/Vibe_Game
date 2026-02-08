using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ParticleCombat
{
    public static class Input
    {
        private static KeyboardState currentKeyboardState, previousKeyboardState;
        private static MouseState currentMouseState, previousMouseState;

        public static Vector2 MousePosition 
        {
            get { return new Vector2(currentMouseState.X, currentMouseState.Y); }
        }

        public static void Update()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }

        public static bool WasKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        }

        public static bool IsKeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        public static bool WasLeftMouseButtonClicked()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        }

        public static bool IsLeftMouseButtonDown()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed;
        }
    }
}