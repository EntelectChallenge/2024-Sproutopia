use botstate::BotState;
use command::{BotAction, BotCommand};
use core::panic;
use once_cell::sync::OnceCell;
use signalrs_client::hub::Hub;
use signalrs_client::SignalRClient;
use std::env;
use std::time::Duration;
use tokio::time::sleep;
use tracing::*;
use tracing_subscriber::{self, filter, prelude::*};

pub mod botstate;
pub mod command;

static BOT_ID: OnceCell<String> = OnceCell::new();
static CLIENT: OnceCell<SignalRClient> = OnceCell::new();

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    set_tracing_subscriber();

    let runner_ip: String = match env::var("RUNNER_IPV4") {
        Ok(ip) => ip,
        Err(_) => "localhost".to_string(),
    };

    let domain: String = match runner_ip.strip_prefix("http://") {
        Some(s) => s.to_string(),
        None => runner_ip,
    };

    let token: String = match env::var("TOKEN") {
        Ok(val) => val,
        Err(e) => panic!("Could not get token from environment: {e}"),
    };

    let nickname: String = match env::var("BOT_NICKNAME") {
        Ok(val) => val,
        Err(_) => String::from("RustBot"),
    };

    let hub = Hub::default()
        .method::<_, String>("Registered", on_register)
        .method::<_, BotState>("ReceiveBotState", on_receive_bot_state)
        .method("Disconnect", on_disconnect)
        .method("ReceiveGameComplete", on_game_complete)
        .method("EndGame", on_game_complete);

    let _ = CLIENT.set(
        SignalRClient::builder(domain)
            .use_unencrypted_connection()
            .use_port(5000)
            .use_hub("runnerhub")
            .with_client_hub(hub)
            .build()
            .await?,
    );

    println!("Connected to runner");

    let client = CLIENT.get().unwrap();

    client
        .method("Register")
        .arg(token)?
        .arg(nickname)?
        .send()
        .await?;

    loop {
        sleep(Duration::from_millis(300)).await;
    }
}

async fn on_disconnect() {
    println!("RunnerHub disconnected");
    std::process::exit(1);
}

async fn on_game_complete() {
    println!("Game complete!");
    std::process::exit(0);
}

async fn on_receive_bot_state(bot_state: BotState) {
    let state_pretty = bot_state.pretty();
    println!("{state_pretty}");

    let client = CLIENT.get().unwrap();
    let bot_id = BOT_ID.get().unwrap();

    let command = BotCommand::new(BotAction::Right, bot_id.to_string());

    let _ = client
        .method("SendPlayerCommand")
        .arg(command)
        .expect("Error sending player command")
        .send()
        .await;
}

async fn on_register(value: String) {
    println!("Registered with ID: {value}");
    let _ = BOT_ID.set(value);
}

fn set_tracing_subscriber() {
    let targets_filter = filter::Targets::new()
        .with_target("signalrs", Level::INFO)
        .with_target("tokio_tungstenite::compat", Level::DEBUG)
        .with_default(Level::DEBUG);

    let fmt_layer = tracing_subscriber::fmt::layer()
        .with_line_number(false)
        .with_file(false)
        .without_time()
        .compact();

    tracing_subscriber::registry()
        .with(fmt_layer)
        .with(targets_filter)
        .init();
}
