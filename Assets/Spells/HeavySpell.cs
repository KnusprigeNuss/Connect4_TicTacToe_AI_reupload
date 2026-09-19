using UnityEngine;

[CreateAssetMenu(fileName = "Heavy Spell", menuName = "Spells/Heavy")]
public class HeavySpell : SpellCard
{
    public override void Cast(int boardIndex, ConnectFour gameLogic)
    {
        int column = boardIndex % 7;
        Debug.Log($"Freezing Column {column}!");
    }
}