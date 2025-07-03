using UnityEngine;
using System.Collections.Generic;

namespace NineMensMorris.Core
{
    [System.Serializable]
    public class Player : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private string playerName;
        [SerializeField] private Color playerColor = Color.white;
        [SerializeField] private GameObject stonePrefab;
        
        private PlayerType playerType;
        private int stonesInHand;
        private int stonesOnBoard;
        private List<Stone> stones;
        
        public string PlayerName => playerName;
        public Color PlayerColor => playerColor;
        public PlayerType PlayerType => playerType;
        public int StonesInHand => stonesInHand;
        public int StonesOnBoard { get => stonesOnBoard; set => stonesOnBoard = value; }
        public int TotalStones => stonesInHand + stonesOnBoard;
        public List<Stone> Stones => stones;
        
        public event System.Action<Player> OnStonesChanged;
        
        public void Initialize(PlayerType type, int initialStones)
        {
            playerType = type;
            stonesInHand = initialStones;
            stonesOnBoard = 0;
            stones = new List<Stone>();
            
            // Oyuncu adını ayarla
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = type == PlayerType.Player1 ? "Oyuncu 1" : "Oyuncu 2";
            }
        }
        
        public Stone PlaceStone()
        {
            if (stonesInHand <= 0)
                return null;
            
            GameObject stoneObject = Instantiate(stonePrefab);
            Stone stone = stoneObject.GetComponent<Stone>();
            
            if (stone == null)
            {
                stone = stoneObject.AddComponent<Stone>();
            }
            
            stone.Initialize(playerType);
            stones.Add(stone);
            stonesInHand--;
            
            OnStonesChanged?.Invoke(this);
            return stone;
        }
        
        public void RemoveStone(Stone stone)
        {
            if (stones.Contains(stone))
            {
                stones.Remove(stone);
                stonesOnBoard--;
                stone.Remove();
                OnStonesChanged?.Invoke(this);
            }
        }
        
        public bool CanPlaceStone()
        {
            return stonesInHand > 0;
        }
        
        public bool HasStonesOnBoard()
        {
            return stonesOnBoard > 0;
        }
        
        public bool IsEliminated()
        {
            return stonesOnBoard < 3 && stonesInHand == 0;
        }
        
        public void Reset()
        {
            // Mevcut taşları temizle
            foreach (Stone stone in stones)
            {
                if (stone != null)
                {
                    Destroy(stone.gameObject);
                }
            }
            
            stones.Clear();
            stonesInHand = 9;
            stonesOnBoard = 0;
            OnStonesChanged?.Invoke(this);
        }
        
        public List<Stone> GetStonesOnBoard()
        {
            List<Stone> boardStones = new List<Stone>();
            foreach (Stone stone in stones)
            {
                if (stone != null && stone.gameObject.activeInHierarchy)
                {
                    boardStones.Add(stone);
                }
            }
            return boardStones;
        }
        
        public override string ToString()
        {
            return $"{playerName} - El: {stonesInHand}, Tahta: {stonesOnBoard}";
        }
    }
} 