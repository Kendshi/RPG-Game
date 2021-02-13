using UnityEngine;
using InventorySystem;
using UnityEngine.AI;
using System.Collections;

public class CharacterControl : MonoBehaviour
{
    [Tooltip("скорость перемещения персонажа")]
    [SerializeField] private float Speed = 5f;          // скорость перемещения персонажа
    [Tooltip("слот 1 оружия ближнего боя")]
    [SerializeField] private GameObject WeaponSlot1;    // слот 1 оружия ближнего боя
    [SerializeField] private float stopDistance = 3;    // дистанция до цели, которая означет, что персонаж достиг цели

    public CharacterControl characterControl { get; private set; } // синглтон

    private NavMeshAgent agent;             // NavMeshAgent игрока     
    private Rigidbody body;                 // физическое тело игрока
    private Vector3 inputs;                 // вектор движения получаемый от нажатий кнопок управления игроком
    private Transform playerTransform;      // трансформ нашего персонажа
    private Camera mainCamera;              // основная камера
    private Transform mainCameraTransform;  // transform основной камеры
    private Vector3 moveGlobal;             // вектор перемещения
    private Quaternion lookRotation;        // направление взгляда персонажа
    private Animator animator;              // аниматор
    private Inventory characterInventory;   // инвентарь персонажа
    private MeleeAttack meleeAttack;        // компонент атаки в ближнем бою
    private GameObject melee1WeaponObj;     // объект оружия ближнего боя из слота 1
    private Collider melee1WeaponCollider;  // коллайдер оружия ближнего боя из слота 1
    private GameObject clickedObj;          // Объект по которому происходит ЛКМ для дальнейшего поиска интерактивности
    private Interactable interactable;      // компонент класса Interactable на выделеном объекте
    private bool mouseInputBlock;        // блок поворота персонажа

    private void Awake()
    {
        if (!characterControl)
        {
            characterControl = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        inputs = Vector3.zero;                          // обнуление ввода перед началом игры на всякий случай
        body = GetComponent<Rigidbody>();               // получение компонента Rigidbody
        playerTransform = GetComponent<Transform>();    // получение Трансформа персонажа
        mainCamera = FindObjectOfType<Camera>();        // находим на сцене главную камеру
        mainCameraTransform = mainCamera.transform;     // получение Трансформа главной камеры для оптимизации
        animator = GetComponent<Animator>();            // получение Аниматора
        agent = GetComponent<NavMeshAgent>();           // Получение агента NavMesh

        if (WeaponSlot1.transform.childCount > 0)       // проверка наличия дочерних компонентов в слоте 1 оружия ближнего боя
        {
            melee1WeaponObj = WeaponSlot1.transform.GetChild(0).gameObject;             // получаем объект оружия ближнего боя из слота 1
            melee1WeaponCollider = melee1WeaponObj.GetComponentInChildren<Collider>();  // получение коллайдера для нанесения урона
        }

        meleeAttack = GetComponent<MeleeAttack>();
        characterInventory = GetComponent<Inventory>();
        mouseInputBlock = false;                        // блок поворота персонажа выключен
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

       
        if (Input.GetMouseButtonDown(0) && !mouseInputBlock)                        // отслеживание нажатия игроком кнопки ЛКМ
        {
            clickedObj = hit.collider.gameObject;
            interactable = clickedObj.GetComponent<Interactable>();                 // получение интерактивного объекта, на который нажали
            
            if (meleeAttack != null && interactable == null)                        // проверка компонента атаки ближнего боя, и запрет проведения атаки при клике на интер.объект
            {
                moveGlobal = Vector3.zero;                                          // обнуление перемещения персонажа
                meleeAttack.InitAttack(melee1WeaponCollider);                       // инициализация атаки
            }
        }

        if (interactable != null && interactable.IsActive)                      // проверка компонента предмета
        {
            agent.enabled = true;                                               // Включаем компонент NavMeshAgent
            agent.SetDestination(clickedObj.transform.position);                // Заставляем персонажа идти к интерактивному объекту

            if (!HasReached())
                animator.SetFloat("Forward", 1);                                // Пока идем включаем анимацию бега

            if (agent.remainingDistance > 1)                                    //Это условие нужно чтобы избежать преждевременного срабатывания метода HasReached
            {
                if (HasReached())
                {
                    interactable.InitInteraction(gameObject);                   //инициализируем взаимодействие с объектом
                    StopFollow();                                               //сбрасываем все действия связанные с интерактивным объектом
                }
            }
        }

        if (inputs != Vector3.zero)                                             //Если нажмем на любые кнопки движения то персонаж перестанет идти к интерактивному объекту
            StopFollow();
    }

    private void FixedUpdate()
    {
        body.MovePosition(body.position + moveGlobal * Speed * Time.fixedDeltaTime);                               // перемещение персонажа

        if (interactable == null    // Если идем к интерактивному объекту, то поворот мышкой запрещен
            && !mouseInputBlock)    // Проверка блока интерфейсом управления мышью
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, lookRotation, 5f * Time.deltaTime);   // поворот персонажа
    }

    /// <summary>
    /// Блок и разблокировка поворота персонажа
    /// </summary>
    public void MouseInputBlockChange(bool enable)
    {
        mouseInputBlock = enable;
    }

    /// <summary>
    /// Метод определяет достиг ли персонаж места назначения
    /// </summary>
    /// <returns></returns>
    private bool HasReached()
    {
        if (agent.remainingDistance < stopDistance)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Метод сбрасывает все действия по достижению интерактивного объекта
    /// </summary>
    private void StopFollow()
    {
        agent.enabled = false;
        clickedObj = null;
        interactable = null;
    }
}
