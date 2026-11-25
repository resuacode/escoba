using Escoba.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Escoba.Tests.EditMode
{
    public class SafeCanvasScalerTests
    {
        private static readonly Vector2 Reference = new Vector2(1920f, 1080f);
        private const float Margin = 24f;

        // Zonas seguras en píxeles: 16:9, móvil 20:9, móvil 20:9 con muesca, navegador panorámico
        // de un iPhone pequeño (barras del navegador), tablet 4:3 y monitor 21:9.
        private static readonly Vector2[] SafeAreas =
        {
            new Vector2(1920f, 1080f), new Vector2(2400f, 1080f), new Vector2(2290f, 1080f),
            new Vector2(568f, 230f), new Vector2(2048f, 1536f), new Vector2(3440f, 1440f),
        };

        [TestCaseSource(nameof(SafeAreas))]
        public void ComputeScale_DesignAndMarginAlwaysFitInsideSafeArea(Vector2 safe)
        {
            float scale = SafeCanvasScaler.ComputeScale(safe, Reference, Margin);
            Vector2 needed = (Reference + (2f * Margin * Vector2.one)) * scale;

            Assert.That(needed.x, Is.LessThanOrEqualTo(safe.x + 0.01f), "el diseño cabe a lo ancho");
            Assert.That(needed.y, Is.LessThanOrEqualTo(safe.y + 0.01f), "el diseño cabe a lo alto");
            Assert.That(Mathf.Max(needed.x / safe.x, needed.y / safe.y), Is.EqualTo(1f).Within(0.001f), "y ocupa todo lo posible en un eje");
        }

        [Test]
        public void ComputeScale_WideScreens_KeepFullDesignHeight()
        {
            // En 20:9 manda la altura: los botones de arriba y abajo quedan a la distancia del margen.
            float scale = SafeCanvasScaler.ComputeScale(new Vector2(2400f, 1080f), Reference, Margin);
            Assert.That((Reference.y + (2f * Margin)) * scale, Is.EqualTo(1080f).Within(0.01f));
            Assert.That(2400f / scale, Is.GreaterThan(Reference.x), "sobra ancho a los lados");
        }

        [Test]
        public void ComputeScale_InvalidSizes_ReturnOne()
        {
            Assert.That(SafeCanvasScaler.ComputeScale(Vector2.zero, Reference, Margin), Is.EqualTo(1f));
        }
    }
}
