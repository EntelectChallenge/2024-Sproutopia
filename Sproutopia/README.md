# Game Engine

This project contains the 2024-Sproutopia project for the Entelect Challenge 2024

## Game Rules

The game for 2024 is Sproutopia. Detailed game rules can be found [here](GAMERULES.md)

## Configuration Options

The engine will respect the following environment variables to change how the game is run:

- `GameSettings.NumberOfPlayers`
    - This sets the expected amount of bots/players to connect before a game will be run. [4 players]
 
- `GameSettings.Seed`
    - This sets the seeds of the world that will be generated in this game, the seed is used to distribute items about the world. 

- `GameSettings.PlayerWindowSize`
    - This sets the square area visible to the bot/player. [8 tiles]

- `GameSettings.PlayerQueueSize`
    - This sets how many commands a player can have in their queue at a time. [10 commands]

- `GameSettings.MaxTicks`
    - This sets the maximum amount of ticks a game will have before it ends. [100 ticks]

When these are not specified, the values present in `/Sproutopia/appsettings.Development.json` will be used.


