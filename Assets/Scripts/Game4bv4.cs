using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Multiplayer.Center.Common;

public class Game4by4 : MonoBehaviour
{
    //private bool isPlayerOTurn = false;
    private bool win = false;
    public TMP_Text[] buttons;
    public GameObject crossPrefab;
    public GameObject circlePrefab;
    private System.Collections.Generic.List<GameObject> spawnedPieces = new System.Collections.Generic.List<GameObject>();
    public enum AIDifficulty { Easy, Medium, Hard }
    public AIDifficulty difficulty = AIDifficulty.Easy;

    private readonly int[][] winCombos = new int[][]
    {
        new[] {0, 1, 2, 3},
        new[] {4, 5, 6, 7},
        new[] {8, 9, 10, 11},
        new[] {12, 13, 14, 15},

        new[] {0, 4, 8, 12},
        new[] {1, 5, 9, 13},
        new[] {2, 6, 10, 14},
        new[] {3, 7, 11, 15},

        new[] {0, 5, 10, 15},
        new[] {3, 6, 9, 12}
    };

    public TMP_Text endText = null;

    private string[] board = new string[16];

    void Start()
    {
        ResetBoard();
        foreach (var btn in buttons)
        {
            btn.text = "";
            btn.color = Color.black;
            btn.color = new Color(0, 0, 0, 0);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (difficulty == AIDifficulty.Easy)
            {
                difficulty = AIDifficulty.Medium;
                print("Difficulty changed to Medium");
            }
            else if (difficulty == AIDifficulty.Medium)
            {
                difficulty = AIDifficulty.Hard;
                print("Difficulty changed to Hard");
            }
            else if (difficulty == AIDifficulty.Hard)
            {
                difficulty = AIDifficulty.Easy;
                print("Difficulty changed to Easy");
            }
        }
    }

    public void OnCellClicked(int index)
    {
        if (win) return;
        TMP_Text cell = buttons[index];

        if (!string.IsNullOrEmpty(cell.text)) return;

        cell.text = "X";
        SpawnPiece(crossPrefab, buttons[index].transform);

        CheckWin();

        if (!win)
        {
            AIMove();
        }
    }

    private void CheckWin()
    {
        foreach (var combo in winCombos)
        {
            string a = buttons[combo[0]].text;
            string b = buttons[combo[1]].text;
            string c = buttons[combo[2]].text;
            string d = buttons[combo[3]].text;

            if (a != "" && a == b && b == c && c == d)
            {
                win = true;
                endText.text = $"The winner is Player {a}";
                return;
            }
        }

        bool allFilled = true;
        foreach (var cell in buttons)
        {
            if (string.IsNullOrEmpty(cell.text))
            {
                allFilled = false;
                break;
            }
        }

        if (allFilled && !win)
        {
            win = true;
            endText.text = "It's a draw!";
        }
    }

    private void AIMove()
    {
        if (win) return;

        int choice;
        switch (difficulty)
        {
            case AIDifficulty.Medium:
                choice = mediumAIMove();
                break;
            case AIDifficulty.Hard:
                choice = hardAIMove();
                break;
            default:
                choice = easyAIMove();
                break;
        }

        buttons[choice].text = "O";
        SpawnPiece(circlePrefab, buttons[choice].transform);

        CheckWin();
    }

    private int easyAIMove()
    {
        var emptyCells = new System.Collections.Generic.List<int>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (string.IsNullOrEmpty(buttons[i].text))
                emptyCells.Add(i);
        }
        if (emptyCells.Count == 0) return -1;

        int choice = emptyCells[Random.Range(0, emptyCells.Count)];

        return choice;
    }

    private int mediumAIMove()
    {
        int winMove = FindWinningMove("O");
        if (winMove != -1)
        {
            return winMove;
        }

        int blockMove = FindWinningMove("X");
        if (blockMove != -1)
        {
            return blockMove;
        }

        return easyAIMove();
    }

    private int FindWinningMove(string symbol)
    {
        foreach (var combo in winCombos)
        {
            int count = 0;
            int emptyIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (buttons[combo[i]].text == symbol) count++;
                else if (string.IsNullOrEmpty(buttons[combo[i]].text)) emptyIndex = combo[i];
            }
            if (count == 3 && emptyIndex != -1) return emptyIndex;
        }
        return -1;
    }
    private int positionsChecked = 0;
    private int MAX_DEPTH = 5;
    private int hardAIMove()
    {
        positionsChecked = 0;
        int bestScore = int.MinValue;
        int move = -1;

        for (int i = 0; i < 16; i++)
        {
            if (string.IsNullOrEmpty(buttons[i].text))
            {
                buttons[i].text = "O";
                int score = Minimax(0, false, int.MinValue, int.MaxValue, MAX_DEPTH);
                buttons[i].text = "";
                if (score > bestScore)
                {
                    bestScore = score;
                    move = i;
                }
            }
        }
        Debug.Log($"<color=cyan>AI Decision:</color> Checked <b>{positionsChecked}</b> possible future positions to pick square {move}.");
        return move;
    }

    private int Minimax(int depth, bool isMaximizing, int alpha, int beta, int maxDepth)
    {
        positionsChecked++;
        string result = EvaluateBoard();
        if (result == "O") return 17 - depth;
        if (result == "X") return depth - 17;
        if (IsBoardFull() || depth >= maxDepth) return 0;

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            for (int i = 0; i < 16; i++)
            {
                if (string.IsNullOrEmpty(buttons[i].text))
                {
                    buttons[i].text = "O";
                    int eval = Minimax(depth + 1, false, alpha, beta, maxDepth);
                    buttons[i].text = "";
                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            for (int i = 0; i < 16; i++)
            {
                if (string.IsNullOrEmpty(buttons[i].text))
                {
                    buttons[i].text = "X";
                    int eval = Minimax(depth + 1, true, alpha, beta, maxDepth);
                    buttons[i].text = "";
                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);
                    if (beta <= alpha) break;
                }
            }
            return minEval;
        }
    }

    private string EvaluateBoard()
    {
        foreach (var combo in winCombos)
        {
            string a = buttons[combo[0]].text;
            string b = buttons[combo[1]].text;
            string c = buttons[combo[2]].text;
            string d = buttons[combo[3]].text;

            if (a != "" && a == b && b == c && c == d)
            {
                return a; 
            }
        }
        return "";
    }

    private bool IsBoardFull()
    {
        foreach (var btn in buttons)
        {
            if (string.IsNullOrEmpty(btn.text)) return false;
        }
        return true;
    }

    private void SpawnPiece(GameObject prefab, Transform parent)
    {
        GameObject piece = Instantiate(prefab, parent);
        piece.transform.localPosition = Vector3.zero;
        spawnedPieces.Add(piece);
    }

    void HighlightWin(int[] combo, Color col)
    {
        foreach (int index in combo)
            buttons[index].color = col;
    }

    public void ResetBoard()
    {
        win = false;
        //isPlayerOTurn = false;
        endText.text = "";

        foreach (GameObject piece in spawnedPieces)
        {
            Destroy(piece);
        }
        spawnedPieces.Clear();

        foreach (var btn in buttons)
        {
            btn.text = "";
            btn.color = Color.black;
            btn.color = new Color(0, 0, 0, 0); 
        }
    }

}
