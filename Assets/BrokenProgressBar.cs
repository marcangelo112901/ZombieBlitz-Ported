using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

public class BrokenProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform[] BackgroundBars;
    [SerializeField] private RectTransform[] ValueBars;
    public bool lerp = true;

    public float smoothTime = 0.3f;
    [Range(0f, 1f)] public float value;
    float velocity;
    public float currentValue = 0f;
    float targetValue = 0f;


    private void OnValidate()
    {
        AssignVariables();
        ChangeValues();
    }

    private void Awake()
    {
        AssignVariables();
        ChangeValues();
    }

    public void SmoothChangeValue(float newValue)
    {
        targetValue = Mathf.Clamp01(newValue);
        
    }

    private void Update()
    {
        if (lerp)
            currentValue = Mathf.SmoothDamp(currentValue, targetValue, ref velocity, smoothTime);
        value = currentValue;
        ChangeValues();
    }

    private void AssignVariables()
    {
        if (ValueBars == null || ValueBars.Length == 0)
        {
            List<RectTransform> ValueBars = new List<RectTransform>();
            foreach (RectTransform BackgroundBar in BackgroundBars)
            {
                ValueBars.Add(BackgroundBar.GetChild(0).GetComponent<RectTransform>());
            }
            this.ValueBars = ValueBars.ToArray();
        }
    }

    private void ChangeValues()
    {
        float currentValue = value * BackgroundBars.Length;
        for (int i = 0; i < BackgroundBars.Length; i++)
        {
            ChangeBar(BackgroundBars[i], ValueBars[i], currentValue);
            currentValue -= 1f;
        }
    }

    private void ChangeBar(RectTransform Background, RectTransform Bar, float Value)
    {
        float width = Background.rect.width;
        float rightPadding = width - (width * Mathf.Clamp01(Value));
        Bar.offsetMax = new Vector2(-rightPadding, Background.offsetMax.y);
    }
}
