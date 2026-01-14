# Toca Boca Dynamics - Yükseltme Planı 🚀

Mevcut "CharacterScene" analizime dayanarak, Toca Boca benzeri canlı ve etkileşimli bir yapı kurmak için gereken adımları çıkardım. Sistem şu an "Kağıt Bebek" (Sprite Stack) mantığında çalışıyor, ancak gelişmiş etkileşimler için **Rig (Kemik/Pivot)** yapısına geçmemiz gerekecek.

---

## 🗺️ Adım Adım Yol Haritası

### 1. 🏗️ Karakter İskelet Sistemi (Rigging)
Mevcut yapı sadece üst üste resimlerden oluşuyor. El sallama, yürüme veya oturma için eklem noktalarına (Pivot) ihtiyacımız var.
*   **Mevcut:** `Beden -> Saç, Göz, Kıyafet` (Hepsi üst üste)
*   **Hedef:** `Beden -> Baş, Gövde, Sol Kol, Sağ Kol, Sol Bacak, Sağ Bacak`
*   **Eklenecekler:**
    *   Hiyerarşik Bone yapısı.
    *   Uzuvların eklem noktalarının (Pivot) ayarlanması.

### 2. ✋ Eşya Tutma Sistemi (Holding System)
Karakterlerin ellerine telefon, elma, bardak gibi eşyaları alabilmesi.
*   **Eklenecek Scriptler:**
    *   `HandSlot.cs`: Karakterin elindeki boş nokta.
    *   `HoldableItem.cs`: Eşyaların tutulabilir olduğunu belirten script.
*   **Mantık:**
    *   Eşya ele yaklaştırılınca "Snap" (Yapışma) efekti.
    *   Eşya elin `child` objesi olur ve el ile birlikte hareket eder.

### 3. 🪑 Oturma & Etkileşim Sistemi (Sitting System)
Karakterin sandalye, koltuk veya yatağa sürüklendiğinde pozisyon alması.
*   **Eklenecek Scriptler:**
    *   `Seat.cs`: Oturulabilir alanları tanımlar.
    *   `CharacterPoseManager.cs`: Karakterin duruşunu (Ayakta, Oturuyor, Yatıyor) yönetir.
*   **Mantık:**
    *   Koltuk üzerine bırakılınca karakterin Sprite'ları "Oturma" versiyonuna geçer (veya bacaklar bükülür).

### 4. 🎭 Gelişmiş Duygu Sistemi (Emotion Manager)
Basit bir Enum yerine, farklı durumlara tepki veren bir yüz sistemi.
*   **Hedef:** Yemek yerken "Ağız Açma", tadı kötüyse "İğrenme", hediye alınca "Şaşırma".
*   **Eklenecekler:**
    *   `FaceController.cs`: Göz, Ağız ve Kaş sprite'larını bağımsız yönetir.
    *   `FeedbackSystem`: Eşyalar karaktere bir duygu (Mood) gönderebilir (Örn: Acı Biber -> Ağız Yanma).

---

## ✅ Onay Sırası
Karmaşıklığı yönetmek için bu sırayla ilerlemeliyiz:

1.  **[ ] İskelet (Hierarchy) Düzenlemesi:** Karakter prefabını parçalara ayırıp pivotlarını ayarlayacağız. (Temel bu).
2.  **[ ] Tutma (Holding) Sistemi:** Ele eşya almayı kodlayacağız.
3.  **[ ] Duygu (Emotion) Sistemi:** Yüz ifadelerini kodlayacağız.
4.  **[ ] Oturma (Sitting) Sistemi:** En zor kısım. İskelet oturduğunda nasıl görünecek?

**Hangi adımdan başlayalım? (Önerim 1. Adım: İskelet)**
