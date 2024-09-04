#pragma once

#include <string>
#include "../model/BotCommand.h"

class BotService {
  public:
    BotService();
    BotCommand GetCommand() const;
    void ProcessState(const struct BotState&);
    void SetBotId(std::string botId);

  private:
    std::string botId;
    BotAction nextAction;
};
