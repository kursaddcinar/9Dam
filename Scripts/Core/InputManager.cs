using UnityEngine;

namespace NineMensMorris.Core
{
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private LayerMask clickableLayers = -1;
        [SerializeField] private float raycastDistance = 100f;
        
        private Camera mainCamera;
        private bool inputEnabled = true;
        
        public event System.Action<int> OnPositionClicked;
        
        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindObjectOfType<Camera>();
        }
        
        public void Initialize()
        {
            inputEnabled = true;
        }
        
        private void Update()
        {
            if (!inputEnabled)
                return;
            
            HandleMouseInput();
            HandleKeyboardInput();
        }
        
        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }
        
        private void HandleKeyboardInput()
        {
            // ESC tuşu ile seçimi iptal et
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // GameManager'dan seçimi temizle
                if (GameManager.Instance != null)
                {
                    // Burada seçimi temizleme işlemi yapılabilir
                }
            }
            
            // R tuşu ile oyunu yeniden başlat
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartGame();
                }
            }
        }
        
        private void HandleMouseClick()
        {
            if (mainCamera == null)
                return;
            
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, raycastDistance, clickableLayers))
            {
                // BoardPosition kontrolü
                BoardPosition boardPosition = hit.collider.GetComponent<BoardPosition>();
                if (boardPosition == null)
                {
                    boardPosition = hit.collider.GetComponentInParent<BoardPosition>();
                }
                
                if (boardPosition != null)
                {
                    OnPositionClicked?.Invoke(boardPosition.PositionIndex);
                    return;
                }
                
                // Stone kontrolü
                Stone stone = hit.collider.GetComponent<Stone>();
                if (stone == null)
                {
                    stone = hit.collider.GetComponentInParent<Stone>();
                }
                
                if (stone != null)
                {
                    OnStoneClicked(stone);
                    return;
                }
            }
        }
        
        public void OnStoneClicked(Stone stone)
        {
            // Stone tıklandığında pozisyon indeksini bul
            BoardPosition boardPos = stone.GetComponent<BoardPosition>();
            if (boardPos != null)
            {
                OnPositionClicked?.Invoke(boardPos.PositionIndex);
            }
        }
        
        public void OnStoneClicked(int positionIndex)
        {
            OnPositionClicked?.Invoke(positionIndex);
        }
        
        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }
        
        public bool IsInputEnabled()
        {
            return inputEnabled;
        }
        
        // Touch desteği için
        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    HandleTouch(touch.position);
                }
            }
        }
        
        private void HandleTouch(Vector2 screenPosition)
        {
            if (mainCamera == null)
                return;
            
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, raycastDistance, clickableLayers))
            {
                BoardPosition boardPosition = hit.collider.GetComponent<BoardPosition>();
                if (boardPosition == null)
                {
                    boardPosition = hit.collider.GetComponentInParent<BoardPosition>();
                }
                
                if (boardPosition != null)
                {
                    OnPositionClicked?.Invoke(boardPosition.PositionIndex);
                }
            }
        }
    }
} 