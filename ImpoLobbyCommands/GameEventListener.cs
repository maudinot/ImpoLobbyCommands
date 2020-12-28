using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Impostor.Plugins.LobbyCommands.Handlers
{
    /// <summary>
    ///     A class that listens for two events.
    /// </summary>
    class GameEventListener : IEventListener
    {
        private readonly ILogger<LobbyCommandsPlugin> _logger;

        public GameEventListener(ILogger<LobbyCommandsPlugin> logger)
        {
            _logger = logger;
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            _logger.LogInformation($"Game is starting.");
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            _logger.LogInformation($"Game has ended.");
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            if (e.Game.GameState == GameStates.NotStarted && e.ClientPlayer.IsHost)
            {
                if (e.Message.StartsWith("/help"))
                {
                    await ServerSendChatAsync("Commands list: /map, /impostors", e.ClientPlayer.Character);

                }

                if (e.Message.StartsWith("/map "))
                {
                    switch (e.Message.ToLowerInvariant()[5..])
                    {
                        case "skeld":
                            if(e.Game.Options.Map == MapTypes.Skeld)
                            {
                                await ServerSendChatAsync("Map is already Skeld!", e.ClientPlayer.Character, true);
                            }
                            else
                            {
                                e.Game.Options.Map = MapTypes.Skeld;
                                await e.Game.SyncSettingsAsync();
                                await ServerSendChatAsync("Map changed to Skeld", e.ClientPlayer.Character, true);
                            }
                            break;
                        case "mira":
                        case "mirahq":
                        case "mira hq":
                            if (e.Game.Options.Map == MapTypes.MiraHQ)
                            {
                                await ServerSendChatAsync("Map is already MiraHQ!", e.ClientPlayer.Character, true);
                            }
                            else
                            {
                                e.Game.Options.Map = MapTypes.MiraHQ;
                                await e.Game.SyncSettingsAsync();
                                await ServerSendChatAsync("Map changed to MiraHQ", e.ClientPlayer.Character, true);
                            }
                            break;
                        case "polus":
                            if (e.Game.Options.Map == MapTypes.Polus)
                            {
                                await ServerSendChatAsync("Map is already Polus!", e.ClientPlayer.Character, true);
                            }
                            else
                            {
                                e.Game.Options.Map = MapTypes.Polus;
                                await e.Game.SyncSettingsAsync();
                                await ServerSendChatAsync("Map changed to Polus", e.ClientPlayer.Character, true);
                            }
                            break;
                    }
                    
                }
                if (e.Message.StartsWith("/impostors "))
                {
                    int num;
                    if (!int.TryParse(e.Message[11].ToString(), out num))
                    {
                        await ServerSendChatAsync("Invalid input: expected a number", e.ClientPlayer.Character);
                    } else
                    {
                        e.Game.Options.NumImpostors = num;
                        await e.Game.SyncSettingsAsync();
                        await ServerSendChatAsync("Impostor count set to " + e.Message[11], e.ClientPlayer.Character, true);
                    }
                }
            }
            //_logger.LogInformation($"{e.PlayerControl.PlayerInfo.PlayerName} said {e.Message}");
        }

        private async ValueTask ServerSendChatAsync(string text, IInnerPlayerControl player, bool toPlayer = false)
        {
            string playername = player.PlayerInfo.PlayerName;
            await player.SetNameAsync($"Server");
            if (toPlayer)
            {
                await player.SendChatToPlayerAsync($"{text}");
            }
            else
            {
                await player.SendChatAsync($"{text}");
            }
            await player.SetNameAsync(playername);
        }
    }
}
