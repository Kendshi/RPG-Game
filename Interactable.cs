using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Интерактивный объект
/// </summary>
public class Interactable : MonoBehaviour
{
    [Tooltip("Флажок активности")]
    public bool IsActive = true;
    [Tooltip("Радиус взаимодействия")]
    public float Radius = 5f;
    [Header("Сюда помещаем то действие которое хотим совершить")]
    public UnityEvent ActionEvent;

    private Outline outline;


    private void Start()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 2;
        outline.enabled = false;
    }

    /// <summary>
    /// Виртуальный метод взаимодействия
    /// </summary>
    protected virtual void Interact(GameObject character)
    {
        Debug.Log("Interactable : Interact with " + gameObject.name);
        ActionEvent.Invoke();
    }

    /// <summary>
    /// Иницализация взаимодействия
    /// </summary>
    public void InitInteraction(GameObject character)
    {
        if (IsActive)   // проверка флажка активности
        {
            if (Vector3.Distance(character.transform.position, gameObject.transform.position) <= Radius)    // проверка расстояния до персонажа
            {
                Interact(character); // взаимодействие
            }
        }
    }

    private void OnMouseEnter()
    {                           
        outline.enabled = true;     //Если наводим мышку то подсвечиваем контур объекта
    }

    private void OnMouseExit()
    {
        outline.enabled = false;    //Если убираем мышку подсветка исчезает
    }
}
