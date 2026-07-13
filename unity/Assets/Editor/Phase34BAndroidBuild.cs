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
    public static class Phase34BAndroidBuild
    {
        public const string MenuPath = "Bang-Sak/Build/Android Debug APK";
        public const string RelativeOutputPath = "Build/Android/BangSak-debug.apk";
        public const string PackageId = "es.palengke.bangsak";
        public const string PhaseVersion = "0.34.6";
        public const int VersionCode = 1;
        public const int MinimumApiLevel = 29;
        public const int TargetApiLevel = 35;
        public const string CurrentPhase = "34E4";

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
                    "Unity Android Build Support is not installed for this Editor version. Add Android, SDK/NDK, and OpenJDK modules in Unity Hub first.");
            }

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

            ConfigurePlayerSettings();
            var apkPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", RelativeOutputPath));
            Directory.CreateDirectory(Path.GetDirectoryName(apkPath));

            EditorUserBuildSettings.buildAppBundle = false;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.CleanBuildCache
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"Phase {CurrentPhase} Android build failed: {report.summary.result} ({report.summary.totalErrors} errors)."
                );
            }

            WriteBuildMetadata(apkPath, report);
            Debug.Log($"Phase {CurrentPhase} Android debug APK completed at {apkPath}");
        }

        private static void ConfigurePlayerSettings()
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
        }

        private static void WriteBuildMetadata(string apkPath, BuildReport report)
        {
            var metadata = new Phase34BBuildMetadata
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
                developmentBuild = true,
                totalBytes = report.summary.totalSize,
                sha256 = CalculateSha256(apkPath),
                builtAtUtc = DateTime.UtcNow.ToString("O")
            };
            File.WriteAllText(
                Path.Combine(Path.GetDirectoryName(apkPath), "build-info.json"),
                JsonUtility.ToJson(metadata, true));
        }

        private static string CalculateSha256(string path)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            }
        }

        [Serializable]
        private sealed class Phase34BBuildMetadata
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
            public ulong totalBytes;
            public string sha256;
            public string builtAtUtc;
        }
    }
}
