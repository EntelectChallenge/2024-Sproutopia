#include "BotService.h"
#include "../model/BotState.h"

BotService::BotService() {

}

void BotService::SetBotId(std::string botId) {
  this->botId = botId;
}

BotCommand BotService::GetCommand() const {
  return BotCommand{
    .botId = botId,
    .action = nextAction
  };
}

void BotService::ProcessState(const BotState& botState) {
  // TODO: Implement your bot logic here!
  nextAction = BotAction::Up;
}
