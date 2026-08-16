using UnityEngine;

public class GlobalLightingHandler : MonoBehaviour
{
    Light l;

    void Start()
    {
        l = GetComponent<Light>();
        GlobalEvents.get().goingIndoors.AddListener(turnLightOff);
        GlobalEvents.get().goingOutdoors.AddListener(turnLightOn);
    }

    public void turnLightOff() { l.enabled = false; }
    public void turnLightOn() { l.enabled = true; }

}
