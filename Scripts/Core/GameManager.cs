using UnityEngine;
using System.Collections.Generic;

namespace NineMensMorris.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private Board board;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private InputManager inputManager;
        
        [Header("Player Settings")]
        [SerializeField] private Player player1;
        [SerializeField] private Player player2;
        
        private GameState currentGameState;
        private Player currentPlayer;
        private GamePhase currentPhase;
        private int selectedPositionIndex = -1;
        
        public static GameManager Instance { get; private set; }
        
        public event System.Action<Player> OnPlayerChanged;
        public event System.Action<GamePhase> OnPhaseChanged;
        public event System.Action<Player> OnGameEnded;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            currentGameState = new GameState();
            currentPhase = GamePhase.Placement;
            currentPlayer = player1;
            
            player1.Initialize(PlayerType.Player1, 9);
            player2.Initialize(PlayerType.Player2, 9);
            
            board.Initialize();
            uiManager.Initialize();
            inputManager.Initialize();
            
            inputManager.OnPositionClicked += HandlePositionClicked;
            
            UpdateUI();
        }
        
        public void HandlePositionClicked(int positionIndex)
        {
            switch (currentPhase)
            {
                case GamePhase.Placement:
                    HandlePlacementPhase(positionIndex);
                    break;
                case GamePhase.Movement:
                    HandleMovementPhase(positionIndex);
                    break;
                case GamePhase.Flying:
                    HandleFlyingPhase(positionIndex);
                    break;
            }
        }
        
        private void HandlePlacementPhase(int positionIndex)
        {
            if (!board.IsPositionEmpty(positionIndex))
                return;
            
            if (PlaceStone(positionIndex))
            {
                if (CheckForMill(positionIndex))
                {
                    StartMillRemoval();
                    return;
                }
                
                if (IsPlacementPhaseComplete())
                {
                    ChangePhase(GamePhase.Movement);
                }
                
                SwitchPlayer();
            }
        }
        
        private void HandleMovementPhase(int positionIndex)
        {
            if (selectedPositionIndex == -1)
            {
                // Taş seçme
                if (board.GetStoneOwner(positionIndex) == currentPlayer.PlayerType)
                {
                    selectedPositionIndex = positionIndex;
                    board.HighlightPosition(positionIndex, true);
                    board.HighlightValidMoves(positionIndex);
                }
            }
            else
            {
                // Taş hareket ettirme
                if (board.CanMoveStone(selectedPositionIndex, positionIndex))
                {
                    MoveStone(selectedPositionIndex, positionIndex);
                    
                    if (CheckForMill(positionIndex))
                    {
                        StartMillRemoval();
                        return;
                    }
                    
                    if (currentPlayer.StonesOnBoard <= 3)
                    {
                        ChangePhase(GamePhase.Flying);
                    }
                    
                    SwitchPlayer();
                }
                
                ClearSelection();
            }
        }
        
        private void HandleFlyingPhase(int positionIndex)
        {
            if (selectedPositionIndex == -1)
            {
                // Taş seçme
                if (board.GetStoneOwner(positionIndex) == currentPlayer.PlayerType)
                {
                    selectedPositionIndex = positionIndex;
                    board.HighlightPosition(positionIndex, true);
                }
            }
            else
            {
                // Taş uçurma
                if (board.IsPositionEmpty(positionIndex))
                {
                    MoveStone(selectedPositionIndex, positionIndex);
                    
                    if (CheckForMill(positionIndex))
                    {
                        StartMillRemoval();
                        return;
                    }
                    
                    SwitchPlayer();
                }
                
                ClearSelection();
            }
        }
        
        private bool PlaceStone(int positionIndex)
        {
            if (currentPlayer.StonesInHand <= 0)
                return false;
            
            Stone stone = currentPlayer.PlaceStone();
            board.PlaceStone(positionIndex, stone);
            currentPlayer.StonesOnBoard++;
            
            return true;
        }
        
        private void MoveStone(int fromIndex, int toIndex)
        {
            board.MoveStone(fromIndex, toIndex);
        }
        
        private bool CheckForMill(int positionIndex)
        {
            return board.CheckForMill(positionIndex, currentPlayer.PlayerType);
        }
        
        private void StartMillRemoval()
        {
            currentGameState.IsRemovalPhase = true;
            uiManager.ShowRemovalMessage(currentPlayer);
        }
        
        public void RemoveStone(int positionIndex)
        {
            if (!currentGameState.IsRemovalPhase)
                return;
            
            Player opponent = GetOpponent();
            if (board.GetStoneOwner(positionIndex) != opponent.PlayerType)
                return;
            
            if (board.IsStoneInMill(positionIndex) && board.HasMovableStones(opponent.PlayerType))
                return;
            
            board.RemoveStone(positionIndex);
            opponent.StonesOnBoard--;
            
            currentGameState.IsRemovalPhase = false;
            
            if (CheckWinCondition())
                return;
            
            SwitchPlayer();
        }
        
        private bool IsPlacementPhaseComplete()
        {
            return player1.StonesInHand == 0 && player2.StonesInHand == 0;
        }
        
        private void SwitchPlayer()
        {
            currentPlayer = (currentPlayer == player1) ? player2 : player1;
            OnPlayerChanged?.Invoke(currentPlayer);
            UpdateUI();
        }
        
        private void ChangePhase(GamePhase newPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(currentPhase);
            UpdateUI();
        }
        
        private Player GetOpponent()
        {
            return (currentPlayer == player1) ? player2 : player1;
        }
        
        private bool CheckWinCondition()
        {
            Player opponent = GetOpponent();
            
            // Rakip 3 taştan az taşa sahipse
            if (opponent.StonesOnBoard < 3)
            {
                EndGame(currentPlayer);
                return true;
            }
            
            // Rakip hareket edemiyorsa
            if (currentPhase != GamePhase.Placement && !board.HasValidMoves(opponent.PlayerType))
            {
                EndGame(currentPlayer);
                return true;
            }
            
            return false;
        }
        
        private void EndGame(Player winner)
        {
            OnGameEnded?.Invoke(winner);
            uiManager.ShowGameEnd(winner);
        }
        
        private void ClearSelection()
        {
            if (selectedPositionIndex != -1)
            {
                board.HighlightPosition(selectedPositionIndex, false);
                board.ClearHighlights();
                selectedPositionIndex = -1;
            }
        }
        
        private void UpdateUI()
        {
            uiManager.UpdateGameInfo(currentPlayer, currentPhase, player1, player2);
        }
        
        public void RestartGame()
        {
            board.ClearBoard();
            InitializeGame();
        }
    }
} 