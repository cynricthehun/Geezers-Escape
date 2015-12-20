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
    class MainMenu
    {
        int menuSelection = 0;
        const int numberOfMenuSelections = 2;
        private Texture2D menuBackground;
        private Texture2D cursor;
        private Texture2D menuOptions;
        private Vector2 cursorPosition;

        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        public bool WasTogglePressed
        {
            get { return wasTogglePressed; }
        }
        bool wasTogglePressed;

        public string GameState
        {
            get { return gameState; }
        }
        string gameState = "Game";


        public void Load()
        {
            menuBackground = content.Load<Texture2D>("Menus/start screen1");
            cursor = content.Load<Texture2D>("Menus/cursor");
            menuOptions = content.Load<Texture2D>("Menus/menuOptions");

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(menuBackground, Vector2.Zero, Color.White);
            spriteBatch.Draw(menuOptions, new Vector2(265, 420), Color.White);
            spriteBatch.Draw(cursor, cursorPosition, Color.White);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            //Check if user switches option
            bool togglePressed =
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.Right) ||
                gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                gamePadState.IsButtonDown(Buttons.DPadRight);

            if (!wasTogglePressed && togglePressed)
            {
                if (menuSelection == 0)
                {
                    menuSelection = 1;
                    cursorPosition.X = 450;
                }
                else
                {
                    menuSelection = 0;
                    cursorPosition.X = 230;
                }
            }
            wasTogglePressed = togglePressed;

            //Actions to take after selecting menu option
            if (keyboardState.IsKeyDown(Keys.Space) ||
                gamePadState.IsButtonDown(Buttons.A))
            {
                if (menuSelection == 0)
                {
                    gameState = "Game";
                }
                if (menuSelection == 1)
                {
                    gameState = "Credits";
                }
            }
        }

        public MainMenu(IServiceProvider serviceProvider)
        {
            content = new ContentManager(serviceProvider, "Content");
            cursorPosition = new Vector2(230, 427);

        }
    }
}
