using UnityEngine;

public static class ComponentUtils
{
    public static T GetComponentInImmediateChildren<T>(this Component parent) where T : Component
    {
        foreach (Transform child in parent.transform)
        {
            T component = child.GetComponent<T>();
            if (component != null && component.transform != parent.transform)
            {
                return component;
            }
        }
        return null;
    }
}

