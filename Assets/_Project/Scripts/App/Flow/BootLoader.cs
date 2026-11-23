using UnityEngine;
using UnityEngine.SceneManagement;

namespace Escoba.App
{
    /// <summary>Escena Boot: tras inicializar los servicios, pasa al menú principal.</summary>
    public sealed class BootLoader : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
