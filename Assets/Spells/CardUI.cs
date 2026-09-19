using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI manaCostText;
    public Canvas canvas;

    [Header("Data")]
    public SpellCard spellData;
    private ConnectFour gameLogic;

    private void Awake()
    {
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    private void Start()
    {
        gameLogic = Object.FindFirstObjectByType<ConnectFour>();
        if (spellData != null) Setup(spellData);
    }

    public void Setup(SpellCard data)
    {
        spellData = data;

        if (iconImage != null) iconImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.spellName;
        if (manaCostText != null) manaCostText.text = data.manaCost.ToString();
    }

    public void OnCardClicked()
    {
        print("pressed buttong");
        if (gameLogic != null)
        {
            gameLogic.SelectSpell(spellData, this);
        }
        else
        {
            print("a");
        }
    }
}