using System;
using System.Threading.Tasks;
using Impostor.Api.Events.Managers;
using Impostor.Api.Plugins;
using Impostor.Plugins.DoYourTasks.Handlers;
using Microsoft.Extensions.Logging;

namespace Impostor.Plugins.DoYourTasks
{
    [ImpostorPlugin(
        package: "gg.impostor.DoYourTasks",
        name: "DoYourTasks",
        author: "AeonLucid",
        version: "1.0.0")]
    public class DoYourTasks : PluginBase
    {
        private readonly ILogger<DoYourTasks> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable _unregister;

        public DoYourTasks(ILogger<DoYourTasks> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }

        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("DoYourTasks is being enabled.");
            _unregister = _eventManager.RegisterListener(new GameEventListener(_logger));
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("DoYourTasks is being disabled.");
            _unregister.Dispose();
            return default;
        }
    }
}