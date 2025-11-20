using UnityEngine;

public static class PoolCleanupUtility
{
    public static void ClearPoolables(this GameObject root)
    {
        MonoBehaviour[] allBehaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            if (behaviour is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }
        }
    }
}