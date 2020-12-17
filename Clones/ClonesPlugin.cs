using System;
using System.Threading.Tasks;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Plugins.Clones.Handlers;
using Microsoft.Extensions.Logging;

namespace Impostor.Plugins.Clones
{
    [ImpostorPlugin(
        package: "gg.impostor.example",
        name: "Example",
        author: "AeonLucid",
        version: "1.0.0")]
    public class ClonesPlugin : PluginBase
    {
        private readonly ILogger<ClonesPlugin> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable _unregister;

        public ClonesPlugin(ILogger<ClonesPlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("Example is being enabled.");
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("Example is being disabled.");
            _unregister.Dispose();
            return default;
        }
    }
}