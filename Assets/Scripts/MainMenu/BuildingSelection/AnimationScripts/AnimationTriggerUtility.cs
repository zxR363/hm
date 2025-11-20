using UnityEngine;

public static class AnimationTriggerUtility
{
    public static void TriggerAreaAnimations(this GameObject root)
    {
        MonoBehaviour[] allBehaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            var method = behaviour.GetType().GetMethod("TriggerAnimations");
            if (method != null)
            {
                method.Invoke(behaviour, null);
            }
        }
    }
}