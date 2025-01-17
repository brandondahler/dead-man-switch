﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeadManSwitch.Logging;

namespace DeadManSwitch.Internal
{
    internal interface IDeadManSwitchTriggerer
    {
        ValueTask TriggerAsync(CancellationToken cancellationToken);
    }

    internal sealed class DeadManSwitchTriggerer : IDeadManSwitchTriggerer
    {
        private readonly IDeadManSwitchContext _deadManSwitchContext;
        private readonly DeadManSwitchOptions _deadManSwitchOptions;
        private readonly IDeadManSwitchLogger<DeadManSwitchTriggerer> _logger;

        public DeadManSwitchTriggerer(IDeadManSwitchContext deadManSwitchContext, DeadManSwitchOptions deadManSwitchOptions, IDeadManSwitchLogger<DeadManSwitchTriggerer> logger)
        {
            _deadManSwitchContext = deadManSwitchContext ?? throw new ArgumentNullException(nameof(deadManSwitchContext));
            _deadManSwitchOptions = deadManSwitchOptions ?? throw new ArgumentNullException(nameof(deadManSwitchOptions));
            _logger = logger;
        }

        public async ValueTask TriggerAsync(CancellationToken cancellationToken)
        {
            _logger.Warning("The worker task did not notify the dead man's switch within the agreed timeout of {TimeoutInSeconds}s " +
                            "and will be cancelled.", _deadManSwitchOptions.Timeout.TotalSeconds);

            var notifications = (await _deadManSwitchContext.GetNotificationsAsync(cancellationToken).ConfigureAwait(false)).ToList();

            _logger.Warning("These were the last {NotificationCount} notifications: ", notifications.Count);

            foreach (var notification in notifications)
            {
                _logger.Warning("{NotificationTimestamp} {NotificationContent}", notification.Timestamp, notification.Content);
            }

            var cancellationTokenSource = _deadManSwitchContext.CancellationTokenSource;

            _deadManSwitchContext.CancellationTokenSource = new CancellationTokenSource();

            _logger.Trace("Marking worker cancellation token as cancelled");
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}