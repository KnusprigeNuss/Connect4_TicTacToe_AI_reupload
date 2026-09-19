using UnityEngine;

[CreateAssetMenu(fileName = "Wind Spell", menuName = "Spells/Wind")]
public class WindSpell : SpellCard
{
    public override void Cast(int boardIndex, ConnectFour gameLogic)
    {
        int column = boardIndex % 7;
        Debug.Log($"Freezing Column {column}!");
        
    }
}