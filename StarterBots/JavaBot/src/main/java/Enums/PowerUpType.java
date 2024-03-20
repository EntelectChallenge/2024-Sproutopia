package Enums;

import java.util.Arrays;
import java.util.Optional;

public enum PowerUpType {
    NONE(0), // Only for initial state and respawn
    TerritoryImmunity(1),
    Unprunable(2),
    Freeze(3);    

    private final int value;

    private PowerUpType(int value) {
        this.value = value;
    }

    public static Optional<PowerUpType> valueOf(int value) {
        return Arrays.stream(PowerUpType.values()).filter(inputCommand -> inputCommand.value == value).findFirst();
    }

    public int getValue() {
        return this.value;
    }

    @Override
    public String toString() {
        return this.name() + "(" + value + ")";
    }
}
