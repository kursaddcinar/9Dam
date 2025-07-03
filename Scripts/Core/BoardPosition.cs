using UnityEngine;

namespace NineMensMorris.Core
{
    public class BoardPosition : MonoBehaviour
    {
        private int positionIndex;
        private Board parentBoard;
        private bool isInitialized;
        
        public int PositionIndex => positionIndex;
        
        public void Initialize(int index, Board board)
        {
            positionIndex = index;
            parentBoard = board;
            isInitialized = true;
            
            // 2D collider olduğundan emin ol
            EnsureCollider2D();
        }
        
        private void EnsureCollider2D()
        {
            // Eğer CircleCollider2D yoksa ekle
            CircleCollider2D collider2D = GetComponent<CircleCollider2D>();
            if (collider2D == null)
            {
                collider2D = gameObject.AddComponent<CircleCollider2D>();
                collider2D.radius = 0.5f;
            }
        }
        
        private void OnMouseDown()
        {
            if (!isInitialized || parentBoard == null)
                return;
            
            parentBoard.OnPositionClick(positionIndex);
        }
        
        // 2D için alternatif tıklama metodu
        private void OnMouseUpAsButton()
        {
            if (!isInitialized || parentBoard == null)
                return;
            
            parentBoard.OnPositionClick(positionIndex);
        }
        
        private void OnMouseEnter()
        {
            // Hover efekti eklenebilir
            if (isInitialized && parentBoard != null)
            {
                SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.cyan, 0.3f);
                }
            }
        }
        
        private void OnMouseExit()
        {
            // Hover efekti kaldırılabilir
            if (isInitialized && parentBoard != null)
            {
                SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
            }
        }
        
        // 2D trigger desteği
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 2D trigger olayları için
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            // 2D trigger olayları için
        }
    }
} 