using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>Hace sonar <see cref="SoundCue.Click"/> al pulsar el botón.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonClickSound : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => Sounds.Play(SoundCue.Click));
        }
    }
}
