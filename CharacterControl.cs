using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    [Tooltip("скорость перемещения персонажа")]
    [SerializeField] private float Speed = 5f;          // скорость перемещения персонажа
    [Tooltip("слот 1 оружия ближнего боя")]
    [SerializeField] private GameObject WeaponSlot1;    // слот 1 оружия ближнего боя

    private Rigidbody body;                 // физическое тело игрока
    private Vector3 inputs;                 // вектор движения получаемый от нажатий кнопок управления игроком
    private Transform playerTransform;      // трансформ нашего персонажа
    private Camera mainCamera;              // основная камера
    private Transform mainCameraTransform;  // transform основной камеры
    private Vector3 moveGlobal;             // вектор перемещения
    private Quaternion lookRotation;        // направление взгляда персонажа
    private Animator animator;              // аниматор

    private MeleeAttack meleeAttack;        // компонент атаки в ближнем бою
    private GameObject melee1WeaponObj;     // объект оружия ближнего боя из слота 1
    private Collider melee1WeaponCollider;  // коллайдер оружия ближнего боя из слота 1

    void Start()
    {
        inputs = Vector3.zero;                          // обнуление ввода перед началом игры на всякий случай
        body = GetComponent<Rigidbody>();               // получение компонента Rigidbody
        playerTransform = GetComponent<Transform>();    // получение Трансформа персонажа
        mainCamera = FindObjectOfType<Camera>();        // находим на сцене главную камеру
        mainCameraTransform = mainCamera.transform;     // получение Трансформа главной камеры для оптимизации
        animator = GetComponent<Animator>();            // получение Аниматора

        if (WeaponSlot1.transform.childCount > 0)       // проверка наличия дочерних компонентов в слоте 1 оружия ближнего боя
        {
            melee1WeaponObj = WeaponSlot1.transform.GetChild(0).gameObject;             // получаем объект оружия ближнего боя из слота 1
            melee1WeaponCollider = melee1WeaponObj.GetComponentInChildren<Collider>();  // получение коллайдера для нанесения урона
        }

        meleeAttack = GetComponent<MeleeAttack>();
    }

    void Update()
    {
        inputs.x = Input.GetAxis("Horizontal"); // получаем от игрока вектор перемещения по x кнопки A и D
        inputs.z = Input.GetAxis("Vertical");   // получаем от игрока вектор перемещения по y кнопки W и S

        float cameraYRotation = mainCameraTransform.eulerAngles.y;          // угол поворота камеры вокруг оси Y  
        RaycastHit hit;
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);   // создаем луч который выходит из камеры и стремится к позиции курсора
        Physics.Raycast(cameraRay, out hit);                                // получаем луч
        lookRotation = Quaternion.LookRotation((new Vector3(hit.point.x, 0, hit.point.z) - new Vector3(playerTransform.position.x, 0, playerTransform.position.z)).normalized);

        moveGlobal = Quaternion.AngleAxis(cameraYRotation, Vector3.up) * inputs;    // перемещение персонажа
        Vector3 moveLocal = playerTransform.InverseTransformDirection(moveGlobal);  // преобразование глобальных координат перемещения персонажа в локальные
        animator.SetFloat("Forward", moveLocal.z);                                  // анимация движения вперёд / назад
        animator.SetFloat("Right", moveLocal.x);                                    // анимация движения влево / вправо
        

        if (Input.GetMouseButtonDown(0) && meleeAttack != null) // отслеживание нажатия игроком кнопки атаки и проверка комонента атаки ближнего боя
        {
            moveGlobal = Vector3.zero;                          // обнуление перемещения персонажа
            meleeAttack.InitAttack(melee1WeaponCollider);       // инициализация атаки
        }
    }

    private void FixedUpdate()
    {
        body.MovePosition(body.position + moveGlobal * Speed * Time.fixedDeltaTime);                                // перемещение персонажа
        playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, lookRotation, 5f * Time.deltaTime);   // поворот персонажа
    }
}
