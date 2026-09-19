using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Multiplayer.Center.Common;
using System.Collections;
using UnityEngine.UIElements;

public class Gamelogic : MonoBehaviour
{
    private bool win = false;
    public TMP_Text[] buttons;
    public UnityEngine.UI.Image[] points;
    public GameObject crossPrefab;
    public GameObject circlePrefab;
    private System.Collections.Generic.List<GameObject> spawnedPieces = new System.Collections.Generic.List<GameObject>();
    public bool playerStarts = true;
    [Header("External Systems")]
    public RobotDialogue robotDialogue;
    public enum AIDifficulty { Easy, Medium, Hard }
    public AIDifficulty difficulty = AIDifficulty.Easy;

    private readonly int[][] winCombos = new int[][]
    {
        new[] {0, 1, 2}, 
        new[] {3, 4, 5},
        new[] {6, 7, 8},
        new[] {0, 3, 6}, 
        new[] {1, 4, 7},
        new[] {2, 5, 8},
        new[] {0, 4, 8}, 
        new[] {2, 4, 6}
    };

    public TMP_Text endText = null;
    private bool AIIsThinking = false;
    private int pointsPlayer = 0;
    private int pointsOpponent = 0;
    private bool gameOver = false;

    void Start() {
        foreach (var btn in buttons)
        {
            btn.text = "";
            btn.color = Color.black;
            btn.color = new Color(0, 0, 0, 0); 
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E))
        {
            changeDifficulty();
        }
        if (pointsPlayer == 3 && !gameOver)
        {
            print("Player Won!!");
            gameOver = true;
        }
        else if (pointsOpponent == 3 && !gameOver)
        {
            print("Opponent Won!!");
            gameOver = true;
        }
    }

    public void changeDifficulty()
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

    public void OnCellClicked(int index)
    {
        if (win || AIIsThinking) return;
        TMP_Text cell = buttons[index];

        if (!string.IsNullOrEmpty(cell.text)) return; 

        cell.text = "X";
        SpawnPiece(crossPrefab, buttons[index].transform); 

        CheckWin();

        if (!win)
        {
            AIIsThinking = true;
            StartCoroutine(AIMove());
        }
    }

    private void CheckWin()
    {
        foreach (var combo in winCombos)
        {
            string a = buttons[combo[0]].text;
            string b = buttons[combo[1]].text;
            string c = buttons[combo[2]].text;

            if (a != "" && a == b && b == c)
            {
                win = true;
                endText.text = $"The winner is Player {a}";
                if (a == "X")
                {
                    robotDialogue.Say("I... lost?!");
                } else
                {
                    robotDialogue.Say("Hahaaa!");
                }
                    givePoint(a);
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

    private void givePoint(string winner)
    {
        if (winner == "X")
        {
            points[pointsPlayer].color = Color.red;
            pointsPlayer += 1;
        }
        else if (winner == "O")
        {
            points[3 + pointsOpponent].color = Color.red;
            pointsOpponent += 1;
        }
    }

    private IEnumerator AIMove()
    {
        if (win) yield break;


        yield return new WaitForSeconds(0.5f);
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
        yield return new WaitForSeconds(0.5f);

        CheckWin();
        AIIsThinking = false;
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
            for (int i = 0; i < 3; i++)
            {
                if (buttons[combo[i]].text == symbol) count++;
                else if (string.IsNullOrEmpty(buttons[combo[i]].text)) emptyIndex = combo[i];
            }
            if (count == 2 && emptyIndex != -1) return emptyIndex;
        }
        return -1;
    }
    private int positionsChecked = 0;
    private int hardAIMove()
    {
        positionsChecked = 0;
        int bestScore = int.MinValue;
        int move = -1;

        for (int i = 0; i < 9; i++)
        {
            if (string.IsNullOrEmpty(buttons[i].text))
            {
                buttons[i].text = "O";
                int score = Minimax(0, false, int.MinValue, int.MaxValue);
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

    private int Minimax(int depth, bool isMaximizing, int alpha, int beta)
    {
        positionsChecked++;
        string result = EvaluateBoard();
        if (result == "O") return 10 - depth;
        if (result == "X") return depth - 10;
        if (IsBoardFull()) return 0;

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            for (int i = 0; i < 9; i++)
            {
                if (string.IsNullOrEmpty(buttons[i].text))
                {
                    buttons[i].text = "O";
                    int eval = Minimax(depth + 1, false, alpha, beta);
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
            for (int i = 0; i < 9; i++)
            {
                if (string.IsNullOrEmpty(buttons[i].text))
                {
                    buttons[i].text = "X";
                    int eval = Minimax(depth + 1, true, alpha, beta);
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

            if (a != "" && a == b && b == c)
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

    public void ResetBoard()
    {
        if (gameOver) 
        {
            gameOver = false;
            pointsOpponent = 0;
            pointsPlayer = 0;
            playerStarts = false;
            foreach (var image in points)
            {
                image.color = Color.white;
            }
            //changeDifficulty();
        }
        
        win = false;
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
        playerStarts = !playerStarts;
        if (!playerStarts)
        {
            AIIsThinking = true;
            StartCoroutine(AIMove());
        }
    }
    
}
