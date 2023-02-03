using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Transitions : MonoBehaviour
{
    public static Transitions Instance;
    public Animator Transition;
    public float TransitionTime = 0.7f;
    
    // Cursor
    public Texture2D CursorDefault;
    public Vector2 CursorDefaultHotspot = Vector2.zero;

    private void Awake()
    {
        if (Instance)
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
        Transition.SetTrigger("Enter");
        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.Auto);
    }

    public void PlayExitTransition()
    {
        Transition.SetTrigger("Exit");
    }

    /// <summary>
    /// Blocks raycasts immediately. They will be unblocked by the enter transition of the next scene finishing.
    /// </summary>
    public void PlayExitEmptyTransition()
    {
        Transition.SetTrigger("ExitEmpty");
    }
}
