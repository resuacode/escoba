using System.Collections;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Reproduce efectos y música. Vive entre escenas (lo crea el bootstrap de la App), así que la
    /// música no se corta al cambiar de escena. Los volúmenes los fija la App según los ajustes.
    /// <para>
    /// Música: el menú repite su tema y la partida elige una pista al azar y, al acabar, enlaza con
    /// otra distinta. Los cambios de pista se hacen con fundido cruzado entre dos fuentes, lo que
    /// además evita el pequeño silencio de los MP3 al repetirse. Los fundidos usan tiempo real,
    /// así que siguen funcionando con la partida en pausa.
    /// </para>
    /// </summary>
    public sealed class AudioService : MonoBehaviour
    {
        private const float PitchVariation = 0.05f;
        private const float FadeSeconds = 2.5f;
        private const float RetrySeconds = 1f;

        private enum MusicMode
        {
            None,
            Menu,
            Game,
        }

        private readonly AudioSource[] _musicSources = new AudioSource[2];
        private readonly float[] _fade = new float[2];
        private readonly float[] _fadeTarget = new float[2];

        private AudioLibrary _library;
        private AudioSource _sfx;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private int _lastIndex = -1;
        private int _active;
        private MusicMode _mode;
        private float _nextCheck;

        /// <summary>Indica si está sonando alguna música.</summary>
        public bool IsMusicPlaying => _musicSources[0] != null && (_musicSources[0].isPlaying || _musicSources[1].isPlaying);

        /// <summary>Pista de música principal en este momento (null si no suena ninguna).</summary>
        public AudioClip CurrentMusic => _mode != MusicMode.None && _musicSources[_active] != null ? _musicSources[_active].clip : null;

        /// <summary>Crea el servicio en un objeto persistente.</summary>
        public static AudioService Create(AudioLibrary library)
        {
            var go = new GameObject("AudioService");
            DontDestroyOnLoad(go);
            var service = go.AddComponent<AudioService>();
            service._library = library;
            for (int i = 0; i < service._musicSources.Length; i++)
            {
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.volume = 0f;
                service._musicSources[i] = source;
            }

            service._sfx = go.AddComponent<AudioSource>();
            service._sfx.playOnAwake = false;
            return service;
        }

        /// <summary>Volúmenes del jugador (0-1). Con la música a 0 se detiene; al subirla vuelve a sonar.</summary>
        public void SetVolumes(float music, float sfx)
        {
            _musicVolume = Mathf.Clamp01(music);
            _sfxVolume = Mathf.Clamp01(sfx);
            if (_musicVolume <= 0f)
            {
                StopMusicSources();
            }
            else if (_mode != MusicMode.None && !IsMusicPlaying)
            {
                Advance();
            }
        }

        /// <summary>Música del menú (si ya suena, sigue sin cortarse).</summary>
        public void PlayMenuMusic()
        {
            if (_mode == MusicMode.Menu && IsMusicPlaying)
            {
                return;
            }

            _mode = MusicMode.Menu;
            Advance();
        }

        /// <summary>Empieza una pista de partida al azar (distinta de la que suena).</summary>
        public void PlayGameMusic()
        {
            _mode = MusicMode.Game;
            Advance();
        }

        /// <summary>Reproduce un efecto (un clip al azar de los del efecto, con un ligero cambio de tono).</summary>
        public void Play(SoundCue cue)
        {
            AudioLibrary.CueClips entry = _library != null ? _library.Get(cue) : null;
            if (entry == null || entry.Clips.Count == 0 || _sfxVolume <= 0f)
            {
                return;
            }

            int index = Random.Range(0, entry.Clips.Count);
            if (entry.Clips.Count > 1 && index == _lastIndex)
            {
                index = (index + 1) % entry.Clips.Count;
            }

            _lastIndex = index;
            AudioClip clip = entry.Clips[index];
            if (clip == null)
            {
                return;
            }

            _sfx.pitch = 1f + Random.Range(-PitchVariation, PitchVariation);
            _sfx.PlayOneShot(clip, entry.Volume * _sfxVolume);
        }

        /// <summary>Reproduce un efecto tras <paramref name="delay"/> segundos (tiempo de juego).</summary>
        public void Play(SoundCue cue, float delay)
        {
            if (delay <= 0f)
            {
                Play(cue);
                return;
            }

            StartCoroutine(PlayLater(cue, delay));
        }

        private IEnumerator PlayLater(SoundCue cue, float delay)
        {
            yield return new WaitForSeconds(delay);
            Play(cue);
        }

        private void Update()
        {
            if (_musicSources[0] == null)
            {
                return;
            }

            float baseVolume = _musicVolume * (_library != null ? _library.MusicVolume : 1f);
            float step = Time.unscaledDeltaTime / FadeSeconds;
            for (int i = 0; i < _musicSources.Length; i++)
            {
                _fade[i] = Mathf.MoveTowards(_fade[i], _fadeTarget[i], step);
                AudioSource source = _musicSources[i];
                source.volume = _fade[i] * baseVolume;
                if (_fade[i] <= 0f && _fadeTarget[i] <= 0f && source.isPlaying)
                {
                    source.Stop();
                }
            }

            // Encadenar la siguiente pista antes de que acabe la actual (o reintentar si no suena).
            if (_mode == MusicMode.None || _musicVolume <= 0f || Time.unscaledTime < _nextCheck)
            {
                return;
            }

            AudioSource active = _musicSources[_active];
            bool ending = active.isPlaying && active.clip != null && active.clip.length - active.time <= FadeSeconds;
            if (ending || !active.isPlaying)
            {
                Advance();
            }
        }

        private void Advance()
        {
            _nextCheck = Time.unscaledTime + RetrySeconds;
            if (_musicVolume <= 0f)
            {
                return;
            }

            AudioClip clip = PickClip();
            if (clip == null)
            {
                return;
            }

            int next = 1 - _active;
            AudioSource source = _musicSources[next];
            source.clip = clip;
            source.time = 0f;
            source.Play();
            _fade[next] = 0f;
            _fadeTarget[next] = 1f;
            _fadeTarget[_active] = 0f;
            _active = next;
        }

        private AudioClip PickClip()
        {
            if (_library == null)
            {
                return null;
            }

            if (_mode == MusicMode.Menu)
            {
                return _library.MenuMusic;
            }

            var tracks = _library.GameMusic;
            if (tracks.Count == 0)
            {
                return null;
            }

            AudioClip current = _musicSources[_active].isPlaying ? _musicSources[_active].clip : null;
            int index = Random.Range(0, tracks.Count);
            if (tracks.Count > 1 && tracks[index] == current)
            {
                index = (index + 1 + Random.Range(0, tracks.Count - 1)) % tracks.Count;
            }

            return tracks[index];
        }

        private void StopMusicSources()
        {
            for (int i = 0; i < _musicSources.Length; i++)
            {
                if (_musicSources[i] != null)
                {
                    _musicSources[i].Stop();
                }

                _fade[i] = 0f;
                _fadeTarget[i] = 0f;
            }
        }
    }
}
