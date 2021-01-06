using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Plugins.Commands.Handlers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Impostor.Plugins.Commands
{
    [ImpostorPlugin(
        package: "gg.impostor.commands",
        name: "Commands",
        author: "asqapro & Nitcholas",
        version: "1.0.0")]
    public class Commands : PluginBase
    {
        private readonly ILogger<Commands> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable _unregister;

        public Commands(ILogger<Commands> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("Commands is being enabled.");
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("Commands is being disabled.");
            _unregister.Dispose();
            return default;
        }
    }
}