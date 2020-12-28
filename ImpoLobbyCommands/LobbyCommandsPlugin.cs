using System;
using System.Threading.Tasks;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Plugins.LobbyCommands.Handlers;
using Microsoft.Extensions.Logging;

namespace Impostor.Plugins.LobbyCommands
{
    [ImpostorPlugin(
        package: "gg.impostor.lobbycommands",
        name: "LobbyCommands",
        author: "Cybours",
        version: "1.0.0")]
    public class LobbyCommandsPlugin : PluginBase
    {
        /// <summary>
        ///     A logger that works seamlessly with the server.
        /// </summary>
        private readonly ILogger<LobbyCommandsPlugin> _logger;

        private readonly IEventManager _eventManager;
        private IDisposable _unregister;

        public LobbyCommandsPlugin(ILogger<LobbyCommandsPlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        /// <summary>
        ///     This is called when your plugin is enabled by the server.
        /// </summary>
        /// <returns></returns>
        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("LobbyCommands is being enabled.");
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        /// <summary>
        ///     This is called when your plugin is disabled by the server.
        ///     Most likely because it is shutting down.
        /// </summary>
        /// <returns></returns>
        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("LobbyCommands is being disabled.");
            _unregister.Dispose();
            return default;
        }
    }
}
