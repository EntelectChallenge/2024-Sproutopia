using Microsoft.Xna.Framework.Input;

namespace Visualiser
{
    public static class OneShotMouseButton
    {

        static MouseState currentMouseState;
        static MouseState previouseMouseState;

        public static MouseState GetState()
        {
            previouseMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            return currentMouseState;
        }

        public static bool IsPressed(bool left)
        {
            if (left)
            {
                return currentMouseState.LeftButton == ButtonState.Pressed;
            }
            else
                return currentMouseState.RightButton == ButtonState.Pressed;
        }

        public static bool HasNotBeenPressed(bool left)
        {

            if (left)
            {
                return currentMouseState.LeftButton == ButtonState.Pressed && !(previouseMouseState.LeftButton == ButtonState.Pressed);
            }
            else
                return currentMouseState.RightButton == ButtonState.Pressed && !(previouseMouseState.RightButton == ButtonState.Pressed);

        }
    }
}
