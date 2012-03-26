namespace Flocking.Pc
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class MouseCursor
    {
        public void LoadContent(ContentManager contentManager)
        {
            this.Cursor = contentManager.Load<Texture2D>("cursor");
            this.ClickState = contentManager.Load<Texture2D>("cursorClicked");

            this.HalfWidth = this.Cursor.Width / 2;
            this.HalfHeight = this.Cursor.Height / 2;
        }

        private Texture2D Cursor;

        private Texture2D ClickState;

        private int HalfWidth;

        private int HalfHeight;

        public void Draw(SpriteBatch spriteBatch)
        {
            var currentState = Mouse.GetState();

            Texture2D toDraw = currentState.LeftButton == ButtonState.Pressed ? this.ClickState : this.Cursor;

            Rectangle destinationRectangle = new Rectangle(currentState.X - HalfWidth, currentState.Y - HalfHeight, this.Cursor.Width, this.Cursor.Height);

            spriteBatch.Draw(toDraw, destinationRectangle, Color.White);
        }
    }
}
