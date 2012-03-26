namespace Flocking
{
    using System.Linq;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Media;

    using Flocking.Pc;
    using System;

    public class MainWrapper : Game
    {
        private GameTime LastLoopTime { get; set; }

        private GameplayHandler GameHandler { get; set; }

        private GraphicsDeviceManager Graphics { get; set; }

        private SpriteBatch SpriteBatch { get; set; }

        private MouseCursor Cursor { get; set; }

        public MainWrapper()
        {
            this.Graphics = new GraphicsDeviceManager(this);
            this.Graphics.PreferredBackBufferHeight = 728;
            this.Graphics.PreferredBackBufferWidth = 1024;
            this.Graphics.ApplyChanges();

            this.TargetElapsedTime = TimeSpan.FromMilliseconds(10);

            Content.RootDirectory = "Content";

            this.GameHandler = new GameplayHandler();
        }

        protected override void Initialize()
        {
            this.Cursor = new MouseCursor();

            this.GameHandler.Initialise(this.GraphicsDevice);

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            this.SpriteBatch = new SpriteBatch(GraphicsDevice);

            this.GameHandler.LoadContent(this.Content);

            this.Cursor.LoadContent(this.Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().GetPressedKeys().Any(x => x == Keys.Escape))
            {
                this.Exit();
                return;
            }

            this.GameHandler.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            this.SpriteBatch.Begin();

            this.GameHandler.Draw(this.SpriteBatch);
            this.Cursor.Draw(this.SpriteBatch);

            this.SpriteBatch.End();

            base.Draw(gameTime);
            this.LastLoopTime = gameTime;
        }
    }
}
