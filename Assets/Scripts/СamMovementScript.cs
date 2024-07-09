using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class СamMovementScript : MonoBehaviour
{
    GameObject rotatex; // Об'єкт для обертання камери по осі X

    Vector2 prevpos; // Попередня позиція миші
    Vector2 currentpos; // Поточна позиція миші

    public float xrotatescale; // Масштабування обертання по осі X
    public float yrotatescale; // Масштабування обертання по осі Y

    public float movespeed = 5f; // Швидкість руху камери

    Vector2 dummymousepos; // Фіктивна позиція миші для обробки вводу

    bool isAltPressed = false; // Прапорець, що вказує на те, чи натиснута клавіша Alt

    // Викликається один раз при запуску скрипта
    void Start()
    {
        // Ініціалізуємо фіктивну позицію миші відповідно до введення миші
        dummymousepos.x += Input.GetAxis("Mouse X");
        dummymousepos.y += Input.GetAxis("Mouse Y");

        currentpos = dummymousepos;
        prevpos = currentpos;

        // Отримуємо об'єкт для обертання по осі X
        rotatex = transform.GetChild(0).gameObject;

        // Блокуємо курсор та робимо його невидимим
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Оновлюється кожен кадр
    void Update()
    {
        // Перевіряємо, чи натиснута клавіша Alt
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            isAltPressed = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Перевіряємо, чи відпущена клавіша Alt
        if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            isAltPressed = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Оновлюємо рух камери, якщо не натиснута клавіша Alt
        if (!isAltPressed)
        {
            // Отримуємо ввід від миші і масштабуємо його
            dummymousepos.x += Input.GetAxis("Mouse X") * 10f;
            dummymousepos.y += Input.GetAxis("Mouse Y") * 10f;

            currentpos = dummymousepos;

            // Нормалізуємо позиції миші відносно розмірів екрану
            Vector2 screensize = new Vector2((float)Screen.width, (float)Screen.height);
            Vector2 currentpos01 = currentpos;
            Vector2 prevpos01 = prevpos;
            currentpos01.x /= screensize.x;
            currentpos01.y /= screensize.y;
            prevpos01.x /= screensize.x;
            prevpos01.y /= screensize.y;

            // Обчислюємо зміну позиції миші
            Vector2 delta01 = currentpos01 - prevpos01;

            // Обертаємо камеру по вісі X і об'єкт rotatex по вісі Y
            transform.Rotate(Vector3.up * delta01.x * xrotatescale);
            rotatex.transform.Rotate(Vector3.right * -delta01.y * yrotatescale);

            // Очищуємо фіктивну позицію миші
            dummymousepos = Vector2.zero;
            currentpos = dummymousepos;

            prevpos = currentpos;

            // Рухаємо камеру вперед, назад, вліво або вправо відповідно до натиснутих клавіш
            if (Input.GetKey(KeyCode.W))
            {
                transform.position = transform.position + rotatex.transform.forward * Time.deltaTime * movespeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position = transform.position + rotatex.transform.forward * Time.deltaTime * -movespeed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position = transform.position + rotatex.transform.right * Time.deltaTime * -movespeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position = transform.position + rotatex.transform.right * Time.deltaTime * movespeed;
            }
        }
    }
}
