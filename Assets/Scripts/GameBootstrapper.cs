using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private string sceneToLoadAfterBoot = "City";

    void Start()
    {
        SceneManager.LoadScene(sceneToLoadAfterBoot);
    }
}