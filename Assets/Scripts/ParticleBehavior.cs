using UnityEngine;
using System.Collections;

public class ParticleBehavior : MonoBehaviour {
    
    public ParticleSystem _Particle;
    public GameObject _Self;

	// Update is called once per frame
	void Update () {
        if (!_Particle.IsAlive())
            Destroy(_Self);
	}
}
