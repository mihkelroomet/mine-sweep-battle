using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;

    public Animator Transition;
    public float TransitionTime = 1f;

    private void Awake()
    {
        Instance = this;
    }
    public void LoadLevelPhoton(string levelName)
    {
        StartCoroutine(LoadPhoton(levelName));
    }
    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadScene(levelName));
    }

    public void PlayTransition()
    {
        Transition.SetTrigger("Start");
    }

    public void ExitTransition()
    {
        Transition.SetTrigger("Fail");
    }

    IEnumerator LoadPhoton(string levelName)
    {
        Transition.SetTrigger("Start");
        yield return new WaitForSeconds(TransitionTime);
        PhotonNetwork.LoadLevel(levelName);
    }

    IEnumerator LoadScene(string levelName)
    {
        Transition.SetTrigger("Start");
        yield return new WaitForSeconds(TransitionTime);
        SceneManager.LoadScene(levelName);
    }
}
