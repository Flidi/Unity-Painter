using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class PaintableScript : MonoBehaviour
{
    Camera mcam;
    RenderTexture mtexture;
    GameObject cameraobj;
    public Shader decalmaskshader; // Шейдер для маски декалю

    // Об'єкт для зберігання дій малювання
    private PaintActions paintActions = new PaintActions();

    public DefaultAsset saveFolder; // Папка для зберігання

    void Start()
    {
        // Ініціалізація камери і текстури для малювання
        cameraobj = new GameObject("cameraobj");
        mcam = cameraobj.AddComponent<Camera>();
        mtexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);

        mtexture.filterMode = FilterMode.Bilinear;
        mcam.enabled = false;
        mcam.targetTexture = mtexture;
        mcam.cullingMask = (1 << LayerMask.NameToLayer("paintablelayer")); // Встановлення маски шару для камери
        mcam.orthographic = true;
        cameraobj.transform.position = new Vector3(0f, 0f, -1f);
        cameraobj.transform.rotation = Quaternion.identity;
        GetComponent<Renderer>().material.SetTexture("_masktex", mtexture);
        GetComponent<Renderer>().material.SetTexture("_MainTex", mtexture);
        mcam.SetReplacementShader(decalmaskshader, ""); // Встановлення замінного шейдера для камери
        mcam.clearFlags = CameraClearFlags.Nothing;
        mcam.orthographicSize = 10f;
        mcam.allowMSAA = false;
    }

    // Функція для нанесення декалю
    public void PaintDecal(Vector3 position, float brushSize, Color brushColor)
    {
        // Конвертація позиції в локальну систему координат об'єкта
        position = position - transform.position;
        Vector2 uv = new Vector2(position.x / transform.localScale.x + 0.5f, position.y / transform.localScale.y + 0.5f);

        GetComponent<Renderer>().enabled = true; // Включення візуалізації об'єкта
        // Передача параметрів в шейдер
        GetComponent<Renderer>().material.SetFloat("_hitposx", position.x);
        GetComponent<Renderer>().material.SetFloat("_hitposy", position.y);
        GetComponent<Renderer>().material.SetFloat("_hitposz", position.z);
        GetComponent<Renderer>().material.SetFloat("_BrushSize", brushSize);
        brushColor.a = 1.0f; // Забезпечення повної непрозорості для кольору пензлика
        GetComponent<Renderer>().material.SetColor("_BrushColor", brushColor);
        GetComponent<Renderer>().material.SetVector("_BrushUV", new Vector4(uv.x, uv.y, 0f, 0f)); // Передача UV-координат в шейдер

        Vector3 mposition = transform.position;
        transform.position = Vector3.zero;
        mcam.Render(); // Виклик рендерингу заміненим шейдером
        transform.position = mposition;
        GetComponent<Renderer>().enabled = false; // Вимкнення візуалізації об'єкта

        // Збереження дії малювання для подальшого відтворення
        PaintAction action = new PaintAction
        {
            position = position,
            brushSize = brushSize,
            brushColor = brushColor,
            uv = uv // Збереження UV-координат
        };
        paintActions.actions.Add(action);
    }

    // Функція для очищення малюнків
    public void Clear()
    {
        paintActions.actions.Clear(); // Очищення списку дій малювання
        // Очищення текстури
        RenderTexture.active = mtexture;
        GL.Clear(true, true, Color.clear);
        // RenderTexture.active = null;
    }

    // Клас для зберігання дій малювання
    [System.Serializable]
    public class PaintAction
    {
        public Vector3 position; // Позиція дії малювання
        public float brushSize; // Розмір пензлика
        public Color brushColor; // Колір пензлика
        public Vector2 uv; // UV-координати для шейдера
    }

    // Клас для зберігання списку дій малювання
    [System.Serializable]
    public class PaintActions
    {
        public List<PaintAction> actions = new List<PaintAction>(); // Список дій малювання
    }
}
