using UnityEngine;
using System.Collections;

public class BlinkBehavior : MonoBehaviour {

    
    private bool _IsFlashing = false;

    private float _Emission = 0.0f;

    // Use this for initialization
    void Awake()
    {
        _IsFlashing = false;
        _Emission = 0.0f;
    }

    void Start()
    {
        _IsFlashing = false;
        _Emission = 0.0f;
    }

    void OnDisable()
    {
        _IsFlashing = false;
        _Emission = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            _Flash();
        if (_IsFlashing)
            _UpdateFlash();
    }

    public void _Flash()
    {
        _Emission = 1.0f;
        _IsFlashing = true;
    }


    private void _UpdateFlash()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material mat = renderer.material;
        _Emission = Mathf.Lerp(_Emission, 0.0f, 0.08f);

        Color baseColor = Color.red; //Replace this with whatever you want for your base color at emission level '1'

        //Color finalColor = baseColor * Mathf.LinearToGammaSpace(_Actual);
        Color finalColor = baseColor * _Emission;

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_SpecColor", finalColor);

        if (_Emission <= 0.0f)
            _IsFlashing = false;
    }
}
