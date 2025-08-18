using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KYS
{
    public class PopUpUI : MonoBehaviour
    {
        // TODO: 팝업 되면 팝업 판넬 범위 바깥을 1. 클릭하지 못하게 막거나, 2. 클릭 시 팝업 이전으로 돌아가게 한다.
        private Stack<BaseUI> stack = new Stack<BaseUI>();
        public static bool IsPopUpActive { get; private set; } = false;

        [SerializeField] GameObject blocker;
        public void OnApplicationQuit()
        {
            DestroyImmediate(gameObject);
        }
        public void PushUIStack(BaseUI ui)
        {
            //Debug.Log($"[PopUpUI] PushUIStack 호출: {ui?.name}, 현재 스택 개수: {stack.Count}");

            IsPopUpActive = true;
            if (stack.Count > 0)
            {
                BaseUI top = stack.Peek();
                Debug.Log($"[PopUpUI] 기존 최상위 팝업 비활성화: {top?.name}");
                top.gameObject.SetActive(false);
            }

            stack.Push(ui);
            //Debug.Log($"[PopUpUI] 팝업 스택에 추가됨: {ui?.name}, 현재 스택 개수: {stack.Count}");


        }

        public void PopUIStack()
        {
            Debug.Log($"[PopUpUI] PopUIStack 호출 - 현재 스택 개수: {stack.Count}");

            if (stack.Count == 1)
            {
                IsPopUpActive = false;
                Debug.Log("[PopUpUI] 마지막 팝업이므로 IsPopUpActive = false");
            }
            if (stack.Count <= 0)
            {
                IsPopUpActive = false;
                Debug.Log("[PopUpUI] 스택이 비어있음");
                return;
            }

            BaseUI popupToDestroy = stack.Peek();
            string popupName = popupToDestroy != null ? popupToDestroy.name : "Unknown";
            Debug.Log($"[PopUpUI] 팝업 파괴 예정: {popupName}");

            DestroyImmediate(stack.Pop().gameObject);  // Destroy 대신 DestroyImmediate 사용
            Debug.Log($"[PopUpUI] 팝업 파괴 완료: {popupName}");

            if (stack.Count > 0)
            {
                BaseUI top = stack.Peek();
                if (top != null && top.gameObject != null)
                {
                    Debug.Log($"[PopUpUI] 이전 팝업 활성화: {top.name}");
                    top.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("[PopUpUI] 이전 팝업이 null이거나 파괴됨");
                }
            }
            else
            {
                Debug.Log("[PopUpUI] 스택이 비어있으므로 Blocker 비활성화");
                if (blocker != null)
                {
                    blocker.SetActive(false);
                }
            }
        }
        public int StackCount()
        {
            return stack.Count;
        }

        // 강제로 모든 팝업 정리 (씬 전환 시 사용)
        public void ForceCleanAll()
        {
            Debug.Log($"[PopUpUI] 강제 정리 시작 - 현재 팝업 개수: {stack.Count}");

            // 모든 팝업을 즉시 파괴
            while (stack.Count > 0)
            {
                BaseUI popup = stack.Pop();
                if (popup != null && popup.gameObject != null)
                {
                    Debug.Log($"[PopUpUI] 팝업 파괴: {popup.name}");
                    DestroyImmediate(popup.gameObject);
                }
            }

            // 스택을 완전히 비우고 상태 초기화
            stack.Clear();
            IsPopUpActive = false;

            // 블로커 비활성화 (NetworkScene이 아닌 경우)
            if (blocker != null)
            {
                // NetworkScene에서는 블로커를 유지 (RoomPopUp이 표시될 예정)
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "NetworkScene")
                {
                    blocker.SetActive(false);
                    Debug.Log("[PopUpUI] 블로커 비활성화 완료");
                }
                else
                {
                    Debug.Log("[PopUpUI] NetworkScene에서는 블로커 유지");
                }
            }

            Debug.Log("[PopUpUI] 강제 정리 완료");
        }

        /// <summary>
        /// 로딩 블로커를 표시합니다 (게임 시작 시 사용)
        /// </summary>
        public void ShowLoadingBlocker()
        {
            Debug.Log("[PopUpUI] 로딩 블로커 표시");
            if (blocker != null)
            {
                blocker.SetActive(true);
                IsPopUpActive = true;
            }
        }

        /// <summary>
        /// 로딩 블로커를 숨깁니다 (게임 씬 로드 완료 시 사용)
        /// </summary>
        public void HideLoadingBlocker()
        {
            Debug.Log("[PopUpUI] 로딩 블로커 숨김");
            if (blocker != null)
            {
                blocker.SetActive(false);
                Debug.Log("[PopUpUI] 로딩 블로커 비활성화 완료");
            }
            else
            {
                Debug.LogWarning("[PopUpUI] blocker가 null입니다");
            }

            // 스택이 비어있으면 IsPopUpActive도 false로 설정
            if (stack.Count == 0)
            {
                IsPopUpActive = false;
                Debug.Log("[PopUpUI] 스택이 비어있어 IsPopUpActive = false로 설정");
            }
        }
    }


}