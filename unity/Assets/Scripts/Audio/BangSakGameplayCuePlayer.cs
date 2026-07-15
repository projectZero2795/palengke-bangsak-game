using UnityEngine;

namespace Palengke.BangSak.Audio
{
    [DisallowMultipleComponent]
    public sealed class BangSakGameplayCuePlayer : MonoBehaviour
    {
        public const string ComponentId = "gameplay_action_audio";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase35d_procedural_v1";
        public const int MaximumSimultaneousVoices = 1;
        public const int MaximumQueuedCues = 1;
        private const string SharedObjectName = "Bang-Sak Gameplay Audio";

        private static BangSakGameplayCuePlayer sharedInstance;

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

            var cue = (BangSakGameplayCue)pendingCueIndex;
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

        public static bool PlayShared(BangSakGameplayCue cue)
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

        public bool Play(BangSakGameplayCue cue)
        {
            Prepare();
            var definition = BangSakGameplayCueCatalog.Get(cue);
            if (output.volume <= 0f)
            {
                pendingCueIndex = -1;
                return false;
            }

            if (output.isPlaying)
            {
                // Keep one bounded pending slot so an immediate confirmed outcome
                // follows its request instead of overlapping or being discarded.
                pendingCueIndex = (int)cue;
                return true;
            }

            pendingCueIndex = -1;
            PlayNow(cue, definition);
            return true;
        }

        public AudioClip GetClip(BangSakGameplayCue cue)
        {
            Prepare();
            BangSakGameplayCueCatalog.Get(cue);
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

        private void PlayNow(BangSakGameplayCue cue)
        {
            PlayNow(cue, BangSakGameplayCueCatalog.Get(cue));
        }

        private void PlayNow(BangSakGameplayCue cue, BangSakGameplayCueDefinition definition)
        {
            output.PlayOneShot(clips[(int)cue], definition.BaseVolume);
        }

        private static BangSakGameplayCuePlayer GetOrCreateShared()
        {
            if (sharedInstance != null)
            {
                return sharedInstance;
            }

            sharedInstance = FindObjectOfType<BangSakGameplayCuePlayer>();
            if (sharedInstance != null)
            {
                return sharedInstance;
            }

            var audioObject = new GameObject(SharedObjectName);
            sharedInstance = audioObject.AddComponent<BangSakGameplayCuePlayer>();
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
            output.priority = 120;
        }

        private void EnsureClips()
        {
            if (clips != null && clips.Length == BangSakGameplayCueCatalog.CueCount)
            {
                return;
            }

            ReleaseClips();
            clips = new AudioClip[BangSakGameplayCueCatalog.CueCount];
            for (var index = 0; index < clips.Length; index += 1)
            {
                clips[index] = BangSakGameplayCueCatalog.CreateClip((BangSakGameplayCue)index);
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
