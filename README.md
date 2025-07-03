# 9 Taş Oyunu (Nine Men's Morris) - Unity

Bu proje, Unity için modüler ve yönetilebilir bir şekilde yazılmış 9 taş oyununun complete implementasyonudur.

## 📋 Özellikler

- **Modüler Kod Yapısı**: Her component ayrı namespace'lerde organize edilmiş
- **3 Oyun Aşaması**: Yerleştirme, Hareket, Uçma aşamaları
- **Mill Tespiti**: Otomatik mill (3 taş dizisi) algılama
- **AI Desteği**: 3 farklı zorluk seviyesinde yapay zeka
- **Güzel UI**: Modern ve kullanıcı dostu arayüz
- **Animasyonlar**: Smooth taş hareketleri ve efektler
- **Input Yönetimi**: Mouse ve touch desteği

## 🎮 Oyun Kuralları

### Genel Bakış
9 Taş oyunu, iki oyuncunun 24 pozisyonlu bir tahtada oynadığı strateji oyunudur. Her oyuncunun 9 taşı vardır.

### Oyun Aşamaları

1. **Yerleştirme Aşaması**
   - Oyuncular sırayla taşlarını boş pozisyonlara yerleştirir
   - Mill oluşturulduğunda rakipten bir taş alınır

2. **Hareket Aşaması**
   - Taşlar komşu pozisyonlara hareket ettirilebilir
   - Mill oluşturulduğunda rakipten bir taş alınır

3. **Uçma Aşaması**
   - Oyuncunun 3 taşı kaldığında başlar
   - Taşlar herhangi bir boş pozisyona "uçabilir"

### Kazanma Koşulları
- Rakibi 3 taştan az taşa düşürmek
- Rakibin hareket edememesini sağlamak

## 🏗️ Kod Yapısı

### Namespace Organizasyonu

```
NineMensMorris/
├── Core/           # Ana oyun mantığı
├── AI/             # Yapay zeka sistemi  
└── UI/             # Kullanıcı arayüzü
```

### Ana Sınıflar

#### Core Namespace
- **GameManager**: Ana oyun kontrolcüsü, Singleton pattern
- **Board**: 24 pozisyonlu tahta yönetimi ve mill tespiti
- **Stone**: Oyun taşları, animasyonlar ve görsel efektler
- **Player**: Oyuncu verisi ve taş yönetimi
- **GameState**: Oyun durumu ve aşama yönetimi
- **InputManager**: Mouse/touch input yönetimi
- **BoardPosition**: Tahta pozisyon interactionları

#### AI Namespace
- **AIPlayer**: 3 zorluk seviyesinde yapay zeka

#### UI Namespace  
- **UIManager**: Tüm UI elementleri ve menü yönetimi

## 🚀 Unity'de Kurulum

### 1. Sahne Kurulumu

```
Game Scene/
├── GameManager (GameManager.cs)
├── Board (Board.cs)
├── Players/
│   ├── Player1 (Player.cs)
│   └── Player2 (Player.cs)
├── Input (InputManager.cs)
├── UI Canvas (UIManager.cs)
└── AI (AIPlayer.cs) [opsiyonel]
```

### 2. GameManager Ayarları

Inspector'da GameManager component'inde:
- Board referansını ayarlayın
- Player1 ve Player2 referanslarını ayarlayın  
- UIManager referansını ayarlayın
- InputManager referansını ayarlayın

### 3. Player Ayarları

Her Player component'inde:
- Player Name belirleyin
- Player Color ayarlayın
- Stone Prefab atayın

### 4. Board Ayarları

Board component'inde:
- Highlight Material
- Normal Material
- Valid Move Material

### 5. Stone Prefab

Stone prefab'inde şunlar olmalı:
- MeshRenderer component
- Stone.cs script
- Collider (mouse detection için)
- Materials (Player1, Player2, Highlight)

## 🎯 Kullanım Örneği

### Oyunu Başlatma

```csharp
// Oyun otomatik olarak başlar
// Manuel başlatmak için:
GameManager.Instance.RestartGame();
```

### AI Eklemek

```csharp
// AIPlayer component'ini Player2'ye ekleyin
AIPlayer ai = player2.GetComponent<AIPlayer>();
ai.Initialize(player2, board, gameManager);
```

### Custom Event Handling

```csharp
// GameManager eventlerini dinleyin
GameManager.Instance.OnPlayerChanged += OnPlayerChanged;
GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
GameManager.Instance.OnGameEnded += OnGameEnded;
```

## ⌨️ Klavye Kısayolları

- **R**: Oyunu yeniden başlat
- **ESC**: Seçimi iptal et / Menüleri kapat
- **H**: Yardım panelini aç/kapat

## 🔧 Özelleştirme

### Yeni Zorluk Seviyeleri

```csharp
public enum AIDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert  // Yeni seviye ekleyin
}
```

### Custom Materials

Board ve Stone sınıflarında material referanslarını değiştirerek görsel stili özelleştirebilirsiniz.

### Animasyon Ayarları

Stone.cs'de animasyon hızlarını değiştirebilirsiniz:
- `moveSpeed`: Hareket hızı
- `scaleSpeed`: Büyüme/küçülme hızı
- `moveCurve`: Hareket eğrisi

## 🐛 Bilinen Sorunlar

- AI'ın mill tespiti optimizasyonu geliştirilebilir
- Touch input tam test edilmemiş
- Board pozisyonları manuel ayarlama gerekebilir

## 📝 Geliştirme Notları

### Mill Patterns

Board.cs'de mill desenleri hardcoded tanımlı:
- Yatay milllar
- Dikey milllar  
- 3 kare (dış, orta, iç) için ayrı desenler

### Pozisyon Indexleri

Tahta pozisyonları 0-23 arası index'lenir:
- 0-8: Dış kare
- 9-16: Orta kare  
- 17-23: İç kare

## 🤝 Katkıda Bulunma

Bu modüler yapı sayesinde kolayca yeni özellikler eklenebilir:
- Yeni AI stratejileri
- Multiplayer network desteği
- Sound manager entegrasyonu
- Particle effect sistemleri

## 📄 Lisans

Bu proje eğitim amaçlı yazılmıştır. Ticari kullanım için uygun değildir.

---

**Not**: Bu implementasyon Unity 2021.3 LTS ve üzeri versiyonlarda test edilmiştir. 