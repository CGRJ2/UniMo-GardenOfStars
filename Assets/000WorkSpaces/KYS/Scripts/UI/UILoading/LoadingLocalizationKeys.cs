using UnityEngine;

namespace KYS
{
    /// <summary>
    /// LoadingScreen에서 사용하는 로컬라이제이션 키들을 정의하는 클래스
    /// </summary>
    public static class LoadingLocalizationKeys
    {
        #region 기본 로딩 메시지
        
        public const string LOADING_PREPARE = "loading_prepare";
        public const string LOADING_PROGRESS = "loading_progress";
        public const string LOADING_COMPLETE = "loading_complete";
        public const string LOADING_SUCCESS = "loading_success";
        public const string LOADING_WAIT = "loading_wait";
        public const string LOADING_DATA = "loading_data";
        public const string LOADING_ALMOST_DONE = "loading_almost_done";
        
        #endregion
        
        #region 스테이지 로딩 메시지
        
        public const string STAGE_PREPARE = "stage_prepare";
        public const string STAGE_LOADING = "stage_loading";
        public const string STAGE_COMPLETE = "stage_complete";
        
        // 스테이지별 메시지 (동적 생성)
        public static string GetStagePrepareKey(string stageId) => $"stage_{stageId}_prepare";
        public static string GetStageLoadingKey(string stageId) => $"stage_{stageId}_loading";
        public static string GetStageCompleteKey(string stageId) => $"stage_{stageId}_complete";
        
        #endregion
        
        #region 빠른 로딩 메시지
        
        public const string LOADING_FAST_PREPARE = "loading_fast_prepare";
        public const string LOADING_FAST_PROGRESS = "loading_fast_progress";
        public const string LOADING_FAST_COMPLETE = "loading_fast_complete";
        public const string LOADING_FAST_SUCCESS = "loading_fast_success";
        
        #endregion
        
        #region 씬 전환 메시지
        
        public const string SCENE_TRANSITION_PREPARE = "scene_transition_prepare";
        public const string SCENE_TRANSITION_LOADING = "scene_transition_loading";
        public const string SCENE_TRANSITION_COMPLETE = "scene_transition_complete";
        
        #endregion
        
        #region 에러 메시지
        
        public const string LOADING_ERROR = "loading_error";
        public const string LOADING_TIMEOUT = "loading_timeout";
        public const string LOADING_RETRY = "loading_retry";
        
        #endregion
        
        #region 기본 폴백 메시지 (로컬라이제이션 실패 시 사용)
        
        public static class FallbackMessages
        {
            public const string LOADING_PREPARE = "준비 중...";
            public const string LOADING_PROGRESS = "로딩 중...";
            public const string LOADING_COMPLETE = "완료 중...";
            public const string LOADING_SUCCESS = "로딩이 완료되었습니다!";
            public const string LOADING_WAIT = "잠시만 기다려주세요...";
            public const string LOADING_DATA = "데이터를 불러오는 중...";
            public const string LOADING_ALMOST_DONE = "거의 완료되었습니다...";
            
            public const string STAGE_PREPARE = "스테이지를 준비하는 중...";
            public const string STAGE_LOADING = "스테이지를 로딩하는 중...";
            public const string STAGE_COMPLETE = "스테이지 로딩 완료!";
            
            public const string LOADING_FAST_PREPARE = "빠른 로딩 준비 중...";
            public const string LOADING_FAST_PROGRESS = "빠른 로딩 중...";
            public const string LOADING_FAST_COMPLETE = "빠른 로딩 완료!";
            public const string LOADING_FAST_SUCCESS = "빠른 로딩이 완료되었습니다!";
            
            public const string SCENE_TRANSITION_PREPARE = "씬 전환 준비 중...";
            public const string SCENE_TRANSITION_LOADING = "씬을 전환하는 중...";
            public const string SCENE_TRANSITION_COMPLETE = "씬 전환 완료!";
            
            public const string LOADING_ERROR = "로딩 중 오류가 발생했습니다.";
            public const string LOADING_TIMEOUT = "로딩 시간이 초과되었습니다.";
            public const string LOADING_RETRY = "다시 시도하시겠습니까?";
        }
        
        #endregion
    }
}
