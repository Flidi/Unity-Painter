using UnityEngine;
using UnityEngine.UI;

public class BrushSettingsUI : MonoBehaviour
{
    // Поля для зв'язку з елементами інтерфейсу
    public Slider brushSizeSlider;         // Слайдер для розміру пензлика
    public Slider redSlider;               // Слайдер для компоненти червоного кольору
    public Slider greenSlider;             // Слайдер для компоненти зеленого кольору
    public Slider blueSlider;              // Слайдер для компоненти синього кольору
    public Image brushColorImage;          // Зображення кольору пензлика

    public PainterScript painterScript;    // Посилання на скрипт PainterScript для зміни параметрів пензлика

    void Start()
    {
        // Додаємо слухачів подій для слайдерів розміру та кольору пензлика
        brushSizeSlider.onValueChanged.AddListener(OnBrushSizeChanged);
        redSlider.onValueChanged.AddListener(OnColorChanged);
        greenSlider.onValueChanged.AddListener(OnColorChanged);
        blueSlider.onValueChanged.AddListener(OnColorChanged);

        // Встановлюємо початкові значення слайдерів на основі поточних параметрів з PainterScript
        SetInitialValues();
    }

    // Метод для встановлення початкових значень слайдерів
    void SetInitialValues()
    {
        // Встановлюємо значення слайдерів розміру та компонент кольору з PainterScript
        brushSizeSlider.value = painterScript.brushSize;
        redSlider.value = painterScript.brushColor.r;
        greenSlider.value = painterScript.brushColor.g;
        blueSlider.value = painterScript.brushColor.b;

        // Оновлюємо відображення кольору пензлика та його розміру
        OnBrushSizeChanged(brushSizeSlider.value);
        OnColorChanged(0);
    }

    // Метод, що викликається при зміні розміру пензлика через слайдер
    void OnBrushSizeChanged(float value)
    {
        // Змінюємо розмір пензлика в PainterScript
        painterScript.brushSize = value;
    }

    // Метод, що викликається при зміні кольору пензлика через слайдери RGB
    void OnColorChanged(float value)
    {
        // Створюємо новий кольор зі значень RGB слайдерів
        Color newColor = new Color(redSlider.value, greenSlider.value, blueSlider.value);
        // Змінюємо кольору пензлика в PainterScript
        painterScript.brushColor = newColor;
        // Оновлюємо зображення кольору пензлика в інтерфейсі
        UpdateBrushColorImage();
    }

    // Метод для оновлення зображення кольору пензлика в інтерфейсі
    void UpdateBrushColorImage()
    {
        // Встановлюємо кольору зображення на основі поточних значень RGB слайдерів
        brushColorImage.color = new Color(redSlider.value, greenSlider.value, blueSlider.value);
    }
}
