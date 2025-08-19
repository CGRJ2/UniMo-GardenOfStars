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
        // TODO: �˾� �Ǹ� �˾� �ǳ� ���� �ٱ��� 1. Ŭ������ ���ϰ� ���ų�, 2. Ŭ�� �� �˾� �������� ���ư��� �Ѵ�.
        private Stack<BaseUI> stack = new Stack<BaseUI>();
        public static bool IsPopUpActive { get; private set; } = false;

        [SerializeField] GameObject blocker;
        public void OnApplicationQuit()
        {
            DestroyImmediate(gameObject);
        }
        public void PushUIStack(BaseUI ui)
        {
            //Debug.Log($"[PopUpUI] PushUIStack ȣ��: {ui?.name}, ���� ���� ����: {stack.Count}");

            IsPopUpActive = true;
            if (stack.Count > 0)
            {
                BaseUI top = stack.Peek();
                Debug.Log($"[PopUpUI] ���� �ֻ��� �˾� ��Ȱ��ȭ: {top?.name}");
                top.gameObject.SetActive(false);
            }

            stack.Push(ui);
            //Debug.Log($"[PopUpUI] �˾� ���ÿ� �߰���: {ui?.name}, ���� ���� ����: {stack.Count}");


        }

        public void PopUIStack()
        {
            Debug.Log($"[PopUpUI] PopUIStack ȣ�� - ���� ���� ����: {stack.Count}");

            if (stack.Count == 1)
            {
                IsPopUpActive = false;
                Debug.Log("[PopUpUI] ������ �˾��̹Ƿ� IsPopUpActive = false");
            }
            if (stack.Count <= 0)
            {
                IsPopUpActive = false;
                Debug.Log("[PopUpUI] ������ �������");
                return;
            }

            BaseUI popupToDestroy = stack.Peek();
            string popupName = popupToDestroy != null ? popupToDestroy.name : "Unknown";
            Debug.Log($"[PopUpUI] �˾� �ı� ����: {popupName}");

            DestroyImmediate(stack.Pop().gameObject);  // Destroy ��� DestroyImmediate ���
            Debug.Log($"[PopUpUI] �˾� �ı� �Ϸ�: {popupName}");

            if (stack.Count > 0)
            {
                BaseUI top = stack.Peek();
                if (top != null && top.gameObject != null)
                {
                    Debug.Log($"[PopUpUI] ���� �˾� Ȱ��ȭ: {top.name}");
                    top.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("[PopUpUI] ���� �˾��� null�̰ų� �ı���");
                }
            }
            else
            {
                Debug.Log("[PopUpUI] ������ ��������Ƿ� Blocker ��Ȱ��ȭ");
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

        // ������ ��� �˾� ���� (�� ��ȯ �� ���)
        public void ForceCleanAll()
        {
            Debug.Log($"[PopUpUI] ���� ���� ���� - ���� �˾� ����: {stack.Count}");

            // ��� �˾��� ��� �ı�
            while (stack.Count > 0)
            {
                BaseUI popup = stack.Pop();
                if (popup != null && popup.gameObject != null)
                {
                    Debug.Log($"[PopUpUI] �˾� �ı�: {popup.name}");
                    DestroyImmediate(popup.gameObject);
                }
            }

            // ������ ������ ���� ���� �ʱ�ȭ
            stack.Clear();
            IsPopUpActive = false;

            // ���Ŀ ��Ȱ��ȭ (NetworkScene�� �ƴ� ���)
            if (blocker != null)
            {
                // NetworkScene������ ���Ŀ�� ���� (RoomPopUp�� ǥ�õ� ����)
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "NetworkScene")
                {
                    blocker.SetActive(false);
                    Debug.Log("[PopUpUI] ���Ŀ ��Ȱ��ȭ �Ϸ�");
                }
                else
                {
                    Debug.Log("[PopUpUI] NetworkScene������ ���Ŀ ����");
                }
            }

            Debug.Log("[PopUpUI] ���� ���� �Ϸ�");
        }

        /// <summary>
        /// �ε� ���Ŀ�� ǥ���մϴ� (���� ���� �� ���)
        /// </summary>
        public void ShowLoadingBlocker()
        {
            Debug.Log("[PopUpUI] �ε� ���Ŀ ǥ��");
            if (blocker != null)
            {
                blocker.SetActive(true);
                IsPopUpActive = true;
            }
        }

        /// <summary>
        /// �ε� ���Ŀ�� ����ϴ� (���� �� �ε� �Ϸ� �� ���)
        /// </summary>
        public void HideLoadingBlocker()
        {
            Debug.Log("[PopUpUI] �ε� ���Ŀ ����");
            if (blocker != null)
            {
                blocker.SetActive(false);
                Debug.Log("[PopUpUI] �ε� ���Ŀ ��Ȱ��ȭ �Ϸ�");
            }
            else
            {
                Debug.LogWarning("[PopUpUI] blocker�� null�Դϴ�");
            }

            // ������ ��������� IsPopUpActive�� false�� ����
            if (stack.Count == 0)
            {
                IsPopUpActive = false;
                Debug.Log("[PopUpUI] ������ ����־� IsPopUpActive = false�� ����");
            }
        }
    }


}
