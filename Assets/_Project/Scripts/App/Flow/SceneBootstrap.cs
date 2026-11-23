using UnityEngine;

namespace Escoba.App
{
    /// <summary>
    /// Presente en cada escena: inicializa <see cref="AppContext"/> antes que cualquier otro script,
    /// para que cada escena pueda arrancarse directamente desde el editor.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameSettings _settings;

        private void Awake()
        {
            AppContext.EnsureInitialized(_settings);
        }
    }
}
