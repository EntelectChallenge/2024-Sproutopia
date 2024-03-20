package models.dtos

data class Location(var x: Int, var y: Int)
data class PowerUpLocation(var location: Location, var type: Int)


data class BotStateDto(var x: Int,
                  var y: Int,
                  var connectionId: String,
                  var elapsedTime: String,
                  var gameTick: Int,
                  var heroWindow: Array<IntArray>,
                  var directionState: Int,
                  var leaderBoard: Map<String, Int>,
                  var botPositions:Array<Location>,
                  var powerUpLocations: Array<PowerUpLocation>,
                  var weeds: Array<BooleanArray>,
                  var powerUp: Int = 0,
                  var superPowerUp: Int = 0

) {

    override fun toString(): String {
        val windowStr = StringBuilder()
        if (heroWindow != null) {
            for (y in heroWindow!![0].indices.reversed()) {
                for (x in heroWindow!!.indices) {
                    windowStr.append(heroWindow!![x][y])
                }
                windowStr.append("\n")
            }
        }
       return String.format("Position: (%d, %d), Direction: %s, GameTick: %d\n%s", x, y, directionState, gameTick, windowStr.toString())
    }
}
