# Board Defence Game - Mimari Dokümantasyonu

## 📋 Proje Özeti

Bu proje, oyuncunun ekranın üstünden gelen düşmanlara karşı 3 farklı savunma öğesiyle üssünü savunduğu bir Board Defence oyunudur.

### Oyun Kuralları

- **Board**: 4x8 grid (4 sütun, 8 satır)
- **Yerleştirme Bölgesi**: Sadece alt yarı (satır 4-7)
- **Düşman Spawn**: Rastgele sütundan, üstten gelir

---

## 🏗️ Mimari Yapı

### Klasör Yapısı

```
Assets/
├── Scripts/
│   ├── Core/           # Ana oyun sistemleri
│   │   ├── Enums/      # Enum tanımlamaları
│   │   └── Events/     # Event sistemi
│   ├── Board/          # Board ve cell yönetimi
│   ├── Defence/        # Savunma öğeleri
│   │   └── AttackStrategies/
│   ├── Enemy/          # Düşman sistemi
│   ├── Data/           # ScriptableObject'ler
│   ├── UI/             # Kullanıcı arayüzü
│   ├── Interfaces/     # Interface tanımlamaları
│   └── Utils/          # Yardımcı sınıflar
├── ScriptableObjects/  # Data assets
│   ├── DefenceItems/
│   ├── Enemies/
│   └── Levels/
└── Documentation/      # Proje dokümantasyonu
```

---

## 🎨 Design Patterns

### 1. Strategy Pattern

**Kullanım**: Savunma öğelerinin farklı saldırı yönleri

- `IAttackStrategy` interface
- `ForwardAttackStrategy` - Sadece ileri yön
- `AllDirectionAttackStrategy` - 4 yön (yukarı, aşağı, sol, sağ)
- `AttackStrategyFactory` - Strategy seçimi

### 2. Factory Pattern

**Kullanım**: Savunma öğesi ve düşman oluşturma

- `DefenceItemFactory` - Savunma öğesi üretimi
- `EnemyPool` - Düşman üretimi (Factory + Pool)

### 3. Object Pool Pattern

**Kullanım**: Düşman yönetimi (performans optimizasyonu)

- `EnemyPool` - Düşmanları yeniden kullanma
- `IPoolable` interface - Pool davranış kontratı

### 4. Observer Pattern

**Kullanım**: Oyun olayları ve sistem iletişimi

- `GameEvents` - Statik event hub
- Loose coupling sağlar

### 5. State Pattern

**Kullanım**: Oyun durumu yönetimi

- `GameState` enum
- `GameManager` - Durum geçişleri

### 6. Singleton Pattern

**Kullanım**: Ana manager (dikkatli kullanım)

- `GameManager.Instance`
- Alternatif: `ServiceLocator`

### 7. Service Locator Pattern

**Kullanım**: Servis erişimi (Singleton alternatifi)

- `ServiceLocator.Register<T>()`
- `ServiceLocator.Get<T>()`

---

## 🔧 SOLID Prensipleri

### Single Responsibility (S)

- Her sınıf tek bir sorumluluğa sahip
- `BoardCell` sadece hücre, `GameBoard` sadece grid yönetimi

### Open/Closed (O)

- `IAttackStrategy` ile yeni saldırı tipleri eklenebilir
- ScriptableObject'ler ile yeni düşman/savunma tipleri

### Liskov Substitution (L)

- Tüm `IAttackStrategy` implementasyonları birbirinin yerine kullanılabilir
- `IDamageable` implementasyonları polymorphic

### Interface Segregation (I)

- `IDamageable` - Sadece hasar alabilme
- `IPlaceable` - Sadece yerleştirme
- `IPoolable` - Sadece pool davranışı

### Dependency Inversion (D)

- Sınıflar concrete sınıflara değil interface'lere bağımlı
- ScriptableObject ile data injection

---

## 📊 Oyun Verileri

### Defence Items (Savunma Öğeleri)

| Tip | Hasar | Menzil | Aralık | Yön   |
| --- | ----- | ------ | ------ | ----- |
| 1   | 3     | 4 blok | 3s     | İleri |
| 2   | 5     | 2 blok | 4s     | İleri |
| 3   | 10    | 1 blok | 5s     | Tüm   |

### Enemies (Düşmanlar)

| Tip | Can | Hız         |
| --- | --- | ----------- |
| 1   | 3   | 1 blok/s    |
| 2   | 10  | 0.25 blok/s |
| 3   | 5   | 0.5 blok/s  |

### Levels (Seviyeler)

| Seviye | Savunma (1/2/3) | Düşman (1/2/3) |
| ------ | --------------- | -------------- |
| 1      | 3/2/1           | 3/1/1          |
| 2      | 3/4/2           | 5/2/3          |
| 3      | 5/7/5           | 7/3/5          |

---

## 🔄 Oyun Akışı

```
MainMenu → Preparation → Battle → Victory/Defeat
              ↑                        ↓
              ←── Restart ←────────────┘
```

1. **MainMenu**: Oyun başlangıç ekranı
2. **Preparation**: Savunma öğelerini yerleştirme
3. **Battle**: Düşmanlar gelir, savunma saldırır
4. **Victory/Defeat**: Sonuç ekranı

---

## 🎮 Kullanım Kılavuzu

### Yeni Level Ekleme

1. `Assets/ScriptableObjects/Levels/` klasörüne sağ tık
2. Create → BoardDefence → Level Data
3. Savunma ve düşman sayılarını ayarla
4. `LevelManager`'daki `_levels` array'ine ekle

### Yeni Savunma Tipi Ekleme

1. `DefenceItemType` enum'a yeni tip ekle
2. Yeni `DefenceItemData` ScriptableObject oluştur
3. `DefenceItemFactory`'ye yeni data'yı ekle

### Yeni Düşman Tipi Ekleme

1. `EnemyType` enum'a yeni tip ekle
2. Yeni `EnemyData` ScriptableObject oluştur
3. `EnemyPool`'a yeni data'yı ekle

---

## ⚠️ Dikkat Edilmesi Gerekenler

1. **Event Unsubscribe**: `OnDestroy`'da event'lerden çık
2. **Pool Limit**: Object pool boyutunu kontrol et
3. **ScriptableObject**: Veriler readonly, runtime'da değiştirme
4. **Singleton**: Sadece zorunlu durumlarda kullan

---

## 🧪 Test Önerileri

- Unit test: Attack strategy'ler
- Integration test: Defence item + Enemy etkileşimi
- Play mode test: Level akışı

---

## 🚀 Hızlı Başlangıç (Quick Setup)

### Adım 1: ScriptableObject'leri Oluştur

Unity Editor'da: `Board Defence → Setup → Create All ScriptableObjects`

### Adım 2: Prefab'ları Oluştur

1. **Cell Prefab**: Sprite Renderer + BoxCollider2D + BoardCell script
2. **Defence Prefab**: Sprite Renderer + DefenceItemBase script
3. **Enemy Prefab**: Sprite Renderer + BoxCollider2D + EnemyBase script

### Adım 3: Scene Setup

1. Boş GameObject → "GameManager" → GameManager script ekle
2. Boş GameObject → "GameBoard" → GameBoard script ekle
3. Boş GameObject → "LevelManager" → LevelManager script ekle
4. Boş GameObject → "BoardManager" → BoardManager script ekle
5. Boş GameObject → "DefenceFactory" → DefenceItemFactory script ekle
6. Boş GameObject → "EnemyPool" → EnemyPool script ekle
7. Boş GameObject → "Bootstrapper" → GameBootstrapper script ekle
8. Tüm referansları bağla

### Adım 4: ScriptableObject'leri Bağla

- DefenceFactory'ye Defence Item Data'ları
- EnemyPool'a Enemy Data'ları
- LevelManager'a Level Data'ları

---

## 📁 Oluşturulan Dosya Listesi

### Interfaces (4 dosya)

- `IDamageable.cs` - Hasar alabilme
- `IPlaceable.cs` - Board'a yerleştirme
- `IPoolable.cs` - Object pool desteği
- `IAttackStrategy.cs` - Saldırı stratejisi

### Core (7 dosya)

- `GameManager.cs` - Ana oyun yöneticisi
- `BoardManager.cs` - Savunma yerleştirme
- `LevelManager.cs` - Level ve spawn yönetimi
- `ServiceLocator.cs` - Servis erişimi
- `GameBootstrapper.cs` - Sistem başlatma
- `GameConstants.cs` - Sabit değerler
- `Enums/GameEnums.cs` - Enum tanımları
- `Events/GameEvents.cs` - Olay sistemi

### Board (2 dosya)

- `GameBoard.cs` - 4x8 grid yönetimi
- `BoardCell.cs` - Tekil hücre

### Defence (4 dosya)

- `DefenceItemBase.cs` - Savunma öğesi base sınıf
- `DefenceItemFactory.cs` - Savunma üretimi
- `AttackStrategies/ForwardAttackStrategy.cs` - İleri saldırı
- `AttackStrategies/AllDirectionAttackStrategy.cs` - Çok yönlü saldırı
- `AttackStrategies/AttackStrategyFactory.cs` - Strateji fabrikası

### Enemy (2 dosya)

- `EnemyBase.cs` - Düşman base sınıf
- `EnemyPool.cs` - Object pool

### Data (3 dosya)

- `DefenceItemData.cs` - Savunma verileri
- `EnemyData.cs` - Düşman verileri
- `LevelData.cs` - Level verileri

### UI (2 dosya)

- `GameUIController.cs` - Ana UI yönetimi
- `DefenceItemButton.cs` - Savunma seçim butonu

### Utils (3 dosya)

- `AutoDestroy.cs` - Otomatik yok etme
- `HealthBar.cs` - Can barı
- `Projectile.cs` - Mermi

### Input (1 dosya)

- `InputHandler.cs` - Oyuncu girdisi

### Editor (1 dosya)

- `BoardDefenceSetup.cs` - Editor araçları

---

_Bu dokümantasyon, projenin gelecekteki geliştiricileri için referans olarak hazırlanmıştır._
