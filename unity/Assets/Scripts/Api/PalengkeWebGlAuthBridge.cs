using System;
using System.Runtime.InteropServices;

namespace Palengke.BangSak.Api
{
    public static class PalengkeWebGlAuthBridge
    {
        public static string TryReadAccessToken()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var pointer = PalengkeGetAccessToken();
            if (pointer == IntPtr.Zero)
            {
                return string.Empty;
            }
            try
            {
                return Marshal.PtrToStringAnsi(pointer) ?? string.Empty;
            }
            finally
            {
                PalengkeFreeString(pointer);
            }
#else
            return string.Empty;
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr PalengkeGetAccessToken();

        [DllImport("__Internal")]
        private static extern void PalengkeFreeString(IntPtr pointer);
#endif
    }
}
