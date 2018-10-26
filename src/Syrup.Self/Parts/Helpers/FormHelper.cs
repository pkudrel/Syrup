namespace Syrup.Self.Parts.Helpers
{
    public interface INotifier
    {
        void SetProgress(int max);
        void UpdateProgress(int i);
        void FinishProgress();
        void AddToLog(string text);
        void CloseMe();
    }


}