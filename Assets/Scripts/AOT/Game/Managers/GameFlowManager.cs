using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPS.Game.Managers
{
    public sealed class GameFlowManager : MonoBehaviour
    {
        [Header("Parameters")]
        [Tooltip("游戏结束到加载新场景之间的基础延迟时间")]
        public float endSceneLoadDelay = 3f;

        [Tooltip("用于控制“黑屏渐变”效果的 UI 组件引用")]
        public CanvasGroup endGameFadeCanvasGroup;

        [Header("Win")]
        [Tooltip("胜利场景")]
        public string winSceneName = "WinScene";

        [Tooltip("渐变前的等待时间")]
        public float delayBeforeFadeToBlack = 4f;

        [Tooltip("胜利文本")]
        public string winGameMessage;

        [Tooltip("文本弹出的延迟")]
        public float delayBeforeWinMessage = 2f;

        [Tooltip("胜利音效")]
        public AudioClip victorySound;

        [Header("Lose")]
        [Tooltip("失败场景")]
        public string loseSceneName = "LoseScene";

        public bool gameIsEnding { get; private set; }

        float m_TimeLoadEndGameScene;
        string m_SceneToLoad;

        void Awake()
        {
            EventManager.AddListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
        }

        void Start()
        {
            //TODO 配置持久化
            AudioUtility.SetMasterVolume(1);
        }

        /// <summary>
        /// 根据进度淡入黑屏，淡出音量,时间到后加载新场景
        /// </summary>
        void Update()
        {
            if (!gameIsEnding)
            {
                return;
            }
            var timeRatio = 1 - Mathf.Clamp01((m_TimeLoadEndGameScene - Time.time) / endSceneLoadDelay);
            endGameFadeCanvasGroup.alpha = timeRatio;

            AudioUtility.SetMasterVolume(1 - timeRatio);

            if (Time.time < m_TimeLoadEndGameScene)
            {
                return;
            }
            SceneManager.LoadScene(m_SceneToLoad);
            gameIsEnding = false;
        }

        void OnAllObjectivesCompleted(AllObjectivesCompletedEvent evt) => EndGame(true);
        void OnPlayerDeath(PlayerDeathEvent evt) => EndGame(false);

        /// <summary>
        /// 解锁鼠标，标记游戏结束，黑屏后加载游戏结束场景
        /// </summary>
        /// <param name="win"></param>
        void EndGame(bool win)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            gameIsEnding = true;
            endGameFadeCanvasGroup.gameObject.SetActive(true);
            if (win)
            {
                m_SceneToLoad = winSceneName;
                // 加载场景的时间点：胜利结算时间+渐变时间
                m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

                // 播放音效
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = victorySound;
                audioSource.playOnAwake = false;
                audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
                audioSource.PlayScheduled(AudioSettings.dspTime + delayBeforeWinMessage);

                var displayMessage = Events.displayMessageEvent;
                displayMessage.message = winGameMessage;
                displayMessage.delayBeforeDisplay = delayBeforeWinMessage;
                EventManager.Broadcast(displayMessage);
            }
            else
            {
                m_SceneToLoad = loseSceneName;
                m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay;
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        }
    }
}