from typing import List, Dict

class Location:
    def __init__(self, x: int, y: int):
        self.x = x
        self.y = y

class PowerUpLocation:
    def __init__(self, location: Location, type: int):
        self.location = location
        self.type = type

class BotStateDto:
    def __init__(self, x: int, y: int, connection_id: str, elapsed_time: str,
                 game_tick: int, hero_window: List[List[int]], direction_state: int,
                 leader_board: Dict[str, int], bot_positions: List[Location],
                 power_up_locations: List[PowerUpLocation], weeds: List[List[bool]],
                 power_up: int = 0, super_power_up: int = 0):
        self.x = x
        self.y = y
        self.connection_id = connection_id
        self.elapsed_time = elapsed_time
        self.game_tick = game_tick
        self.hero_window = hero_window
        self.direction_state = direction_state
        self.leader_board = leader_board
        self.bot_positions = bot_positions
        self.power_up_locations = power_up_locations
        self.weeds = weeds
        self.power_up = power_up
        self.super_power_up = super_power_up