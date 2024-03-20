package Enums;

import java.util.Arrays;
import java.util.Optional;

public enum Actions {
    IDLE(0), // Only for initial state and respawn
    Up(1),
    Down(2),
    Left(3),
    Right(4);

    private final int value;

    private Actions(int value) {
        this.value = value;
    }

    public static Optional<Actions> valueOf(int value) {
        return Arrays.stream(Actions.values()).filter(inputCommand -> inputCommand.value == value).findFirst();
    }

    public int getValue() {
        return this.value;
    }

    @Override
    public String toString() {
        return this.name() + "(" + value + ")";
    }
}
