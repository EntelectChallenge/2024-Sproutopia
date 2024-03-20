package enums

import java.util.*

enum class InputCommand(val value: Int) {
    UP(1),
    DOWN(2),
    LEFT(3),
    RIGHT(4);

    override fun toString(): String {
        return "$name($value)"
    }

    companion object {
        fun valueOf(value: Int): Optional<InputCommand> {
            return Arrays.stream(values()).filter { inputCommand: InputCommand -> inputCommand.value == value }
                .findFirst()
        }
    }
}
