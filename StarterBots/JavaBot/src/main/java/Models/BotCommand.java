package Models;

import Enums.Actions;

import java.util.UUID;

public class BotCommand {
    private UUID botId;
    private Actions action;

    public BotCommand(UUID botId, Actions action) {
        this.botId = botId;
        this.action = action;
    }

    public UUID getBotId() {
        return botId;
    }

    public void setBotId(UUID botId) {
        this.botId = botId;
    }

    public Actions getAction() {
        return action;
    }

    public void setAction(Actions action) {
        this.action = action;
    }

    @Override
    public String toString() {
        return "BotCommand{" +
                "botId=" + botId +
                ", action=" + action +
                '}';
    }
}
