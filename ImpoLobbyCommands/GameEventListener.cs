using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Games;
using Impostor.Api.Innersloth;
using Impostor.Api.Net.Inner.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Impostor.Plugins.LobbyCommands.Handlers
{
    enum Gamemode { standard, hns };

    /// <summary>
    ///     A class that listens for two events.
    /// </summary>
    class GameEventListener : IEventListener
    {
        private readonly ILogger<LobbyCommandsPlugin> _logger;
        private Dictionary<string, Api.Innersloth.Customization.ColorType> colors = new Dictionary<string, Api.Innersloth.Customization.ColorType>();
        private Dictionary<string, Api.Innersloth.Customization.HatType> hats = new Dictionary<string, Api.Innersloth.Customization.HatType>();
        private Dictionary<string, Api.Innersloth.Customization.SkinType> skins = new Dictionary<string, Api.Innersloth.Customization.SkinType>();
        private Dictionary<string, Api.Innersloth.Customization.PetType> pets = new Dictionary<string, Api.Innersloth.Customization.PetType>();
        private Dictionary<IGame, Gamemode> mode = new Dictionary<IGame, Gamemode>();
        private Dictionary<IGame, Dictionary<Gamemode, GameOptionsData>> savedOptions = new Dictionary<IGame, Dictionary<Gamemode, GameOptionsData>>();
        public GameEventListener(ILogger<LobbyCommandsPlugin> logger)
        {
            _logger = logger;
            foreach (Api.Innersloth.Customization.ColorType c in Enum.GetValues(typeof(Api.Innersloth.Customization.ColorType)))
            {
                colors[c.ToString().ToLowerInvariant()] = c;
            }
            foreach (Api.Innersloth.Customization.HatType h in Enum.GetValues(typeof(Api.Innersloth.Customization.HatType)))
            {
                hats[h.ToString().ToLowerInvariant()] = h;
            }
            foreach (Api.Innersloth.Customization.SkinType s in Enum.GetValues(typeof(Api.Innersloth.Customization.SkinType)))
            {
                skins[s.ToString().ToLowerInvariant()] = s;
            }
            foreach (Api.Innersloth.Customization.PetType p in Enum.GetValues(typeof(Api.Innersloth.Customization.PetType)))
            {
                pets[p.ToString().ToLowerInvariant()] = p;
            }
        }

        private static GameOptionsData GetHnsOptions()
        {
            GameOptionsData hnsoptions = new GameOptionsData();
            hnsoptions.ImpostorLightMod = 0.25f;
            hnsoptions.CrewLightMod = 0.75f;
            hnsoptions.KillDistance = KillDistances.Short;
            hnsoptions.DiscussionTime = 0;
            hnsoptions.VotingTime = 1;
            hnsoptions.KillCooldown = 10f;
            hnsoptions.NumEmergencyMeetings = 0;
            hnsoptions.NumImpostors = 1;
            //hnsoptions.MaxPlayers = 10;
            hnsoptions.EmergencyCooldown = 0;
            hnsoptions.ConfirmImpostor = true;
            hnsoptions.VisualTasks = true;
            hnsoptions.AnonymousVotes = true;
            hnsoptions.NumCommonTasks = 0;
            hnsoptions.NumShortTasks = 3;
            hnsoptions.NumLongTasks = 1;
            hnsoptions.PlayerSpeedMod = 1f;
            hnsoptions.TaskBarUpdate = TaskBarUpdate.Always;
            return hnsoptions;
        }

        [EventListener]
        public void OnGameCreated(IGameCreatedEvent e)
        {
            Dictionary<Gamemode, GameOptionsData> gameSavedOptions = new Dictionary<Gamemode, GameOptionsData>();
            gameSavedOptions.Add(Gamemode.standard, new GameOptionsData());
            gameSavedOptions.Add(Gamemode.hns, GetHnsOptions());
            savedOptions.Add(e.Game, gameSavedOptions);
            mode.Add(e.Game, Gamemode.standard);
            _logger.LogInformation($"Game created with code {e.Game.Code}");
        }

        [EventListener]
        public void OnGameDestroyed(IGameDestroyedEvent e)
        {
            _ = savedOptions.Remove(e.Game);
            _ = mode.Remove(e.Game);
            _logger.LogInformation($"Game with code {e.Game.Code} was destroyed");
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            //_logger.LogInformation("Game is starting.");
            if (mode[e.Game] == Gamemode.hns)
            {
                foreach (var player in e.Game.Players)
                {
                    if (player.Character.PlayerInfo.IsImpostor)
                    {
                        player.Character.SetColorAsync(Api.Innersloth.Customization.ColorType.Red);
                    }
                    else
                    {
                        player.Character.SetColorAsync(Api.Innersloth.Customization.ColorType.Blue);
                    }
                }
            }
        }

        [EventListener]
        public void OnGameEnded(IGameEndedEvent e)
        {
            //_logger.LogInformation("Game has ended.");
        }

        [EventListener]
        public async ValueTask OnPlayerChat(IPlayerChatEvent e)
        {
            if (e.Game.GameState == GameStates.NotStarted && e.ClientPlayer.IsHost)
            {
                if (e.ClientPlayer.IsHost)
                {
                    if (e.Message.StartsWith("/help"))
                    {
                        await ServerSendChatAsync("Commands list: /map, /impostors, /killcd, /disctime, /votetime, /speed, /color, /name, /hat, /skin, /pet", e.ClientPlayer.Character);

                    }
                    if (e.Message.StartsWith("/map "))
                    {
                        switch (e.Message.ToLowerInvariant()[5..])
                        {
                            case "the skeld":
                            case "skeld":
                                if (e.Game.Options.Map == MapTypes.Skeld)
                                {
                                    await ServerSendChatAsync("Map is already The Skeld!", e.ClientPlayer.Character, true);
                                }
                                else
                                {
                                    e.Game.Options.Map = MapTypes.Skeld;
                                    await e.Game.SyncSettingsAsync();
                                    await ServerSendChatAsync("Map changed to The Skeld", e.ClientPlayer.Character);
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
                                    await ServerSendChatAsync("Map changed to MiraHQ", e.ClientPlayer.Character);
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
                                    await ServerSendChatAsync("Map changed to Polus", e.ClientPlayer.Character);
                                }
                                break;
                            default:
                                await ServerSendChatAsync($"Unrecognized map name \"{e.Message[5..]}\"", e.ClientPlayer.Character, true);
                                break;
                        }

                    }
                    if (e.Message.StartsWith("/impostors "))
                    {
                        int num;
                        string param = e.Message[11..];
                        if (!int.TryParse(param, out num))
                        {
                            await ServerSendChatAsync("Invalid input: expected a number", e.ClientPlayer.Character, true);
                        }
                        else
                        {
                            e.Game.Options.NumImpostors = num;
                            await e.Game.SyncSettingsAsync();
                            await ServerSendChatAsync("Impostor count set to " + param, e.ClientPlayer.Character);
                        }
                    }
                    if (e.Message.StartsWith("/killcd "))
                    {
                        float num;
                        string param = e.Message[8..];
                        if (!float.TryParse(param, out num))
                        {
                            await ServerSendChatAsync("Invalid input: expected a number", e.ClientPlayer.Character, true);
                        }
                        else
                        {
                            e.Game.Options.KillCooldown = num;
                            await e.Game.SyncSettingsAsync();
                            await ServerSendChatAsync("Kill cooldown set to " + param, e.ClientPlayer.Character);
                        }
                    }
                    if (e.Message.StartsWith("/disctime "))
                    {
                        int num;
                        string param = e.Message[10..];
                        if (!int.TryParse(param, out num))
                        {
                            await ServerSendChatAsync("Invalid input: expected a number", e.ClientPlayer.Character, true);
                        }
                        else
                        {
                            e.Game.Options.DiscussionTime = num;
                            await e.Game.SyncSettingsAsync();
                            await ServerSendChatAsync("Discussion time set to " + param, e.ClientPlayer.Character);
                        }
                    }
                    if (e.Message.StartsWith("/votetime "))
                    {
                        int num;
                        string param = e.Message[10..];
                        if (!int.TryParse(param, out num))
                        {
                            await ServerSendChatAsync("Invalid input: expected a number", e.ClientPlayer.Character, true);
                        }
                        else
                        {
                            e.Game.Options.VotingTime = num;
                            await e.Game.SyncSettingsAsync();
                            await ServerSendChatAsync("Voting time set to " + param, e.ClientPlayer.Character);
                        }
                    }
                    if (e.Message.StartsWith("/speed "))
                    {
                        float num;
                        string param = e.Message[7..];
                        if (!float.TryParse(param, out num))
                        {
                            await ServerSendChatAsync("Invalid input: expected a number", e.ClientPlayer.Character, true);
                        }
                        else
                        {
                            e.Game.Options.PlayerSpeedMod = num;
                            await e.Game.SyncSettingsAsync();
                            await ServerSendChatAsync("Player speed set to " + param, e.ClientPlayer.Character);
                        }
                    }
                    if (e.Message.StartsWith("/gamemode "))
                    {
                        string param = e.Message[10..];
                        switch (param)
                        {
                            case "standard":
                                if (mode[e.Game] != Gamemode.standard)
                                {
                                    //save options
                                    loadOptions(savedOptions[e.Game][mode[e.Game]], e.Game.Options);
                                    //load options
                                    mode[e.Game] = Gamemode.standard;
                                    loadOptions(e.Game.Options, savedOptions[e.Game][mode[e.Game]]);
                                    await e.Game.SyncSettingsAsync();
                                    await ServerSendChatAsync("Gamemode set to standard!", e.ClientPlayer.Character);
                                }
                                else
                                {
                                    await ServerSendChatAsync("Gamemode is already standard!", e.ClientPlayer.Character, true);
                                }
                                break;
                            case "hns":
                                if (mode[e.Game] != Gamemode.hns)
                                {
                                    //save options
                                    loadOptions(savedOptions[e.Game][mode[e.Game]], e.Game.Options);
                                    //load options
                                    mode[e.Game] = Gamemode.hns;
                                    loadOptions(e.Game.Options, savedOptions[e.Game][mode[e.Game]]);
                                    await e.Game.SyncSettingsAsync();
                                    await ServerSendChatAsync("Gamemode set to hide and seek!", e.ClientPlayer.Character);
                                }
                                else
                                {
                                    await ServerSendChatAsync("Gamemode is already hide and seek!", e.ClientPlayer.Character, true);
                                }
                                break;
                            default:
                                await ServerSendChatAsync($"Invalid gamemode \"{param}\"", e.ClientPlayer.Character, true);
                                break;
                        }
                    }
                }
                if (!e.ClientPlayer.IsHost)
                {
                    if (e.Message.StartsWith("/help"))
                    {
                        await ServerSendChatAsync("Commands list: /color, /name, /hat, /skin, /pet", e.ClientPlayer.Character);
                    }
                }
                //Commands common for host & non host
                if (e.Message.StartsWith("/color "))
                {
                    string param = e.Message.ToLowerInvariant()[7..];
                    if (colors.ContainsKey(param))
                    {
                        await e.ClientPlayer.Character.SetColorAsync(colors[param]);
                        await ServerSendChatAsync($"Color changed to {e.Message[7..]}", e.ClientPlayer.Character, true);
                    }
                    else
                    {
                        await ServerSendChatAsync($"Invalid color \"{e.Message[7..]}\"", e.ClientPlayer.Character, true);
                    }
                }
                if (e.Message.StartsWith("/name "))
                {
                    await e.ClientPlayer.Character.SetNameAsync(e.Message[6..]);
                    await ServerSendChatAsync($"Name changed to \"{e.Message[6..]}\"", e.ClientPlayer.Character, true);
                }
                if (e.Message.StartsWith("/hat "))
                {
                    string param = e.Message.ToLowerInvariant()[5..];
                    if (hats.ContainsKey(param))
                    {
                        await e.ClientPlayer.Character.SetHatAsync(hats[param]);
                        await ServerSendChatAsync($"Hat changed to {e.Message[5..]}", e.ClientPlayer.Character, true);
                    }
                    else
                    {
                        await ServerSendChatAsync($"Invalid hat \"{e.Message[5..]}\"", e.ClientPlayer.Character, true);
                    }
                }
                if (e.Message.StartsWith("/skin "))
                {
                    string param = e.Message.ToLowerInvariant()[6..];
                    if (skins.ContainsKey(param))
                    {
                        await e.ClientPlayer.Character.SetSkinAsync(skins[param]);
                        await ServerSendChatAsync($"Skin changed to {e.Message[6..]}", e.ClientPlayer.Character, true);
                    }
                    else
                    {
                        await ServerSendChatAsync($"Invalid skin \"{e.Message[6..]}\"", e.ClientPlayer.Character, true);
                    }
                }
                if (e.Message.StartsWith("/pet "))
                {
                    string param = e.Message.ToLowerInvariant()[5..];
                    if (pets.ContainsKey(param))
                    {
                        await e.ClientPlayer.Character.SetPetAsync(pets[param]);
                        await ServerSendChatAsync($"Pet changed to {e.Message[5..]}", e.ClientPlayer.Character, true);
                    }
                    else
                    {
                        await ServerSendChatAsync($"Invalid pet \"{e.Message[5..]}\"", e.ClientPlayer.Character, true);
                    }
                }
            }
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

        private void loadOptions(GameOptionsData dest, GameOptionsData source)
        {
            dest.ImpostorLightMod = source.ImpostorLightMod;
            dest.CrewLightMod = source.CrewLightMod;
            dest.DiscussionTime = source.DiscussionTime;
            dest.VotingTime = source.VotingTime;
            dest.NumEmergencyMeetings = source.NumEmergencyMeetings;
            dest.KillDistance = source.KillDistance;
            dest.KillCooldown = source.KillCooldown;
            dest.PlayerSpeedMod = source.PlayerSpeedMod;
            //options.MaxPlayers = toload.MaxPlayers;
            dest.ConfirmImpostor = source.ConfirmImpostor;
            dest.NumImpostors = source.NumImpostors;
            dest.NumCommonTasks = source.NumCommonTasks;
            dest.NumShortTasks = source.NumShortTasks;
            dest.NumLongTasks = source.NumLongTasks;
            dest.TaskBarUpdate = source.TaskBarUpdate;
            dest.VisualTasks = source.VisualTasks;
            dest.EmergencyCooldown = source.EmergencyCooldown;
        }
    }
}
