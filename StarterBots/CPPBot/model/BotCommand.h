#pragma once

#include <string>
#include "../enums/BotAction.h"

struct BotCommand {
    std::string botId;
    BotAction action;
};
