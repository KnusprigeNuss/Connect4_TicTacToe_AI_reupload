# Board Game AI & RPG Battle System

A Unity-based collection of strategic board games (Tic-Tac-Toe, Connect Four) and an adaptive AI opponent. 

## Key Features & AI Algorithms
* **Minimax with Alpha-Beta Pruning:** The hard-difficulty AI evaluates future board states using a recursive Minimax algorithm, optimized with alpha-beta pruning to efficiently discard irrelevant move branches.
* **Tiered AI Logic:** Opponent decision-making scales across Easy (random empty cells), Medium (identifying immediate win/block opportunities) and Hard (deep move prediction).
* **Dynamic Board Mechanics & Combos:** The Connect Four system recursively identifies multi-directional win states and dynamically collapses floating pieces using gravity calculations.
* **Procedural Animation:** Employs mathematical sine wave functions to create smooth, code-driven vertical hovering and horizontal wobbling effects for the robot opponent.

## Built With
* Unity 
* C# (.NET)

Employs ScriptableObjects, IEnumerator Sequences
The Main AI Scripts can be found in Assets/Scripts!