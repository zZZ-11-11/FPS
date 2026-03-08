using System.Diagnostics;
using UnityEngine;

namespace FPS.Game
{
    public static class DebugUtility
    {
        [Conditional("UNITY_EDITOR")]
        public static void HandleErrorIfNullGetComponent<TO, TS>(Component component, Component source,
            GameObject onObject)
        {
            if (component == null)
            {
                UnityEngine.Debug.LogError($"Error: Component of type {typeof(TS)} on GameObject {source.gameObject.name} " +
                                           $"expected to find a component of type {typeof(TO)} on GameObject {onObject.name}, but none were found.");
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void HandleErrorIfNullFindObject<TO, TS>(UnityEngine.Object obj, Component source)
        {
            if (obj == null)
            {
                UnityEngine.Debug.LogError($"Error: Component of type {typeof(TS)} on GameObject {source.gameObject.name} " +
                                           $"expected to find an object of type {typeof(TO)} in the scene, but none were found.");
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void HandleErrorIfNoComponentFound<TO, TS>(int count, Component source, GameObject onObject)
        {
            if (count == 0)
            {
                UnityEngine.Debug.LogError($"Error: Component of type {typeof(TS)} on GameObject {source.gameObject.name} " +
                                           $"expected to find at least one component of type {typeof(TO)} on GameObject {onObject.name}, but none were found.");
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void HandleWarningIfDuplicateObjects<TO, TS>(int count, Component source, GameObject onObject)
        {
            if (count > 1)
            {
                UnityEngine.Debug.LogWarning($"Warning: Component of type {typeof(TS)} on GameObject {source.gameObject.name} " +
                                             $"expected to find only one component of type {typeof(TO)} on GameObject {onObject.name}, but several were found.");
            }
        }
    }
}