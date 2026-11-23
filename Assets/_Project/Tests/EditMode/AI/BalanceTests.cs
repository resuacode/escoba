using Escoba.AI;
using Escoba.Core;
using NUnit.Framework;

namespace Escoba.Tests.EditMode
{
    /// <summary>
    /// Torneos de balance (objetivos del GDD §9). Lentos (unos segundos): categoría "Slow".
    /// Semillas distintas de las del menú de editor para no ajustar los pesos a una sola muestra.
    /// </summary>
    [Category("Slow")]
    public class BalanceTests
    {
        private const int Games = 200;

        [Test]
        public void MediumBeatsEasy_AtLeast75Percent()
        {
            TournamentResult result = BotTournament.Run(new RulesConfig(), () => new MediumBot(), () => new EasyBot(), Games, 77_000);
            TestContext.WriteLine(result.ToString());
            Assert.That(result.WinRateA, Is.GreaterThanOrEqualTo(0.75), result.ToString());
        }

        [Test]
        public void HardBeatsMedium_AtLeast60Percent()
        {
            TournamentResult result = BotTournament.Run(new RulesConfig(), () => new HardBot(), () => new MediumBot(), Games, 88_000);
            TestContext.WriteLine(result.ToString());
            Assert.That(result.WinRateA, Is.GreaterThanOrEqualTo(0.60), result.ToString());
        }

        [Test]
        public void HardBot_DecidesWellWithinBudget()
        {
            TournamentResult result = BotTournament.Run(new RulesConfig(), () => new HardBot(), () => new HardBot(), 40, 99_000);
            TestContext.WriteLine(result.ToString());
            Assert.That(result.AverageDecisionMsA, Is.LessThan(5.0), "presupuesto: < 50 ms en móvil/WebGL; en editor debe ir muy por debajo");
        }

        [Test]
        public void EasyVersusEasy_IsBalancedBySeatAlternation()
        {
            TournamentResult result = BotTournament.Run(new RulesConfig(), () => new EasyBot(), () => new EasyBot(), 400, 55_000);
            TestContext.WriteLine(result.ToString());
            Assert.That(result.WinRateA, Is.InRange(0.40, 0.60), result.ToString());
        }
    }
}
