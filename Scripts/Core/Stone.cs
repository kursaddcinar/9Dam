using UnityEngine;

namespace NineMensMorris.Core
{
    public class Stone : MonoBehaviour
    {
        [Header("Stone Settings")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material player1Material;
        [SerializeField] private Material player2Material;
        [SerializeField] private Material highlightMaterial;
        
        [Header("Animation")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float scaleSpeed = 5f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private PlayerType owner;
        private Material originalMaterial;
        private bool isHighlighted;
        private Vector3 targetPosition;
        private bool isMoving;
        
        public PlayerType Owner => owner;
        public bool IsMoving => isMoving;
        
        public event System.Action<Stone> OnMoveCompleted;
        
        private void Awake()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
        }
        
        private void Update()
        {
            if (isMoving)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                
                if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                {
                    transform.position = targetPosition;
                    isMoving = false;
                    OnMoveCompleted?.Invoke(this);
                }
            }
        }
        
        public void Initialize(PlayerType playerType)
        {
            owner = playerType;
            SetMaterial();
            SetScale(0f);
            AnimateScale(1f);
        }
        
        public void SetOwner(PlayerType playerType)
        {
            owner = playerType;
            SetMaterial();
        }
        
        private void SetMaterial()
        {
            Material material = owner switch
            {
                PlayerType.Player1 => player1Material,
                PlayerType.Player2 => player2Material,
                _ => null
            };
            
            if (material != null)
            {
                meshRenderer.material = material;
                originalMaterial = material;
            }
        }
        
        public void MoveTo(Vector3 position, System.Action onComplete = null)
        {
            targetPosition = position;
            isMoving = true;
            
            if (onComplete != null)
            {
                System.Action<Stone> tempHandler = null;
                tempHandler = (stone) =>
                {
                    OnMoveCompleted -= tempHandler;
                    onComplete.Invoke();
                };
                OnMoveCompleted += tempHandler;
            }
        }
        
        public void SetHighlight(bool highlight)
        {
            if (isHighlighted == highlight)
                return;
                
            isHighlighted = highlight;
            
            if (highlight)
            {
                meshRenderer.material = highlightMaterial;
            }
            else
            {
                meshRenderer.material = originalMaterial;
            }
        }
        
        public void AnimateScale(float targetScale)
        {
            StartCoroutine(ScaleCoroutine(targetScale));
        }
        
        private System.Collections.IEnumerator ScaleCoroutine(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            float elapsed = 0f;
            float duration = 1f / scaleSpeed;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curveValue = moveCurve.Evaluate(t);
                transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
                yield return null;
            }
            
            transform.localScale = endScale;
        }
        
        public void SetScale(float scale)
        {
            transform.localScale = Vector3.one * scale;
        }
        
        public void Remove()
        {
            AnimateScale(0f);
            Destroy(gameObject, 1f / scaleSpeed);
        }
        
        private void OnMouseDown()
        {
            // Taş tıklandığında InputManager'a bildir
            InputManager inputManager = FindObjectOfType<InputManager>();
            if (inputManager != null)
            {
                int positionIndex = GetComponent<BoardPosition>()?.PositionIndex ?? -1;
                if (positionIndex >= 0)
                {
                    inputManager.OnStoneClicked(positionIndex);
                }
            }
        }
    }
} 