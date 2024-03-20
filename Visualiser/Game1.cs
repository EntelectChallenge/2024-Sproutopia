using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sproutopia;
using Sproutopia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Visualiser.Camera;

namespace Visualiser
{
    public class Game1 : Game
    {
        private bool tempTest = true;
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private SproutopiaEngine sproutopiaEngine;
        private bool _mouseLeftPressed;
        private MouseState _oneShotMouseState;
        private LevelLoader levelLoader;
        private HubConnection connection;
        private static IConfigurationRoot Configuration;

        private GameStateDto GameState { get; set; }
        private GameState[] GameStateLogs { get; }
        private CameraWindow[] _windows;
        private int _heroToFollow;

        public bool _followHero;
        private int _currentTick { get; set; }
        private int _padding { get; set; }
        private int _initalPadding { get; set; }
        private Vector2 _botControllerPlacement { get; set; }
        private SpriteFont _arial { get; set; }

        #region Declare Textures
        Texture2D buttonPlayTexture;
        private Texture2D UnClaimedLandTextures;
        private Texture2D ClaimedLandTextures;
        private Texture2D TrailTexture;
        private Texture2D EmptyTexture;
        private List<Texture2D> PlayerTextures;
        // private BotStateSprite botStateSprites;

        #endregion

        #region UI Elements

        private Button _playToggleButton;
        private Button _pauseButton;
        private Button _stepButton;
        private Button _continueButton;

        #endregion

        public Game1()
        {
            _currentTick = 0;
            _padding = 8;
            _initalPadding = 125;
            _heroToFollow = 0;
            _graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            GameStateLogs = Array.Empty<GameState>();
            _mouseLeftPressed = false;
            _followHero = false;
        }

        protected override void Initialize()
        {
            string environment;
#if DEBUG
            environment = "Development";
#elif RELEASE
            environment = "Production";
#endif
            /*
                        Configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

                        var test = Configuration.GetSection("GameSettings");

                        var rows = test.GetValue("Rows", 50);
                        var cols = test.GetValue("Cols", 50);*/



            _oneShotMouseState = OneShotMouseButton.GetState();

            //TODO: how to make this configurable
            levelLoader = new(_padding, 50, 50);
            levelLoader.AddLevel(_initalPadding);

            Console.WriteLine("Initialized bot windows...");
            #region Screen Options
            //* set to screen size * //
            _graphics.PreferredBackBufferWidth = 1900;
            _graphics.PreferredBackBufferHeight = 1000;

            //* apply changes * //
            _graphics.ApplyChanges();
            _graphicsDevice = _graphics.GraphicsDevice;
            #endregion

            _botControllerPlacement = new(20, _graphics.PreferredBackBufferHeight - 300);

            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/visualiserhub")
                .WithAutomaticReconnect()
                .Build();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _arial = Content.Load<SpriteFont>("Arial");

            #region Load Textures
            UnClaimedLandTextures = Content.Load<Texture2D>("tiny_tile");
            ClaimedLandTextures = Content.Load<Texture2D>("tiny_tile");
            TrailTexture = Content.Load<Texture2D>("tiny_tile");
            EmptyTexture = Content.Load<Texture2D>("tiny_tile");

            PlayerTextures = new List<Texture2D>() {
                Content.Load<Texture2D>("tiny_tile"),
                Content.Load<Texture2D>("tiny_tile"),
                Content.Load<Texture2D>("tiny_tile"),
                Content.Load<Texture2D>("tiny_tile"),
            };
            #endregion

            #region Load Buttons

            _playToggleButton = new(
                staticImage: Content.Load<Texture2D>("play"),
                clickedImage: Content.Load<Texture2D>("play"),
                dimensions: new(64, 64),
                position: new(0, 0),
                name: "play",
                id: 33,
                visible: true,
                layerDepth: 1.0f);

            _pauseButton = new(
                staticImage: Content.Load<Texture2D>("pause"),
                clickedImage: Content.Load<Texture2D>("pause"),
                dimensions: new(64, 64),
                position: new(0, 84),
                name: "pause",
                id: 34,
                visible: true,
                layerDepth: 1.0f);

            /*            _continueButton = new(
                            staticImage: Content.Load<Texture2D>("ButtonStop"),
                            clickedImage: Content.Load<Texture2D>("ButtonStop"),
                            dimensions: new(131, 121),
                            position: new(0, 261),
                            name: "pause",
                            id: 35,
                            visible: true,
                            layerDepth: 1.0f);*/

            _stepButton = new(
                staticImage: Content.Load<Texture2D>("step"),
                clickedImage: Content.Load<Texture2D>("step"),
                dimensions: new(64, 64),
                position: new(0, 158),
                name: "pause",
                id: 35,
                visible: true,
                layerDepth: 1.0f);
            #endregion
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (GameStateLogs.Length > 0)
            {
                //Update GameState with the current log
            }

            HandelInput(gameTime);
            _playToggleButton.UpdateButton();
            _pauseButton.UpdateButton();
            _stepButton.UpdateButton();
            //       _continueButton.UpdateButton();

            connection.On<string>(VisualiserCommands.SendDummyString, state =>
            {
                var dummyString = state;
            });

            connection.On<GameStateDto>(VisualiserCommands.ReceiveInitialGameState, state =>
            {
                GameState = state;
            });

            if (_mouseLeftPressed)
            {
                _mouseLeftPressed = false;

                try
                {
                    if (connection.State == HubConnectionState.Connected)
                    {
                        if (CheckIfButtonWasClicked(_pauseButton))
                            connection.SendAsync("PauseGame").Wait();

                        if (CheckIfButtonWasClicked(_playToggleButton))
                            connection.SendAsync("ContinueGame").Wait();

                        if (CheckIfButtonWasClicked(_stepButton))
                            connection.SendAsync("StepIntoGame").Wait();
                    }
                    else if (CheckIfButtonWasClicked(_playToggleButton)) connection.StartAsync().Wait();


                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }

            /*            if (RunnerClient.connection != null)
                        {
                            if (RunnerClient.connection.State == HubConnectionState.Connected)
                            {
                                RunnerClient.connection.On<GameState>("RecieveInitialGameState", (state) =>
                                {
                                    InitialiseBotWindows();
                                    GameState = state;
                                });

                                RunnerClient.connection.On<GameState>("RecieveChangeLog", (state) =>
                                {
                                    GameState = state;
                                });
                            }

                        }*/

            if (GameState is not null)
            {
                var gamestateTemp = GameState;
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            _spriteBatch.Begin();

            #region Draw UI 
            _spriteBatch.Draw(_playToggleButton.Texture, _playToggleButton.Position, Color.White);
            _spriteBatch.Draw(_pauseButton.Texture, _pauseButton.Position, Color.White);
            _spriteBatch.Draw(_stepButton.Texture, _stepButton.Position, Color.White);
            //  _spriteBatch.Draw(_continueButton.Texture, _continueButton.Position, Color.White);
            #endregion

            if (GameState != null)
            {
                #region Draw Game State Window 
                for (int r = 0; r < levelLoader.Rows; r++)
                {
                    for (int c = 0; c < levelLoader.Columns; c++)
                    {
                        var position = levelLoader.Maps[r, c];
                        var tyleType = GameState.Land[r][c];
                        DrawTile(position, tyleType, GetColor(tyleType));

                        if (GameState.Weeds[r][c])
                        {
                            DrawTile(position, 8, GetColor(8));

                        }

                    }
                }
                #endregion

                #region Draw PowerUps

                foreach (var powerUp in GameState.PowerUps)
                {
                    var x = powerUp.Location.X;
                    var y = powerUp.Location.Y;

                    var position = levelLoader.Maps[x, y];
                    DrawTile(position, powerUp.Type, GetPowerUpColor(powerUp.Type));

                }

                #endregion

                #region Draw Bots

                foreach (var x in GameState.Bots.Select((bot, index) => new { bot, index }))
                {
                    var translatedPosition = levelLoader.Maps[x.bot.Value.X, x.bot.Value.Y];
                    DrawTile(translatedPosition, x.index, GetBotColor(x.index));
                }

                #endregion

            }


            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void HandelInput(GameTime gameTime)
        {
            _oneShotMouseState = OneShotMouseButton.GetState();

            if (_oneShotMouseState.LeftButton == ButtonState.Pressed)
            {
                if (OneShotMouseButton.HasNotBeenPressed(true))
                {
                    _mouseLeftPressed = true;
                }
            }
        }

        private Rectangle Rectangle(Point position) => new Rectangle(position.X, position.Y, _padding + 1, _padding + 1);

        /*        private Matrix CalculateTranslation(CameraWindow window)
                {
                    //var dx = (size.X / 2) - position.X;
                    //var dy = (size.Y / 2) - position.Y;
                    // var dx = (_graphics.PreferredBackBufferWidth / 2) - x;
                    // var dy = (_graphics.PreferredBackBufferHeight / 2) - y;

                    var map = levelLoader.Maps;

                //    var bot = GameState.Bots[window.BotIndex];
                //    var normalisedPosition = map[bot.CurrentPosition.X, bot.CurrentPosition.X];

        *//*            var dx = (window.RenderTarget.Width / 2) - normalisedPosition.X;
                    var dy = (window.RenderTarget.Height / 2) - normalisedPosition.Y;
        *//*

                    var paddingA = _padding;

                    *//*            dx = MathHelper.Clamp(dx, 500, _padding / 2);
                                dy = MathHelper.Clamp(dy, 500, _padding / 2);*//*

                    var mapSizeY = 4160;
                    var mapSizeX = 4060;

               //     dx = MathHelper.Clamp(dx, -mapSizeX + window.RenderTarget.Width + (-paddingA / 2), -paddingA / 2);
               //     dy = MathHelper.Clamp(dy, -mapSizeY + window.RenderTarget.Height + (paddingA / 2), (paddingA) / 2);

               //     return Matrix.CreateTranslation(dx, dy, 0f);
                }*/

        /*        private void DrawWindow(CameraWindow window)
                {
                    //Setting the "camera"
                    _graphicsDevice.SetRenderTarget(window.RenderTarget);
                    // var bot = cyFiLog[_currentTick].Bots[window.BotIndex];
                    Point[,] map = levelLoader.Maps;
                    var transform = CalculateTranslation(window);
                    _spriteBatch.Begin(transformMatrix: transform);

                    var mapSizeY = 4160;
                    var mapSizeX = 4060;


                    //** Setting the background **
                    var rectangleBackground = new Rectangle(0, -60, mapSizeX, mapSizeY);
                    //_spriteBatch.Draw(backgroundTextures[bot.CurrentLevel], rectangleBackground, Color.White);

                    //** Drawing the map **

        *//*            for (int r = 0; r < GameState.Land.GetLength(0); r++)
                    {
                        for (int c = 0; c < GameState.Land.GetLength(r); c++)
                        {
                            var position = map[r, c];
                            DrawTile(position, (int)GameState.Land[r][c]);

                        }
                    }*//*

                    #region Draw Changes
                    *//*            var changeLog = cyFiLog[_currentTick].Levels[bot.CurrentLevel].ChangeLog;

                                changeLog.ForEach(t =>
                                {
                                    //Point position = level[t.pointX, t.pointY];
                                    cyFiLog[0].Levels[bot.CurrentLevel].map[t.pointX][t.pointY] = t.tileType;

                                    // DrawTile(position, bot.CurrentLevel, t.tileType);
                                });*//*
                    #endregion

                    #region Draw Bots
                    *//*            for (int i = 0; i < cyFiLog[_currentTick].Bots.Count; i++)
                                {
                                    var otherBot = cyFiLog[_currentTick].Bots[i];
                                    if (otherBot.CurrentLevel == bot.CurrentLevel)
                                    {
                                        Point positionBot = level[otherBot.Hero.XPosition, otherBot.Hero.YPosition];
                                        Rectangle playerRect = playerRectangle(positionBot.X, positionBot.Y);
                                        bool isLadder = cyFiLog[0].Levels[otherBot.CurrentLevel].map[otherBot.Hero.XPosition][otherBot.Hero.YPosition] == 5 ||
                                            cyFiLog[0].Levels[otherBot.CurrentLevel].map[otherBot.Hero.XPosition][otherBot.Hero.YPosition + 1] == 5 ||
                                            cyFiLog[0].Levels[otherBot.CurrentLevel].map[otherBot.Hero.XPosition + 1][otherBot.Hero.YPosition] == 5 ||
                                            cyFiLog[0].Levels[otherBot.CurrentLevel].map[otherBot.Hero.XPosition + 1][otherBot.Hero.YPosition + 1] == 5;
                                        bool isDigging = cyFiLog[0].Levels[otherBot.CurrentLevel].map[otherBot.Hero.NextXPosition][otherBot.Hero.NextYPosition] == 1 ||
                                                                cyFiLog[0].Levels[otherBot.CurrentLevel].map[otherBot.Hero.NextXPosition + 1][otherBot.Hero.NextYPosition + 1] == 1;

                                        //_spriteBatch.Draw(mockTexture, playerRect, GetBotColour(i));
                                        bool flip = _currentTick > 0 && cyFiLog[_currentTick].Bots[i].Hero.XPosition < cyFiLog[_currentTick - 1].Bots[i].Hero.XPosition;

                                        DrawBot(otherBot, playerRect, i, otherBot.NickName, isLadder, isDigging, flip, _currentTick);
                                    }
                                }*//*
                    #endregion

                    _spriteBatch.End();
                    _graphicsDevice.SetRenderTarget(null);
                }*/
        private void DrawTile(Point position, int tileType, Color color)
        {
            Texture2D texture = GetTileTexture(tileType);
            Rectangle rect = Rectangle(position);

            _spriteBatch.Draw(texture, rect, color);

            //   _spriteBatch.DrawString(hudFont, $"x:{position.X}", new Vector2(position.X, position.Y), Color.Yellow);
            //   _spriteBatch.DrawString(hudFont, $"y:{position.Y}", new Vector2(position.X, position.Y + 10), Color.Yellow);


        }


        private Texture2D GetTileTexture(int tileType)
        {
            switch (tileType)
            {
                case 1:
                    return EmptyTexture;
                case 2:
                    return EmptyTexture;
                case 3:
                    return EmptyTexture;
                case 4:
                    return EmptyTexture;
                default:
                    return EmptyTexture;
            }
        }

        private Color GetBotColor(int bot)
        {
            switch (bot)
            {
                case 0:
                    return Color.Red;
                case 1:
                    return Color.Blue;
                case 2:
                    return Color.Green;
                case 3:
                    return Color.Orange;
                default: return Color.White;
            }
        }

        private Color GetPowerUpColor(int powerUp)
        {
            /*
             *         TerritoryImmunity = 1,
             *         Unprunable = 2,
             *         Freeze = 3,
             *         TrailProtection = 4,
             *         SuperFertilizer = 5,
             */

            switch (powerUp)
            {
                case 0:
                    return Color.Purple;
                case 1:
                    return Color.HotPink;
                case 2:
                    return Color.MidnightBlue;
                case 3:
                    return Color.MistyRose;
                case 4:
                    return Color.Lavender;
                case 5:
                    return Color.LightBlue;
                default: return Color.LightBlue;
            }

        }
        private Color GetColor(int tileType)
        {
            /*  
             *  Bot0Territory = 0,
             *  Bot1Territory = 1,
             *  Bot2Territory = 2,
             *  Bot3Territory = 3,
             *  Bot0Trail     = 4,
             *  Bot1Trail     = 5,
             *  Bot2Trail     = 6,
             *  Bot3Trail     = 7,
             *  OutOfBounds   = 254,
             *  Unclaimed     = 255,
            */
            switch (tileType)
            {
                case 0:
                    return Color.DarkRed;
                case 1:
                    return Color.DarkBlue;
                case 2:
                    return Color.Yellow;
                case 3:
                    return Color.Red;
                case 4:
                    return Color.PaleVioletRed;
                case 5:
                    return Color.LightBlue;
                case 6:
                    return Color.MediumOrchid;
                case 7:
                    return Color.Maroon;
                case 8:
                    return Color.Black;
                default: return Color.White;
            }
        }

        /*       private void InitialiseBotWindows()
               {
                   var windowWidth = _graphics.PreferredBackBufferWidth / 2;
                   var windowHeight = _graphics.PreferredBackBufferHeight / 2;
                   Point windowDimensions = new(windowWidth - 1, windowHeight - 1);

                   if (GameState != null && GameState.Bots.Count > 0)
                   {
                       var bots = GameState.Bots.Keys.ToList();

                       Dictionary<Guid, Rectangle> availableWindowBounds = new()
                       {
                           { bots[0], new Rectangle(new(0, 0), windowDimensions)},
                           //   { bots[1], new Rectangle(new(windowWidth + 2, 0), windowDimensions)},
                           //   { bots[2], new Rectangle(new(0, windowHeight + 2), windowDimensions) },
                           //   { bots[3], new Rectangle(new(windowWidth + 2, windowHeight + 2), windowDimensions)}

                       };

                       _windows = availableWindowBounds.Select(i => GetWindow(i.Value, i.Key)).ToArray();
                   }
               }*/

        private CameraWindow GetWindow(Rectangle bounds, Guid index)
        {
            return new(index, bounds, _graphics);
        }

        private void MinimiseAllWindows()
        {
            foreach (var win in _windows)
            {
                win.Minimise();
            }
        }

        private bool CheckIfButtonWasClicked(Button button)
        {
            if (_oneShotMouseState.X >= button.Position.X && _oneShotMouseState.X <= (button.Position.X + button.Dimensions.X))
            {
                if (_oneShotMouseState.Y >= button.Position.Y && _oneShotMouseState.Y <= (button.Position.Y + _playToggleButton.Dimensions.Y) && button.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        public sealed class LevelLoader
        {
            public int Rows { get; }
            public int Columns { get; }
            public int CurrentLevel { get; private set; }
            private readonly int _padding;

            public Point[,] Maps { get; private set; }

            public LevelLoader(int paddingBetweenCells, int rows, int columns)
            {
                Rows = rows;
                Columns = columns;
                _padding = paddingBetweenCells;

                Maps = new Point[Rows, Columns];
            }

            public void AddFlipLevel(int startingPadding)
            {
                var paddingX = startingPadding;
                var paddingY = 0;
                for (int r = 0; r < Rows; r++)
                {
                    paddingY = _padding * -Columns;

                    if (r == Rows - 1)
                    {
                        // MaxHeight = (paddingY + 1) * -1;
                    }

                    for (int c = 0; c < Columns; c++)
                    {
                        Maps[r, c] = new Point(r + paddingX, (Columns - 1 - c) - paddingY);
                        paddingY += _padding;

                        //   if (c == Columns - 1) MaxWidth = paddingX + 1;

                    }
                    paddingX += _padding;

                }
            }
            public void AddLevel(int startingPadding)
            {
                var paddingX = startingPadding;
                var paddingY = 0;
                for (int r = 0; r < Rows; r++)
                {
                    for (int c = 0; c < Columns; c++)
                    {
                        Maps[r, c] = new Point(r + paddingX, c + paddingY);
                        paddingY += _padding;
                    }
                    paddingX += _padding;
                    paddingY = 0;
                }
            }
        }

    }
}
