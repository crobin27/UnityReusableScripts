using UnityEngine;
using System;

namespace RobinsonGaming.Debugging
{
    /// <summary>
    /// The Bug class provides enhanced control over debugging logs in Unity.
    /// It allows toggling debugging logs on or off and provides methods for logging messages, warnings, errors, and exceptions.
    /// </summary>
    public static class Bug
    {
        // Event for handling exceptions
        public static event Action<Exception> OnException;

        // Toggle this to turn Bugging on/off.
        private static bool _isBugMode = false;

        public static void Log(string message)
        {
            if (_isBugMode)
            {
                Debug.Log("Bug: " + message);
            }
        }

        public static void LogError(string message = "")
        {
            if (_isBugMode)
            {
                Debug.LogError("Bug Error: " + message);
            }
        }

        public static void LogException(Exception ex)
        {
            OnException?.Invoke(ex);
            if (_isBugMode)
            {
                Debug.LogException(ex);
                Log(ex.ToString());
            }
        }

        public static void LogWarning(string message)
        {
            if (_isBugMode)
            {
                Debug.LogWarning("Bug Warning: " + message);
            }
        }
    }
}
