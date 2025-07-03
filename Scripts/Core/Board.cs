using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NineMensMorris.Core
{
    public class Board : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private Transform[] positions;
        [SerializeField] private LineRenderer[] connectionLines;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material validMoveMaterial;
        
        // 24 pozisyonlu 9 taş oyunu tahtası
        // Pozisyonlar 0-23 arası numaralandırılmış
        private Stone[] boardStones;
        private bool[] highlightedPositions;
        private static readonly int[][] MILL_PATTERNS = new int[][]
        {
            // Dış kare yatay
            new int[] {0, 1, 2}, new int[] {6, 7, 8},
            // Dış kare dikey
            new int[] {0, 9, 21}, new int[] {2, 4, 7}, new int[] {8, 12, 17}, new int[] {21, 19, 6},
            
            // Orta kare yatay
            new int[] {3, 4, 5}, new int[] {10, 11, 12},
            // Orta kare dikey
            new int[] {3, 10, 18}, new int[] {5, 13, 20}, new int[] {12, 17, 22}, new int[] {18, 19, 20},
            
            // İç kare yatay
            new int[] {13, 14, 15}, new int[] {16, 17, 18},
            // İç kare dikey
            new int[] {13, 16, 19}, new int[] {15, 17, 20}
        };
        
        // Komşuluk matrisi - hangi pozisyonlar birbirine bağlı
        private static readonly Dictionary<int, int[]> ADJACENCY_MAP = new Dictionary<int, int[]>
        {
            {0, new int[] {1, 9}}, {1, new int[] {0, 2, 4}}, {2, new int[] {1, 14}},
            {3, new int[] {4, 10}}, {4, new int[] {1, 3, 5, 7}}, {5, new int[] {4, 13}},
            {6, new int[] {7, 11}}, {7, new int[] {4, 6, 8}}, {8, new int[] {7, 12}},
            {9, new int[] {0, 10, 21}}, {10, new int[] {3, 9, 11, 18}}, {11, new int[] {6, 10, 15}},
            {12, new int[] {8, 13, 17}}, {13, new int[] {5, 12, 14, 20}}, {14, new int[] {2, 13, 23}},
            {15, new int[] {11, 16}}, {16, new int[] {15, 17, 19}}, {17, new int[] {12, 16, 22}},
            {18, new int[] {10, 19}}, {19, new int[] {16, 18, 20, 22}}, {20, new int[] {13, 19, 23}},
            {21, new int[] {9, 22}}, {22, new int[] {17, 19, 21, 23}}, {23, new int[] {14, 20, 22}}
        };
        
        public event System.Action<int> OnPositionClicked;
        
        private void Awake()
        {
            InitializeBoard();
        }
        
        private void InitializeBoard()
        {
            boardStones = new Stone[24];
            highlightedPositions = new bool[24];
            
            // Pozisyonları otomatik oluştur eğer manuel ayarlanmamışsa
            if (positions == null || positions.Length != 24)
            {
                CreateBoardPositions();
            }
            
            // Her pozisyona BoardPosition component'i ekle
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] != null)
                {
                    BoardPosition boardPos = positions[i].GetComponent<BoardPosition>();
                    if (boardPos == null)
                    {
                        boardPos = positions[i].gameObject.AddComponent<BoardPosition>();
                    }
                    boardPos.Initialize(i, this);
                }
            }
        }
        
        public void Initialize()
        {
            ClearBoard();
            ClearHighlights();
        }
        
        private void CreateBoardPositions()
        {
            positions = new Transform[24];
            
            // 9 taş oyunu tahtasının pozisyonlarını oluştur
            // Dış kare (0-8)
            Vector2[] outerSquare = CreateSquarePositions(Vector2.zero, 4f);
            // Orta kare (9-16) 
            Vector2[] middleSquare = CreateSquarePositions(Vector2.zero, 2.5f);
            // İç kare (17-23)
            Vector2[] innerSquare = CreateSquarePositions(Vector2.zero, 1f);
            
            // Pozisyonları diziye ekle
            for (int i = 0; i < 8; i++)
            {
                positions[i] = CreatePosition(outerSquare[i], i);
                positions[i + 9] = CreatePosition(middleSquare[i], i + 9);
                if (i < 7)
                    positions[i + 17] = CreatePosition(innerSquare[i], i + 17);
            }
        }
        
        private Vector2[] CreateSquarePositions(Vector2 center, float size)
        {
            Vector2[] square = new Vector2[8];
            float half = size / 2f;
            
            // Köşeler ve kenar ortaları (2D'de)
            square[0] = center + new Vector2(-half, half);   // Sol üst
            square[1] = center + new Vector2(0, half);       // Üst orta
            square[2] = center + new Vector2(half, half);    // Sağ üst
            square[3] = center + new Vector2(half, 0);       // Sağ orta
            square[4] = center + new Vector2(half, -half);   // Sağ alt
            square[5] = center + new Vector2(0, -half);      // Alt orta
            square[6] = center + new Vector2(-half, -half);  // Sol alt
            square[7] = center + new Vector2(-half, 0);      // Sol orta
            
            return square;
        }
        
        private Transform CreatePosition(Vector2 position, int index)
        {
            GameObject posObj = new GameObject($"Position_{index}");
            posObj.transform.parent = transform;
            posObj.transform.position = new Vector3(position.x, position.y, 0);
            
            // Görsel marker ekle (2D sprite kullan)
            GameObject marker = new GameObject("Marker");
            marker.transform.parent = posObj.transform;
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * 0.2f;
            
            // SpriteRenderer ekle
            SpriteRenderer spriteRenderer = marker.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateCircleSprite();
            spriteRenderer.color = Color.white;
            
            // 2D Collider ekle tıklama için
            CircleCollider2D collider = marker.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            
            return posObj.transform;
        }
        
        private Sprite CreateCircleSprite()
        {
            // Basit bir circle sprite oluştur
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            
            Vector2 center = new Vector2(32, 32);
            float radius = 30f;
            
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= radius)
                    {
                        pixels[y * 64 + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * 64 + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }
        
        public bool IsPositionEmpty(int index)
        {
            return IsValidIndex(index) && boardStones[index] == null;
        }
        
        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < 24;
        }
        
        public PlayerType GetStoneOwner(int index)
        {
            if (!IsValidIndex(index) || boardStones[index] == null)
                return PlayerType.None;
            
            return boardStones[index].Owner;
        }
        
        public void PlaceStone(int index, Stone stone)
        {
            if (!IsValidIndex(index) || !IsPositionEmpty(index))
                return;
            
            boardStones[index] = stone;
            stone.transform.position = positions[index].position;
        }
        
        public void MoveStone(int fromIndex, int toIndex)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex))
                return;
            
            if (boardStones[fromIndex] == null || boardStones[toIndex] != null)
                return;
            
            Stone stone = boardStones[fromIndex];
            boardStones[fromIndex] = null;
            boardStones[toIndex] = stone;
            
            stone.MoveTo(positions[toIndex].position);
        }
        
        public void RemoveStone(int index)
        {
            if (!IsValidIndex(index) || boardStones[index] == null)
                return;
            
            Stone stone = boardStones[index];
            boardStones[index] = null;
            stone.Remove();
        }
        
        public bool CanMoveStone(int fromIndex, int toIndex)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex))
                return false;
            
            if (boardStones[fromIndex] == null || boardStones[toIndex] != null)
                return false;
            
            // Komşuluk kontrolü
            if (ADJACENCY_MAP.ContainsKey(fromIndex))
            {
                return ADJACENCY_MAP[fromIndex].Contains(toIndex);
            }
            
            return false;
        }
        
        public bool CheckForMill(int positionIndex, PlayerType playerType)
        {
            foreach (var pattern in MILL_PATTERNS)
            {
                if (pattern.Contains(positionIndex))
                {
                    bool isCompleteMill = true;
                    foreach (int pos in pattern)
                    {
                        if (GetStoneOwner(pos) != playerType)
                        {
                            isCompleteMill = false;
                            break;
                        }
                    }
                    
                    if (isCompleteMill)
                        return true;
                }
            }
            
            return false;
        }
        
        public bool IsStoneInMill(int positionIndex)
        {
            PlayerType owner = GetStoneOwner(positionIndex);
            if (owner == PlayerType.None)
                return false;
            
            return CheckForMill(positionIndex, owner);
        }
        
        public bool HasMovableStones(PlayerType playerType)
        {
            for (int i = 0; i < 24; i++)
            {
                if (GetStoneOwner(i) == playerType && HasValidMoves(i))
                    return true;
            }
            return false;
        }
        
        public bool HasValidMoves(PlayerType playerType)
        {
            for (int i = 0; i < 24; i++)
            {
                if (GetStoneOwner(i) == playerType && HasValidMoves(i))
                    return true;
            }
            return false;
        }
        
        private bool HasValidMoves(int fromIndex)
        {
            if (!ADJACENCY_MAP.ContainsKey(fromIndex))
                return false;
            
            foreach (int toIndex in ADJACENCY_MAP[fromIndex])
            {
                if (IsPositionEmpty(toIndex))
                    return true;
            }
            
            return false;
        }
        
        public void HighlightPosition(int index, bool highlight)
        {
            if (!IsValidIndex(index))
                return;
            
            highlightedPositions[index] = highlight;
            
            if (positions[index] != null)
            {
                SpriteRenderer renderer = positions[index].GetComponentInChildren<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = highlight ? Color.yellow : Color.white;
                }
            }
        }
        
        public void HighlightValidMoves(int fromIndex)
        {
            if (!IsValidIndex(fromIndex) || !ADJACENCY_MAP.ContainsKey(fromIndex))
                return;
            
            foreach (int toIndex in ADJACENCY_MAP[fromIndex])
            {
                if (IsPositionEmpty(toIndex))
                {
                    HighlightPosition(toIndex, true);
                    
                    if (positions[toIndex] != null)
                    {
                        SpriteRenderer renderer = positions[toIndex].GetComponentInChildren<SpriteRenderer>();
                        if (renderer != null)
                        {
                            renderer.color = Color.green;
                        }
                    }
                }
            }
        }
        
        public void ClearHighlights()
        {
            for (int i = 0; i < 24; i++)
            {
                highlightedPositions[i] = false;
                if (positions[i] != null)
                {
                    SpriteRenderer renderer = positions[i].GetComponentInChildren<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.color = Color.white;
                    }
                }
            }
        }
        
        public void ClearBoard()
        {
            for (int i = 0; i < 24; i++)
            {
                if (boardStones[i] != null)
                {
                    Destroy(boardStones[i].gameObject);
                    boardStones[i] = null;
                }
            }
        }
        
        public void OnPositionClick(int index)
        {
            OnPositionClicked?.Invoke(index);
        }
        
        public List<int> GetAvailablePositions()
        {
            List<int> availablePositions = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (IsPositionEmpty(i))
                    availablePositions.Add(i);
            }
            return availablePositions;
        }
    }
} 