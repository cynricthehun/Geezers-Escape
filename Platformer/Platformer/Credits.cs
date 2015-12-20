using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Platformer
{
    class Credits
    {
        float pageBottom;
        private Texture2D text;

        public Vector2 Position
        {
            get { return position; }
        }        
        Vector2 position;

        public void SetPosition()
        {
            position = new Vector2(0,pageBottom);
        }

        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        int speed = 3;

        public Credits(IServiceProvider serviceProvider)
        {
            content = new ContentManager(serviceProvider, "Content");
        }

        public void Load(GraphicsDevice GraphicsDevice)
        {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            pageBottom = titleSafeArea.Y + titleSafeArea.Height;
            position = new Vector2(0,pageBottom);
            text = content.Load<Texture2D>("Menus/Credits");
        }

        public void Update(GameTime gameTime)
        {
            if (position.Y + text.Height > pageBottom)
            {
                position.Y -= speed;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(text, position, Color.White);
        }


    }
}
