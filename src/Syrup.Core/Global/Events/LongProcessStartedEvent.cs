using MediatR;

namespace Syrup.Core.Global.Events
{
    public enum LongProcessType
    {
        IsIndeterminate,
        IsDeterminate
    }

    public class LongProcessStartedEvent : INotification
    {
        public LongProcessStartedEvent(string message, int progress, LongProcessType type)
        {
            Message = message;
            Progress = progress;
            Type = type;
        }

        public string Message { get; }
        public int Progress { get; }
        public LongProcessType Type { get; }
    }

    public class LongProcessNotifiedEvent : INotification
    {
        public LongProcessNotifiedEvent(string message, int progress, LongProcessType type)
        {
            Message = message;
            Progress = progress;
            Type = type;
        }

        public string Message { get; }
        public int Progress { get; }
        public LongProcessType Type { get; set; }
    }

    public class LongProcessEndedEvent : INotification
    {
        public LongProcessEndedEvent(string message, int progress, LongProcessType type)
        {
            Message = message;
            Progress = progress;
            Type = type;
        }

        public string Message { get; }
        public int Progress { get; }
        public LongProcessType Type { get; set; }
    }
}