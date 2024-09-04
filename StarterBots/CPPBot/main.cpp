#define NO_SIGNALRCLIENT_EXPORTS

#include "enums/BotAction.h"
#include <signalrclient/log_writer.h>
#include <signalrclient/trace_level.h>

#include "fmt/core.h"
#include "model/BotCommand.h"
#include "model/BotState.h"
#include "service/BotService.h"
#include "signalrclient/hub_connection.h"
#include "signalrclient/hub_connection_builder.h"
#include <cstdlib>
#include <exception>
#include <future>
#include <vector>

void convertWeeds(const std::vector<signalr::value> &value,
                  std::vector<std::vector<bool>> &weeds) {
  for (auto val : value) {
    std::vector<bool> row;
    for (auto cell : val.as_array()) {
      row.push_back(cell.as_bool());
    }
    weeds.push_back(row);
  }
}

BotState convertBotState(const std::vector<signalr::value> &value) {
  std::map<std::string, signalr::value> map = value[0].as_map();
  std::vector<std::vector<bool>> weeds;
  convertWeeds(map["weeds"].as_array(), weeds);
  return BotState{
      .weeds = weeds,
      .x = (int)map["x"].as_double(),
      .y = (int)map["y"].as_double(),
  };
}

void stopConnection(signalr::hub_connection &connection,
                    std::promise<void> &stop_task) {
  connection.stop(
      [&stop_task](std::exception_ptr exc) { stop_task.set_value(); });
}

signalr::value convertBotCommand(const BotCommand &command) {
  std::map<std::string, signalr::value> map;

  signalr::value action(static_cast<double>(command.action));
  signalr::value botId(command.botId);
  map.emplace("botId", botId);
  map.emplace("action", action);

  return signalr::value(map);
}

void handleExceptionPtr(fmt::format_string<std::string> fmtString,
                        const std::exception_ptr &exc) {
  if (!exc) {
    return;
  }
  try {
    std::rethrow_exception(exc);
  } catch (const std::exception &e) {
    fmt::println(fmtString, e.what());
  }
}

int main() {
  std::promise<void> start_task;
  std::promise<void> register_task;
  std::promise<void> stop_task;

  std::string runnerIp = std::getenv("RUNNER_IPV4");
  runnerIp = runnerIp.empty() ? "http://localhost" : runnerIp;
  std::string hubUrl = runnerIp.find("http://") == -1
                           ? fmt::format("http://{}:5000/runnerhub", runnerIp)
                           : fmt::format("{}:5000/runnerhub", runnerIp);

  std::string token = std::getenv("TOKEN");
  std::string nickname = std::getenv("BOT_NICKNAME");

  signalr::hub_connection connection =
      signalr::hub_connection_builder::create(hubUrl)
          .with_logging(nullptr, signalr::trace_level::warning)
          .build();

  BotService botService;

  connection.on("Registered",
                [&botService](const std::vector<signalr::value> &value) {
                  std::string botId = value[0].as_string();
                  fmt::println("Registered with bot ID: {}", botId);
                  botService.SetBotId(value[0].as_string());
                });

  connection.on(
      "ReceiveBotState",
      [&botService, &connection](const std::vector<signalr::value> &value) {
        fmt::print("Received bot state. ");
        auto botState = convertBotState(value);
        botService.ProcessState(botState);
        auto command = botService.GetCommand();
        fmt::println("Responding with {}", BotActionName(command.action));
        auto convertedCommand = convertBotCommand(command);
        connection.send("SendPlayerCommand", std::vector<signalr::value>({convertedCommand}),
                        [](const std::exception_ptr exc) {
                          handleExceptionPtr("Error sending command: {}", exc);
                        });
      });

  connection.on("Disconnect", [&connection, &stop_task](
                                  const std::vector<signalr::value> &value) {
    if (!value.empty()) {
      fmt::println("Received Disconnect with value: {}", value[0].as_string());
    } else {
      fmt::println("Received Disconnect");
    }
    stopConnection(connection, stop_task);
  });

  connection.on(
      "ReceiveGameComplete",
      [&connection, &stop_task](const std::vector<signalr::value> &value) {
        fmt::println("Game complete!");
        stopConnection(connection, stop_task);
      });

  connection.set_disconnected([&stop_task](std::exception_ptr exc) {
    if (exc) {
      handleExceptionPtr("Server disconnected with reason: {}", exc);
    } else {
      fmt::println("Server disconnected");
    }
    stop_task.set_value();
  });

  connection.start([&start_task](std::exception_ptr exc) {
    if (exc) {
      handleExceptionPtr("Error starting connection: {}", exc);
      std::exit(1);
    } else {
      start_task.set_value();
    }
  });

  start_task.get_future().get();

  std::vector<signalr::value> registerParams(
      {signalr::value(token), signalr::value(nickname)});

  connection.send("Register", registerParams,
                  [&register_task](std::exception_ptr exc) {
                    if (exc) {
                      handleExceptionPtr("Error registering bot: {}", exc);
                      std::exit(1);
                    } else {
                      register_task.set_value();
                    }
                  });

  register_task.get_future().get();
  stop_task.get_future().get();
}
