using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("Player Stats")]
    public Slider playerHealthBar;
    public Slider playerManaBar;
    public TMP_Text playerHealthBarText;
    public TMP_Text playerManaBarText;


    [Header("Enemy Stats")]
    public Slider enemyHealthBar;
    public TMP_Text enemyHealthBarText;

    public void UpdateHealth(int currentHP, bool isPlayer)
    {
        if (isPlayer)
        {
            playerHealthBar.value = currentHP;
            playerHealthBarText.text = currentHP.ToString() + "/10";
        }
        else
            enemyHealthBar.value = currentHP;
            enemyHealthBarText.text = currentHP.ToString() + "/10";

    }

    public void UpdateMana(int currentMana)
    {
        playerManaBar.value = currentMana;
        playerManaBarText.text = currentMana.ToString() + "/10";
    }
}