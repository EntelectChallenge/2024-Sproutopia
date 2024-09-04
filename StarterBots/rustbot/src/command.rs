#![allow(non_snake_case)]

use serde::{Deserialize, Serialize};
use signalrs_derive::HubArgument;

#[derive(Serialize, Deserialize, HubArgument)]
pub enum BotAction {
    IDLE = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4,
}

impl From<i32> for BotAction {
    fn from(value: i32) -> Self {
        value.into()
    }
}

impl Into<i32> for BotAction {
    fn into(self) -> i32 {
        match self {
            BotAction::IDLE => 0,
            BotAction::Up => 1,
            BotAction::Down => 2,
            BotAction::Left => 3,
            BotAction::Right => 4,
        }
    }
}

#[derive(Serialize, Deserialize, HubArgument)]
#[serde(rename_all = "camelCase")]
pub struct BotCommand {
    pub action: i32,
    pub bot_id: String,
}

impl BotCommand {
    pub fn new(action: BotAction, bot_id: String) -> BotCommand {
        BotCommand { action: action.into(), bot_id }
    }
}
