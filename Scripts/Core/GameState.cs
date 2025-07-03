using UnityEngine;

namespace NineMensMorris.Core
{
    public enum GamePhase
    {
        Placement,  // Taş yerleştirme aşaması
        Movement,   // Taş hareket ettirme aşaması
        Flying      // Uçma aşaması (3 taş kaldığında)
    }
    
    public enum PlayerType
    {
        None,
        Player1,
        Player2
    }
    
    [System.Serializable]
    public class GameState
    {
        public bool IsRemovalPhase { get; set; }
        public bool IsGameEnded { get; set; }
        public Player Winner { get; set; }
        
        public GameState()
        {
            IsRemovalPhase = false;
            IsGameEnded = false;
            Winner = null;
        }
        
        public void Reset()
        {
            IsRemovalPhase = false;
            IsGameEnded = false;
            Winner = null;
        }
    }
} 