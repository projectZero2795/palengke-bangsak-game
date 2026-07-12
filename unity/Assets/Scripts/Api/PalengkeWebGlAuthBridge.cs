using System;
using System.Runtime.InteropServices;

namespace Palengke.BangSak.Api
{
    public static class PalengkeWebGlAuthBridge
    {
        public const string BridgeUrl = "https://palengke.es/api/game-auth/bang-sak";
        public const string RequestMessageType = "palengke-bang-sak-auth-request";
        public const string ResponseMessageType = "palengke-bang-sak-auth-response";

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

        public static void RequestAccessToken(string gameObjectName, string callbackMethod)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(gameObjectName)
                && !string.IsNullOrWhiteSpace(callbackMethod))
            {
                PalengkeRequestAccessToken(gameObjectName, callbackMethod);
            }
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern IntPtr PalengkeGetAccessToken();

        [DllImport("__Internal")]
        private static extern void PalengkeFreeString(IntPtr pointer);

        [DllImport("__Internal")]
        private static extern void PalengkeRequestAccessToken(string gameObjectName, string callbackMethod);
#endif
    }
}
