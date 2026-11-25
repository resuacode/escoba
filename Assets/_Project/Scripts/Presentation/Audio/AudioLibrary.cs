using System;
using System.Collections.Generic;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Clips de audio del juego: varios clips por efecto (se elige uno al azar para que no suene
    /// repetitivo), la música del menú y la lista de músicas de partida. Asset en
    /// <c>Assets/_Project/Data/AudioLibrary.asset</c>, generado por <c>Escoba/Build/Create Default Assets</c>
    /// a partir de <c>Assets/_Project/Audio/</c> (efectos en <c>Sfx/</c>, música en <c>Music/Menu</c> y <c>Music/Game</c>).
    /// </summary>
    [CreateAssetMenu(menuName = "Escoba/Audio Library", fileName = "AudioLibrary")]
    public sealed class AudioLibrary : ScriptableObject
    {
        [SerializeField] private CueClips[] _cues = new CueClips[0];
        [SerializeField] private AudioClip _menuMusic;
        [SerializeField] private AudioClip[] _gameMusic = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.6f;

        /// <summary>Música del menú principal (puede ser null).</summary>
        public AudioClip MenuMusic => _menuMusic;

        /// <summary>Músicas de partida: suena una al azar y enlaza con las demás.</summary>
        public IReadOnlyList<AudioClip> GameMusic => _gameMusic;

        /// <summary>Volumen base de la música (se multiplica por el del jugador).</summary>
        public float MusicVolume => _musicVolume;

        /// <summary>Clips y volumen base de un efecto (null si no hay entrada).</summary>
        public CueClips Get(SoundCue cue)
        {
            foreach (CueClips entry in _cues)
            {
                if (entry != null && entry.Cue == cue)
                {
                    return entry;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        /// <summary>Solo editor: rellena la biblioteca (usado al generar los assets por defecto).</summary>
        public void EditorConfigure(IEnumerable<CueClips> cues, AudioClip menuMusic, IEnumerable<AudioClip> gameMusic)
        {
            _cues = new List<CueClips>(cues).ToArray();
            _menuMusic = menuMusic;
            _gameMusic = new List<AudioClip>(gameMusic).ToArray();
        }
#endif

        /// <summary>Clips de un efecto.</summary>
        [Serializable]
        public sealed class CueClips
        {
            [SerializeField] private SoundCue _cue;
            [SerializeField] private AudioClip[] _clips = new AudioClip[0];
            [SerializeField, Range(0f, 1f)] private float _volume = 1f;

            public CueClips(SoundCue cue, AudioClip[] clips, float volume)
            {
                _cue = cue;
                _clips = clips;
                _volume = volume;
            }

            public SoundCue Cue => _cue;

            public IReadOnlyList<AudioClip> Clips => _clips;

            public float Volume => _volume;
        }
    }
}
