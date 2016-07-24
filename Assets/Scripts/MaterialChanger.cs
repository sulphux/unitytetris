using UnityEngine;
using System.Collections;

public class MaterialChanger : MonoBehaviour {

    [SerializeField]
    private Material _Material_1;

    [SerializeField]
    private Material _Material_2;
    
    private Renderer _Renderer;

    private float changerTimer;

    // Use this for initialization
    void Start () {
        _Renderer = GetComponent<Renderer>();
        _Renderer.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
        changerTimer -= Time.deltaTime * 4.0f;

        if (changerTimer < 0)
        {
            changerTimer = 10.0f;
            _Renderer.material = _Material_1;
        }

        if (changerTimer > 5.0f && changerTimer < 5.1f)
        {
            _Renderer.material = _Material_2;
        }


	}
}
