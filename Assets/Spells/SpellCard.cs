using UnityEngine;

public enum SpellType { TargetColumn, TargetRow, TargetCell, Passive }

public abstract class SpellCard : ScriptableObject
{
    public string spellName;
    [TextArea] public string description;
    public Sprite icon;
    public int manaCost;
    public SpellType type;

    public abstract void Cast(int boardIndex, ConnectFour gameLogic);
}