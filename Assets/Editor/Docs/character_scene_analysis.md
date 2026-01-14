# CharacterScene Analizi 🎭

Bu rapor, Unity projesindeki **"CharacterScene"** yapısının, GameObjects ve Scripts açısından detaylı teknik analizini içerir.

---

## 🏗️ 1. Sahne Hiyerarşisi (GameObject Structure)
Sahnede (`CharacterScene.unity`) yer alan temel yapılar şunlardır:

*   **ShowArea**: Karakterin canlı önizlemesinin yapıldığı merkez alan. `CharacterCreationManager` burayı referans alır (`previewArea`).
*   **CharacterSlot (Örn: Slot_9, Slot_17)**: Hazır karakterlerin veya oyuncunun yarattığı karakterlerin listelendiği slotlar.
    *   İçerisinde `CharacterSlot` scripti bulunur. Veri tutma görevi görür.
*   **Floor**: Karakterin bastığı zemin. `CharacterDrag` scripti tarafından sınır belirleyici (`Background`) olarak kullanılabilir.
*   **OptionGrid**: Özelleştirme seçeneklerinin (Saç, Göz, vb.) dinamik olarak listelendiği UI paneli.
    *   `GridLayoutGroup` ile öğeleri dizer.
    *   `DynamicCategoryManager` burayı doldurup boşaltır.
*   **Tab Butonları (Tab_Beard vb.)**: Kategoriler arası geçişi sağlayan butonlar.
    *   Tıklandığında `CharacterCreationManager.SetCategory(...)` fonksiyonunu tetikler.

---

## 📜 2. Script Mimarisi (Code Logic)

Sistem 3 ana yönetici script üzerine kuruludur:

### 🧠 A. `CharacterCreationManager.cs` (Beyin)
Tüm operasyonu yöneten merkezdir.
*   **Görevi:**
    *   **Resource Loader:** `Resources/Images/Character/Style/...` yolundan tüm sprite'ları (Saç, Göz, Kıyafet) listelere yükler.
    *   **Preview Manager:** `previewInstance` (Karakter) üzerinde değişiklikleri anlık uygular (Sprite Swap).
    *   **Data Holder:** Renk paletlerini (`skinColors`, `hairColors`) ve Sprite listelerini tutar.
*   **Önemli Fonksiyonlar:**
    *   `SetCategory(enum)`: Seçilen kategoriye göre (Saç, Göz) ilgili listeyi UI'a gönderir.
    *   `SelectSkinColor(int)`, `SelectHair(int)`: Seçilen öğeyi karakter üzerindeki `Image` bileşenine uygular.

### 🎛️ B. `DynamicCategoryManager.cs` (UI Yöneticisi)
Kullanıcı arayüzünü dinamik olarak yönetir.
*   **Görevi:**
    *   **Grid Population:** `OptionGrid` içine butonları (`OptionItem`) spawn eder.
    *   **Folder Scanning:** `Resources` klasöründeki alt klasörleri tarayarak kategori butonlarını yaratır (Örn: Hair -> Boy, Girl, Mixed).
    *   **Tone Slider:** Renk tonunu (Açık/Koyu) HSV manipülasyonu ile ayarlar (`AdjustColorTone`).
*   **Önemli Fonksiyonlar:**
    *   `PopulateCategoryButtons()`: Klasör yapısına göre buton üretir.
    *   `PopulateOptionGrid()`: Seçilen klasördeki resimleri ızgaraya dizer.
    *   `ApplyTone()`: Slider değerine göre rengin parlaklığını (V - Value) değiştirir.

### ✋ C. `CharacterDrag.cs` (Etkileşim)
Karakterin sahne içinde hareket etmesini sağlar.
*   **Görevi:** Mouse ile tut-sürükle (Drag & Drop) mantığını işletir.
*   **Sınırlama:** `Background` isimli objenin `BoxCollider2D` sınırları dışına çıkmayı engeller (`Mathf.Clamp`).
*   **Kamera:** Sürükleme başladığında kamerayı karaktere odaklar (`CameraFollowing`).

---

## 📂 3. Veri Akışı (Data Flow)

Sistem **"Resource-Based"** bir yapı kullanmaktadır. Yani veritabanı yerine klasör yapısına güvenir.

`Assets/Resources/Images/Character/Style/`
├── `Skin_Image`       -> Deri renkleri
├── `Hair_Image`       -> Alt klasörler: `BoyHair`, `GirlHair`
├── `Outfit`           -> Sakal, Göz, Kaş vb.
└── `Accessories`      -> Aksesuarlar

*   **Avantajı:** Yeni bir saç eklemek için kod yazmaya gerek yok. Klasöre resmi atmak yeterli.
*   **Dezavantajı:** Çok fazla dosya olduğunda oyunun açılış süresini (Resource Indexing) uzatabilir. (İleride Addressables'a geçilebilir).

---

## 🚀 4. Öneri & İyileştirmeler

1.  **Hardcoded Strings:** Scriptlerde `"Images/Character/Style/..."` gibi dosya yolları elle yazılmış. Klasör adı değişirse sistem çöker. Bunlar `const string` olarak bir Config dosyasında tutulmalı.
2.  **Performance:** `Resources.LoadAll` işlemi pahalıdır (ağır). Bu işlem sadece oyun açılışında (`Start`) bir kere yapılıyor, bu doğru bir yaklaşım. Ancak mobil cihazlarda bellek şişmesine dikkat edilmeli.
3.  **Hiyerarşi Bağlılığı:** `CharacterCreationManager`, karakterin parçalarını bulmak için `transform.Find("Hair")` gibi isimle arama yapıyor. Eğer prefab içindeki objenin adını değiştirirseniz kod çalışmaz. Buna dikkat edin.
