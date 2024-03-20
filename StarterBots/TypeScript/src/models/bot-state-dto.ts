interface Location {
    x: number;
    y: number;
}

interface PowerUpLocation {
    location: Location;
    type: number; 
}

interface BotStateDto {
    x: number;
    y: number;
    connectionId: string;
    elapsedTime: string;
    gameTick: number;
    heroWindow: number[][];
    directionState: number;
    leaderBoard: Map<string, number>;
    botPositions: Location[];
    powerUpLocations: PowerUpLocation[];
    weeds: boolean[][];
    powerUp?: number; 
    superPowerUp?: number; 
}
