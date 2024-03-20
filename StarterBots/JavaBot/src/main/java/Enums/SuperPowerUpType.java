package Enums;

import java.util.Arrays;
import java.util.Optional;

public enum SuperPowerUpType {
    NONE(0), // Only for initial state and respawn
    TrailProtection(1),
    SuperFertilizer(2);

    private final int value;

    private SuperPowerUpType(int value) {
        this.value = value;
    }

    public static Optional<SuperPowerUpType> valueOf(int value) {
        return Arrays.stream(SuperPowerUpType.values()).filter(inputCommand -> inputCommand.value == value).findFirst();
    }

    public int getValue() {
        return this.value;
    }

    @Override
    public String toString() {
        return this.name() + "(" + value + ")";
    }
}
