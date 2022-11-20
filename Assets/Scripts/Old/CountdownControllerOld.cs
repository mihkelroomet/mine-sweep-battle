using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownControllerOld : MonoBehaviour
{
    private int _seconds;

    public TMP_Text countdownText;

    private void Awake() {
        _seconds = 3;
    }

    void Start()
    {
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown() {
        // For down to 1 second remaining display number of seconds left
        while (_seconds > 0) {
            countdownText.text = _seconds.ToString();
            HUDPresenterOld.Instance.TickAudio.Play();
            yield return new WaitForSeconds(1f);
            _seconds--;
        }

        // At 0 display "GO!" and start the game
        countdownText.fontSize = 24;
        countdownText.text = "GO!";
        HUDPresenterOld.Instance.StartAudio.Play();
        GameControllerOld.Instance.GameActive = true;

        // 1 second after that remove "GO!"
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}
