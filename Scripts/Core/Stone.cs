using UnityEngine;

namespace NineMensMorris.Core
{
    public class Stone : MonoBehaviour
    {
        [Header("Stone Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite player1Sprite;
        [SerializeField] private Sprite player2Sprite;
        [SerializeField] private Color player1Color = Color.white;
        [SerializeField] private Color player2Color = Color.black;
        [SerializeField] private Color highlightColor = Color.yellow;
        
        [Header("Animation")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float scaleSpeed = 5f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private PlayerType owner;
        private Color originalColor;
        private bool isHighlighted;
        private Vector3 targetPosition;
        private bool isMoving;
        
        public PlayerType Owner => owner;
        public bool IsMoving => isMoving;
        
        public event System.Action<Stone> OnMoveCompleted;
        
        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            // Eğer sprite atanmamışsa varsayılan sprite oluştur
            if (player1Sprite == null || player2Sprite == null)
            {
                CreateDefaultSprites();
            }
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
        
        private void CreateDefaultSprites()
        {
            // Varsayılan circle sprite oluştur
            Sprite defaultSprite = CreateCircleSprite();
            if (player1Sprite == null) player1Sprite = defaultSprite;
            if (player2Sprite == null) player2Sprite = defaultSprite;
        }
        
        private Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            
            Vector2 center = new Vector2(32, 32);
            float radius = 28f;
            
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance <= radius)
                    {
                        // Gradient efekti için
                        float alpha = 1f - (distance / radius) * 0.3f;
                        pixels[y * 64 + x] = new Color(1f, 1f, 1f, alpha);
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
        
        public void Initialize(PlayerType playerType)
        {
            owner = playerType;
            SetSpriteAndColor();
            SetScale(0f);
            AnimateScale(1f);
        }
        
        public void SetOwner(PlayerType playerType)
        {
            owner = playerType;
            SetSpriteAndColor();
        }
        
        private void SetSpriteAndColor()
        {
            if (spriteRenderer == null) return;
            
            switch (owner)
            {
                case PlayerType.Player1:
                    spriteRenderer.sprite = player1Sprite;
                    spriteRenderer.color = player1Color;
                    originalColor = player1Color;
                    break;
                case PlayerType.Player2:
                    spriteRenderer.sprite = player2Sprite;
                    spriteRenderer.color = player2Color;
                    originalColor = player2Color;
                    break;
                default:
                    spriteRenderer.color = Color.white;
                    originalColor = Color.white;
                    break;
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
            if (isHighlighted == highlight || spriteRenderer == null)
                return;
                
            isHighlighted = highlight;
            
            if (highlight)
            {
                spriteRenderer.color = highlightColor;
            }
            else
            {
                spriteRenderer.color = originalColor;
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