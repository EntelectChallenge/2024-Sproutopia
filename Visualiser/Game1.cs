using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Runner.DTOs;
using Sproutopia;
using Sproutopia.Models;
using Sproutopia.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Visualiser.Utils.Camera;
using Forms = System.Windows.Forms;


namespace Visualiser
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private SproutopiaEngine sproutopiaEngine;
        private bool _mouseLeftPressed;
        private MouseState _oneShotMouseState;
        private LevelLoader levelLoader;
        private HubConnection connection;
        private static IConfigurationRoot Configuration;
        private KeyboardState keyboardState;
        private readonly IConfiguration GameConfig;
        private IConfigurationBuilder GameConfigBuilder;
        private bool _connectToEngine;
        private Stopwatch stopwatch = Stopwatch.StartNew();


        private GameStateDto GameState { get; set; }
        private List<GameStateDto> GameStateLogs { get; set; }
        private CameraWindow[] _windows;
        private int _heroToFollow;

        #region Cursor
        Vector2 _cursor;
        public int _speed;
        private Vector2 _minPos, _maxPos;
        private Matrix _cursorTranslation;
        #endregion

        public bool _enableCursor = true;
        private int _currentTick { get; set; }
        private int _padding { get; set; }
        private int _initalPadding { get; set; }
        private Vector2 _botControllerPlacement { get; set; }
        private SpriteFont _arial { get; set; }

        private int _delay = 1000;
        private int _direction = 1;


        #region Declare Textures
        Texture2D buttonPlayTexture;
        private Texture2D UnClaimedLandTextures;
        private Texture2D ClaimedLandTextures;
        private Texture2D TrailTexture;
        private Texture2D EmptyTexture;
        private List<Texture2D> PlayerTextures;

        private Texture2D SuperFertilizerTexture;
        private Texture2D TerritoryImmunityTexture;
        private Texture2D UnprunableTexture;
        private Texture2D FreezeTexture;
        private Texture2D TrailProtectionTexture;
        // private BotStateSprite botStateSprites;

        #endregion

        #region UI Elements

        // Game engine buttons
        private Button _playToggleButton;
        private Button _pauseButton;
        private Button _stepButton;
        private Button _continueButton;

        // Log replay buttons
        private Button _replayButton;
        private Button _forwardButton;
        private Button _backwardButton;
        private Button _pauseLogButton;
        private Button _walkButton;
        private Button _runButton;
        private Button _sprintButton;

        #endregion

        public Game1(bool connectToEngine)
        {
            _currentTick = 0;
            _initalPadding = 0;
            _heroToFollow = 0;
            _graphics = new(this);
            _cursor = new(_initalPadding, 0);
            Content.RootDirectory = "Content";
            GameStateLogs = [];
            IsMouseVisible = true;
            _mouseLeftPressed = false;
            IsMouseVisible = true;
            GameConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            _connectToEngine = connectToEngine;
        }

        protected override void Initialize()
        {
            var GameSettings = GameConfig.GetSection("GameSettings");
            var VisualiserSettings = GameConfig.GetSection("VisualiserSettings");
            var rows = GameSettings.GetValue<int>("Rows");
            var cols = GameSettings.GetValue<int>("Cols");

            if (!_connectToEngine)
            {
                var logFilePath = string.Empty;

                var thread = new Thread(() =>
                {
                    Forms.OpenFileDialog openFileDialog = new()
                    {
                        Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                        FilterIndex = 2,
                        RestoreDirectory = true
                    };

                    if (openFileDialog.ShowDialog() == Forms.DialogResult.OK)
                    {
                        logFilePath = openFileDialog.FileName;
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();

                var logType = Helpers.IdentifyLogFile(logFilePath);

                switch (logType)
                {
                    case LogType.FullLog:
                        var logs = Helpers.LoadJson<GameStateDto>(logFilePath, ["\r\n", "\n", "\r"]);

                        foreach (var log in logs)
                        {
                            GameStateLogs.Add(log);
                        }

                        break;

                    case LogType.DiffLog:
                        var diffLogs = Helpers.LoadJson<DiffLog>(logFilePath, ["\r\n", "\n", "\r"]);

                        var newTerritory = Helpers.CreateJaggedArray<int[][]>(rows, cols);
                        Helpers.SetAllValues(newTerritory, 255);

                        var newTrails = Helpers.CreateJaggedArray<int[][]>(rows, cols);
                        Helpers.SetAllValues(newTrails, 255);

                        var gameState = new GameStateDto(
                            timeStamp: DateTime.Now,
                            currentTick: 0,
                            botSnapshots: [],
                            territory: newTerritory,
                            trails: newTrails,
                            leaderBoard: [],
                            powerUps: Array.Empty<PowerUpLocation>(),
                            weeds: Helpers.CreateJaggedArray<bool[][]>(rows, cols)
                            );

                        foreach (var diffLog in diffLogs)
                        {
                            gameState = gameState.ApplyDiff(diffLog);
                            GameStateLogs.Add(gameState);
                        }

                        break;

                    default:
                        Console.WriteLine("Unknown log file format.");
                        return;
                }
            }

             _oneShotMouseState = OneShotMouseButton.GetState();

            levelLoader = new(_padding, rows, cols);
            levelLoader.AddLevel(_initalPadding);

            Console.WriteLine("Initialized bot windows...");
            #region Screen Options
            //* set to screen size * //
            _graphics.PreferredBackBufferWidth = VisualiserSettings.GetValue<int>("WindowWidth");
            _graphics.PreferredBackBufferHeight = VisualiserSettings.GetValue<int>("WindowHeight");

            //* apply changes * //
            _graphics.ApplyChanges();
            _graphicsDevice = _graphics.GraphicsDevice;
            #endregion

            #region Asset Options
            _padding = 16; //VisualiserSettings.GetValue<int>("AssetSize");
            #endregion

            #region Cursor Options
            _enableCursor = VisualiserSettings.GetValue<bool>("EnableCursor");
            _speed = VisualiserSettings.GetValue<int>("CursorSpeed");
            #endregion

            _oneShotMouseState = OneShotMouseButton.GetState();

            levelLoader = new(_padding, rows, cols);
            levelLoader.AddLevel(_initalPadding);

            _botControllerPlacement = new(20, _graphics.PreferredBackBufferHeight - 300);


            if (_connectToEngine)
            {
                connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/visualiserhub")
                    .WithAutomaticReconnect()
                    .Build();

                connection.On<GameStateDto>(VisualiserCommands.ReceiveInitialGameState, state =>
                {
                    GameState = state;
                });

            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            var GameSettings = GameConfig.GetSection("GameSettings");
            var rows = GameSettings.GetValue<int>("Rows");
            var cols = GameSettings.GetValue<int>("Cols");

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

            //PowerUps

            this.SuperFertilizerTexture = Content.Load<Texture2D>("PowerUps/superFertilizer");
            this.FreezeTexture = Content.Load<Texture2D>("PowerUps/freeze2");
            this.TerritoryImmunityTexture = Content.Load<Texture2D>("PowerUps/territoryImmunity2");
            this.UnprunableTexture = Content.Load<Texture2D>("PowerUps/unprunable2");
            this.TrailProtectionTexture = Content.Load<Texture2D>("PowerUps/trailProtection2");

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
                dimensions: new(6, 68),
                position: new(44, 0),
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
                dimensions: new(64, 86),
                position: new(90, 0),
                name: "pause",
                id: 35,
                visible: true,
                layerDepth: 1.0f);

            _replayButton = new(
                staticImage: Content.Load<Texture2D>("replay"),
                clickedImage: Content.Load<Texture2D>("replay"),
                dimensions: new(64, 64),
                position: new(42 * 0 + _padding, _padding * rows),
                name: "replay",
                id: 36,
                visible: true,
                layerDepth: 1.0f);

            _backwardButton = new(
                staticImage: Content.Load<Texture2D>("backward"),
                clickedImage: Content.Load<Texture2D>("backward"),
                dimensions: new(64, 64),
                position: new(42 * 2 + _padding, _padding * rows),
                name: "backward",
                id: 37,
                visible: true,
                layerDepth: 1.0f);

            _pauseLogButton = new(
                staticImage: Content.Load<Texture2D>("pause"),
                clickedImage: Content.Load<Texture2D>("pause"),
                dimensions: new(64, 64),
                position: new(42 * 4 + _padding, _padding * rows),
                name: "pauselog",
                id: 38,
                visible: true,
                layerDepth: 1.0f);

            _forwardButton = new(
                staticImage: Content.Load<Texture2D>("forward"),
                clickedImage: Content.Load<Texture2D>("forward"),
                dimensions: new(64, 64),
                position: new(42 * 6 + _padding, _padding * rows),
                name: "forward",
                id: 39,
                visible: true,
                layerDepth: 1.0f);

            _walkButton = new(
                staticImage: Content.Load<Texture2D>("walk"),
                clickedImage: Content.Load<Texture2D>("walk"),
                dimensions: new(64, 64),
                position: new(42 * 8 + _padding, _padding * rows),
                name: "walk",
                id: 40,
                visible: true,
                layerDepth: 1.0f);

            _runButton = new(
                staticImage: Content.Load<Texture2D>("run"),
                clickedImage: Content.Load<Texture2D>("run"),
                dimensions: new(64, 64),
                position: new(42 * 10 + _padding, _padding * rows),
                name: "run",
                id: 41,
                visible: true,
                layerDepth: 1.0f);

            _sprintButton = new(
                staticImage: Content.Load<Texture2D>("sprint"),
                clickedImage: Content.Load<Texture2D>("sprint"),
                dimensions: new(64, 64),
                position: new(42 * 12 + _padding, _padding * rows),
                name: "sprint",
                id: 42,
                visible: true,
                layerDepth: 1.0f);


            #endregion
        }

        protected override void Update(GameTime gameTime)
        {
            if (_connectToEngine)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Exit();
                }

                #region Keyboard Inputs 
                keyboardState = Keyboard.GetState();

                //Move map

                var nextCursor = _cursor;

                if (keyboardState.IsKeyDown(Keys.Up)) nextCursor.Y -= 1 * _speed;
                if (keyboardState.IsKeyDown(Keys.Down)) nextCursor.Y += 1 * _speed;
                if (keyboardState.IsKeyDown(Keys.Left)) nextCursor.X -= 1 * _speed;
                if (keyboardState.IsKeyDown(Keys.Right)) nextCursor.X += 1 * _speed;

                _cursor = Vector2.Clamp(nextCursor, new(_initalPadding, 0), new(_initalPadding + levelLoader.MapPxWidth - EmptyTexture.Width, levelLoader.MapPxHight - EmptyTexture.Height));
                CalculateCursorTranslation();

                #endregion


                HandelInput(gameTime);
                _playToggleButton.UpdateButton();
                _pauseButton.UpdateButton();
                _stepButton.UpdateButton();

                #region UI Inputs

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
                        else if (CheckIfButtonWasClicked(_playToggleButton)
                            && connection.State != HubConnectionState.Reconnecting)
                            connection.StartAsync().Wait();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                }

                #endregion
            }
            else
            {
                HandelInput(gameTime);
                
                _replayButton.UpdateButton();
                
                _forwardButton.UpdateButton();
                _backwardButton.UpdateButton();
                _pauseLogButton.UpdateButton();
                
                _walkButton.UpdateButton();
                _runButton.UpdateButton();
                _sprintButton.UpdateButton();

                if (_mouseLeftPressed)
                {
                    _mouseLeftPressed = false;

                    if (CheckIfButtonWasClicked(_replayButton))
                        _currentTick = 0;

                    if (CheckIfButtonWasClicked(_forwardButton))
                    {
                        _direction = 1;
                    }
                    if (CheckIfButtonWasClicked(_backwardButton))
                    {
                        _direction = -1;
                    }
                    if (CheckIfButtonWasClicked(_pauseLogButton))
                    {
                        _direction = 0;
                    }
                    if (CheckIfButtonWasClicked(_walkButton))
                    {
                        _delay = 1000;
                    }
                    if (CheckIfButtonWasClicked(_runButton))
                    {
                        _delay = 300;
                    }
                    if (CheckIfButtonWasClicked(_sprintButton))
                    {
                        _delay = 20;
                    }
                }

                if (stopwatch.ElapsedMilliseconds < _delay)
                    return;

                stopwatch.Restart();

                if (GameStateLogs.Count > 0 && _currentTick < GameStateLogs.Count && _currentTick >= 0)
                {
                    GameState = GameStateLogs[_currentTick];
                    _currentTick += _direction;

                    if (_currentTick < 0)
                    {
                        _currentTick += 1;
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            if (GameState != null)
            {
                if (_connectToEngine)
                    _spriteBatch.Begin(transformMatrix: _cursorTranslation);
                else
                    _spriteBatch.Begin();

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
                    DrawPowerUpTile(position, powerUp.Type, Color.White);

                }

                #endregion

                #region Draw Bots

                foreach (var x in GameState.BotSnapshots.Select((bot, index) => new { bot, index }))
                {
                    var translatedPosition = levelLoader.Maps[x.bot.X, x.bot.Y];
                    DrawTile(translatedPosition, x.index, GetBotColor(x.index));
                }

                #endregion


                if (_enableCursor)
                {
                    DrawCursor(_cursor);
                }

                #region Print Game State

                var sb = new StringBuilder();
                foreach (var x in GameState.BotSnapshots.Select((bot, index) => new { bot, index }))
                {
                    sb.AppendLine($"{x.index}:{x.bot.BotId.ToString().Substring(0, 4)}...");
                }

                _spriteBatch.DrawString(_arial, sb.ToString(), new Vector2(810, 10), Color.Black);
                _spriteBatch.DrawString(_arial, GameState.ToString(), new Vector2(810, 100), Color.Black);

                #endregion

                _spriteBatch.End();
            }

            #region Draw UI 
            _spriteBatch.Begin();

            if (_connectToEngine)
            {
                _spriteBatch.Draw(_playToggleButton.Texture, UIRectangle(_playToggleButton.Position, 40), Color.White);
                _spriteBatch.Draw(_pauseButton.Texture, UIRectangle(_pauseButton.Position, 40), Color.White);
                _spriteBatch.Draw(_stepButton.Texture, UIRectangle(_stepButton.Position, 40), Color.White);
                //  _spriteBatch.Draw(_continueButton.Texture, _continueButton.Position, Color.White);
            }
            else
            {
                _spriteBatch.Draw(_replayButton.Texture, UIRectangle(_replayButton.Position, 40), Color.White);
                _spriteBatch.Draw(_forwardButton.Texture, UIRectangle(_forwardButton.Position, 40), Color.White);
                _spriteBatch.Draw(_backwardButton.Texture, UIRectangle(_backwardButton.Position, 40), Color.White);
                _spriteBatch.Draw(_pauseLogButton.Texture, UIRectangle(_pauseLogButton.Position, 40), Color.White);
                _spriteBatch.Draw(_walkButton.Texture, UIRectangle(_walkButton.Position, 40), Color.White);
                _spriteBatch.Draw(_runButton.Texture, UIRectangle(_runButton.Position, 40), Color.White);
                _spriteBatch.Draw(_sprintButton.Texture, UIRectangle(_sprintButton.Position, 40), Color.White);
            }

            _spriteBatch.End();
            #endregion

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

        private Rectangle Rectangle(Point position) => new Rectangle(position.X, position.Y, _padding, _padding);
        private Rectangle UIRectangle(Point position, int buttonSize) => new Rectangle(position.X, position.Y, buttonSize, buttonSize);

        private Rectangle PowerUpRectangle(Point position)
        {
            var percentageIncrease = _padding + (10 / 100);
            var puSize = percentageIncrease + _padding;
            return new(position.X - (percentageIncrease / 2), position.Y - (percentageIncrease / 2), puSize, puSize);
        }



        private Matrix CalculateTranslation(CameraWindow window, VertexPosition cursor)
        {
            /*          var dx = (size.X / 2) - position.X;
                        var dy = (size.Y / 2) - position.Y;
                        var dx = (_graphics.PreferredBackBufferWidth / 2) - x;
                        var dy = (_graphics.PreferredBackBufferHeight / 2) - y;
            */
            var map = levelLoader.Maps;

            var dx = (window.RenderTarget.Width / 2) - cursor.Position.X;
            var dy = (window.RenderTarget.Height / 2) - cursor.Position.Y;

            var paddingA = _padding;

            dx = MathHelper.Clamp(dx, 500, _padding / 2);
            dy = MathHelper.Clamp(dy, 500, _padding / 2);

            var mapSizeY = 4160;
            var mapSizeX = 4060;

            dx = MathHelper.Clamp(dx, -mapSizeX + window.RenderTarget.Width + (-paddingA / 2), -paddingA / 2);
            dy = MathHelper.Clamp(dy, -mapSizeY + window.RenderTarget.Height + (paddingA / 2), (paddingA) / 2);

            return Matrix.CreateTranslation(dx, dy, 0f);
        }

        private void DrawTile(Point position, int tileType, Color color)
        {
            Texture2D texture = GetTileTexture(tileType);
            Rectangle rect = Rectangle(position);

            _spriteBatch.Draw(texture, rect, color);

            //   _spriteBatch.DrawString(hudFont, $"x:{position.X}", new Vector2(position.X, position.Y), Color.Yellow);
            //   _spriteBatch.DrawString(hudFont, $"y:{position.Y}", new Vector2(position.X, position.Y + 10), Color.Yellow);


        }

        private void DrawPowerUpTile(Point position, int powerUpType, Color color)
        {
            Texture2D texture = GetPowerUpTexture(powerUpType);
            Rectangle rect = PowerUpRectangle(position);
            _spriteBatch.Draw(texture, rect, color);
        }

        private void DrawCursor(Vector2 position) =>
            _spriteBatch.Draw(EmptyTexture, position, Color.OrangeRed);

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

        private Texture2D GetPowerUpTexture(int powerUpType)
        {
            switch (powerUpType)
            {
                case 1:
                    return this.TerritoryImmunityTexture;
                case 2:
                    return this.UnprunableTexture;
                case 3:
                    return this.FreezeTexture;
                case 4:
                    return this.TrailProtectionTexture;
                case 5:
                    return this.SuperFertilizerTexture;
                default:
                    return this.EmptyTexture;
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
                    return Color.DarkGreen;
                case 3:
                    return Color.SandyBrown;
                case 4:
                    return Color.Pink;
                case 5:
                    return Color.LightBlue;
                case 6:
                    return Color.LightGreen;
                case 7:
                    return Color.Yellow;
                case 8:
                    return Color.Black;
                default: return Color.White;
            }
        }

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
            public int MapPxHight
            {
                get { return Rows * _padding; }
                private set { }
            }
            public int MapPxWidth
            {
                get { return Columns * _padding; }
                private set { }
            }

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

                    for (int c = 0; c < Columns; c++)
                    {
                        Maps[r, c] = new Point(r + paddingX, (Columns - 1 - c) - paddingY);
                        paddingY += _padding;

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
                        Maps[r, c] = new Point(paddingX, paddingY);
                        paddingY += _padding;
                    }
                    paddingX += _padding;
                    paddingY = 0;
                }
            }
        }

        //TODO: move to Utils
        public void CalculateCursorTranslation()
        {
            var widthval1 = Math.Max(_graphics.PreferredBackBufferWidth, levelLoader.MapPxWidth);
            var widthval2 = Math.Min(_graphics.PreferredBackBufferWidth, levelLoader.MapPxWidth);

            var dx = (_graphics.PreferredBackBufferWidth / 2) - _cursor.X;
            dx = MathHelper.Clamp(dx, -widthval1 + widthval2 + (EmptyTexture.Width / 2) - 10, EmptyTexture.Width);

            var heighthval1 = Math.Max(_graphics.PreferredBackBufferWidth, levelLoader.MapPxWidth);
            var heightval2 = Math.Min(_graphics.PreferredBackBufferWidth, levelLoader.MapPxWidth);

            var dy = (_graphics.PreferredBackBufferHeight / 2) - _cursor.Y;
            dy = MathHelper.Clamp(dy, -heighthval1 + heightval2 + (EmptyTexture.Height / 2) - 10, EmptyTexture.Height);

            _cursorTranslation = Matrix.CreateTranslation(dx, dy, 0f);
        }

    }

}
