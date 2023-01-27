using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Transitions : MonoBehaviour
{
    public static Transitions Instance;
    public Animator Transition;
    public float TransitionTime = 1f;
    
    // Cursor
    public Texture2D CursorDefault;
    public Vector2 CursorDefaultHotspot = Vector2.zero;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.Auto);
    }

    public void ExitSceneWithTransition(string newSceneName)
    {
        StartCoroutine(ExitSceneWithTransitionCoroutine(newSceneName));
    }

    public IEnumerator ExitSceneWithTransitionCoroutine(string newSceneName)
    {
        PlayExitTransition();
        yield return new WaitForSeconds(TransitionTime);
        PhotonNetwork.LoadLevel(newSceneName);
    }

    public void PlayEnterTransition()
    {
        Transition.SetTrigger("Open");
        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.Auto);
    }

    public void PlayExitTransition()
    {
        Transition.SetTrigger("Close");
    }
}
