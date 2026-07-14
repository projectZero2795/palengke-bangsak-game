using System;
using NUnit.Framework;
using Palengke.BangSak.Audio;
using UnityEngine;

public sealed class Phase35AAudioSettingsTests
{
    [SetUp]
    public void SetUp()
    {
        BangSakAudioSettings.ResetToDefaults();
    }

    [TearDown]
    public void TearDown()
    {
        BangSakAudioSettings.ResetToDefaults();
    }

    [Test]
    public void Defaults_AreAudibleAndUseFullVolume()
    {
        var settings = BangSakAudioSettings.Current;

        Assert.That(settings.Muted, Is.False);
        Assert.That(settings.MasterVolume, Is.EqualTo(1f));
        Assert.That(settings.MusicVolume, Is.EqualTo(1f));
        Assert.That(settings.SfxVolume, Is.EqualTo(1f));
    }

    [Test]
    public void Setters_PersistNormalizedValues()
    {
        BangSakAudioSettings.SetMuted(true);
        BangSakAudioSettings.SetMasterVolume(0.8f);
        BangSakAudioSettings.SetMusicVolume(-0.5f);
        BangSakAudioSettings.SetSfxVolume(1.5f);

        Assert.That(PlayerPrefs.GetInt(BangSakAudioSettings.MutedKey), Is.EqualTo(1));
        Assert.That(PlayerPrefs.GetFloat(BangSakAudioSettings.MasterVolumeKey), Is.EqualTo(0.8f));
        Assert.That(PlayerPrefs.GetFloat(BangSakAudioSettings.MusicVolumeKey), Is.Zero);
        Assert.That(PlayerPrefs.GetFloat(BangSakAudioSettings.SfxVolumeKey), Is.EqualTo(1f));
    }

    [Test]
    public void ResolveVolume_AppliesMasterAndSelectedChannel()
    {
        BangSakAudioSettings.SetMasterVolume(0.5f);
        BangSakAudioSettings.SetMusicVolume(0.4f);
        BangSakAudioSettings.SetSfxVolume(0.8f);

        Assert.That(BangSakAudioSettings.ResolveVolume(BangSakAudioChannel.Music), Is.EqualTo(0.2f).Within(0.0001f));
        Assert.That(BangSakAudioSettings.ResolveVolume(BangSakAudioChannel.Sfx, 0.5f), Is.EqualTo(0.2f).Within(0.0001f));
    }

    [Test]
    public void ResolveVolume_WhenMuted_AlwaysReturnsSilence()
    {
        BangSakAudioSettings.SetMuted(true);

        Assert.That(BangSakAudioSettings.ResolveVolume(BangSakAudioChannel.Music), Is.Zero);
        Assert.That(BangSakAudioSettings.ResolveVolume(BangSakAudioChannel.Sfx), Is.Zero);
    }

    [Test]
    public void ResolveVolume_RejectsUnknownChannels()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BangSakAudioSettings.ResolveVolume((BangSakAudioChannel)99));
    }

    [Test]
    public void UnchangedStoredValue_DoesNotRaiseDuplicateEvent()
    {
        var eventCount = 0;
        void CountChange() => eventCount++;
        BangSakAudioSettings.SettingsChanged += CountChange;

        try
        {
            BangSakAudioSettings.SetMasterVolume(0.7f);
            BangSakAudioSettings.SetMasterVolume(0.7f);
        }
        finally
        {
            BangSakAudioSettings.SettingsChanged -= CountChange;
        }

        Assert.That(eventCount, Is.EqualTo(1));
    }
}
