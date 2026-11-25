using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>Botón "Menú" de la mesa: abre la pausa (continuar, ajustes o salir al menú).</summary>
    [RequireComponent(typeof(Button))]
    public sealed class GameMenuButton : MonoBehaviour
    {
        [SerializeField] private PausePanel _pause;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (_pause != null)
                {
                    _pause.Open();
                }
                else
                {
                    SceneManager.LoadScene(SceneNames.MainMenu);
                }
            });
        }
    }
}
