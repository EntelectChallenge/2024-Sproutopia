class Location {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }
}

class PowerUpLocation {
    constructor(location, type) {
        this.location = location;
        this.type = type;
    }
}

class BotStateDto {
    constructor(x, y, connectionId, elapsedTime, gameTick, heroWindow, directionState, leaderBoard, botPositions, powerUpLocations, weeds, powerUp = 0, superPowerUp = 0) {
        this.x = x;
        this.y = y;
        this.connectionId = connectionId;
        this.elapsedTime = elapsedTime;
        this.gameTick = gameTick;
        this.heroWindow = heroWindow;
        this.directionState = directionState;
        this.leaderBoard = leaderBoard;
        this.botPositions = botPositions;
        this.powerUpLocations = powerUpLocations;
        this.weeds = weeds;
        this.powerUp = powerUp;
        this.superPowerUp = superPowerUp;
    }
}