using System;

public static class QuestEventBus
{
    public static event Action<int, int> OnQuestProgressChanged;

    public static void Publish(int stageIndex, int newProgress)
    {
        OnQuestProgressChanged?.Invoke(stageIndex, newProgress);
    }
}
