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
        }
        
        private void OnMouseDown()
        {
            if (!isInitialized || parentBoard == null)
                return;
            
            parentBoard.OnPositionClick(positionIndex);
        }
        
        private void OnMouseEnter()
        {
            // Hover efekti eklenebilir
        }
        
        private void OnMouseExit()
        {
            // Hover efekti kaldırılabilir
        }
    }
} 