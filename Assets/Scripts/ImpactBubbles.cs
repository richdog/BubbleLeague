using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class ImpactBubbles : MonoBehaviour
{
    private static ImpactBubbles _instance;

    private VisualEffect impactBubblesVfx;

    private void Awake()
    {
        _instance = this;
        impactBubblesVfx = GetComponent<VisualEffect>();
    }

    public static void PlayHitEffect(float bubbleCount, Color bubbleColor, Vector3 hitPoint, float hitRadius, Vector3 direction, float force, float directionBlend, float lifetimeMin = 0.25f, float lifetimeMax = 1.5f)
    {
        _instance.impactBubblesVfx.SetFloat("BubbleAmount", bubbleCount);
        _instance.impactBubblesVfx.SetVector4("BubbleColor", bubbleColor);
        _instance.impactBubblesVfx.SetVector3("HitPoint", hitPoint);
        _instance.impactBubblesVfx.SetFloat("HitRadius", hitRadius);
        _instance.impactBubblesVfx.SetVector3("Direction", direction);
        _instance.impactBubblesVfx.SetFloat("OutputSpeed", force);
        _instance.impactBubblesVfx.SetFloat("DirectionBlend", directionBlend);
        _instance.impactBubblesVfx.SetFloat("LifetimeMin", lifetimeMin);
        _instance.impactBubblesVfx.SetFloat("LifetimeMax", lifetimeMax);

        _instance.impactBubblesVfx.SendEvent("Hit");
    }
}
