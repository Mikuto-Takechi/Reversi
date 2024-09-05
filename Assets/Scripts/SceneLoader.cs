using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reversi
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadSceneByName(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
