# Entelect Challenge 2024 - Sproutopia 🌱 - Release 2024.0.0

---

>### _NB:_ Change log 
> Initial Release

- Entelect Challenge 2024 - Sproutopia 🌱 - Release 2024.0.0
  - [The Game](#the-game)
  - [General](#general)
  - [Rules](#rules)
  - [Game Ticks](#game-ticks)
  - [Farmers](#farmers)
  - [Visibility](#visibility)
  - [The Commands](#the-commands)
    - [Command: UP](#command-up)
    - [Command: DOWN](#command-down)
    - [Command: LEFT](#command-left)
    - [Command: RIGHT](#command-right)
  - [Command Structure](#command-structure)
  - [The Power-Ups](#the-power-ups)
    - [Temporarily Territory Immunity](#temporarily-territory-immunity)
    - [Unprunable](#unprunable)
    - [Freeze](#freeze)
  - [The Super Power-Ups](#the-super-power-ups)
    - [Trail Protection](#trail-protection)
    - [Super Fertilizer](#super-fertilizer)
  - [GAME TICK PAYLOAD](#game-tick-payload)
  - [ENDGAME](#endgame)
  - [SCORING](#scoring)

---
## The Game

Sproutopia is the official territorial stylized game for the Entelect Challenge 2024. 🌿

Sproutopia was once an empty land, waiting for someone to give it life. Then came the farmers/ bots , claiming land for their crops to grow. The farmers have to work hard to collect as much land as they can before time runs out.

But be careful! There are other farmers in Sproutopia, and they might try to hinder you. Keep an eye out for them!

---
## General

📒 _Note: All configuration is subject to change while we balance the game. For the latest configurations please navigate to `2024-Sproutopia/Sproutopia` for `appsettings.json`, `appsettings.Development.json` and `appsettings.Production.json`.

---
## Rules

Throughout the game, farmers (`bots`) must gain as much territory as possible. To gain territory a player needs to completely encircle a group of tiles returning back to their own territory. 

While player A is encircling a potential territory a trail is left behind. This trail can be cut off by player B and Player B will now gain all the territory of Player A (this is called Pruning) Player A will be respawned in the next tick. Similarly, if player A runs into their own trail, they will be respawned and their territory will become empty again. 

Power-ups will randomly pop-up and can be picked up by running through it. 

Weeds will also randomly spawn, and will eventually grow to a maximum size, cleaning a weed will reveal a super power-up. You clean a weed by completely encircling it, but be careful to not run into the weed, as this will cause your bot to respawn. 

Respawning makes players lose all their power-ups.

Players can only have one power-up and one super power-up at a time.

---
## Game Ticks

Sproutopia is a real time game that utilises `Ticks`, as a unit of time to keep track of the game.

---

## Farmers

Four Farmers (`bots`) will be placed into the map together, and have to gather / conquer as much territory as possible before time runs out! 

---

## Visibility

The heroes can only see a certain square distance around themselves (8 tiles). 

---
## The Commands
When a player is spawned, they spawn in an IDLE state, and will begin moving as soon as the first command of movement is sent. Players may traverse the map via basic movement.

📒 _Note: A player has continuous movement and will move in the last direction until a new direction is given or it is at the edge of the map where the bot will stop_.

The Commands are as follows:

* `UP` - 1
* `DOWN`- 2
* `LEFT` - 3
* `RIGHT` - 4

### Command: UP

Allows the player to move up continuously

---

### Command: DOWN

Allows the player to move down continuously

---

### Command: LEFT

Allows the player to move left continuously

---

### Command: RIGHT

Allows the player to move right continuously

---

## Command Structure

The command is sent with the following structure:

```json
{
    "Action" : 1,                                    // UP action type - int
    "BotId" : "410d392c-ecf5-43b9-a228-299c0a8d224a" // Bot ID - string/UUID/GUID
}
```

## The power-ups

These activate as soon as the bot picks it up and lasts for a certain number of ticks, which are all configurable values.

### Temporary Territory Immunity

Other players cannot enter your territory, and players within your territory are stunned. This lasts for 20 Ticks.

### Unprunable

A player cannot be pruned by another player (but they can be pruned by themselves). This lasts for 20 Ticks.

### Freeze

Other players are unable to move for 10 Ticks.

## The Super power-ups 

These power-ups are spawned only once a weed is cleared.

### Trail protection 

A player's trail is protected from themselves, and if they run into their own trail, they will be reset to the start of their trail. Once picked up, this powerup remains with the player until it is used. 

### Super fertilizer 

When a player runs over neutral territory, they immediately get the tiles to the left and right of them added to their territory. They do not have a trail. This lasts for 40 Ticks.


## Game Tick Payload 
After every `Tick` the `runnerHub` will send a `BotStateDTO` response.

This will consist of the following values:  
 - `DirectionState` - Current bot direction
 - `ElapsedTime` - Time elapsed since game started
 - `GameTick` - Current game tick
 - `PowerUp` - Current power-up (if any) your bot has
 - `SuperPowerUp` - Current super power-up (if any) your bot has
 - `LeaderBoard` - Represents the current leader board 
 - `HeroWindow` - The number of tiles a hero can view around themselves
 - `X` - Hero's current x position
 - `Y` - Hero's current y position

## Endgame
When a farmer has every block of territory, the game ends and that hero is the winner. Alternatively, when the timer runs out, the farmer with the most territory wins. Subsequent farmers with the next largest territory come in second, third and fourth. In the case of equivalent land claimed we will place the player with the higher bonus above the player they tied with.

---

## Scoring 

Farmers (`Bots`) get scores based on the amount of `territory` obtained. You get bonus points for bigger sections of territory claimed (example : 25 tiles <= 1 times. 50 tiles <= 1.5 times. 75 tiles <= 2 times etc).