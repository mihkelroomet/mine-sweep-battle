using UnityEngine;

public class PlayerCustomizer : MonoBehaviour
{
    public static PlayerCustomizer Instance;

    private string _playerName;
    private int _hatColor;
    private int _shirtColor;
    private int _pantsColor;

    public int MaxPlayerNameLength;

    private Color32[] _hatColors;
    private Color32[] _shirtColors;
    private Color32[] _pantsColors;

    public byte DarkenSleevesBy;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        Events.OnGetPlayerName += GetPlayerName;
        Events.OnSetPlayerName += SetPlayerName;
        Events.OnGetHatColor += GetHatColor;
        Events.OnSetHatColor += SetHatColor;
        Events.OnGetShirtColor += GetShirtColor;
        Events.OnSetShirtColor += SetShirtColor;
        Events.OnGetPantsColor += GetPantsColor;
        Events.OnSetPantsColor += SetPantsColor;

        _hatColors = new Color32[] {
            new Color32(239, 17, 17, 255), new Color32(29, 60, 233, 255), new Color32(27, 145, 62, 255),
            new Color32(255, 99, 212, 255), new Color32(255, 141, 28, 255), new Color32(255, 255, 103, 255),
            new Color32(74, 86, 94, 255), new Color32(233, 247, 255, 255), new Color32(120, 61, 210, 255),
            new Color32(128, 88, 45, 255), new Color32(68, 255, 247, 255), new Color32(91, 254, 75, 255),
            new Color32(108, 43, 61, 255), new Color32(255, 214, 236, 255), new Color32(255, 255, 190, 255),
            new Color32(131, 151, 167, 255), new Color32(159, 153, 137, 255), new Color32(236, 117, 120, 255),
            new Color32(38, 166, 98, 255), new Color32(97, 114, 24, 255), Color.clear
            };

        _shirtColors = new Color32[] {
            new Color32(239, 17, 17, 255), new Color32(29, 60, 233, 255), new Color32(27, 145, 62, 255),
            new Color32(255, 99, 212, 255), new Color32(255, 141, 28, 255), new Color32(255, 255, 103, 255),
            new Color32(74, 86, 94, 255), new Color32(233, 247, 255, 255), new Color32(120, 61, 210, 255),
            new Color32(128, 88, 45, 255), new Color32(68, 255, 247, 255), new Color32(91, 254, 75, 255),
            new Color32(108, 43, 61, 255), new Color32(255, 214, 236, 255), new Color32(255, 255, 190, 255),
            new Color32(131, 151, 167, 255), new Color32(159, 153, 137, 255), new Color32(236, 117, 120, 255),
            new Color32(38, 166, 98, 255), new Color32(97, 114, 24, 255), Color.clear
            };

        _pantsColors = new Color32[]{new Color32(26, 72, 95, 255), new Color32(106, 77, 23, 255), new Color32(91, 82, 63, 255), new Color32(168, 161, 118, 255),
        Color.black, new Color32(183, 183, 183, 255), new Color32(90, 60, 60, 255), new Color32(60, 90, 60, 255), new Color32(60, 60, 90, 255)};
    }

    private string GetPlayerName()
    {
        return _playerName;
    }

    private void SetPlayerName(string value)
    {
        if (value.Length > MaxPlayerNameLength) _playerName = value.Substring(0, MaxPlayerNameLength);
        else _playerName = value;
    }

    private int GetHatColor()
    {
        return _hatColor;
    }

    private void SetHatColor(int value)
    {
        _hatColor = value;
    }

    private int GetShirtColor()
    {
        return _shirtColor;
    }

    private void SetShirtColor(int value)
    {
        _shirtColor = value;
    }

    private int GetPantsColor()
    {
        return _pantsColor;
    }

    private void SetPantsColor(int value)
    {
        _pantsColor = value;
    }

    public int GetNextHat()
    {
        if (_hatColor >= _hatColors.Length - 1) return 0;
        else return _hatColor + 1;
    }

    public int GetPrevHat()
    {
        if (_hatColor <= 0) return _hatColors.Length - 1;
        else return _hatColor - 1;
    }

    public int GetNextShirt()
    {
        if (_shirtColor >= _shirtColors.Length - 1) return 0;
        else return _shirtColor + 1;
    }

    public int GetPrevShirt()
    {
        if (_shirtColor <= 0) return _shirtColors.Length - 1;
        else return _shirtColor - 1;
    }

    public int GetNextPants()
    {
        if (_pantsColor >= _pantsColors.Length - 1) return 0;
        else return _pantsColor + 1;
    }

    public int GetPrevPants()
    {
        if (_pantsColor <= 0) return _pantsColors.Length - 1;
        else return _pantsColor - 1;
    }

    public Color32 TranslateHatColor(int value)
    {
        return _hatColors[value];
    }

    public Color32 TranslateShirtColor(int value)
    {
        return _shirtColors[value];
    }

    public Color32 TranslateShirtColorIntoSleeveColor(int value)
    {
        Color32 shirtColor = _shirtColors[value];
        return new Color32(
            (byte) Mathf.Max(shirtColor.r - DarkenSleevesBy, 0),
            (byte) Mathf.Max(shirtColor.g - DarkenSleevesBy, 0),
            (byte) Mathf.Max(shirtColor.b - DarkenSleevesBy, 0),
            (byte) shirtColor.a
            );
    }

    public Color32 TranslatePantsColor(int value)
    {
        return _pantsColors[value];
    }

    private void OnDestroy()
    {
        Events.OnGetPlayerName -= GetPlayerName;
        Events.OnSetPlayerName -= SetPlayerName;
        Events.OnGetHatColor -= GetHatColor;
        Events.OnSetHatColor -= SetHatColor;
        Events.OnGetShirtColor -= GetShirtColor;
        Events.OnSetShirtColor -= SetShirtColor;
        Events.OnGetPantsColor -= GetPantsColor;
        Events.OnSetPantsColor -= SetPantsColor;
    }
}
