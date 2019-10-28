﻿using System;

namespace DeadManSwitch
{
    public class DeadManSwitchOptions
    {
        /// <summary>
        /// The amount of time the dead man's switch will wait for a signal before canceling the worker
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// How many notifications to keep in memory (for diagnostic purposes)
        /// </summary>
        public int NumberOfNotificationsToKeep { get; set; } = 10;
    }
}