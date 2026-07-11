using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Palengke.BangSak.Editor
{
    public static class Phase28BWebGlBuild
    {
        public const string MenuPath = "Bang-Sak/Build/Phase 28B WebGL";
        public const string RelativeOutputPath = "Build/WebGL";
        public const string PhaseVersion = "0.33.0";
        public const string CurrentPhase = "33";

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
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                throw new BuildFailedException(
                    "Unity WebGL Build Support is not installed for this Editor version. Add it in Unity Hub first.");
            }

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are configured for the WebGL build.");
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                throw new BuildFailedException("Unity could not switch to the WebGL build target.");
            }

            ConfigurePlayerSettings();
            var outputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", RelativeOutputPath));
            Directory.CreateDirectory(outputPath);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.CleanBuildCache
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"Phase 28B WebGL build failed: {report.summary.result} ({report.summary.totalErrors} errors).");
            }

            WriteBuildMetadata(outputPath, report);
            Debug.Log($"Phase 28B WebGL build completed at {outputPath}");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Palengke";
            PlayerSettings.productName = "Bang-Sak for Palengke";
            PlayerSettings.bundleVersion = PhaseVersion;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.initialMemorySize = 256;
            PlayerSettings.runInBackground = true;
        }

        private static void WriteBuildMetadata(string outputPath, BuildReport report)
        {
            var metadata = new Phase28BBuildMetadata
            {
                phase = CurrentPhase,
                version = PhaseVersion,
                unityVersion = Application.unityVersion,
                buildTarget = report.summary.platform.ToString(),
                totalBytes = report.summary.totalSize,
                builtAtUtc = DateTime.UtcNow.ToString("O")
            };
            File.WriteAllText(
                Path.Combine(outputPath, "build-info.json"),
                JsonUtility.ToJson(metadata, true));
        }

        [Serializable]
        private sealed class Phase28BBuildMetadata
        {
            public string phase;
            public string version;
            public string unityVersion;
            public string buildTarget;
            public ulong totalBytes;
            public string builtAtUtc;
        }
    }
}
