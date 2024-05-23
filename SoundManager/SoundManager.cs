using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

namespace RobinsonGaming.Audio
{
    /// <summary>
    /// The SoundManager class handles the playback and management of sound effects and background music in the game.
    /// It also manages the persistence of user settings using PlayerPrefs.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        // Singleton instance
        public static SoundManager Instance;

        // separate Audio sources for effects and background music
        [SerializeField] private AudioSource _effectsSource;
        [SerializeField] private AudioSource _backgroundMusicSource;
        [SerializeField] private float _backgroundMusicVolume = 0.1f;


        // Preference keys for player settings (allows for caching of settings)
        private const string EffectsPreferenceKey = "EffectsStatus";
        private const string BackgroundMusicPreferenceKey = "BackgroundMusicStatus";
        private const string HapticsPreferenceKey = "HapticsStatus";

        // Public properties to access sound and haptic settings
        public bool FxEnabled { get; private set; }
        public bool BackgroundMusicEnabled { get; private set; }
        public bool HapticEnabled { get; private set; }

        // Paths to audio clips in the Resources folder
        private const string BackgroundLoopPath = "Audio/BackgroundMusic";
        private Dictionary<string, string> _audioClipPaths = new Dictionary<string, string>
        {
            { "ButtonClick", "Audio/ButtonClick.mp3" },
            { "Example1", "Audio/Example1.mp3" },
            { "Example2", "Audio/Example2.ogg" },
        };


        public static event Action OnSoundSettingsChanged;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Initialize player sound and haptic preferences
            InitializePreferences();
            ApplySettings();
        }

        /// Initializes player preferences with default values if they do not exist.
        private void InitializePreferences()
        {
            if (!PlayerPrefs.HasKey(EffectsPreferenceKey))
                PlayerPrefs.SetInt(EffectsPreferenceKey, 1);
            if (!PlayerPrefs.HasKey(BackgroundMusicPreferenceKey))
                PlayerPrefs.SetInt(BackgroundMusicPreferenceKey, 1);
            if (!PlayerPrefs.HasKey(HapticsPreferenceKey))
                PlayerPrefs.SetInt(HapticsPreferenceKey, 1);
        }

        /// Applies the sound settings based on stored player preferences.
        private void ApplySettings()
        {
            FxEnabled = PlayerPrefs.GetInt(EffectsPreferenceKey) == 1;
            BackgroundMusicEnabled = PlayerPrefs.GetInt(BackgroundMusicPreferenceKey) == 1;
            HapticEnabled = PlayerPrefs.GetInt(HapticsPreferenceKey) == 1;

            _effectsSource.mute = !FxEnabled;
            _backgroundMusicSource.mute = !BackgroundMusicEnabled;

            if (BackgroundMusicEnabled)
            {
                _backgroundMusicSource.clip = Resources.Load<AudioClip>(BackgroundLoopPath);
                _backgroundMusicSource.loop = true;
                _backgroundMusicSource.volume = _backgroundMusicVolume;
                _backgroundMusicSource.Play();
            }


            OnSoundSettingsChanged?.Invoke();   // HapticManager would subscribe to this event
        }

        /// Toggles the haptic feedback setting.
        public void ToggleHaptics()
        {
            HapticEnabled = !HapticEnabled;
            PlayerPrefs.SetInt(HapticsPreferenceKey, HapticEnabled ? 1 : 0);
            Bug.Log("Haptics Preference set to: " + HapticEnabled);

            OnSoundSettingsChanged?.Invoke();
        }

        /// Toggles the sound effects setting.
        public void ToggleFx()
        {
            FxEnabled = !FxEnabled;
            _effectsSource.mute = !FxEnabled;
            PlayerPrefs.SetInt(EffectsPreferenceKey, FxEnabled ? 1 : 0);
            Bug.Log("Fx Preference set to: " + FxEnabled);

            if (FxEnabled)
            {
                PlayButtonClick();
            }

            OnSoundSettingsChanged?.Invoke();
        }

        /// Toggles the background music setting.
        public void ToggleBackground()
        {
            BackgroundMusicEnabled = !BackgroundMusicEnabled;
            _backgroundMusicSource.mute = !BackgroundMusicEnabled;
            PlayerPrefs.SetInt(BackgroundMusicPreferenceKey, BackgroundMusicEnabled ? 1 : 0);
            Bug.Log("Background Music Preference set to: " + BackgroundMusicEnabled);

            if (BackgroundMusicEnabled)
            {
                _backgroundMusicSource.clip = Resources.Load<AudioClip>(BackgroundLoopPath);
                _backgroundMusicSource.loop = true;
                _backgroundMusicSource.volume = _backgroundMusicVolume;
                _backgroundMusicSource.Play();
            }

            PlayButtonClick();
            OnSoundSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Plays a sound effect from the Resources folder.
        /// </summary>
        /// <param name="clipName">The name of the audio clip to play.</param>
        private void PlaySoundEffect(string clipName)
        {
            if (FxEnabled && _audioClipPaths.ContainsKey(clipName))
            {
                AudioClip clip = Resources.Load<AudioClip>(_audioClipPaths[clipName]);
                if (clip != null)
                {
                    _effectsSource.PlayOneShot(clip);
                }
                else
                {
                    Bug.LogWarning("Audio clip not found in Resources: " + clipName);
                }
            }
        }

        // Public methods for playing specific sound effects
        public void PlayButtonClick() => PlaySoundEffect("ButtonClick");
        public void PlayExample1() => PlaySoundEffect("Example1");
        public void PlayExample2() => PlaySoundEffect("Example2");

    }
}