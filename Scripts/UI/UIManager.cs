using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NineMensMorris.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Game Info UI")]
        [SerializeField] private TextMeshProUGUI currentPlayerText;
        [SerializeField] private TextMeshProUGUI gamePhaseText;
        [SerializeField] private TextMeshProUGUI player1InfoText;
        [SerializeField] private TextMeshProUGUI player2InfoText;
        
        [Header("Game Messages")]
        [SerializeField] private GameObject messagePanel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button messageOkButton;
        
        [Header("Game End UI")]
        [SerializeField] private GameObject gameEndPanel;
        [SerializeField] private TextMeshProUGUI gameEndText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        
        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        
        [Header("Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button settingsBackButton;
        
        [Header("Help Panel")]
        [SerializeField] private GameObject helpPanel;
        [SerializeField] private Button helpButton;
        [SerializeField] private Button helpCloseButton;
        
        private bool isInitialized = false;
        
        public void Initialize()
        {
            SetupButtons();
            HideAllPanels();
            ShowMainMenu();
            isInitialized = true;
        }
        
        private void SetupButtons()
        {
            // Main menu butonları
            if (startGameButton != null)
                startGameButton.onClick.AddListener(StartGame);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettings);
            
            if (exitButton != null)
                exitButton.onClick.AddListener(QuitGame);
            
            // Game end butonları
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitToMainMenu);
            
            // Message panel butonları
            if (messageOkButton != null)
                messageOkButton.onClick.AddListener(HideMessage);
            
            // Settings butonları
            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(HideSettings);
            
            // Help butonları
            if (helpButton != null)
                helpButton.onClick.AddListener(ShowHelp);
            
            if (helpCloseButton != null)
                helpCloseButton.onClick.AddListener(HideHelp);
            
            // Settings kontrolları
            if (volumeSlider != null)
            {
                volumeSlider.value = AudioListener.volume;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }
            
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = Screen.fullScreen;
                fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            }
        }
        
        private void HideAllPanels()
        {
            if (messagePanel != null) messagePanel.SetActive(false);
            if (gameEndPanel != null) gameEndPanel.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (helpPanel != null) helpPanel.SetActive(false);
        }
        
        public void ShowMainMenu()
        {
            HideAllPanels();
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }
        
        public void UpdateGameInfo(Core.Player currentPlayer, Core.GamePhase currentPhase, 
                                 Core.Player player1, Core.Player player2)
        {
            if (!isInitialized)
                return;
            
            // Mevcut oyuncu bilgisi
            if (currentPlayerText != null)
            {
                currentPlayerText.text = $"Sıra: {currentPlayer.PlayerName}";
                currentPlayerText.color = currentPlayer.PlayerColor;
            }
            
            // Oyun aşaması
            if (gamePhaseText != null)
            {
                string phaseText = currentPhase switch
                {
                    Core.GamePhase.Placement => "Taş Yerleştirme",
                    Core.GamePhase.Movement => "Taş Hareket Ettirme",
                    Core.GamePhase.Flying => "Uçma Aşaması",
                    _ => "Bilinmeyen Aşama"
                };
                gamePhaseText.text = $"Aşama: {phaseText}";
            }
            
            // Oyuncu bilgileri
            if (player1InfoText != null)
            {
                player1InfoText.text = $"{player1.PlayerName}\nEl: {player1.StonesInHand}\nTahta: {player1.StonesOnBoard}";
                player1InfoText.color = player1.PlayerColor;
            }
            
            if (player2InfoText != null)
            {
                player2InfoText.text = $"{player2.PlayerName}\nEl: {player2.StonesInHand}\nTahta: {player2.StonesOnBoard}";
                player2InfoText.color = player2.PlayerColor;
            }
        }
        
        public void ShowMessage(string message, float duration = 3f)
        {
            if (messagePanel != null && messageText != null)
            {
                messageText.text = message;
                messagePanel.SetActive(true);
                
                if (duration > 0)
                {
                    Invoke(nameof(HideMessage), duration);
                }
            }
        }
        
        public void ShowRemovalMessage(Core.Player player)
        {
            ShowMessage($"{player.PlayerName} mill yaptı! Rakip taşını seçin.", 0f);
        }
        
        public void ShowGameEnd(Core.Player winner)
        {
            if (gameEndPanel != null && gameEndText != null)
            {
                gameEndText.text = $"{winner.PlayerName} Kazandı!";
                gameEndText.color = winner.PlayerColor;
                gameEndPanel.SetActive(true);
            }
        }
        
        private void HideMessage()
        {
            if (messagePanel != null)
                messagePanel.SetActive(false);
            
            CancelInvoke(nameof(HideMessage));
        }
        
        private void StartGame()
        {
            HideAllPanels();
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.RestartGame();
            }
        }
        
        private void RestartGame()
        {
            HideAllPanels();
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.RestartGame();
            }
        }
        
        private void QuitToMainMenu()
        {
            ShowMainMenu();
        }
        
        private void QuitGame()
        {
            Application.Quit();
        }
        
        private void ShowSettings()
        {
            HideAllPanels();
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }
        
        private void HideSettings()
        {
            HideAllPanels();
            ShowMainMenu();
        }
        
        private void ShowHelp()
        {
            if (helpPanel != null)
                helpPanel.SetActive(true);
        }
        
        private void HideHelp()
        {
            if (helpPanel != null)
                helpPanel.SetActive(false);
        }
        
        private void SetVolume(float volume)
        {
            AudioListener.volume = volume;
        }
        
        private void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
        }
        
        // Klavye kısayolları için
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (helpPanel != null)
                {
                    if (helpPanel.activeInHierarchy)
                        HideHelp();
                    else
                        ShowHelp();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (helpPanel != null && helpPanel.activeInHierarchy)
                {
                    HideHelp();
                }
                else if (settingsPanel != null && settingsPanel.activeInHierarchy)
                {
                    HideSettings();
                }
                else if (gameEndPanel != null && gameEndPanel.activeInHierarchy)
                {
                    QuitToMainMenu();
                }
            }
        }
    }
} 