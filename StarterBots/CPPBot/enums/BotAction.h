#pragma once

#include <string>

enum class BotAction {
  IDLE = 0,
  Up = 1,
  Down = 2,
  Left = 3,
  Right = 4,
};

static std::string BotActionName(const BotAction &action) {
  switch (action) {
  case BotAction::IDLE:
    return "Idle";
  case BotAction::Up:
    return "Up";
  case BotAction::Down:
    return "Down";
  case BotAction::Left:
    return "Left";
  case BotAction::Right:
    return "Right";
  }
  return "Unknown";
}
