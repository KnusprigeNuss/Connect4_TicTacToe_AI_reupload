using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Multiplayer.Center.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ConnectFour : MonoBehaviour
{
    private bool win = false;
    public TMP_Text[] buttons; 
    public UnityEngine.UI.Image redChip;
    public UnityEngine.UI.Image yellowChip;
    private System.Collections.Generic.List<GameObject> spawnedPieces = new System.Collections.Generic.List<GameObject>();
    public bool playerStarts = true;
    [Header("External Systems")]
    public RobotDialogue robotDialogue;
    public BattleUI battleUI;
    public enum AIDifficulty { Easy, Medium, Hard }
    public AIDifficulty difficulty = AIDifficulty.Hard;
    public int playerHealth = 10;
    public int enemyHealth = 10;
    public int playerMana = 0;
    public int enemyMana = 0;
    private bool gameOver = false;
    private bool AIIsThinking = false;
    private SpellCard selectedSpell;
    private CardUI selectedCardUI;
    public Texture2D spellCursor;
    public GameObject iceColumnPrefab;
    public int[] frozenColumns = new int[7];
    private GameObject[] frozenVisuals = new GameObject[7];

    public Vector3[] startPositions = new Vector3[]
    {
        new Vector3(0.0f, 700.0f, 0.0f),
        new Vector3(0.0f, 100.0f, 0.0f),
        new Vector3(0.0f, 200.0f, 0.0f)
    };

    void Start()
    {
        foreach (var btn in buttons)
        {
            btn.text = "";
            btn.color = Color.black;
        }
        battleUI.playerHealthBar.value = playerHealth;
        battleUI.playerManaBar.value = playerMana;
        battleUI.enemyHealthBar.value = enemyHealth;
        battleUI.UpdateHealth(playerHealth, true);
        battleUI.UpdateHealth(enemyHealth, false);
        battleUI.UpdateMana(playerMana);
        //frozenColumns = new[] {false, false, false, false, false, false, false};
    }

    void Update() {
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    changeDifficulty();
        //}
        if (!gameOver)
        {
            if (playerHealth <= 0 || enemyHealth <= 0)
            {
                gameOver = true;
                robotDialogue.Say(playerHealth <= 0 ? "Biological life terminated." : "System Error... shutdown...");
            }
        }
        if (selectedSpell == null)
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    //public void changeDifficulty()
    //{
    //    if (difficulty == AIDifficulty.Easy)
    //    {
    //        difficulty = AIDifficulty.Medium;
    //        print("Difficulty changed to Medium");
    //    }
    //    else if (difficulty == AIDifficulty.Medium)
    //    {
    //        difficulty = AIDifficulty.Hard;
    //        print("Difficulty changed to Hard");
    //    }
    //    else if (difficulty == AIDifficulty.Hard)
    //    {
    //        difficulty = AIDifficulty.Easy;
    //        print("Difficulty changed to Easy");
    //    }
    //}

    public void SelectSpell(SpellCard spell, CardUI uiElement)
    {
        if (playerMana >= spell.manaCost)
        {
            selectedSpell = spell;
            selectedCardUI = uiElement;
            UnityEngine.Cursor.SetCursor(spellCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnCellClicked(int index)
    {
        if (win || AIIsThinking || gameOver) return;

        if (selectedSpell != null)
        {
            selectedSpell.Cast(index, this);

            playerMana -= selectedSpell.manaCost;
            battleUI.UpdateMana(playerMana);

            //Destroy(selectedCardUI.gameObject);

            selectedSpell = null;
            selectedCardUI = null;

            StartCoroutine(CheckForCombos(true));
            return;
        }
        TMP_Text cell = buttons[index];

        int column = index % 7;
        for (int test = 5; test >= 0; test--)
        {
            int curr_index = test * 7 + column;
            TMP_Text curr_cell = buttons[curr_index];
            if (string.IsNullOrEmpty(curr_cell.text))
            {
                curr_cell.text = "X";
                AIIsThinking = true;
                float globalDropY = 700f;
                float verticalOffset = globalDropY - buttons[curr_index].transform.localPosition.y;
                float fallDuration = (verticalOffset / 2000f);
                SpawnPiece(redChip, curr_cell.transform, curr_index);
                StartCoroutine(WaitAndCheckCombos(fallDuration));
                break;
            }
        }
        
        //StartCoroutine(CheckForCombos());
        //if (win)
        //{
        //    print("winner");
        //}

        //if (!win)
        //{
        //    if (enemyMana < 10)
        //    {
        //        enemyMana += 1;
        //    }
        //    AIIsThinking = true;
        //    StartCoroutine(AIMove());
        //}
    }

    private IEnumerator WaitAndCheckCombos(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(CheckForCombos(true));

        if (!gameOver)
        {
            if (enemyMana < 10) enemyMana += 1;
            battleUI.UpdateMana(playerMana);
            //AIIsThinking = true;
            StartCoroutine(AIMove());
        }
    }

    private IEnumerator CheckForCombos(bool isPlayer)
    {
        int turnComboStep = 0;
        HashSet<int> winningIndices = GetAllWinningIndices();
        bool first = true;
        int first_damage = 0;

        while (winningIndices.Count > 0)
        {
            //turnComboStep++;
            int firstIdx = 0;
            foreach (int i in winningIndices) { firstIdx = i; break; }
            string winnerSymbol = buttons[firstIdx].text;

            int totalDamage;
            if (first)
            {
                totalDamage = (int)Mathf.Pow(2, count_sets_of_four-1) + turnComboStep;
                first_damage = totalDamage;
            } else
            {
                totalDamage = first_damage * 2;
                first_damage = totalDamage;
            }
            first = false;

            if (isPlayer)
            {
                if (winnerSymbol == "X")
                {
                    enemyHealth -= totalDamage;
                    battleUI.UpdateHealth(enemyHealth, false);
                    print("Player Point: Dealt " + totalDamage + " damage to the enemy! Combo: " + turnComboStep);
                    turnComboStep++;
                }
                else
                {
                    playerHealth -= 1;
                    battleUI.UpdateHealth(playerHealth, true);
                    print("Player Point: Dealt " + 1 + " damage to the self -reduced-! Combo: " + turnComboStep);
                }
            } else
            {
                if (winnerSymbol == "O")
                {
                    playerHealth -= totalDamage;
                    battleUI.UpdateHealth(playerHealth, true);
                    print("Enemy Point: Dealt " + totalDamage + " damage to the player! Combo: " + turnComboStep);
                    turnComboStep++;
                }
                else
                {
                    enemyHealth -= 1;
                    battleUI.UpdateHealth(enemyHealth, false);
                    print("Enemy Point: Dealt " + 1 + " damage to the self -reduced-! Combo: " + turnComboStep);

                }
            }



                foreach (int index in winningIndices)
                {
                    buttons[index].text = "";
                    if (buttons[index].transform.childCount > 0)
                    {
                        GameObject pieceToDestroy = buttons[index].transform.GetChild(0).gameObject;

                        if (spawnedPieces.Contains(pieceToDestroy))
                            spawnedPieces.Remove(pieceToDestroy);

                        Destroy(pieceToDestroy);
                    }
                }
            yield return StartCoroutine(CollapseBoard());

            winningIndices = GetAllWinningIndices();
            yield return new WaitForSeconds(1f);
        }
        count_sets_of_four = 0;
    }

    private IEnumerator CollapseBoard()
    {
        int rows = 6;
        int cols = 7;
        float rowHeight = 200f; 

        bool piecesMoved = true;
        while (piecesMoved) 
        {
            piecesMoved = false;

            for (int c = 0; c < cols; c++)
            {
                for (int r = rows - 1; r > 0; r--)
                {
                    int current = r * cols + c;
                    int above = (r - 1) * cols + c;

                    if (string.IsNullOrEmpty(buttons[current].text) && !string.IsNullOrEmpty(buttons[above].text))
                    {
                        buttons[current].text = buttons[above].text;
                        buttons[above].text = "";

                        if (buttons[above].transform.childCount > 0)
                        {
                            Transform pieceTransform = buttons[above].transform.GetChild(0);

                            pieceTransform.SetParent(buttons[current].transform);

                            Vector3 fallStart = new Vector3(0, rowHeight, 0);
                            float fallDuration = 1.0f; 

                            StartCoroutine(SmoothFall(pieceTransform, fallStart, fallDuration));
                        }

                        piecesMoved = true;
                    }
                }
            }

            if (piecesMoved)
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private int count_sets_of_four = 0;
    private HashSet<int> GetAllWinningIndices()
    {
        HashSet<int> winner_indices = new HashSet<int> { };
        int rows = 6;
        int cols = 7;
        string winner = "";
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                TMP_Text curr_cell = buttons[r * cols + c];
                if (string.IsNullOrEmpty(curr_cell.text)) continue;

                if ((c+3) < cols)
                {
                    if (buttons[r*cols + c + 1].text == curr_cell.text && buttons[r * cols + c + 2].text == curr_cell.text && buttons[r * cols + c + 3].text == curr_cell.text)
                    {
                        winner = curr_cell.text;
                        winner_indices.Add(r * cols + c);
                        winner_indices.Add(r * cols + c + 1);
                        winner_indices.Add(r * cols + c + 2);
                        winner_indices.Add(r * cols + c + 3);
                        count_sets_of_four++;
                    }
                }

                if ((r + 3) < rows)
                {
                    if (buttons[(r + 1) * cols + c].text == curr_cell.text && buttons[(r + 2) * cols + c].text == curr_cell.text && buttons[(r + 3) * cols + c].text == curr_cell.text)
                    {
                        winner = curr_cell.text;
                        winner_indices.Add(r * cols + c);
                        winner_indices.Add((r+1) * cols + c);
                        winner_indices.Add((r+2) * cols + c);
                        winner_indices.Add((r+3) * cols + c);
                        count_sets_of_four++;
                    }
                }

                if ((r - 3) >= 0 && (c + 3) < cols)
                {
                    if (buttons[(r - 1) * cols + (c + 1)].text == curr_cell.text && buttons[(r - 2) * cols + (c + 2)].text == curr_cell.text && buttons[(r - 3) * cols + (c + 3)].text == curr_cell.text)
                    {
                        winner = curr_cell.text;
                        winner_indices.Add(r * cols + c);
                        winner_indices.Add((r-1) * cols + (c+1));
                        winner_indices.Add((r-2) * cols + (c+2));
                        winner_indices.Add((r-3) * cols + (c+3));
                        count_sets_of_four++;
                    }
                }

                if ((r - 3) >= 0 && (c - 3) >= 0)
                {
                    if (buttons[(r - 1) * cols + (c - 1)].text == curr_cell.text && buttons[(r - 2) * cols + (c - 2)].text == curr_cell.text && buttons[(r - 3) * cols + (c - 3)].text == curr_cell.text)
                    {
                        winner = curr_cell.text;
                        winner_indices.Add(r * cols + c);
                        winner_indices.Add((r - 1) * cols + (c - 1));
                        winner_indices.Add((r - 2) * cols + (c - 2));
                        winner_indices.Add((r - 3) * cols + (c - 3));
                        count_sets_of_four++;
                    }
                }
            }
        }
        return winner_indices;
    }

    private IEnumerator AIMove()
    {
        if (gameOver) yield break;

        yield return new WaitForSeconds(0.5f);
        int choice = hardAIMove();

        buttons[choice].text = "O";

        float globalDropY = 700f;
        float verticalOffset = globalDropY - buttons[choice].transform.localPosition.y;
        float fallDuration = (verticalOffset / 2000f);

        SpawnPiece(yellowChip, buttons[choice].transform, choice);

        yield return new WaitForSeconds(fallDuration);
        yield return StartCoroutine(CheckForCombos(false));

        processSpells();

        AIIsThinking = false;
        if (playerMana < 10) playerMana += 1;
        battleUI.UpdateMana(playerMana);
    }

    private int hardAIMove()
    {
        int bestScore = int.MinValue;
        int bestCol = 3; 

        for (int col = 0; col < 7; col++)
        {
            if (frozenColumns[col] > 0)
            {
                continue;
            }
            int row = GetLowestEmptyRow(col);
            if (row != -1)
            {
                int index = row * 7 + col;
                buttons[index].text = "O";
                int score = Minimax(0, false, int.MinValue, int.MaxValue, 4);
                buttons[index].text = "";

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCol = col;
                }
            }
        }
        return GetLowestEmptyRow(bestCol) * 7 + bestCol;
    }

    private int GetLowestEmptyRow(int col)
    {
        for (int r = 5; r >= 0; r--)
        {
            if (string.IsNullOrEmpty(buttons[r * 7 + col].text)) return r;
        }
        return -1;
    }

    private int Minimax(int depth, bool isMaximizing, int alpha, int beta, int maxDepth)
    {
        int boardScore = EvaluateBoard();
        if (Mathf.Abs(boardScore) >= 10000 || depth >= maxDepth || IsBoardFull())
        {
            return boardScore;
        }

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            for (int col = 0; col < 7; col++)
            {
                int row = GetLowestEmptyRow(col);
                if (row == -1) continue;

                buttons[row * 7 + col].text = "O";
                int eval = Minimax(depth + 1, false, alpha, beta, maxDepth);
                buttons[row * 7 + col].text = "";
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha) break;
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            for (int col = 0; col < 7; col++)
            {
                int row = GetLowestEmptyRow(col);
                if (row == -1) continue;

                buttons[row * 7 + col].text = "X";
                int eval = Minimax(depth + 1, true, alpha, beta, maxDepth);
                buttons[row * 7 + col].text = "";
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha) break;
            }
            return minEval;
        }
    }

    private int EvaluateBoard()
    {
        int score = 0;
        int rows = 6;
        int cols = 7;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (c + 3 < cols) score += ScoreSection(new int[] { r * 7 + c, r * 7 + c + 1, r * 7 + c + 2, r * 7 + c + 3 });
                if (r + 3 < rows) score += ScoreSection(new int[] { r * 7 + c, (r + 1) * 7 + c, (r + 2) * 7 + c, (r + 3) * 7 + c });
                if (r - 3 >= 0 && c + 3 < cols) score += ScoreSection(new int[] { r * 7 + c, (r - 1) * 7 + c + 1, (r - 2) * 7 + c + 2, (r - 3) * 7 + c + 3 });
                if (r - 3 >= 0 && c - 3 >= 0) score += ScoreSection(new int[] { r * 7 + c, (r - 1) * 7 + c - 1, (r - 2) * 7 + c - 2, (r - 3) * 7 + c - 3 });
            }
        }
        return score;
    }

    private int ScoreSection(int[] indices)
    {
        int aiCount = 0;
        int playerCount = 0;

        foreach (int i in indices)
        {
            if (buttons[i].text == "O") aiCount++;
            else if (buttons[i].text == "X") playerCount++;
        }

        if (aiCount == 4) return 10000;       
        if (aiCount == 3 && playerCount == 0) return 100;  
        if (aiCount == 2 && playerCount == 0) return 10;   
        if (playerCount == 3 && aiCount == 0) return -500; 

        return 0;
    }

    private bool IsBoardFull()
    {
        foreach (var btn in buttons)
        {
            if (string.IsNullOrEmpty(btn.text)) return false;
        }
        return true;
    }

    
    private void SpawnPiece(UnityEngine.UI.Image prefab, Transform parent, int index)
    {
        UnityEngine.UI.Image pieceImage = Instantiate(prefab, parent);
        GameObject piece = pieceImage.gameObject;
        piece.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

        float globalDropY = 700f;
        float verticalOffset = globalDropY - parent.localPosition.y;
        Vector3 fallStart = new Vector3(0, verticalOffset, 0);
        float duration = (verticalOffset / 2000f);

        StartCoroutine(SmoothFall(piece.transform, fallStart, duration));
        spawnedPieces.Add(piece);
    }

    public void ResetBoard()
    {   
        win = false;

        foreach (GameObject piece in spawnedPieces)
        {
            Destroy(piece);
        }
        spawnedPieces.Clear();

        foreach (var btn in buttons)
        {
            btn.text = "";
            btn.color = Color.black;
            //btn.color = new Color(0, 0, 0, 0);
        }
        gameOver = false;
        //playerStarts = !playerStarts;
        //if (!playerStarts)
        //{
        //    AIIsThinking = true;
        //    StartCoroutine(AIMove());
        //}
    }

    private IEnumerator SmoothFall(Transform piece, Vector3 startPos, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            piece.localPosition = Vector3.Lerp(startPos, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        piece.localPosition = Vector3.zero;
    }

    public void FreezeColumn(int index, int duration)
    {
        int col = index % 7;

        if (frozenColumns[col] > 0)
        {
            frozenColumns[col] = duration;
            return;
        }

        GameObject ice = Instantiate(iceColumnPrefab.gameObject, buttons[0].transform.parent.parent.parent);
        RectTransform iceRect = ice.GetComponent<RectTransform>();
        Vector3 buttonPos = buttons[col].transform.parent.localPosition;

        iceRect.localPosition = new Vector3(buttonPos.x, 0.8f, 0);
        ice.transform.localScale = iceRect.localScale;
        
        frozenColumns[col] = duration;
        frozenVisuals[col] = ice;
    }

    public void processSpells()
    {
        for (int i = 0; i < frozenColumns.Length; i++)
        {
            if (frozenColumns[i] > 0)
            {
                frozenColumns[i] -= 1;
                if (frozenColumns[i] <= 0)
                {
                    if (frozenVisuals[i] != null)
                    {
                        Destroy(frozenVisuals[i]);
                        frozenVisuals[i] = null;
                    }
                }
            }
        }
    }
}
