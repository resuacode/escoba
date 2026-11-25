using System.Linq;
using Escoba.App;
using Escoba.Core;
using Escoba.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Escoba.Tests.EditMode
{
    public class HowToPlayGuideTests
    {
        private static LocalizationTable Spanish()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/_Project/Localization/es.json");
            return LocalizationTable.FromJson(asset.text);
        }

        [Test]
        public void Guide_EveryPageHasItsTexts()
        {
            LocalizationTable es = Spanish();
            Assert.That(HowToPlayGuide.Pages.Count, Is.GreaterThanOrEqualTo(8));
            foreach (HowToPlayPage page in HowToPlayGuide.Pages)
            {
                Assert.That(es.TryGet(page.TitleKey, out _), Is.True, page.TitleKey);
                Assert.That(es.TryGet(page.BodyKey, out _), Is.True, page.BodyKey);
                Assert.That(es.TryGet(page.CaptionKey, out _), Is.EqualTo(page.HasExample), $"{page.CaptionKey}: pie solo si hay ejemplo");
            }

            Assert.That(HowToPlayGuide.Pages.Select(p => p.Key).Distinct().Count(), Is.EqualTo(HowToPlayGuide.Pages.Count));
        }

        [Test]
        public void Guide_ExamplesUseDistinctCardsAndFitTheSlots()
        {
            foreach (HowToPlayPage page in HowToPlayGuide.Pages)
            {
                var cards = page.Played.Concat(page.Table).ToList();
                Assert.That(cards.Distinct().Count(), Is.EqualTo(cards.Count), page.Key);
                Assert.That(cards.Count, Is.LessThanOrEqualTo(6), $"{page.Key}: el panel tiene 6 huecos");
            }
        }

        [Test]
        public void Guide_PlayExamples_AreLegalCapturesOfFifteen()
        {
            foreach (HowToPlayPage page in HowToPlayGuide.Pages.Where(p => p.IsPlay))
            {
                Assert.That(page.Played.Count, Is.EqualTo(1), page.Key);
                int played = page.Played[0].Value;
                int tableCount = page.Table.Count;
                int captures = 0;
                for (int mask = 1; mask < (1 << tableCount); mask++)
                {
                    int sum = played + Enumerable.Range(0, tableCount).Where(i => (mask & (1 << i)) != 0).Sum(i => page.Table[i].Value);
                    if (sum == 15)
                    {
                        captures++;
                    }
                }

                Assert.That(captures, Is.GreaterThan(0), $"{page.Key}: el ejemplo debe poder sumar 15");
            }

            // Ejemplos concretos que describe el texto.
            HowToPlayPage mandatory = HowToPlayGuide.Pages.Single(p => p.Key == "howto.mandatory");
            Assert.That(mandatory.Played[0].Value, Is.EqualTo(10), "rey = 10");
            HowToPlayPage escoba = HowToPlayGuide.Pages.Single(p => p.Key == "howto.escoba");
            Assert.That(escoba.Played[0].Value + escoba.Table.Sum(c => c.Value), Is.EqualTo(15), "la escoba se lleva toda la mesa");
        }
    }
}
