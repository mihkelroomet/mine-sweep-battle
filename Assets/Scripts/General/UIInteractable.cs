using UnityEngine;
using UnityEngine.EventSystems;

public class UIInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Cursors
    public Texture2D CursorPointer;
    public Vector2 CursorPointerHotspot = new Vector2(4, 0);
    public Texture2D CursorDefault;
    public Vector2 CursorDefaultHotspot = Vector2.zero;

    // Audio
    public SFXClipGroup ChooseAudio;

    public void OnPointerEnter(PointerEventData data)
    {
        ChooseAudio.Play(SFXSourcePool.Instance.transform);
        Cursor.SetCursor(CursorPointer, CursorPointerHotspot, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData data)
    {
        Cursor.SetCursor(CursorDefault, CursorDefaultHotspot, CursorMode.ForceSoftware);
    }
}