using UnityEngine;
using DrawAndRace.Vehicle;

namespace DrawAndRace.Core
{
    /// <summary>
    /// Dynamic Audio Engine providing real-time pitch-modulated engine sound,
    /// tire screech skid audio, off-track surface rumble, and checkpoint chime SFX.
    /// Supports procedural audio tone synthesis if external audio clips are unassigned.
    /// </summary>
    public class RacingAudioController : MonoBehaviour
    {
        [Header("Vehicle Reference")]
        [SerializeField] private CarPhysicsController _carController;
        [SerializeField] private OffTrackPenaltyHandler _penaltyHandler;
        [SerializeField] private LapTracker _lapTracker;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _engineAudioSource;
        [SerializeField] private AudioSource _skidAudioSource;
        [SerializeField] private AudioSource _offTrackAudioSource;
        [SerializeField] private AudioSource _sfxAudioSource;

        [Header("Engine Sound Tuning")]
        [SerializeField] private float _minPitch = 0.75f;
        [SerializeField] private float _maxPitch = 2.65f;
        [SerializeField] private float _maxSpeedKmh = 220f;

        private bool _isInitialized = false;

        private void Start()
        {
            InitializeAudioSources();
            SubscribeEvents();
        }

        private void Update()
        {
            if (_carController == null) AutoBindComponents();
            if (_carController == null) return;

            UpdateEngineAudio();
            UpdateSkidAudio();
            UpdateOffTrackAudio();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void AutoBindComponents()
        {
            if (_carController == null)
            {
                _carController = GetComponentInParent<CarPhysicsController>() ?? FindObjectOfType<CarPhysicsController>();
                if (_carController != null)
                {
                    _penaltyHandler = _carController.GetComponent<OffTrackPenaltyHandler>();
                    _lapTracker = _carController.GetComponent<LapTracker>();
                    SubscribeEvents();
                }
            }
        }

        private void InitializeAudioSources()
        {
            if (_engineAudioSource == null) _engineAudioSource = CreateAudioSource("EngineAudioSource", true, 0.45f);
            if (_skidAudioSource == null) _skidAudioSource = CreateAudioSource("SkidAudioSource", true, 0.0f);
            if (_offTrackAudioSource == null) _offTrackAudioSource = CreateAudioSource("OffTrackAudioSource", true, 0.0f);
            if (_sfxAudioSource == null) _sfxAudioSource = CreateAudioSource("SFXAudioSource", false, 0.8f);

            // Assign procedural synthetic audio clips if empty
            if (_engineAudioSource.clip == null) _engineAudioSource.clip = CreateProceduralEngineClip();
            if (!_engineAudioSource.isPlaying && _engineAudioSource.clip != null)
            {
                _engineAudioSource.Play();
            }

            _isInitialized = true;
        }

        private AudioSource CreateAudioSource(string name, bool loop, float defaultVolume)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);
            AudioSource source = child.AddComponent<AudioSource>();
            source.loop = loop;
            source.volume = defaultVolume;
            source.playOnAwake = false;
            source.spatialBlend = 0.5f; // 3D spatial mix
            return source;
        }

        private void UpdateEngineAudio()
        {
            if (_engineAudioSource == null) return;

            float speedKmh = _carController.CurrentSpeedKmh;
            float normalizedSpeed = Mathf.Clamp01(speedKmh / _maxSpeedKmh);

            // Calculate pitch based on current gear RPM cycle
            float rpmProgress = (speedKmh % 35f) / 35f;
            float targetPitch = Mathf.Lerp(_minPitch, _maxPitch, (normalizedSpeed * 0.5f) + (rpmProgress * 0.5f));

            _engineAudioSource.pitch = Mathf.Lerp(_engineAudioSource.pitch, targetPitch, Time.deltaTime * 6.0f);
            _engineAudioSource.volume = Mathf.Lerp(0.3f, 0.7f, normalizedSpeed);
        }

        private void UpdateSkidAudio()
        {
            if (_skidAudioSource == null) return;

            bool isDrifting = _carController.IsHandbraking && _carController.CurrentSpeedKmh > 15f;
            float targetVolume = isDrifting ? 0.6f : 0.0f;
            _skidAudioSource.volume = Mathf.Lerp(_skidAudioSource.volume, targetVolume, Time.deltaTime * 10f);
        }

        private void UpdateOffTrackAudio()
        {
            if (_offTrackAudioSource == null || _penaltyHandler == null) return;

            bool isOffTrack = _penaltyHandler.IsOffTrack && _carController.CurrentSpeedKmh > 5f;
            float targetVolume = isOffTrack ? 0.5f : 0.0f;
            _offTrackAudioSource.volume = Mathf.Lerp(_offTrackAudioSource.volume, targetVolume, Time.deltaTime * 8f);
        }

        private void SubscribeEvents()
        {
            if (_lapTracker != null)
            {
                _lapTracker.OnCheckpointPassed += PlayCheckpointChime;
                _lapTracker.OnRaceFinished += PlayRaceFinishFanfare;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_lapTracker != null)
            {
                _lapTracker.OnCheckpointPassed -= PlayCheckpointChime;
                _lapTracker.OnRaceFinished -= PlayRaceFinishFanfare;
            }
        }

        private void PlayCheckpointChime(int checkpointIndex, float time)
        {
            if (_sfxAudioSource != null)
            {
                _sfxAudioSource.pitch = 1.0f + (checkpointIndex * 0.05f);
                _sfxAudioSource.PlayOneShot(CreateProceduralChimeClip());
            }
        }

        private void PlayRaceFinishFanfare(float totalTime)
        {
            if (_sfxAudioSource != null)
            {
                _sfxAudioSource.pitch = 1.2f;
                _sfxAudioSource.PlayOneShot(CreateProceduralFanfareClip());
            }
        }

        #region Procedural Audio Synthesizers
        private AudioClip CreateProceduralEngineClip()
        {
            int sampleRate = 44100;
            int sampleCount = sampleRate * 2; // 2 sec loop
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float wave1 = Mathf.Sin(2f * Mathf.PI * 110f * t); // Low fundamental 110Hz
                float wave2 = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.5f;
                float noise = (Random.value * 2f - 1f) * 0.15f;
                samples[i] = (wave1 + wave2 + noise) * 0.35f;
            }

            AudioClip clip = AudioClip.Create("ProceduralEngine", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateProceduralChimeClip()
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * 0.3f);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t * 12f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("ProceduralChime", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateProceduralFanfareClip()
        {
            int sampleRate = 44100;
            int sampleCount = sampleRate * 1;
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(1f - t);
                float chord = (Mathf.Sin(2f * Mathf.PI * 523.25f * t) + Mathf.Sin(2f * Mathf.PI * 659.25f * t) + Mathf.Sin(2f * Mathf.PI * 783.99f * t)) * 0.33f;
                samples[i] = chord * envelope;
            }

            AudioClip clip = AudioClip.Create("ProceduralFanfare", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
        #endregion
    }
}
