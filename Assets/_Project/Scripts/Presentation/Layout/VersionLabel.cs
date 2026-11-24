using TMPro;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Muestra la versión de la build (<see cref="Application.version"/>). En CI la versión incluye
    /// el número de ejecución y el commit (p. ej. "0.4.0-dev.12+cace7b2") para saber qué se prueba.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class VersionLabel : MonoBehaviour
    {
        private void OnEnable()
        {
            GetComponent<TMP_Text>().text = "v" + Application.version;
        }
    }
}
