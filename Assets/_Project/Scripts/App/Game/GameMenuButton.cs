using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>Botón "Menú" de la mesa: abandona la partida y vuelve al menú principal.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class GameMenuButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(SceneNames.MainMenu));
        }
    }
}
