using UnityEngine;

namespace Audio
{
    /// <summary>
    /// 音效管理：移动、吃取、攻击、破坏、失败、通关
    /// GameJam 时间紧可先空着，后续填 AudioClip
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("音效")]
        [SerializeField] private AudioClip moveClip;
        [SerializeField] private AudioClip eatClip;
        [SerializeField] private AudioClip attackClip;
        [SerializeField] private AudioClip breakClip;
        [SerializeField] private AudioClip loseClip;
        [SerializeField] private AudioClip winClip;

        private AudioSource source;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
            source = GetComponent<AudioSource>();
        }

        public void PlayMove() => Play(moveClip);
        public void PlayEat() => Play(eatClip);
        public void PlayAttack() => Play(attackClip);
        public void PlayBreakObstacle() => Play(breakClip);
        public void PlayLose() => Play(loseClip);
        public void PlayWin() => Play(winClip);

        private void Play(AudioClip clip)
        {
            if (clip != null && source != null)
                source.PlayOneShot(clip);
        }
    }
}
