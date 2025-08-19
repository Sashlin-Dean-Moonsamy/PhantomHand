using UnityEngine;
using UnityEngine.SceneManagement;

// Attach this to a trigger collider (like an invisible box) to load a new scene
// When Carl touches this object, Unity will load the specified scene
public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad; // Name of the scene to load (must be added to Build Settings)

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the thing entering the trigger is the Player
        if (other.CompareTag("Player"))
        {
            // Load the new scene by name
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
