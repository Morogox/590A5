using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.Experimental.GlobalIllumination;
using System.Collections;

public class ExtensionScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float extention_speed = 0.2f;
    bool on = true;
    float min_scale = 0;
    float max_scale;
    float curr_scale;
    float scale_step;
    float init_x;
    float init_z;

    public GameObject crossguard;
    float crossguard_max_scale;
    float crossguard_curr_scale;
    float crossguard_scale_step;

    float crossguard_init_x;
    float crossguard_init_y;
    float crossguard_init_z;

    bool crossguard_on = true;

    public AudioSource effectsSource;
    public AudioSource humSource;

    public AudioClip startSound;
    public AudioClip humSound;
    public AudioClip endSound;

    public GameObject blade;


    public Renderer[] bladeRenderers;
    public Light[] lights;
    MaterialPropertyBlock props;
    Color targetColor;
    Color currentColor;
    float colorTimer = 0f;
    float currentHue = 0f;

    bool partyMode = false;
    public AudioSource partyIntroSource;
    public AudioSource partyLoopSource;
    void Start()
    {
        init_x = transform.localScale.x;
        init_z = transform.localScale.z;
        max_scale = transform.localScale.y;
        curr_scale = max_scale;
        scale_step = max_scale / extention_speed;

        crossguard_init_x = crossguard.transform.localScale.x;
        crossguard_init_z = crossguard.transform.localScale.z;

        crossguard_max_scale = crossguard.transform.localScale.y;
        crossguard_curr_scale = crossguard_max_scale;
        crossguard_scale_step = crossguard_max_scale / extention_speed;

        props = new MaterialPropertyBlock();
        currentColor = Color.red;
        targetColor = Color.red;

    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            on = !on;
            scale_step = on ? Mathf.Abs(scale_step) : -Mathf.Abs(scale_step);

            if (on)
            {
                effectsSource.PlayOneShot(startSound);
                humSource.clip = humSound;
                humSource.Play();
                if (partyMode)
                {
                    partyIntroSource.Play();
                    StartCoroutine(PlayPartyLoop());
                }
            }
            else
            {
                humSource.Stop();
                partyLoopSource.Stop();
                effectsSource.PlayOneShot(endSound);
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            
            if (on)
            {
                partyMode = !partyMode;
            }

            if (partyMode && on)
            {
                partyIntroSource.Play();
                StartCoroutine(PlayPartyLoop());
                //targetColor = Random.ColorHSV(
                //    0f, 1f,    // Any hue
                //    0.8f, 1f,  // full saturation
                //    1f, 1f     // Full brightness
                //);
            }
            else
            {
                partyLoopSource.Stop();
            }
        }

        if (on)
        {
            if(curr_scale < max_scale)
            {
                curr_scale += scale_step * Time.deltaTime;
                curr_scale = Mathf.Clamp(curr_scale, min_scale, max_scale);
                //transform.localScale = new Vector3(init_x, curr_scale, init_z);
            }
            else if (crossguard_curr_scale < crossguard_max_scale)
            {
                crossguard_curr_scale += Mathf.Abs(crossguard_scale_step) * Time.deltaTime;
                crossguard_curr_scale = Mathf.Min(crossguard_curr_scale, crossguard_max_scale);
            }

        }
        else
        {
            if (crossguard_curr_scale > 0)
            {
                crossguard_curr_scale -= Mathf.Abs(crossguard_scale_step) * Time.deltaTime;
                crossguard_curr_scale = Mathf.Max(crossguard_curr_scale, 0);
            }
            else if (curr_scale > 0)
            {
                curr_scale -= Mathf.Abs(scale_step) * Time.deltaTime;
                curr_scale = Mathf.Max(curr_scale, 0);
            }
        }
        transform.localScale = new Vector3(init_x, curr_scale, init_z);
        crossguard.transform.localScale = new Vector3(crossguard_init_x, crossguard_curr_scale, crossguard_init_z);
        
        blade.SetActive(curr_scale > 0);
        crossguard.SetActive(crossguard_curr_scale > 0);


        if (partyMode && on)
        {
            colorTimer += Time.deltaTime;

            currentColor = Color.Lerp(
                currentColor,
                targetColor,
                Time.deltaTime * 20f
            );

            foreach (Light l in lights)
            {
                l.color = Color.Lerp(
                    currentColor,
                    targetColor,
                    Time.deltaTime * 20f
                );
            }
            

            if (colorTimer >= 0.3636f)
            {
                currentHue += 0.1f;

                if (currentHue >= 1f)
                    currentHue -= 1f;

                targetColor = Color.HSVToRGB(currentHue, 1f, 1f);
                colorTimer = 0f;
            }

            foreach (Renderer rend in bladeRenderers)
            {
                rend.material.SetColor("_BladeColor", currentColor);
            }
        }
    }

    IEnumerator PlayPartyLoop()
    {
        yield return new WaitForSeconds(partyIntroSource.clip.length);

        if (partyMode)
        {
            partyLoopSource.Play();
        }
    }
}
