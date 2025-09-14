using System;
using NUnit.Framework;

[TestFixture]
public class EnergyExtensionsTests
{
    [Test]
    public void TestGetPredictedAmount_WithEnergyAndRechargeInterval_ShouldReturnPredictedAmount()
    {
        var energy = new Energy { CurrentAmount = 10, LastRechargeAt = DateTime.Now };
        var configs = new Configs
        {
            Energy = new EnergyConfig
            {
                MaxEnergy = 100,
                RechargeIntervalSeconds = 10,
            }
        };

        var predictedAmount = energy.GetPredictedAmount(DateTime.Now.AddSeconds(20), configs.Energy);

        Assert.AreEqual(12, predictedAmount);
    }

    [Test]
    public void TestUpdateEnergy_WithEnergyAndRechargeInterval_ShouldUpdateEnergy()
    {
        var lastRechargeAt = DateTime.Now;
        var energy = new Energy { CurrentAmount = 10, LastRechargeAt = lastRechargeAt };
        var configs = new Configs
        {
            Energy = new EnergyConfig
            {
                MaxEnergy = 100,
                RechargeIntervalSeconds = 10,
            }
        };

        energy.UpdateEnergy(DateTime.Now.AddSeconds(23), configs.Energy);

        Assert.AreEqual(12, energy.CurrentAmount);
        Assert.That(energy.LastRechargeAt, Is.EqualTo(lastRechargeAt.AddSeconds(20)));
    }
}