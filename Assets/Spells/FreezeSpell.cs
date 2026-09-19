using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(fileName = "Freeze Spell", menuName = "Spells/Freeze")]
public class FreezeSpell : SpellCard
{
    public override void Cast(int boardIndex, ConnectFour gameLogic)
    {
        int column = boardIndex % 7;
        Debug.Log($"Freezing Column {column}!");
        Debug.Log($"{boardIndex}!");

        gameLogic.FreezeColumn(boardIndex, 3);
    }
}