using UnityEngine;

namespace Palengke.BangSak.Audio
{
    [DisallowMultipleComponent]
    public sealed class BangSakMenuCuePlayer : MonoBehaviour
    {
        public const string ComponentId = "menu_interface_audio";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase35c_procedural_v2_distinct";
        public const int MaximumSimultaneousVoices = 1;
        private const string SharedObjectName = "Bang-Sak Menu Audio";

        private static BangSakMenuCuePlayer sharedInstance;

        private AudioSource output;
        private AudioClip[] clips;

        public AudioSource Output => output;
        public int CachedClipCount => clips == null ? 0 : clips.Length;

        private void OnEnable()
        {
            BangSakAudioSettings.SettingsChanged -= RefreshOutputVolume;
            BangSakAudioSettings.SettingsChanged += RefreshOutputVolume;
            Prepare();
        }

        private void OnDisable()
        {
            BangSakAudioSettings.SettingsChanged -= RefreshOutputVolume;
            if (output != null)
            {
                output.Stop();
            }

            if (sharedInstance == this)
            {
                sharedInstance = null;
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

        public static bool PlayShared(BangSakMenuCue cue)
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

        public bool Play(BangSakMenuCue cue)
        {
            Prepare();
            var definition = BangSakMenuCueCatalog.Get(cue);
            if (output.volume <= 0f)
            {
                return false;
            }

            // Replace any still-fading menu cue so rapid input cannot stack volume.
            output.Stop();
            output.PlayOneShot(clips[(int)cue], definition.BaseVolume);
            return true;
        }

        public AudioClip GetClip(BangSakMenuCue cue)
        {
            Prepare();
            BangSakMenuCueCatalog.Get(cue);
            return clips[(int)cue];
        }

        public void RefreshOutputVolume()
        {
            if (output != null)
            {
                output.volume = BangSakAudioSettings.ResolveVolume(BangSakAudioChannel.Sfx);
            }
        }

        private static BangSakMenuCuePlayer GetOrCreateShared()
        {
            if (sharedInstance != null)
            {
                return sharedInstance;
            }

            sharedInstance = FindObjectOfType<BangSakMenuCuePlayer>();
            if (sharedInstance != null)
            {
                return sharedInstance;
            }

            var audioObject = new GameObject(SharedObjectName);
            sharedInstance = audioObject.AddComponent<BangSakMenuCuePlayer>();
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
            output.priority = 128;
        }

        private void EnsureClips()
        {
            if (clips != null && clips.Length == BangSakMenuCueCatalog.CueCount)
            {
                return;
            }

            ReleaseClips();
            clips = new AudioClip[BangSakMenuCueCatalog.CueCount];
            for (var index = 0; index < clips.Length; index += 1)
            {
                clips[index] = BangSakMenuCueCatalog.CreateClip((BangSakMenuCue)index);
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
