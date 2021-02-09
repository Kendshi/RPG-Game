using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;           //ссылка на экземпляр класс SO параметров НПС
    private NavMeshAgent agent;                             
    private Animator animator;
    [SerializeField] private float health;                  //Здоровье персонажа. Если падает > 0 то персонаж умирает
    [SerializeField] private float stamina;                 //Выносливость, без нее нельзя наносить удары, со временем восстанавливается
    [SerializeField] private float distanceToAttack;        //Расстояние до цели на котором НПС нападает (не путать! с растоянием до цели на ктотором НПС должен остановится)
    [SerializeField] private Transform Target;              //Цель к которй стремится НПС чтобы атаковать, это не обязательно должен быть игрок, но по умолчанию это он.
    [SerializeField] private float staminaRecoverySpeed;    //множитель влияющий на скорость восстановления выносливости
    [SerializeField] private Collider weaponCollider;       //Коллайдер оружия для нанесения урона игроку или другим НПС
    [SerializeField] private GameObject rightHand;          //косточка правой руки персонажа в которой рсполагается оружие
    private MeleeAttack meleeAttack;                        // компонент атаки в ближнем бою
    private float staminaCost;                              //значение которое отнимается от выносливости за 1 удар
    private bool isAttackEnabled;                           //Переключатель корутины, не дает ей запустится больше 1 раза одномоментно
    private bool deathOn;                                   //вспомогательный переключатель не дающий включится методу Death больше 1 раза
    
    void Start()
    {
        if (Target == null)
        { // Ищем объект с тэгом игрок и присваиваем его трансформ Target, для того чтобы преследовать его
            Target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>(); 
        }
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        //далее идет присвоение значений параметров их эталонам в SO
        health = enemyData.health;
        stamina = enemyData.stamina;
        staminaRecoverySpeed = enemyData.staminaRecoverySpeed;
        staminaCost = enemyData.staminaCostToAttack;
        weaponCollider = rightHand.GetComponentInChildren<Collider>();  // получаем коллайдер оружия
        weaponCollider.enabled = false;                                 // по умолчанию выключен во избежание случайных срабатываний
        isAttackEnabled = false;                                        // Выключаем на старте чтобы ложно не срабатывала анимация атаки
        deathOn = true;
        meleeAttack = GetComponent<MeleeAttack>();                      // компонент атаки в ближнем бою
        if (meleeAttack != null)
            meleeAttack.OnAttackStop += AttackEnable;                   // обработчик события конца атаки
        Invoke(nameof(AttackEnable), 2f);                               // через 2 секунды после старта возвращаем НПС возможность атаковать
    }

    
    void Update()
    {
        if (!HasReached())
        {   //включаем анимацию бега во время движения
            animator.SetBool("RunOn", true);
        }
        else animator.SetBool("RunOn", false);

        if (Target != null)
        {   //Указываем НПС куда ему следует двигаться
            agent.SetDestination(Target.position);
            FaceTarget();
        }
        // условие при котоором происходит регенерация выносливости
        if (stamina < enemyData.stamina)
        {
            stamina += Time.deltaTime * staminaRecoverySpeed;
        }
        //Условия при которых начинается атака
        if (HasReached() && stamina > staminaCost)
        {
            if (meleeAttack != null && isAttackEnabled && deathOn)
            {
                Debug.Log("InitAtack");
                isAttackEnabled = false;
                stamina -= staminaCost; // отнимаем стоимость атаки от общей выносливости
                meleeAttack.InitAttack(weaponCollider); // инициализация атаки
            }
        }
        // Условие при котором НПС умирает
        if (health <= 0 && deathOn)
        {
            deathOn = false;
            Death();
        }
    }
    /// <summary>
    /// Метод разрешает атаку НПС
    /// </summary>
    private void AttackEnable()
    {
        isAttackEnabled = true;
    }

    /// <summary>
    /// Метод высчитывающий когда можно считать, что НПС достиг своей цели
    /// </summary>
    /// <returns></returns>
    private bool HasReached()
    {
        if (agent.remainingDistance < distanceToAttack)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Метод Разворачивающий НПС лицом к цели
    /// </summary>
    private void FaceTarget()
    {
        Vector3 direction = (Target.position - transform.position).normalized;
        Quaternion lookRotatin = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotatin, Time.deltaTime * 5f);
    }
    
    private void OnTriggerEnter(Collider other)
    {   //Если по нам попали чем-то угрожающем то отнимаем жизни
        if (other.gameObject.CompareTag("Danger"))
        {
            health -= 1;
        }
    }

    /// <summary>
    /// Метод описывающий смерть НПС
    /// </summary>
    private void Death()
    {
        isAttackEnabled = false;            //Блокируем возможность атаковать
        meleeAttack.AttackStop();           // Остановка атаки
        Target = null;                      //Больше некого преследовать
        animator.SetTrigger("DeathOn");     //Включаем анимацию смерти
        Destroy(gameObject, 5f);            //Удаляем НПС
    }

    // Вызывается при уничтожении компонента
    private void OnDestroy()
    {
        meleeAttack.OnAttackStop -= AttackEnable;   // отписаться от событий
    }
}
