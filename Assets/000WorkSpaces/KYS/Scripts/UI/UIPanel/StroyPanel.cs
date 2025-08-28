using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KYS
{
    public class StroyPanel : BaseUI
    {
        [Header("UI Element Names (BaseUI GetUI<T>() 사용)")]
        [SerializeField] private string titleTextName = "TitleText";
        [SerializeField] private string storyTextName = "StoryText";
        [SerializeField] private string nextButtonName = "NextButton";
        [SerializeField] private string previousButtonName = "PreviousButton";
        [SerializeField] private string closeButtonName = "CloseButton";
        [SerializeField] private string pageTextName = "PageText";
        [SerializeField] private string characterImageName = "CharacterImage";

        // UI 요소들 (BaseUI GetUI<T>() 사용)
        private TextMeshProUGUI titleText => GetUI<TextMeshProUGUI>(titleTextName);
        private TextMeshProUGUI storyText => GetUI<TextMeshProUGUI>(storyTextName);
        private Button nextButton => GetUI<Button>(nextButtonName);
        private Button previousButton => GetUI<Button>(previousButtonName);
        private Button closeButton => GetUI<Button>(closeButtonName);
        private TextMeshProUGUI pageText => GetUI<TextMeshProUGUI>(pageTextName);
        private Image characterImage => GetUI<Image>(characterImageName);

        [Header("Story Settings")]
        [SerializeField] private string[] storyPages = new string[0];
        [SerializeField] private Sprite[] characterImages = new Sprite[0];
        [SerializeField] private int currentPage = 0;
        [SerializeField] private bool autoPlay = false;
        [SerializeField] private float autoPlayDelay = 3f;

        private Coroutine autoPlayCoroutine;

        protected override void Awake()
        {
            base.Awake();
        }

        public override string[] GetAutoLocalizeKeys()
        {
            return new string[]
            {
                "ui_story_title",
                "ui_next_button",
                "ui_previous_button",
                "ui_close_button",
                "ui_page_label"
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SetupButtons();
            UpdateUI();
            if (autoPlay)
                StartAutoPlay();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            StopAutoPlay();
        }

        private void SetupButtons()
        {
            // BaseUI의 GetEventWithSFX 사용 (PointerHandler 기반)
            var nextEventHandler = GetEventWithSFX(nextButtonName, "SFX_ButtonClick");
            if (nextEventHandler != null)
            {
                nextEventHandler.Click += (data) => OnNextButtonClicked();
            }

            var previousEventHandler = GetEventWithSFX(previousButtonName, "SFX_ButtonClick");
            if (previousEventHandler != null)
            {
                previousEventHandler.Click += (data) => OnPreviousButtonClicked();
            }

            var closeEventHandler = GetEventWithSFX(closeButtonName, "SFX_ButtonClick");
            if (closeEventHandler != null)
            {
                closeEventHandler.Click += (data) => OnCloseButtonClicked();
            }
        }

        private void UpdateUI()
        {
            if (titleText != null)
                titleText.text = GetLocalizedText("ui_story_title");

            if (storyText != null && storyPages.Length > 0)
                storyText.text = storyPages[currentPage];

            if (pageText != null)
                pageText.text = $"{GetLocalizedText("ui_page_label")}: {currentPage + 1}/{storyPages.Length}";

            if (characterImage != null && characterImages.Length > 0)
                characterImage.sprite = characterImages[currentPage];

            // 버튼 활성화 상태
            if (nextButton != null)
                nextButton.interactable = currentPage < storyPages.Length - 1;

            if (previousButton != null)
                previousButton.interactable = currentPage > 0;
        }

        public void SetStoryData(string[] pages, Sprite[] characters = null)
        {
            storyPages = pages;
            characterImages = characters ?? new Sprite[0];
            currentPage = 0;
            UpdateUI();
        }

        public void SetAutoPlay(bool enable, float delay = 3f)
        {
            autoPlay = enable;
            autoPlayDelay = delay;
            
            if (autoPlay)
                StartAutoPlay();
            else
                StopAutoPlay();
        }

        private void StartAutoPlay()
        {
            StopAutoPlay();
            if (storyPages.Length > 1)
                autoPlayCoroutine = StartCoroutine(AutoPlayCoroutine());
        }

        private void StopAutoPlay()
        {
            if (autoPlayCoroutine != null)
            {
                StopCoroutine(autoPlayCoroutine);
                autoPlayCoroutine = null;
            }
        }

        private System.Collections.IEnumerator AutoPlayCoroutine()
        {
            while (currentPage < storyPages.Length - 1)
            {
                yield return new WaitForSeconds(autoPlayDelay);
                NextPage();
            }
        }

        private void NextPage()
        {
            if (currentPage < storyPages.Length - 1)
            {
                currentPage++;
                UpdateUI();
                Debug.Log($"[StroyPanel] 다음 페이지로 이동: {currentPage + 1}");
            }
        }

        private void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdateUI();
                Debug.Log($"[StroyPanel] 이전 페이지로 이동: {currentPage + 1}");
            }
        }

        private void OnNextButtonClicked()
        {
            NextPage();
        }

        private void OnPreviousButtonClicked()
        {
            PreviousPage();
        }

        private void OnCloseButtonClicked()
        {
            Debug.Log("[StroyPanel] 스토리 패널 닫기");
            Hide();
        }

        [ContextMenu("UI 요소 정보 출력")]
        public void PrintUIElementInfo()
        {
            Debug.Log($"[StroyPanel] TitleText: {titleText != null}");
            Debug.Log($"[StroyPanel] StoryText: {storyText != null}");
            Debug.Log($"[StroyPanel] NextButton: {nextButton != null}");
            Debug.Log($"[StroyPanel] PreviousButton: {previousButton != null}");
            Debug.Log($"[StroyPanel] CloseButton: {closeButton != null}");
            Debug.Log($"[StroyPanel] PageText: {pageText != null}");
            Debug.Log($"[StroyPanel] CharacterImage: {characterImage != null}");
        }
    }
}
