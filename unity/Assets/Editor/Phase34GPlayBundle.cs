using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Palengke.BangSak.Editor
{
    public static class Phase34GPlayBundle
    {
        public const string MenuPath = "Bang-Sak/Build/Signed Play AAB";
        public const string RelativeOutputPath = "Build/Android/BangSak-0.34.9.aab";
        public const string PackageId = "es.palengke.bangsak";
        public const string PhaseVersion = "0.34.9";
        public const int VersionCode = 1;
        public const int MinimumApiLevel = 29;
        public const int TargetApiLevel = 35;
        public const string CurrentPhase = "34G";
        public const string KeystorePathEnvironment = "BANGSAK_ANDROID_KEYSTORE_PATH";
        public const string KeystorePasswordEnvironment = "BANGSAK_ANDROID_KEYSTORE_PASSWORD";
        public const string KeyAliasEnvironment = "BANGSAK_ANDROID_KEY_ALIAS";
        public const string KeyPasswordEnvironment = "BANGSAK_ANDROID_KEY_PASSWORD";

        [MenuItem(MenuPath)]
        public static void BuildFromMenu()
        {
            Build();
        }

        public static void BuildCommandLine()
        {
            Build();
        }

        private static void Build()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new BuildFailedException(
                    "Unity Android Build Support is not installed for this Editor version.");
            }

            var signing = ReadSigningEnvironment();
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are configured for the Android build.");
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new BuildFailedException("Unity could not switch to the Android build target.");
            }

            var bundlePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", RelativeOutputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(bundlePath));

            var previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;
            var previousUseCustomKeystore = PlayerSettings.Android.useCustomKeystore;
            var previousKeystoreName = PlayerSettings.Android.keystoreName;
            var previousKeyaliasName = PlayerSettings.Android.keyaliasName;

            try
            {
                ConfigurePlayerSettings(signing);
                EditorUserBuildSettings.buildAppBundle = true;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = bundlePath,
                    target = BuildTarget.Android,
                    options = BuildOptions.CleanBuildCache
                });

                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new BuildFailedException(
                        $"Phase {CurrentPhase} Play bundle failed: {report.summary.result} ({report.summary.totalErrors} errors).");
                }

                WriteBuildMetadata(bundlePath, signing.KeyAlias, report);
                Debug.Log($"Phase {CurrentPhase} signed Play AAB completed at {bundlePath}");
            }
            finally
            {
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
                PlayerSettings.Android.keystoreName = previousKeystoreName;
                PlayerSettings.Android.keyaliasName = previousKeyaliasName;
                PlayerSettings.Android.useCustomKeystore = previousUseCustomKeystore;
                EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            }
        }

        private static SigningEnvironment ReadSigningEnvironment()
        {
            var keystorePath = ReadRequiredEnvironment(KeystorePathEnvironment);
            if (!Path.IsPathRooted(keystorePath) || !File.Exists(keystorePath))
            {
                throw new BuildFailedException(
                    $"{KeystorePathEnvironment} must be an existing absolute path outside the repository.");
            }

            return new SigningEnvironment
            {
                KeystorePath = keystorePath,
                KeystorePassword = ReadRequiredEnvironment(KeystorePasswordEnvironment),
                KeyAlias = ReadRequiredEnvironment(KeyAliasEnvironment),
                KeyPassword = ReadRequiredEnvironment(KeyPasswordEnvironment)
            };
        }

        private static string ReadRequiredEnvironment(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BuildFailedException($"Required signing environment variable is missing: {name}");
            }

            return value;
        }

        private static void ConfigurePlayerSettings(SigningEnvironment signing)
        {
            PlayerSettings.companyName = "Palengke";
            PlayerSettings.productName = "Bang-Sak for Palengke";
            PlayerSettings.bundleVersion = PhaseVersion;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, PackageId);
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TargetApiLevel;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.runInBackground = true;
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = signing.KeystorePath;
            PlayerSettings.Android.keystorePass = signing.KeystorePassword;
            PlayerSettings.Android.keyaliasName = signing.KeyAlias;
            PlayerSettings.Android.keyaliasPass = signing.KeyPassword;
        }

        private static void WriteBuildMetadata(string bundlePath, string keyAlias, BuildReport report)
        {
            var metadata = new Phase34GBuildMetadata
            {
                phase = CurrentPhase,
                versionName = PhaseVersion,
                versionCode = VersionCode,
                packageId = PackageId,
                unityVersion = Application.unityVersion,
                buildTarget = report.summary.platform.ToString(),
                minimumApiLevel = MinimumApiLevel,
                targetApiLevel = TargetApiLevel,
                architecture = "arm64-v8a",
                developmentBuild = false,
                appBundle = true,
                signed = true,
                keyAlias = keyAlias,
                artifactBytes = new FileInfo(bundlePath).Length,
                sha256 = CalculateSha256(bundlePath),
                builtAtUtc = DateTime.UtcNow.ToString("O")
            };
            File.WriteAllText(
                Path.Combine(Path.GetDirectoryName(bundlePath), "play-bundle-info.json"),
                JsonUtility.ToJson(metadata, true));
        }

        private static string CalculateSha256(string path)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private sealed class SigningEnvironment
        {
            public string KeystorePath;
            public string KeystorePassword;
            public string KeyAlias;
            public string KeyPassword;
        }

        [Serializable]
        private sealed class Phase34GBuildMetadata
        {
            public string phase;
            public string versionName;
            public int versionCode;
            public string packageId;
            public string unityVersion;
            public string buildTarget;
            public int minimumApiLevel;
            public int targetApiLevel;
            public string architecture;
            public bool developmentBuild;
            public bool appBundle;
            public bool signed;
            public string keyAlias;
            public long artifactBytes;
            public string sha256;
            public string builtAtUtc;
        }
    }
}
