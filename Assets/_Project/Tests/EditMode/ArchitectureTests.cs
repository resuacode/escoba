using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Escoba.Tests.EditMode
{
    /// <summary>
    /// Tests de arquitectura: los ensamblados de lógica (Core, AI, Net) deben ser
    /// C# puro, sin dependencias de Unity, para ser deterministas, testeables y
    /// reutilizables en red (Photon).
    /// </summary>
    public class ArchitectureTests
    {
        private static readonly Type[] PureAssemblyMarkers =
        {
            typeof(Escoba.Core.CoreAssemblyMarker),
            typeof(Escoba.AI.AIAssemblyMarker),
            typeof(Escoba.Net.NetAssemblyMarker),
        };

        [TestCaseSource(nameof(PureAssemblyMarkers))]
        public void PureAssembly_DoesNotReferenceUnity(Type marker)
        {
            Assembly assembly = marker.Assembly;
            string[] unityRefs = assembly.GetReferencedAssemblies()
                .Select(a => a.Name)
                .Where(n => n.StartsWith("UnityEngine", StringComparison.Ordinal)
                         || n.StartsWith("UnityEditor", StringComparison.Ordinal)
                         || n.StartsWith("Unity.", StringComparison.Ordinal))
                .ToArray();

            Assert.That(unityRefs, Is.Empty,
                $"{assembly.GetName().Name} no debe referenciar Unity: {string.Join(", ", unityRefs)}");
        }
    }
}
