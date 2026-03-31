using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
        [Tooltip("This string has to be the name of the scene you want to load when winning")]
        public string winSceneName = "WinScene";

        [Tooltip("Duration of delay before the fade-to-black, if winning")]
        public float delayBeforeFadeToBlack = 4f;

        [Tooltip("Win game message")]
        public string winGameMessage;

        [Tooltip("Duration of delay before the win message")]
        public float delayBeforeWinMessage = 2f;

        [Tooltip("Sound played on win")]
        public AudioClip victorySound;

        [Header("Lose")]
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
            AudioUtility.SetMasterVolume(1);
        }

        void Update()
        {
            if (gameIsEnding)
            {
                var timeRatio = 1 - (m_TimeLoadEndGameScene - Time.time) / endSceneLoadDelay;
                endGameFadeCanvasGroup.alpha = timeRatio;

                AudioUtility.SetMasterVolume(1 - timeRatio);

                // See if it's time to load the end scene (after the delay)
                if (Time.time >= m_TimeLoadEndGameScene)
                {
                    SceneManager.LoadScene(m_SceneToLoad);
                    gameIsEnding = false;
                }
            }
        }

        void OnAllObjectivesCompleted(AllObjectivesCompletedEvent evt) => EndGame(true);
        void OnPlayerDeath(PlayerDeathEvent evt) => EndGame(false);

        void EndGame(bool win)
        {
            // unlocks the cursor before leaving the scene, to be able to click buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Remember that we need to load the appropriate end scene after a delay
            gameIsEnding = true;
            endGameFadeCanvasGroup.gameObject.SetActive(true);
            if (win)
            {
                m_SceneToLoad = winSceneName;
                m_TimeLoadEndGameScene = Time.time + endSceneLoadDelay + delayBeforeFadeToBlack;

                // play a sound on win
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = victorySound;
                audioSource.playOnAwake = false;
                audioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDVictory);
                audioSource.PlayScheduled(AudioSettings.dspTime + delayBeforeWinMessage);

                // create a game message
                //var message = Instantiate(WinGameMessagePrefab).GetComponent<DisplayMessage>();
                //if (message)
                //{
                //    message.delayBeforeShowing = delayBeforeWinMessage;
                //    message.GetComponent<Transform>().SetAsLastSibling();
                //}

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