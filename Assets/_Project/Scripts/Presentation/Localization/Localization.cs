using System;

namespace Escoba.Presentation
{
    /// <summary>
    /// Punto de acceso al servicio de localización para los componentes de UI. Lo inicializa el
    /// bootstrap de la App. No guarda estado de partida, solo el servicio de textos.
    /// </summary>
    public static class Localization
    {
        private static LocalizationService _service;

        /// <summary>Se lanza al inicializar el servicio o cambiar de idioma.</summary>
        public static event Action Changed;

        /// <summary>Servicio activo (null antes del bootstrap).</summary>
        public static LocalizationService Service => _service;

        /// <summary>Establece el servicio activo.</summary>
        public static void Initialize(LocalizationService service)
        {
            if (_service != null)
            {
                _service.LanguageChanged -= RaiseChanged;
            }

            _service = service;
            if (_service != null)
            {
                _service.LanguageChanged += RaiseChanged;
            }

            RaiseChanged();
        }

        /// <summary>Texto de una clave (o <c>[clave]</c> si el servicio no está listo).</summary>
        public static string Get(string key, params object[] args)
        {
            return _service != null ? _service.Get(key, args) : $"[{key}]";
        }

        /// <summary>Texto en plural.</summary>
        public static string GetPlural(string keyBase, int count, params object[] extraArgs)
        {
            return _service != null ? _service.GetPlural(keyBase, count, extraArgs) : $"[{keyBase}]";
        }

        private static void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}
