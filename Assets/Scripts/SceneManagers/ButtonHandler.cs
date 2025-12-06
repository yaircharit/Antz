using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public void MoveToScene(string sceneName = "GameSetupScene")
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        // TODO: exit game
    }
}
