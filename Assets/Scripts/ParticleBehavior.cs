using UnityEngine;
using System.Collections;

public class ParticleBehavior : MonoBehaviour {
    
    [SerializeField]
    private ParticleSystem _Particle;

    [SerializeField]
    private GameObject _Self;

    [SerializeField]
    private Light _Light;

    private float _LightLevel;


    void Start()
    {
        _LightLevel = 4.0f;
        _Light.intensity = _LightLevel;
    }
    

    // Update is called once per frame
    void Update () {
        if (!_Particle.IsAlive())
            Destroy(_Self);

        _UpdateLightLevel();

	}

    private void _UpdateLightLevel()
    {
        _LightLevel = Mathf.Lerp(_LightLevel, 0.0f, 0.2f);

        _Light.intensity = _LightLevel;
    }
}
