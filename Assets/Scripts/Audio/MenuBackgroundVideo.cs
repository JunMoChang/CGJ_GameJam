using Core;
using UnityEngine;
using UnityEngine.Video;

namespace Audio
{
    public class MenuBackgroundVideo : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RenderTexture sharedRT;
        [SerializeField] private VideoClip[] candidateClips;
        private GameManager gameManager;

        private void Awake()
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = sharedRT;
            videoPlayer.isLooping = true;
            videoPlayer.waitForFirstFrame = true;
        }

        private void Start()
        {
            gameManager = GameManager.Instance;
            if (gameManager != null) gameManager.OnStateChanged += OnStateChanged;

            Play(0);
        }

        private void OnDestroy()
        {
            if (gameManager != null)
                gameManager.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.StartMenu:
                    Play(0); break;
                case GameState.MainMenu: 
                    Play(1); break;
                case GameState.Playing: 
                    Play(2); break;
            }
        }

        private void Play(int index)
        {
            if (candidateClips == null || index >= candidateClips.Length) return;
            var clip = candidateClips[index];
            if (videoPlayer.clip == clip) return;

            videoPlayer.Stop();
            videoPlayer.clip = clip;
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.Prepare();
        }

        private void OnPrepared(VideoPlayer vp)
        {
            vp.Play();
            vp.prepareCompleted -= OnPrepared;
        }
    }
}
