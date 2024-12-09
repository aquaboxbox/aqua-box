using UnityEngine;

[ExecuteAlways]
public class SunAndMoon : MonoBehaviour
{
    public Transform tilt;
    public Transform direction;

    [Header("Render settings")]

    public Light sun;
    public Light moon;
    public Material skyboxMaterial;

    [Header("Time Settings")]

    [Range(-90, 90)]
    [Tooltip("How high the sun will reach (zenith or not)")]
    private float sunAngle = 0;

    [Range(-180, 180)]
    [Tooltip("Where the sun will set/rise")]
    public float sunDirection;

    [Header("Light Settings")]

    public Color dayAmbientColor;
    public Color nightAmbientColor;
    public int sunNoonTemperature = 6500;
    public int sunSetTemperature = 1600;
    public float maxSunIntensity = 1.4f;
    public float maxMoonIntensity = 0.2f;

    private float timeOfDay;

    private void Start()
    {
        ConfigureRenderSettings();
    }

    public void Update()
    {
        timeOfDay = TimeManager.Instance.GetNormalizedTime();
        tilt.transform.localRotation = Quaternion.Euler(0, 0, sunAngle);
        direction.transform.localRotation = Quaternion.Euler(0, sunDirection, 0);

        // update rotation given time of day
        float rotation = (timeOfDay - 0.5f) * 360f + 90f;
        sun.transform.localRotation = Quaternion.Euler(rotation, 0, 0);
        moon.transform.localRotation = Quaternion.Euler(rotation + 180, 0, 0);

        UpdateLightParameters();
    }


    private void UpdateLightParameters()
    {
        // [-1, 1] where -1 is midnight and 1 is midday
        float groundAngle = Vector3.Dot(sun.transform.forward, Vector3.down);

        SetSunColor(groundAngle);
        SetIntensity(groundAngle);
        SetAmbientColor(groundAngle);
    }

    private void SetIntensity(float groundAngle)
    {
        float k = 10;
        float i = 1 - Mathf.Exp(-k * groundAngle * groundAngle);
        sun.intensity = i * maxSunIntensity;
        moon.intensity = i * maxMoonIntensity;

        if (groundAngle < 0)
        {
            sun.intensity = 0;
        }
        else
        {
            moon.intensity = 0;
        }
    }

    private void SetAmbientColor(float groundAngle)
    {
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, groundAngle);
    }

    private void SetSunColor(float groundAngle)
    {
        if (groundAngle < 0)
        {
            sun.colorTemperature = sunSetTemperature;
            return;
        }

        sun.colorTemperature = Mathf.Lerp(sunSetTemperature, sunNoonTemperature, groundAngle);
    }

    private void ConfigureRenderSettings()
    {
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.sun = sun;
    }
}
