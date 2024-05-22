# Entelect Challenge 2024 - Sproutopia 🌱 - Release 2024.0.0

---

>### _NB:_ Change log 
> Initial Release

- Entelect Challenge 2024 - Sproutopia 🌱 - Release 2024.0.0
  - [The Game](#the-game)
  - [General](#general)
  - [Rules](#rules)
    - [Territory](#more-on-territory)
    - [Interactions Between Farmers](#more-on-interactions-between-farmers)
      - [Pruning](#pruning)
      - [Encircling](#encircling)
      - [Stealing of Territory](#stealing-of-territory)
      - [Collisions](#collisions)
    - [Power-Ups](#power-ups)
      - [Temporary Territorial Immunity](#temporary-territorial-immunity)
      - [Unprunable](#unprunable)
      - [Freeze](#freeze)
    - [Super Power-Ups](#super-Power-Ups)
      - [Trail Protection](#trail-protection)
      - [Super Fertilizer](#super-fertilizer)
    - [Weeds](#weeds)
  - [Game Ticks](#game-ticks)
  - [Farmers](#farmers)
  - [Visibility](#visibility)
  - [Commands](#commands)
    - [Command Structure](#command-structure)
    - [Command Processing Order](#command-processing-order)
  - [Game Tick Payload](#game-tick-payload)
  - [Endgame](#endgame)
  - [Scoring](#scoring)

---
## The Game

Sproutopia is the official territorial stylized game for the Entelect Challenge 2024. 🌿

Sproutopia was once an empty land, waiting for someone to give it life. Then came the farmers (bots), claiming land for their crops to grow. The farmers have to work hard to collect as much land as they can before time runs out.

But be careful! There are other farmers in Sproutopia, and they might try to hinder you. Keep an eye out for them!

---
## General

📒 _Note: All configuration is subject to change while we balance the game. For the latest configurations please navigate to `2024-Sproutopia/Sproutopia` for `appsettings.json`, `appsettings.Development.json` and `appsettings.Production.json`.

---
## Rules

Throughout the game, farmers must gain as much territory as possible. To gain territory a farmer needs to completely encircle a group of tiles returning back to their own territory. 

Each farmer is allocated a spawn position on the map. When the game starts the farmers are spawned in this position and surrounded by a starting territory of 3x3 tiles. Whenever a farmer is pruned (see below), it will be respawned in this position and given a starting 3x3 territory.

Power-Ups will randomly spawn and can be picked up by running through it. (See the section on [Power-Ups](#power-ups) for more information)

Weeds will also randomly spawn, and will slowly grow to encroach on the playing field. Cleaning a weed will reveal a Super Power-Up. You clean a weed by completely encircling it, but be careful to not run into the weed, as this will cause your farmer to respawn. (See the sections on [Super Power-Ups](#super-power-ups) and [Weeds](#weeds) for more information)

Power-Ups and Super Power-Ups are lost when a farmer is pruned.

Farmers can only have one Power-Up and one Super Power-Up active at a time.

### More on territory

If a farmer completes a trail between two disjoint territories, the trail is simply converted to territory, acting as a "bridge" of sorts betweent the territories. If the farmer subsequently completes another trail connecting these two territories, they are in fact no longer two, but one territory and the enclosed area will be claimed.

### More on interactions between farmers

The land of Sproutopia is a much sought after land and with four farmers vying for its riches, it is inevitable that there will be some crossing of pitchforks and clashing of hoes. These are the different types of interactions that might occur between farmers.

- #### Pruning

  While a farmer is encircling a potential territory it leaves behind a trail. This trail can be cut off by another farmer in which case the other farmer will gain all the territory of the first. The first farmer will be respawned in the next tick.

  Similarly, if a farmer runs into their own trail, they will lose their territory and be respawned.

- #### Encircling

  If a farmer completely encircles another farmer and completes its trail to claim the encircled territory, the encircled farmer is pruned and respawned in its respawn position. All of the territory that belonged to the encircled farmer is transfered to the encircling farmer, **with the exception of the encircled farmer's starting territory ( the orignal 9 plots/cells given at the start of a match)**.

- #### Stealing of Territory

  If a farmer completes a trail running through a section of another farmer's territory, the section of the other territory that was encircled is transfered to the industrious farmer.

- #### Collisions

  Two farmers involved in a head on collision will result in one of the two being pruned. The farmer who's last command was received first will be the victor while the other will be pruned.

  If a collision happens inside a farmer's territory that farmer will automatically be the victor, irrespective of which farmer's last command was issued first. 

### Power-Ups

After a certain number of ticks have passed (config:`PowerUpStartTick`), Power-Ups will start spawning randomly up to a defined maximum number of Power-Ups (config:`PowerUpsMaxAmount`). 

Power-Ups activate as soon as a farmer picks it up and lasts for a certain number of ticks, which is a configurable value depending on the type of Power-Up.

A farmer can only have one Power-Up active at any given time. If a farmer collects a second Power-Up while another is still active, the active Power-Up will be discarded and the new Power-Up activated.

- #### Temporary Territorial Immunity

  Other farmers cannot enter your territory, and farmers within your territory are stunned. The duration of this powerup is defined in the `LifespanImmunity` configuration value.

- #### Unprunable

  A farmer cannot be pruned by another farmer (but they can be pruned by themselves). The duration of this powerup is defined in the `LifespanUnprunable` configuration value.

  It is important to note that a farmer's trail does not become indestructible when "Unprunable" is active. If a farmer's trail is cut of by another farmer while it has "Unprunable" active, the farmer is respawned back inside its claimed territory where the trail started and the trail is discarded. The farmer does not lose its claimed territory though like it would have without "Unprunable".

- #### Freeze

  Other farmers are unable to move. The duration of this powerup is defined in the `LifespanFreeze` configuration value.


### Super Power-Ups 

These Power-Ups are spawned only once a weed is cleared. See the [weeds](#weeds) section for more information on weeds.

A farmer can only have one Super Power-Up active at any given time. If a farmer collects a second Super Power-Up while another is still active, the active Super Power-Up will be discarded and the new Super Power-Up activated.

- #### Trail protection 

  A farmer's trail is protected from themselves, and if they run into their own trail, they will be reset to the start of their trail. Once picked up, this powerup remains active until it is used. 

- #### Super Fertilizer 

  When a farmer runs over neutral territory, they immediately get the tiles to the left and right of them added to their territory. They do not have a trail. The duration of this powerup is defined in the `LifespanFertilizer` configuration value.

  If a farmer re-enters its territory while "Super Fertilizer" is active, it is deactivated.

### Weeds

After a certain number of ticks have passed (config:`WeedsStartTick`), weeds will start spawning randomly up to a defined maximum number of weeds (config:`WeedsMaxAmount`). Once a weed has spawned it will grow by one tile after every n ticks (where n is defined by config:`WeedGrowthRate`) up to a maximum size (config:`WeedsMaxGrowth`).

If a farmer collides with a weed it will be pruned. The only way to clear a weed is to completely surround it and claim the territory in which it is growing.

Once a weed is cleared, a Super Power-Up will spawn where the weed was growing (see the [Super Power-Ups](#super-Power-Ups) section). This super powerup still needs to be picked up before it is granted to a farmer. Simply clearing the weed is not enough.

Weeds will only spawn in neutral territory but might grow into existing claimed territory. They don't detract from a farmer's territory.


---
## Game Ticks

Sproutopia is a real time game that utilises `Ticks`, as a unit of time to keep track of the game.

---
## Farmers

Four Farmers (bots) will be placed into the map together, and have to gather / conquer as much territory as possible before time runs out! 

---
## Visibility

Each farmer can only see a certain square distance around their curent position. 

---
## Commands

When a farmer is spawned, they spawn in an IDLE state, and will begin moving as soon as the first command of movement is sent. Farmers may traverse the map via basic movement.

📒 _Note: Farmer have continuous movement and will move in their last direction until a new command is issued or it is at the edge of the map where the farmer will stop_.

The following commands are available:

* `UP` - 1
* `DOWN` - 2
* `LEFT` - 3
* `RIGHT` - 4

A farmer can't double back on the direction it is moving as that would effectively result in the farmer stepping on its own trail. If an invalid command is received it is simply discarded and the farmer continues moving in its current direction.

### Command Structure

The command is sent with the following structure:

```json
{
    "Action" : 1,                                    // UP action type - int
    "BotId" : "410d392c-ecf5-43b9-a228-299c0a8d224a" // Bot ID - string/UUID/GUID
}
```

### Command Processing Order

The Game Engine maintains a command queue for each farmer. A farmer can issue a command at any point which will then be added to that farmer's command queue. With each game tick the Game Engine pops the first command off of each farmer's queue and orders them by received timestamp. The four commands are then processed in that order.

---
## Game Tick Payload

After every `Tick` the `runnerHub` will send a `BotStateDTO` response.

This will consist of the following values:  
 - `DirectionState` - Current direction of farmer
 - `ElapsedTime` - Time elapsed since game started
 - `GameTick` - Current game tick
 - `PowerUp` - Current Power-Up (if any) your farmer has
 - `SuperPowerUp` - Current Super Power-Up (if any) your farmer has
 - `LeaderBoard` - Represents the current leader board 
 - `HeroWindow` - The number of tiles a hero can view around themselves
 - `X` - Hero's current x position
 - `Y` - Hero's current y position

---
## Endgame

When the timer runs out, the farmer with the most territory wins. Subsequent farmers with the next largest territory come in second, third and fourth. In the case of equivalent land claimed the farmer with the higher tie breaking points is placed above the farmer they tied with. See the [Scoring](#scoring) section for details about the calculation of tie breaking points.

If a farmer manages to capture 100% of the territory, the game is ended early and that farmer is the outright winner.

---
## Scoring 

Winning is easy 😁. The Farmer with the largest territory when the game ends wins. In the (probably unlikely) event of two or more farmers being tied for territory ownership when the game ends, tie breaking points will be used to decide a winner.

Tie breaking points are awarded throughout the game each time a farmer claims territory with larger claims being awarded more than smaller claims. The formula for points awarded is as follows:
> TBP = (T/10)^2

Where TBP = tie breaking points, T = number of tiles claimed