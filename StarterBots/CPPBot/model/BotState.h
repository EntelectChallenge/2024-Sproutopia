#pragma once

#include <string>
#include <map>
#include <vector>

struct Location {
  int x;
  int y;
};

struct PowerUpLocation {
  Location location;
  int type;
};

struct BotState {
  int directionState;
  std::string elapsedTime;
  int gameTick;
  int powerUp;
  int superPowerUp;

  std::map<std::string, int> leaderBoard;
  std::vector<Location> botPositions;
  std::vector<PowerUpLocation> powerUpLocations;
  std::vector<std::vector<bool>> weeds;
  std::vector<std::vector<int>> heroWindow;
  int x;
  int y;
};
