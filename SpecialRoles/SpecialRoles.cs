using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Plugins.SpecialRoles.Handlers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Impostor.Plugins.SpecialRoles
{
    [ImpostorPlugin(
        package: "gg.impostor.SpecialRoles",
        name: "SpecialRoles",
        author: "asqapro",
        version: "1.0.0")]
    public class SpecialRoles : PluginBase
    {
        private readonly ILogger<SpecialRoles> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable _unregister;

        public SpecialRoles(ILogger<SpecialRoles> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("SpecialRoles is being enabled.");
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("SpecialRoles is being disabled.");
            _unregister.Dispose();
            return default;
        }
    }
}