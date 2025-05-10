using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraScript : MonoBehaviour
{
    public static CameraScript Instance { private set; get; }
    private CinemachineCamera cCamera;
    private CinemachineBasicMultiChannelPerlin noiseComponent;

    public float amplitude = 0;
    public float shakeTime = 0;
    public float shakeTimer = 0;

    private void Awake()
    {
        noiseComponent = GetComponent<CinemachineBasicMultiChannelPerlin>();
        cCamera = GetComponent<CinemachineCamera>();
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void setTrackingTargets(Transform main, Transform cursor)
    {
        CinemachineTargetGroup targetGroup = cCamera.Follow.GetComponent<CinemachineTargetGroup>();
        targetGroup.Targets.Clear();
        targetGroup.AddMember(main, 2f, 0f);
        targetGroup.AddMember(cursor, 1f, 0f);
    }

    public void shakeCamera(float shakeTime, float amplitude, float frequency)
    {
        if (noiseComponent.AmplitudeGain <= amplitude)
        {
            this.shakeTime = shakeTime;
            shakeTimer = shakeTime;
            this.amplitude = amplitude;

            noiseComponent.AmplitudeGain = amplitude;
            noiseComponent.FrequencyGain = frequency;
        }
    }

    private void Update()
    {

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            noiseComponent.AmplitudeGain = (shakeTimer / shakeTime) * amplitude;
        }
        else
        {
            noiseComponent.AmplitudeGain = 0f;
        }
    }
}
