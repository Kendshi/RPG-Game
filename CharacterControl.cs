using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    [Tooltip("скорость перемещения персонажа")]
    [SerializeField] private float speed = 5f;   // скорость перемещения персонажа

    private Rigidbody body;                 // физическое тело игрока
    private Vector3 inputs;                 // вектор движения получаемый от нажатий кнопок управления игроком
    private Transform playerTransform;      // трансформ нашего персонажа
    private Camera mainCamera;              // основная камера
    private Transform mainCameraTransform;  // transform основной камеры
    private Vector3 moveGlobal;             // вектор перемещения
    private Vector3 lookDirection;          // вектор направления взгляда персонажа
    private Animator animator;              // аниматор
    private bool OnOffAttack;               // переключаем возможность двигатся во время атаки

    void Start()
    {
        inputs = Vector3.zero;                          // обнуляем перед началом игры на всякий случай
        body = GetComponent<Rigidbody>();               //Получаем компонент Rigidbody
        playerTransform = GetComponent<Transform>();    // получаем transform
        mainCamera = FindObjectOfType<Camera>();        // НАходим на сцене главную камеру
        mainCameraTransform = mainCamera.transform;     //Получаем трансформ главной камеры для оптимизации
        animator = GetComponent<Animator>();            //Получаем Аниматор
        OnOffAttack = true;
    }

    void Update()
    {
        inputs.x = Input.GetAxis("Horizontal"); // получаем от игрока вектор перемещения по x кнопки A и D
        inputs.z = Input.GetAxis("Vertical");   // получаем от игрока вектор перемещения по y кнопки W и S

        float cameraYRotation = mainCameraTransform.eulerAngles.y;                  // угол поворота камеры вокруг оси Y  
        moveGlobal = Quaternion.AngleAxis(cameraYRotation, Vector3.up) * inputs;    // перемещение персонажа

        RaycastHit hit;
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);                   // создаем луч который выходит из камеры и стремится к позиции курсора
        Physics.Raycast(cameraRay, out hit);                                                // получаем луч
        lookDirection = new Vector3(hit.point.x, playerTransform.position.y, hit.point.z);  // направление взгляда персонажа

        Vector3 moveLocal = playerTransform.InverseTransformDirection(moveGlobal); // преобразование глобальных координат перемещения персонажа в локальные
        animator.SetFloat("Forward", moveLocal.z);   // анимация движения впепёд / назад
        animator.SetFloat("Right", moveLocal.x);     // анимация движения влево / вправо

        if (Input.GetMouseButtonDown(0))            // Отслеживаем нажатие игроком кнопки атаки
        {
            OnOffAttack = false;                    //Переключаем переменную чтобы персонаж не двигался пока длится атака
            if (co != null)                         // Останавливаем карутину если она уже была запущена
            {
                StopCoroutine(co);
            }
            co = StartCoroutine(Attack());
        }
    }

    private void FixedUpdate()
    {
        if (OnOffAttack)
            body.MovePosition(body.position + moveGlobal * speed * Time.fixedDeltaTime);    // перемещение персонажа

        playerTransform.LookAt(lookDirection);                                              // поворот персонажа
    }
    /// <summary>
    /// Корутина анимации атаки
    /// </summary>
    /// <returns></returns>
    IEnumerator Attack()
    {
        animator.SetTrigger("Attack");          // включаем анимацию атаки
        yield return new WaitForSeconds(1.4f);  // ждем пока анимация проиграет
        OnOffAttack = true;                     // Возвращаем игроку управление
    }
    Coroutine co; //инициалзируем контейнер для корутины
}
