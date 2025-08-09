using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace _02_Scripts.Reward
{
    /// <summary>
    /// 출석 보상 팝업 UI 컴포넌트
    /// 상단에 나타나서 일정 시간 후 사라지는 출석 보상 알림 팝업
    /// </summary>
    public class AttendancePopupUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI coinAmountText;
        
        [Header("애니메이션 설정")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        // 내부 상태
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private bool isInitialized = false;
        private Coroutine autoHideCoroutine;
        private Vector2 originalPosition; // Unity에서 설정된 원본 위치 저장
        
        #region Unity Lifecycle
        private void Awake()
        {
            SetupComponents();
        }
        
        private void Start()
        {
            // 닫기 버튼 제거됨 - 자동으로만 사라짐
        }
        
        private void OnDestroy()
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
            }
        }
        #endregion
        
        #region Initialization
        /// <summary>
        /// 컴포넌트 설정
        /// </summary>
        private void SetupComponents()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            rectTransform = GetComponent<RectTransform>();
            
            // Unity에서 설정된 원본 위치 저장
            originalPosition = rectTransform.anchoredPosition;
            
            // 초기 상태: 숨김 (위치는 그대로, 알파만 0)
            canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 팝업 초기화 및 표시
        /// </summary>
        public void Initialize(string message, long coinAmount, float displayDuration)
        {
            if (isInitialized)
            {
                Debug.LogWarning("[AttendancePopupUI] 이미 초기화된 팝업입니다.");
                return;
            }
            
            // GameObject가 비활성화되어 있다면 활성화
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                Debug.Log("[AttendancePopupUI] 팝업 GameObject 활성화");
            }
            
            // 텍스트 설정
            if (messageText != null)
            {
                messageText.text = message;
            }
            else
            {
                Debug.LogWarning("[AttendancePopupUI] MessageText가 연결되지 않았습니다.");
            }
            
            if (coinAmountText != null)
            {
                coinAmountText.text = $"+{coinAmount:N0}";
            }
            else
            {
                Debug.LogWarning("[AttendancePopupUI] CoinAmountText가 연결되지 않았습니다.");
            }
            
            isInitialized = true;
            
            // 팝업 표시 애니메이션 시작
            StartCoroutine(ShowPopupSequence(displayDuration));
            
            Debug.Log($"[AttendancePopupUI] 팝업 초기화 완료 - 메시지: {message}, 코인: {coinAmount}, 표시시간: {displayDuration}초");
        }
        #endregion
        
        #region Animation Sequences
        /// <summary>
        /// 팝업 표시 시퀀스
        /// </summary>
        private IEnumerator ShowPopupSequence(float displayDuration)
        {
            // 1. 페이드 인
            yield return StartCoroutine(FadeInAnimation());
            
            // 2. 표시 시간 대기
            float remainingTime = displayDuration - fadeInDuration;
            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }
            
            // 3. 자동 숨김 (수동으로 닫지 않은 경우)
            if (gameObject != null && isInitialized)
            {
                yield return StartCoroutine(HidePopupSequence());
            }
        }
        
        /// <summary>
        /// 페이드 인 애니메이션
        /// </summary>
        private IEnumerator FadeInAnimation()
        {
            float elapsedTime = 0f;
            
            // Unity에서 설정된 위치로 이동
            rectTransform.anchoredPosition = originalPosition;
            
            while (elapsedTime < fadeInDuration)
            {
                float progress = elapsedTime / fadeInDuration;
                float curveValue = fadeInCurve.Evaluate(progress);
                
                // 알파값 애니메이션만 처리
                canvasGroup.alpha = curveValue;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 최종 상태 보장
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 팝업 숨김 시퀀스
        /// </summary>
        private IEnumerator HidePopupSequence()
        {
            yield return StartCoroutine(FadeOutAnimation());
            
            // 애니메이션 완료 후 오브젝트 제거
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 페이드 아웃 애니메이션
        /// </summary>
        private IEnumerator FadeOutAnimation()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeOutDuration)
            {
                float progress = elapsedTime / fadeOutDuration;
                float curveValue = fadeOutCurve.Evaluate(progress);
                
                // 알파값 애니메이션만 처리
                canvasGroup.alpha = curveValue;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 최종 상태 보장
            canvasGroup.alpha = 0f;
        }
        #endregion
        
        #region Public Interface
        /// <summary>
        /// 팝업을 즉시 숨김 (외부에서 호출 가능)
        /// </summary>
        public void HideImmediately()
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
            
            StartCoroutine(HidePopupSequence());
        }
        
        /// <summary>
        /// 현재 팝업이 표시 중인지 확인
        /// </summary>
        public bool IsVisible()
        {
            return isInitialized && canvasGroup.alpha > 0f;
        }
        #endregion
        
        #region Debug Methods
        [ContextMenu("Debug/Test Show Animation")]
        private void CM_TestShowAnimation()
        {
            if (!isInitialized)
            {
                Initialize("테스트 출석 보상!", 1000, 3f);
            }
        }
        
        [ContextMenu("Debug/Test Hide Animation")]
        private void CM_TestHideAnimation()
        {
            HideImmediately();
        }
        #endregion
    }
}
