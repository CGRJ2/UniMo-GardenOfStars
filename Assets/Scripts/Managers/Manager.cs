using UnityEngine;
using KYS;
using GooglePlayGames;

public static class Manager
{
    public static GameManager game => GameManager.Instance;

    public static FirebaseManager firebase => FirebaseManager.Instance;
    public static PoolManager pool => PoolManager.Instance;
    public static PlayerManager player => PlayerManager.Instance;

    public static DataManager data => DataManager.Instance;
    public static AudioManager Audio => AudioManager.Instance;
    public static BuildingManager buildings => BuildingManager.Instance;
    public static QuestManager quest => QuestManager.Instance;
    public static NpcManager npc => NpcManager.Instance;
    public static UIManager ui => UIManager.Instance;
    public static LocalizationManager localization => LocalizationManager.Instance;
    public static CameraManager camera => CameraManager.Instance;
    public static AdManager ad => AdManager.Instance;
    public static DialogueManager dialogue => DialogueManager.Instance;



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initailize()
    {
        // Google Play Games 초기화 (다른 매니저들보다 먼저)
        InitializePlayGames();
        
        GameManager.CreateInstance();
        FirebaseManager.CreateInstance();
        PoolManager.CreateInstance();
        PlayerManager.CreateInstance();
        DataManager.CreateInstance();
        AudioManager.CreateInstance();
        BuildingManager.CreateInstance();
        QuestManager.CreateInstance();
        NpcManager.CreateInstance();
        UIManager.CreateInstance();
        LocalizationManager.CreateInstance();
        CameraManager.CreateInstance();
        AdManager.CreateInstance();
        DialogueManager.CreateInstance();
    }
    
    private static void InitializePlayGames()
    {
        // Google Play Games 서비스 활성화
        PlayGamesPlatform.Activate();
        
        // 디버그 로그 활성화 (개발 중에만)
        PlayGamesPlatform.DebugLogEnabled = true;
        
        Debug.Log("[Manager] Google Play Games 초기화 완료");
    }
}