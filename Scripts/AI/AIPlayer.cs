using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NineMensMorris.AI
{
    public enum AIDifficulty
    {
        Easy,
        Medium,
        Hard
    }
    
    public class AIPlayer : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private AIDifficulty difficulty = AIDifficulty.Medium;
        [SerializeField] private float thinkingTime = 1f;
        [SerializeField] private bool enableDebugLog = false;
        
        private Core.Player aiPlayer;
        private Core.Board gameBoard;
        private Core.GameManager gameManager;
        private System.Random random;
        
        public bool IsThinking { get; private set; }
        
        private void Awake()
        {
            random = new System.Random();
        }
        
        public void Initialize(Core.Player player, Core.Board board, Core.GameManager manager)
        {
            aiPlayer = player;
            gameBoard = board;
            gameManager = manager;
        }
        
        public void MakeMove(Core.GamePhase currentPhase)
        {
            if (IsThinking)
                return;
            
            StartCoroutine(ThinkAndMove(currentPhase));
        }
        
        private System.Collections.IEnumerator ThinkAndMove(Core.GamePhase currentPhase)
        {
            IsThinking = true;
            
            yield return new WaitForSeconds(thinkingTime);
            
            int movePosition = -1;
            
            switch (currentPhase)
            {
                case Core.GamePhase.Placement:
                    movePosition = GetBestPlacementMove();
                    break;
                case Core.GamePhase.Movement:
                    ExecuteMovementMove();
                    break;
                case Core.GamePhase.Flying:
                    ExecuteFlyingMove();
                    break;
            }
            
            if (movePosition >= 0)
            {
                gameManager.HandlePositionClicked(movePosition);
            }
            
            IsThinking = false;
        }
        
        private int GetBestPlacementMove()
        {
            List<int> availablePositions = gameBoard.GetAvailablePositions();
            
            if (availablePositions.Count == 0)
                return -1;
            
            switch (difficulty)
            {
                case AIDifficulty.Easy:
                    return GetRandomMove(availablePositions);
                
                case AIDifficulty.Medium:
                    return GetMediumPlacementMove(availablePositions);
                
                case AIDifficulty.Hard:
                    return GetHardPlacementMove(availablePositions);
                
                default:
                    return GetRandomMove(availablePositions);
            }
        }
        
        private int GetRandomMove(List<int> positions)
        {
            return positions[random.Next(positions.Count)];
        }
        
        private int GetMediumPlacementMove(List<int> availablePositions)
        {
            // 1. Mill oluşturabilecek pozisyonu bul
            foreach (int pos in availablePositions)
            {
                if (WouldCreateMill(pos, aiPlayer.PlayerType))
                {
                    LogDebug($"AI Mill oluşturuyor: {pos}");
                    return pos;
                }
            }
            
            // 2. Rakibin mill oluşturmasını engelle
            Core.PlayerType opponentType = GetOpponentType();
            foreach (int pos in availablePositions)
            {
                if (WouldCreateMill(pos, opponentType))
                {
                    LogDebug($"AI Rakibin millini engelliyor: {pos}");
                    return pos;
                }
            }
            
            // 3. Rastgele seç
            return GetRandomMove(availablePositions);
        }
        
        private int GetHardPlacementMove(List<int> availablePositions)
        {
            // Hard AI için daha gelişmiş strateji
            
            // 1. Mill oluştur
            foreach (int pos in availablePositions)
            {
                if (WouldCreateMill(pos, aiPlayer.PlayerType))
                {
                    return pos;
                }
            }
            
            // 2. Rakibin millini engelle
            Core.PlayerType opponentType = GetOpponentType();
            foreach (int pos in availablePositions)
            {
                if (WouldCreateMill(pos, opponentType))
                {
                    return pos;
                }
            }
            
            // 3. Stratejik pozisyonları tercih et (köşeler ve merkezler)
            List<int> strategicPositions = GetStrategicPositions(availablePositions);
            if (strategicPositions.Count > 0)
            {
                return GetRandomMove(strategicPositions);
            }
            
            // 4. En fazla bağlantısı olan pozisyonu seç
            int bestPos = availablePositions[0];
            int maxConnections = GetConnectionCount(bestPos);
            
            foreach (int pos in availablePositions)
            {
                int connections = GetConnectionCount(pos);
                if (connections > maxConnections)
                {
                    maxConnections = connections;
                    bestPos = pos;
                }
            }
            
            return bestPos;
        }
        
        private void ExecuteMovementMove()
        {
            List<AIMove> possibleMoves = GetAllPossibleMoves();
            
            if (possibleMoves.Count == 0)
                return;
            
            AIMove bestMove = null;
            
            switch (difficulty)
            {
                case AIDifficulty.Easy:
                    bestMove = possibleMoves[random.Next(possibleMoves.Count)];
                    break;
                    
                case AIDifficulty.Medium:
                case AIDifficulty.Hard:
                    bestMove = GetBestMove(possibleMoves);
                    break;
            }
            
            if (bestMove != null)
            {
                gameManager.HandlePositionClicked(bestMove.fromPosition);
                StartCoroutine(DelayedMove(bestMove.toPosition));
            }
        }
        
        private void ExecuteFlyingMove()
        {
            List<int> aiStones = GetAIStonePositions();
            List<int> emptyPositions = gameBoard.GetAvailablePositions();
            
            if (aiStones.Count == 0 || emptyPositions.Count == 0)
                return;
            
            // En iyi uçma hamlesi
            int bestFrom = -1;
            int bestTo = -1;
            
            foreach (int from in aiStones)
            {
                foreach (int to in emptyPositions)
                {
                    if (WouldCreateMill(to, aiPlayer.PlayerType))
                    {
                        bestFrom = from;
                        bestTo = to;
                        break;
                    }
                }
                if (bestFrom >= 0) break;
            }
            
            // Mill oluşturacak hamle bulunamadıysa rastgele seç
            if (bestFrom < 0)
            {
                bestFrom = aiStones[random.Next(aiStones.Count)];
                bestTo = emptyPositions[random.Next(emptyPositions.Count)];
            }
            
            gameManager.HandlePositionClicked(bestFrom);
            StartCoroutine(DelayedMove(bestTo));
        }
        
        private System.Collections.IEnumerator DelayedMove(int position)
        {
            yield return new WaitForSeconds(0.5f);
            gameManager.HandlePositionClicked(position);
        }
        
        private bool WouldCreateMill(int position, Core.PlayerType playerType)
        {
            // Geçici olarak taşı yerleştir ve mill kontrolü yap
            // Bu sadece simülasyon, gerçek tahtayı değiştirmez
            return gameBoard.CheckForMill(position, playerType);
        }
        
        private Core.PlayerType GetOpponentType()
        {
            return aiPlayer.PlayerType == Core.PlayerType.Player1 ? 
                   Core.PlayerType.Player2 : Core.PlayerType.Player1;
        }
        
        private List<int> GetStrategicPositions(List<int> availablePositions)
        {
            // Köşeler ve önemli pozisyonlar
            int[] strategicIndices = { 0, 2, 6, 8, 9, 11, 18, 20 };
            return availablePositions.Where(pos => strategicIndices.Contains(pos)).ToList();
        }
        
        private int GetConnectionCount(int position)
        {
            // Bu pozisyonun kaç bağlantısı olduğunu hesapla
            // Board sınıfının ADJACENCY_MAP'ini kullanmak gerekir
            // Şimdilik basit bir implementasyon
            return random.Next(2, 5);
        }
        
        private List<AIMove> GetAllPossibleMoves()
        {
            List<AIMove> moves = new List<AIMove>();
            List<int> aiStones = GetAIStonePositions();
            
            foreach (int from in aiStones)
            {
                List<int> validMoves = GetValidMovesFromPosition(from);
                foreach (int to in validMoves)
                {
                    moves.Add(new AIMove { fromPosition = from, toPosition = to });
                }
            }
            
            return moves;
        }
        
        private List<int> GetAIStonePositions()
        {
            List<int> positions = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (gameBoard.GetStoneOwner(i) == aiPlayer.PlayerType)
                {
                    positions.Add(i);
                }
            }
            return positions;
        }
        
        private List<int> GetValidMovesFromPosition(int fromPosition)
        {
            List<int> validMoves = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (gameBoard.CanMoveStone(fromPosition, i))
                {
                    validMoves.Add(i);
                }
            }
            return validMoves;
        }
        
        private AIMove GetBestMove(List<AIMove> possibleMoves)
        {
            // Mill oluşturacak hamle
            foreach (var move in possibleMoves)
            {
                if (WouldCreateMill(move.toPosition, aiPlayer.PlayerType))
                {
                    return move;
                }
            }
            
            // Rakibin millini engelleyecek hamle
            Core.PlayerType opponentType = GetOpponentType();
            foreach (var move in possibleMoves)
            {
                if (WouldCreateMill(move.toPosition, opponentType))
                {
                    return move;
                }
            }
            
            // Rastgele hamle
            return possibleMoves[random.Next(possibleMoves.Count)];
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log($"[AI] {message}");
            }
        }
        
        private struct AIMove
        {
            public int fromPosition;
            public int toPosition;
        }
    }
} 