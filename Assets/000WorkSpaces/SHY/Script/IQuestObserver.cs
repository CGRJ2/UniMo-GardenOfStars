public interface IQuestObserver
{
    void OnQuestProgressChanged(int stageIndex, int newProgress);
}
