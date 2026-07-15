using UnityEngine;

namespace Palengke.BangSak.Audio
{
    [DisallowMultipleComponent]
    public sealed class BangSakMenuCueBinding : MonoBehaviour
    {
        public const string ComponentId = "menu_cue_binding";
        public const int ComponentVersion = 1;

        [SerializeField]
        private BangSakMenuCue cue = BangSakMenuCue.Navigate;

        public BangSakMenuCue Cue => cue;

        public void Configure(BangSakMenuCue configuredCue)
        {
            BangSakMenuCueCatalog.Get(configuredCue);
            cue = configuredCue;
        }
    }
}
