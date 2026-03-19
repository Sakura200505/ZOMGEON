using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectScene : MonoBehaviour
{
    public void change_button()
    {
        SceneManager.LoadScene("SelectScene");
    }
}

