#![allow(non_snake_case)]

use std::collections::HashMap;

use serde::{Deserialize, Serialize};
use signalrs_derive::HubArgument;
use serde_json::Value;

#[derive(Serialize, Deserialize, HubArgument)]
#[serde(rename_all = "camelCase")]
pub(crate) struct BotState {
    pub(crate) x: i32,
    pub(crate) y: i32,
    pub(crate) direction_state: i32,
    pub(crate) elapsed_time: String,
    pub(crate) game_tick: i32,
    pub(crate) power_up: i32,
    pub(crate) super_power_up: i32,
    pub(crate) leader_board: HashMap<String, i32>,
    //pub(crate) bot_positions: Value,
    //pub(crate) power_up_locations: Value,
    pub(crate) weeds: Vec<Vec<bool>>,
    pub(crate) hero_window: Vec<Vec<u8>>,
    #[serde(flatten)]
    extra: HashMap<String, Value>,
}

impl BotState {
    pub fn pretty(self) -> String {
        let x = self.x;
        let y = self.y;
        let game_tick = self.game_tick;
        return format!("Position: ({x}, {y}), Game Tick: {game_tick}")
    }
}

#[derive(Serialize, Deserialize, HubArgument)]
#[serde(rename_all = "camelCase")]
pub(crate) struct Location {
    pub(crate) x: i32,
    pub(crate) y: i32,
}

#[derive(Serialize, Deserialize, HubArgument)]
#[serde(rename_all = "camelCase")]
pub(crate) struct PowerUpLocation {
    pub(crate) location: Location,
    pub(crate) r#type: i32,
}
