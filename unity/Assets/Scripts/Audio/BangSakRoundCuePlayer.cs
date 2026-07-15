using UnityEngine;

namespace Palengke.BangSak.Audio
{
    [DisallowMultipleComponent]
    public sealed class BangSakRoundCuePlayer : MonoBehaviour
    {
        public const string ComponentId = "round_reveal_result_audio";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase35e_procedural_v1_semantic";
        public const int MaximumSimultaneousVoices = 1;
        public const int MaximumQueuedCues = 1;
        private const string SharedObjectName = "Bang-Sak Round Audio";

        private static BangSakRoundCuePlayer sharedInstance;

        private AudioSource output;
        private AudioClip[] clips;
        private int pendingCueIndex = -1;

        public AudioSource Output => output;
        public int CachedClipCount => clips == null ? 0 : clips.Length;
        public bool HasPendingCue => pendingCueIndex >= 0;

        private void OnEnable()
        {
            BangSakAudioSettings.SettingsChanged -= RefreshOutputVolume;
            BangSakAudioSettings.SettingsChanged += RefreshOutputVolume;
            Prepare();
        }

        private void OnDisable()
        {
            BangSakAudioSettings.SettingsChanged -= RefreshOutputVolume;
            pendingCueIndex = -1;
            if (output != null)
            {
                output.Stop();
            }

            if (sharedInstance == this)
            {
                sharedInstance = null;
            }
        }

        private void Update()
        {
            if (pendingCueIndex < 0 || output == null || output.isPlaying)
            {
                return;
            }

            var cue = (BangSakRoundCue)pendingCueIndex;
            pendingCueIndex = -1;
            if (output.volume > 0f)
            {
                PlayNow(cue);
            }
        }

        private void OnDestroy()
        {
            BangSakAudioSettings.SettingsChanged -= RefreshOutputVolume;
            ReleaseClips();
            if (sharedInstance == this)
            {
                sharedInstance = null;
            }
        }

        public static bool PlayShared(BangSakRoundCue cue)
        {
            if (!Application.isPlaying)
            {
                return false;
            }

            return GetOrCreateShared().Play(cue);
        }

        public void Prepare()
        {
            if (isActiveAndEnabled)
            {
                BangSakAudioSettings.SettingsChanged -= RefreshOutputVolume;
                BangSakAudioSettings.SettingsChanged += RefreshOutputVolume;
            }

            EnsureOutput();
            EnsureClips();
            RefreshOutputVolume();
        }

        public bool Play(BangSakRoundCue cue)
        {
            Prepare();
            var definition = BangSakRoundCueCatalog.Get(cue);
            if (output.volume <= 0f)
            {
                pendingCueIndex = -1;
                return false;
            }

            if (output.isPlaying)
            {
                pendingCueIndex = (int)cue;
                return true;
            }

            pendingCueIndex = -1;
            PlayNow(cue, definition);
            return true;
        }

        public AudioClip GetClip(BangSakRoundCue cue)
        {
            Prepare();
            BangSakRoundCueCatalog.Get(cue);
            return clips[(int)cue];
        }

        public void RefreshOutputVolume()
        {
            if (output != null)
            {
                output.volume = BangSakAudioSettings.ResolveVolume(BangSakAudioChannel.Sfx);
                if (output.volume <= 0f)
                {
                    pendingCueIndex = -1;
                    output.Stop();
                }
            }
        }

        private void PlayNow(BangSakRoundCue cue)
        {
            PlayNow(cue, BangSakRoundCueCatalog.Get(cue));
        }

        private void PlayNow(BangSakRoundCue cue, BangSakRoundCueDefinition definition)
        {
            output.PlayOneShot(clips[(int)cue], definition.BaseVolume);
        }

        private static BangSakRoundCuePlayer GetOrCreateShared()
        {
            if (sharedInstance != null)
            {
                return sharedInstance;
            }

            sharedInstance = FindObjectOfType<BangSakRoundCuePlayer>();
            if (sharedInstance != null)
            {
                return sharedInstance;
            }

            var audioObject = new GameObject(SharedObjectName);
            sharedInstance = audioObject.AddComponent<BangSakRoundCuePlayer>();
            DontDestroyOnLoad(audioObject);
            return sharedInstance;
        }

        private void EnsureOutput()
        {
            if (output == null)
            {
                output = GetComponent<AudioSource>();
            }

            if (output == null)
            {
                output = gameObject.AddComponent<AudioSource>();
            }

            output.playOnAwake = false;
            output.loop = false;
            output.spatialBlend = 0f;
            output.priority = 116;
        }

        private void EnsureClips()
        {
            if (clips != null && clips.Length == BangSakRoundCueCatalog.CueCount)
            {
                return;
            }

            ReleaseClips();
            clips = new AudioClip[BangSakRoundCueCatalog.CueCount];
            for (var index = 0; index < clips.Length; index += 1)
            {
                clips[index] = BangSakRoundCueCatalog.CreateClip((BangSakRoundCue)index);
            }
        }

        private void ReleaseClips()
        {
            if (clips == null)
            {
                return;
            }

            foreach (var clip in clips)
            {
                if (clip == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(clip);
                }
                else
                {
                    DestroyImmediate(clip);
                }
            }

            clips = null;
        }
    }
}
