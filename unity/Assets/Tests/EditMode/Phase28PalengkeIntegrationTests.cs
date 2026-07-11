using NUnit.Framework;
using Palengke.BangSak.Api;
using Palengke.BangSak.Game;

public sealed class Phase28PalengkeIntegrationTests
{
    [Test]
    public void ScoreCalculation_IsBoundedAndRewardsObjectives()
    {
        var winnerScore = PalengkeRoundScoreSubmitter.CalculateScore(
            PrototypeRoundResult.TayaWins, 40f, 3, 0);
        var partialScore = PalengkeRoundScoreSubmitter.CalculateScore(
            PrototypeRoundResult.None, 0f, 3, 2);

        Assert.That(winnerScore, Is.EqualTo(1300));
        Assert.That(partialScore, Is.EqualTo(200));
        Assert.That(winnerScore, Is.InRange(0, 100000));
    }

    [Test]
    public void WebGlBridge_UsesGuestFallbackOutsideWebGl()
    {
        Assert.That(PalengkeWebGlAuthBridge.TryReadAccessToken(), Is.Empty);
    }
}
