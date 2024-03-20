package Models.Dtos;

import javax.management.openmbean.ArrayType;


public class BotStateDto {

    private int directionState;
    private String elapsedTime;
    private String connectionId;
    private int gameTick;
    private int powerUp = 0;
    private int superPowerUp = 0;
    private Location[] botPositions;
    private PowerUpLocation[] powerUpLocations;
    private boolean[][] weeds;
    private int[][] heroWindow;
    private int x;
    private int y;

    public BotStateDto(int directionState, String elapsedTime, String connectionId, int gameTick, int powerUp, int superPowerUp, Location[] botPositions, PowerUpLocation[] powerUpLocations, boolean[][] weeds, int[][] heroWindow, int x, int y) {
        this.directionState = directionState;
        this.elapsedTime = elapsedTime;
        this.connectionId = connectionId;
        this.gameTick = gameTick;
        this.powerUp = powerUp;
        this.superPowerUp = superPowerUp;
        this.botPositions = botPositions;
        this.powerUpLocations = powerUpLocations;
        this.weeds = weeds;
        this.heroWindow = heroWindow;
        this.x = x;
        this.y = y;
    }

    public int getDirectionState() {
        return directionState;
    }

    public PowerUpLocation[] getPowerUpLocations() {
        return powerUpLocations;
    }

    public String getConnectionId() {
        return connectionId;
    }

    public void setConnectionId(String connectionId) {
        this.connectionId = connectionId;
    }

    public int getGameTick() {
        return gameTick;
    }

    public void setGameTick(int gameTick) {
        this.gameTick = gameTick;
    }


    public String getElapsedTime() {
        return elapsedTime;
    }

    public void setElapsedTime(String elapsedTime) {
        this.elapsedTime = elapsedTime;
    }

    public int[][] getHeroWindow() {
        return heroWindow;
    }

    public void setHeroWindow(int[][] heroWindow) {
        this.heroWindow = heroWindow;
    }

    public Location[] getBotPositions() {
        return botPositions;
    }

    public PowerUpLocation[] getPowerUps() {
        return powerUpLocations;
    }

    public boolean[][] getWeeds() {
        return weeds;
    }

    public void setWeeds(boolean[][] weeds) {
        this.weeds = weeds;
    }

    public int getPowerUp() {
        return powerUp;
    }

    public void setPowerUp(int powerUp) {
        this.powerUp = powerUp;
    }

    public int getSuperPowerUp() {
        return superPowerUp;
    }

    public void setSuperPowerUp(int superPowerUp) {
        this.superPowerUp = superPowerUp;
    }

    public int getX() {
        return x;
    }

    public void setX(int x) {
        this.x = x;
    }

    public int getY() {
        return y;
    }

    public void setY(int y) {
        this.y = y;
    }

    public void setDirectionState(int directionState) {
        this.directionState = directionState;
    }

    public void setBotPositions(Location[] botPositions) {
        this.botPositions = botPositions;
    }

    public void setPowerUpLocations(PowerUpLocation[] powerUpLocations) {
        this.powerUpLocations = powerUpLocations;
    }

    @Override
    public String toString() {
        StringBuilder windowStr = new StringBuilder();
        if (heroWindow != null) {
            for (int y = heroWindow[0].length - 1; y >= 0; y--) {
                for (int x = 0; x < heroWindow.length; x++) {
                    windowStr.append(heroWindow[x][y]);
                }
                windowStr.append("\n");
            }
        }
        return String.format("Position: (%d, %d), \n%s", x, y, windowStr);
    }
}
