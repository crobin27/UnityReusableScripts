using UnityEngine;

namespace RobinsonGaming.Bootstrap
{
    /// <summary>
    /// The Bootstrapper class is responsible for initializing essential systems
    /// before the first scene is loaded. This "Systems" prefab contains mainly singleton
    /// managers that persist throught the entirety of the game. 
    /// </summary>
    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute()
        {
            // Load the Systems prefab from the Resources folder
            GameObject systemsPrefab = Resources.Load<GameObject>("Systems");

            // Check if the Systems prefab was found
            if (systemsPrefab != null)
            {
                // Instantiate the Systems prefab
                GameObject systemsInstance = Object.Instantiate(systemsPrefab);
                // Prevent the Systems instance from being destroyed when loading new scenes
                Object.DontDestroyOnLoad(systemsInstance);
            }
            else
            {
                // Log an error if the Systems prefab was not found
                Debug.LogError("Systems prefab not found in Resources folder.");
            }
        }
    }
}
