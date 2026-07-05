using Core;
using Snake;
using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("背景音乐")]
        [SerializeField] private AudioClip startMenuBGM;
        [SerializeField] private AudioClip mainMenuBGM;
        [SerializeField] private AudioClip gameBGM;

        [Header("音效")]
        [SerializeField] private AudioClip eatClip;
        [SerializeField] private AudioClip attackClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;

        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private AudioSource attackSource;

        private GameManager gameManager;
        private SnakeController snake;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 背景音乐
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

            // 通用音效
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            
            attackSource = gameObject.AddComponent<AudioSource>();
            attackSource.playOnAwake = false;
        }

        private void Start()
        {
            PreloadClip(attackClip, attackSource);
            PreloadClip(eatClip);
            PreloadClip(winClip);
            PreloadClip(loseClip);

            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.OnStateChanged += OnStateChanged;
                PlayBGMForState(gameManager.CurrentState);
            }

            BindSnakeEvents();
        }

        /// <summary>预加载 AudioClip 数据</summary>
        private void PreloadClip(AudioClip clip, AudioSource dedicatedSource = null)
        {
            if (clip == null) return;
            
            clip.LoadAudioData();
            if (dedicatedSource != null) dedicatedSource.clip = clip;
        }

        private void OnDestroy()
        {
            if (gameManager != null) gameManager.OnStateChanged -= OnStateChanged;
            UnbindSnakeEvents();
        }

        private void OnEnable()  => BindSnakeEvents();
        private void OnDisable() => UnbindSnakeEvents();

        private void BindSnakeEvents()
        {
            if (snake != null) return;
            snake = FindObjectOfType<SnakeController>();
            if (snake != null)
            {
                snake.OnFoodEaten += OnFoodEaten;
                snake.OnAttack += OnAttack;
            }
        }

        private void UnbindSnakeEvents()
        {
            if (snake != null)
            {
                snake.OnFoodEaten -= OnFoodEaten;
                snake.OnAttack -= OnAttack;
                snake = null;
            }
        }

        #region 游戏状态

        private void OnStateChanged(GameState state)
        {
            PlayBGMForState(state);
            PlaySfxForState(state);
        }

        private void PlayBGMForState(GameState state)
        {
            switch (state)
            {
                case GameState.StartMenu:
                    PlayBGM(startMenuBGM);
                    break;
                case GameState.MainMenu:
                    PlayBGM(mainMenuBGM);
                    break;
                case GameState.Playing:
                    PlayBGM(gameBGM);
                    break;
                case GameState.Win:
                case GameState.Lose:
                    StopBGM();
                    break;
            }
        }

        private void PlaySfxForState(GameState state)
        {
            switch (state)
            {
                case GameState.Win:
                    PlaySfx(winClip);
                    break;
                case GameState.Lose:
                    PlaySfx(loseClip);
                    break;
            }
        }

        #endregion

        #region 事件

        private void OnFoodEaten(Vector2Int pos) => PlaySfx(eatClip);

        private void OnAttack()
        {
            if (attackSource != null)
                attackSource.Play();
        }

        #endregion

        #region 公开 API

        public void PlayEat()    => PlaySfx(eatClip);
        public void PlayAttack() => OnAttack();
        public void PlayWin()    => PlaySfx(winClip);
        public void PlayLose()   => PlaySfx(loseClip);

        #endregion

        #region 内部播放

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
                sfxSource.PlayOneShot(clip);
        }

        private void PlayBGM(AudioClip clip)
        {
            if (clip == null || bgmSource == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.Play();
        }

        private void StopBGM()
        {
            if (bgmSource != null)
                bgmSource.Stop();
        }

        #endregion
    }
}
