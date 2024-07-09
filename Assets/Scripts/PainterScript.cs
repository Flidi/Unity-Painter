using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class PainterScript : MonoBehaviour
{
    [System.Serializable]
    public class PaintAction
    {
        public Vector3 position; // Позиція, де було зроблено малюнок
        public float brushSize; // Розмір пензлика
        public Color brushColor; // Колір пензлика
        public Vector2 uv; // UV-координати місця малюнка

        // Порівняння об'єктів PaintAction на основі їхніх властивостей.
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PaintAction other = (PaintAction)obj;
            return position == other.position &&
                   brushSize == other.brushSize &&
                   brushColor.Equals(other.brushColor) &&
                   uv == other.uv;
        }

        // Хеш-код для швидкого порівняння об'єктів при використанні в структурах даних
        public override int GetHashCode()
        {
            return position.GetHashCode() ^ brushSize.GetHashCode() ^ brushColor.GetHashCode() ^ uv.GetHashCode();
        }
    }

    [System.Serializable]
    public class PaintActionWrapper
    {
        public List<PaintAction> paintActions = new List<PaintAction>(); // Список дій малювання
    }

    RenderTexture mtexture; // Текстура для малювання
    List<PaintAction> paintActions = new List<PaintAction>(); // Список дій малювання
    PaintableScript[] paintableobjects; // Масив об'єктів, які можна малювати
    public Shader replacementshadertest; // Тестовий замінний шейдер
    public float brushSize = 1.0f; // Початковий розмір пензлика
    public Color brushColor = Color.red; // Початковий колір пензлика
    public DefaultAsset saveFolder; // Папка для зберігання JSON

    float lastPaintTime = 0f; // Час останнього малювання
    public float minPaintInterval = 0.1f; // Мінімальний інтервал між малюванням, в секундах

    Dictionary<PaintableScript, Material[]> originalMaterials = new Dictionary<PaintableScript, Material[]>(); // Оригінальні матеріали для кожного PaintableScript

    void Start()
    {
        paintableobjects = GameObject.FindObjectsOfType<PaintableScript>(); // Знаходження усіх PaintableScript в сцені
        Camera.main.SetReplacementShader(replacementshadertest, ""); // Встановлення замінного шейдера для головної камери

        // Зберегти оригінальні матеріали для кожного PaintableScript
        foreach (PaintableScript paintable in paintableobjects)
        {
            originalMaterials[paintable] = paintable.GetComponent<Renderer>().materials;
        }
    }

    void Update()
    {
        disablepaintableobjects(); // Вимкнення відображення PaintableScript

        // Перевірка на зажату ліву кнопку миші і відсутність зажатої кнопки Alt
        if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f)); // Створення луча з позиції миші
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                PaintableScript paintablenow = hit.transform.GetComponent<PaintableScript>();
                if (paintablenow != null)
                {
                    // Перевірка, чи пройшов достатній час з останнього малювання
                    if (Time.time - lastPaintTime >= minPaintInterval)
                    {
                        paintablenow.PaintDecal(hit.point, brushSize, brushColor); // Виклик методу малювання на PaintableScript

                        // Логування дії малювання
                        PaintAction action = new PaintAction();
                        action.position = hit.point;
                        action.brushSize = brushSize;
                        action.brushColor = brushColor;
                        action.uv = hit.textureCoord; // Збереження UV-координат

                        paintActions.Add(action); // Додавання дії в список

                        // Оновлення часу останнього малювання
                        lastPaintTime = Time.time;

                        // Додатковий вивід у консоль для дебагу
                        //Debug.Log($"Paint action added: Position = {action.position}, Size = {action.brushSize}, Color = {action.brushColor}");
                    }
                }
            }
        }

        enablepaintableobjects(); // Увімкнення відображення PaintableScript
    }

    // Вимкнення відображення PaintableScript
    void disablepaintableobjects()
    {
        foreach (PaintableScript paintable in paintableobjects)
        {
            paintable.GetComponent<Renderer>().enabled = false;
        }
    }

    // Увімкнення відображення PaintableScript
    void enablepaintableobjects()
    {
        foreach (PaintableScript paintable in paintableobjects)
        {
            paintable.GetComponent<Renderer>().enabled = true;
        }
    }

    // Збереження дій малювання у форматі JSON
    public void SavePaintActions()
    {
        if (saveFolder == null)
        {
            Debug.LogError("Save folder not specified!");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(saveFolder); // Отримання шляху до папки для зберігання
        string filePath = Path.Combine(folderPath, "paint_actions.json"); // Формування шляху до файлу JSON

        PaintActionWrapper wrapper = new PaintActionWrapper();
        wrapper.paintActions = GetUniquePaintActions(paintActions); // Отримання унікальних дій малювання

        string json = JsonUtility.ToJson(wrapper); // Конвертація в JSON
        File.WriteAllText(filePath, json); // Запис JSON у файл

        Debug.Log("Paint actions saved to: " + filePath);
    }

    // Отримання унікальних дій малювання
    private List<PaintAction> GetUniquePaintActions(List<PaintAction> actions)
    {
        List<PaintAction> uniqueActions = new List<PaintAction>();

        foreach (var action in actions)
        {
            if (!uniqueActions.Contains(action))
            {
                uniqueActions.Add(action);
            }
        }

        return uniqueActions;
    }

    // Очищення всіх дій малювання
    public void ClearPaintActions()
    {
        // Очищення дій малювання на усіх PaintableScript
        foreach (PaintableScript paintable in paintableobjects)
        {
            paintable.Clear();
        }

        // Очищення текстури малювання (якщо потрібно)
        if (mtexture != null)
        {
            RenderTexture.active = mtexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = null;
        }
    }

    // Відтворення дій малювання з файлу JSON
    public void ReplayPaintActions()
    {
        string folderPath = AssetDatabase.GetAssetPath(saveFolder); // Отримання шляху до папки збереження
        string filePath = Path.Combine(folderPath, "paint_actions.json"); // Формування шляху до файлу JSON

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath); // Читання JSON з файлу
            PaintActionWrapper wrapper = JsonUtility.FromJson<PaintActionWrapper>(json); // Десеріалізація JSON

            foreach (PaintAction action in wrapper.paintActions)
            {
                foreach (PaintableScript paintable in paintableobjects)
                {
                    paintable.PaintDecal(action.position, action.brushSize, action.brushColor); // Відтворення дії малювання на кожному PaintableScript
                }
            }

            Debug.Log("Paint actions replayed.");
        }
        else
        {
            Debug.Log("No saved paint actions found.");
        }
    }
}
