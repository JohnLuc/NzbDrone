using System;
using NLog;

namespace NzbDrone.Core.Entities.Notification
{
    public class ProgressNotification : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ProgressNotification(string title)
        {
            Title = title;
            CurrentStatus = String.Empty;
            Id = Guid.NewGuid();
            ProgressMax = 100;
            ProgressValue = 0;
        }


        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        /// <value>The Id.</value>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the title for this notification.
        /// </summary>
        /// <value>The title.</value>
        public String Title { get; set; }

        /// <summary>
        /// Gets or sets the current status of this task. this field could be use to show the currently processing item in a long running task.
        /// </summary>
        /// <value>The current status.</value>
        public String CurrentStatus { get; set; }

        /// <summary>
        /// Gets or sets the completion status in percent.
        /// </summary>
        /// <value>The percent complete.</value>
        public int PercentComplete
        {
            get
            {
                return Convert.ToInt32(Convert.ToDouble(ProgressValue) / Convert.ToDouble(ProgressMax) * 100);
            }
        }

        /// <summary>
        /// Gets or sets the total number of items that need to be completed
        /// </summary>
        /// <value>The progress max.</value>
        public int ProgressMax { get; set; }

        /// <summary>
        /// Gets or sets the number of items successfully completed.
        /// </summary>
        /// <value>The progress value.</value>
        public int ProgressValue { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        public NotificationStatus Status { get; set; }

        public void Dispose()
        {
            if (Status == NotificationStatus.InProgress)
            {
                Logger.Error("Progress notification '{0}' was unexpectedly abandoned. ID:{1} Status:{2} CurrentStatus:{3} PercentComplete:{4}", Title, Id, Status, CurrentStatus, PercentComplete);
                Status = NotificationStatus.Failed;
            }
        }
    }
}