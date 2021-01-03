using System;
using System.Threading.Tasks;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Plugins.Bomber.Handlers;
using Microsoft.Extensions.Logging;

namespace Impostor.Plugins.Bomber
{
    [ImpostorPlugin(
        package: "gg.impostor.Bomber",
        name: "Example",
        author: "AeonLucid",
        version: "1.0.0")]
    public class Bomber : PluginBase
    {
        private readonly ILogger<Bomber> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable _unregister;

        public Bomber(ILogger<Bomber> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("Bomber is being enabled.");
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("Bomber is being disabled.");
            _unregister.Dispose();
            return default;
        }
    }
}