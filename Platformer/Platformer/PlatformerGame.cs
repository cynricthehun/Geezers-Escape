#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using Platformer;


namespace Platformer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Global content.
        private SpriteFont hudFont;
        public static SpriteFont monoFont;
        private Texture2D Face;
        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        // Health Sprites
        Texture2D BF;
        Texture2D BQG;
        Texture2D BH;
        Texture2D BQL;
        Texture2D BE;
        Texture2D mHealthBar;
        
        // Meta-level game state.
        public static int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;

        //77x
        string gameState = "MainMenu";
        private MainMenu mainMenu;
        private Credits credits;
        private bool togglePause = false;
        bool wasTogglePausePressed;
        private Texture2D paused;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private TouchCollection touchState;
        private AccelerometerState accelerometerState;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;
        
        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 20;

        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif

            Accelerometer.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load the HealthBar image from the disk into the Texture2D object
            mHealthBar = Content.Load<Texture2D>("Sprites/Healthbar") as Texture2D;
            BF = Content.Load<Texture2D>("Sprites/Health/BF") as Texture2D;
            BQG = Content.Load<Texture2D>("Sprites/Health/BQG") as Texture2D;
            BH = Content.Load<Texture2D>("Sprites/Health/BH") as Texture2D;
            BQL = Content.Load<Texture2D>("Sprites/Health/BQL") as Texture2D;
            BE = Content.Load<Texture2D>("Sprites/Health/BE") as Texture2D;

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");
            monoFont = Content.Load<SpriteFont>("Fonts/Hud");

            // Load overlay textures
            Face = Content.Load<Texture2D>("Overlays/Face");
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

            //77x
            paused = Content.Load<Texture2D>("Menus/paused");

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away

            //77x
            mainMenu = new MainMenu(Services);
            mainMenu.Load();
            credits = new Credits(Services);
            credits.Load(GraphicsDevice);
            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Handle polling for our input and handling high-level input
            HandleInput();

            //77x
            switch (gameState)
            {
                case "Game":
                    // update our level, passing down the GameTime along with all of our input states
                    if (togglePause)
                    {
                        gameState = "Pause";
                    }
                    level.Update(gameTime, keyboardState, gamePadState, touchState,
                                 accelerometerState, Window.CurrentOrientation);
                    break;
                case "Pause":
                    if (togglePause)
                    {
                        gameState = "Game";
                    }
                    break;
                case "Credits":
                    if (keyboardState.IsKeyDown(Keys.Enter) ||
                        gamePadState.IsButtonDown(Buttons.B))
                    {
                        gameState = "MainMenu";
                    }
                    credits.Update(gameTime);
                    break;
                default://MainMenu
                    credits.SetPosition();
                    if (levelIndex > 0)
                    {
                    }
                    mainMenu.Update(gameTime, keyboardState, gamePadState);
                    if (keyboardState.IsKeyDown(Keys.Space) ||
                        gamePadState.IsButtonDown(Buttons.A))
                    {
                        gameState = mainMenu.GameState;
                        if (gameState == "Game")
                        {
                            levelIndex = -1;
                            LoadNextLevel();
                        }
                    }
                    break;
            }
            base.Update(gameTime);
        }

        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            touchState = TouchPanel.GetState();
            accelerometerState = Accelerometer.GetState();

            //Admin Windows Speed Key
            if (keyboardState.IsKeyDown(Keys.E) || gamePadState.IsButtonDown(Buttons.RightShoulder))
            {
                LoadNextLevel();
            }

            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                Exit();
            bool togglePausePressed = gamePadState.IsButtonDown(Buttons.Start) ||
                keyboardState.IsKeyDown(Keys.P) ||
                keyboardState.IsKeyDown(Keys.Enter);

            if (!wasTogglePausePressed && togglePausePressed)
            {
                togglePause = true;
            }
            else
            {
                togglePause = false;
            }
            wasTogglePausePressed = togglePausePressed;


            if (gameState == "Game")
            {
                bool continuePressed =
                            keyboardState.IsKeyDown(Keys.Space) ||
                            gamePadState.IsButtonDown(Buttons.A) ||
                            touchState.AnyTouch();

                // Perform the appropriate action to advance the game and
                // to get the player back to playing.
                if (!wasContinuePressed && continuePressed)
                {
                    if (!level.Player.IsAlive)
                    {
                        level.StartNewLife();
                    }
                    else if (level.TimeRemaining == TimeSpan.Zero)
                    {
                        if (level.ReachedExit)
                            //77x
                            if (levelIndex == numberOfLevels - 1)
                            {
                                gameState = "Credits";
                            }
                            else
                            {
                                LoadNextLevel();
                            }
                        else
                            ReloadCurrentLevel();
                    }
                }

                wasContinuePressed = continuePressed;
            }
        }

        public void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            if (levelIndex <= 4)
            {
                // Load the level.
                string levelPath = string.Format("Content/Levels/Nursing/{0}.txt", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    level = new Level(Services, fileStream, levelIndex);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Music/Nursing"));
                MediaPlayer.Volume = 0.25f;
            }
            else if (levelIndex <= 9 && levelIndex > 4)
            {
                // Load the level.
                string levelPath = string.Format("Content/Levels/Prehistoric/{0}.txt", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    level = new Level(Services, fileStream, levelIndex);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Music/Prehistoric"));
                MediaPlayer.Volume = 0.25f;
            }
            else if (levelIndex <= 14 && levelIndex >= 10)
            {
                // Load the level.
                string levelPath = string.Format("Content/Levels/Midieval/{0}.txt", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    level = new Level(Services, fileStream, levelIndex);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Music/Midieval"));
                MediaPlayer.Volume = 0.25f;
            }
            else if (levelIndex <= 19 && levelIndex >= 15)
            {
                // Load the level.
                string levelPath = string.Format("Content/Levels/Space/{0}.txt", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    level = new Level(Services, fileStream, levelIndex);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Content.Load<Song>("Music/Space"));
                MediaPlayer.Volume = 0.25f;
            }
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (gameState)
            {
                case "Game":
                    #region Game
                    level.Draw(gameTime, spriteBatch);

                    DrawHud();

                    string lifestring = "Geezer";
                    Vector2 lifepos = new Vector2(680.0f, 0.0f);
                    string levelstring = "Level: " + levelIndex;
                    Vector2 levelpos = new Vector2(370.0f, 0.0f);
                    spriteBatch.Begin();
                    spriteBatch.DrawString(monoFont, lifestring, lifepos, Color.OrangeRed);
                    spriteBatch.DrawString(monoFont, levelstring, levelpos, Color.OrangeRed);
                    //Start Health Bar
                    //Determine Health Bar State
                    if (level.Player.life == 5)
                    {
                        spriteBatch.Draw(BF,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.SteelBlue);
                        spriteBatch.Draw(mHealthBar,
                            new Rectangle(680, 30, 22, (int)(100 * ((double)level.Player.currenthealth / 100))),
                            new Rectangle(100, 55, 0, 0), Color.Red);
                        spriteBatch.Draw(BF,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.White);
                    }
                    else if (level.Player.life == 4)
                    {
                        spriteBatch.Draw(BQG,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.SteelBlue);
                        spriteBatch.Draw(mHealthBar,
                            new Rectangle(680, 30, 22, (int)(100 * ((double)level.Player.currenthealth / 100))),
                            new Rectangle(100, 55, 0, 0), Color.Red);
                        spriteBatch.Draw(BQG,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.White);
                    }
                    else if (level.Player.life == 3)
                    {
                        spriteBatch.Draw(BH,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.SteelBlue);
                        spriteBatch.Draw(mHealthBar,
                            new Rectangle(680, 30, 22, (int)(100 * ((double)level.Player.currenthealth / 100))),
                            new Rectangle(100, 55, 0, 0), Color.Red);
                        spriteBatch.Draw(BH,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.White);
                    }
                    else if (level.Player.life == 2)
                    {
                        spriteBatch.Draw(BQL,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.SteelBlue);
                        spriteBatch.Draw(mHealthBar,
                            new Rectangle(680, 30, 22, (int)(100 * ((double)level.Player.currenthealth / 100))),
                            new Rectangle(100, 55, 0, 0), Color.Red);
                        spriteBatch.Draw(BQL,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.White);
                    }
                    else if (level.Player.life <= 1)
                    {
                        spriteBatch.Draw(BE,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.SteelBlue);
                        spriteBatch.Draw(mHealthBar,
                            new Rectangle(680, 30, 22, (int)(100 * ((double)level.Player.currenthealth / 100))),
                            new Rectangle(100, 55, 0, 0), Color.Red);
                        spriteBatch.Draw(BE,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(BE,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.SteelBlue);
                        spriteBatch.Draw(mHealthBar,
                            new Rectangle(680, 30, 22, (int)(100 * ((double)level.Player.currenthealth / 100))),
                            new Rectangle(100, 55, 0, 0), Color.Red);
                        spriteBatch.Draw(BE,
                            new Rectangle(680, 30, 100, 100),
                            new Rectangle(0, 0, 100, 100), Color.White);
                    }
                    spriteBatch.End();
                    //end health bar

                    #endregion
                    break;
                case "Pause":
                    spriteBatch.Begin();
                    spriteBatch.Draw(paused, Vector2.Zero, Color.White);
                    spriteBatch.End();
                    break;
                case "Credits":
                    spriteBatch.Begin();
                    graphics.GraphicsDevice.Clear(Color.Black);
                    credits.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;
                default://MainMenu
                    spriteBatch.Begin();
                    mainMenu.Draw(gameTime, spriteBatch);
                    spriteBatch.End();
                    break;
            }

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            spriteBatch.Begin();

            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(monoFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = monoFont.MeasureString(timeString).Y;
            DrawShadowedString(monoFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);
           
            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!level.Player.IsAlive)
            {
                if (level.Player.life > 0)
                    status = diedOverlay;
                else
                    status = loseOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            spriteBatch.End();
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
