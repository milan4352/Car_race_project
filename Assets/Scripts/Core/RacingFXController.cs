using UnityEngine;
using DrawAndRace.Vehicle;

namespace DrawAndRace.Core
{
    /// <summary>
    /// Manages visual particle FX including tire smoke during drifting,
    /// exhaust backfire pops during high acceleration, and grass debris when driving off-track.
    /// </summary>
    public class RacingFXController : MonoBehaviour
    {
        [Header("Vehicle References")]
        [SerializeField] private CarPhysicsController _carController;
        [SerializeField] private OffTrackPenaltyHandler _penaltyHandler;

        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem _leftTireSmoke;
        [SerializeField] private ParticleSystem _rightTireSmoke;
        [SerializeField] private ParticleSystem _exhaustFlamePop;
        [SerializeField] private ParticleSystem _grassDebris;

        private void Start()
        {
            AutoBindComponents();
            CreateProceduralParticleFX();
        }

        private void Update()
        {
            if (_carController == null) AutoBindComponents();
            if (_carController == null) return;

            UpdateDriftTireSmoke();
            UpdateOffTrackGrassDebris();
            UpdateExhaustFlame();
        }

        private void AutoBindComponents()
        {
            if (_carController == null)
            {
                _carController = GetComponentInParent<CarPhysicsController>() ?? FindObjectOfType<CarPhysicsController>();
                if (_carController != null)
                {
                    _penaltyHandler = _carController.GetComponent<OffTrackPenaltyHandler>();
                }
            }
        }

        private void UpdateDriftTireSmoke()
        {
            bool isDrifting = _carController.IsHandbraking && _carController.CurrentSpeedKmh > 15f;

            SetParticleEmission(_leftTireSmoke, isDrifting);
            SetParticleEmission(_rightTireSmoke, isDrifting);
        }

        private void UpdateOffTrackGrassDebris()
        {
            if (_penaltyHandler == null) return;
            bool isOffTrack = _penaltyHandler.IsOffTrack && _carController.CurrentSpeedKmh > 8f;
            SetParticleEmission(_grassDebris, isOffTrack);
        }

        private void UpdateExhaustFlame()
        {
            // Trigger exhaust flame pop during high throttle gear changes
            if (_carController.CurrentSpeedKmh > 80f && Input.GetKeyDown(KeyCode.W))
            {
                if (_exhaustFlamePop != null) _exhaustFlamePop.Play();
            }
        }

        private void SetParticleEmission(ParticleSystem ps, bool enable)
        {
            if (ps == null) return;
            var emission = ps.emission;
            emission.enabled = enable;
        }

        private void CreateProceduralParticleFX()
        {
            if (_leftTireSmoke == null) _leftTireSmoke = CreateParticleChild("LeftTireSmoke", new Vector3(-0.95f, 0.1f, -1.4f), Color.white);
            if (_rightTireSmoke == null) _rightTireSmoke = CreateParticleChild("RightTireSmoke", new Vector3(0.95f, 0.1f, -1.4f), Color.white);
            if (_grassDebris == null) _grassDebris = CreateParticleChild("GrassDebris", new Vector3(0, 0.1f, -1.5f), new Color(0.2f, 0.5f, 0.1f));
            if (_exhaustFlamePop == null) _exhaustFlamePop = CreateParticleChild("ExhaustFlame", new Vector3(0, 0.3f, -2.25f), new Color(1.0f, 0.4f, 0.1f));
        }

        private ParticleSystem CreateParticleChild(string name, Vector3 localPos, Color color)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = localPos;

            ParticleSystem ps = child.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = 0.4f;
            main.startLifetime = 0.5f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 30f;
            emission.enabled = false;

            return ps;
        }
    }
}
