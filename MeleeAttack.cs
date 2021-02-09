using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// класс атаки ближнего боя
public class MeleeAttack : MonoBehaviour
{
    public event Action OnAttackStart;          // событие начала атаки
    public event Action OnAttackStop;           // событие конца атаки

    public float AttackFrequency = 1;           // частота атаки в секундах (для тестирования)
    public bool IsRandomAnimation = false;      // флажок случайных анимаций атаки
    public bool IsComboEnabled = true;          // флажок разрешённых комбо-атак

    private Animator animator;                  // аниматор
    private List<string> attackAnimList;        // список анимаций атаки
    private Collider weaponCollider;            // коллайдер урона на оружии
    private int animationIndex;                 // индекс анимации в списке
    private bool isAttackInQueue;               // флажок очереди атак
    private bool isAttackCoRunning;             // флажок запущенной корутины атаки

    private void Start()
    {
        animator = GetComponent<Animator>();    // получение Аниматора
        isAttackInQueue = false;                // обнуление очереди атак
        isAttackCoRunning = false;              // флажок запущенной корутины атаки выключен
        attackAnimList = new List<string>();    // инициализация списка названий анимаций атаки
        attackAnimList.Add("Attack");           // получение списка названий анимаций атаки
        attackAnimList.Add("Attack2");
        attackAnimList.Add("Attack3");
        UpdateAnimationSpeed();                 // обновление скорости анимации атаки
    }

    /// <summary>
    /// Инициализация атаки
    /// </summary>
    public void InitAttack(Collider _weaponCollider)
    {
        weaponCollider = _weaponCollider;
        if (isAttackCoRunning)      // проверка запущенной корутины атаки
        {
            isAttackInQueue = IsComboEnabled;                   // установка флажка очереди атак
        }
        else
        {
            isAttackInQueue = true;                             // установка флажка очереди атак
            attackCo = StartCoroutine(Attack(weaponCollider));  // вызов корутины атаки
        }
    }


    /// <summary>
    /// Остановка атаки
    /// </summary>
    public void AttackStop()
    {
        weaponCollider.enabled = false; // отключение коллайдера урона на оружии
        if (isAttackCoRunning)          // проверка запущенной корутины атаки
            StopCoroutine(attackCo);    // остановка корутины атаки
        foreach (string animName in attackAnimList)
            animator.ResetTrigger(animName);    // сброс триггеров анимации атаки
    }

    /// <summary>
    /// Обновление скорости анимации атаки
    /// </summary>
    public void UpdateAnimationSpeed()
    {
        animator.SetFloat("AttackSpeedMultiplier", 1 / AttackFrequency);    // множитель скорости атаки
    }

    /// <summary>
    /// Метод, вызывамый в начале фазы удара по событию анимации атаки
    /// </summary>
    public void HitStartAnimEvent()
    {
        weaponCollider.enabled = true;      // включение коллайдера урона на оружии
    }

    /// <summary>
    /// Метод, вызывамый в конце фазы удара по событию анимации атаки
    /// </summary>
    public void HitStopAnimEvent()
    {
        weaponCollider.enabled = false;     // отключение коллайдера урона на оружии
    }

    /// <summary>
    /// Обновление индекса анимации из списка
    /// </summary>
    private void UpdateAnimationIndex(bool defaultAnim = false)
    {
        if (defaultAnim)
            animationIndex = IsRandomAnimation ? UnityEngine.Random.Range(0, attackAnimList.Count) : 0; // установка индекса анимации по умолчанию
        else
        {
            if (IsRandomAnimation)      // проверка флажка случайных анимаций атаки
                animationIndex = UnityEngine.Random.Range(0, attackAnimList.Count); // установка случайного индекса анимации
            else if (animationIndex < attackAnimList.Count - 1)
                animationIndex++;       // обновление индекса анимации
            else
                animationIndex = 0;     // обновление индекса анимации
        }
    }

    /// <summary>
    /// Корутина анимации атаки
    /// </summary>
    /// <returns></returns>
    IEnumerator Attack(Collider weaponCollider)
    {
        isAttackCoRunning = true;       // флажок запущенной корутины включен
        UpdateAnimationIndex(true);     // установка индекса анимации по умолчанию

        while (isAttackInQueue)         // проверка счётчика очереди атак
        {
            OnAttackStart?.Invoke();    // вызов события начала атаки
            animator.SetTrigger(attackAnimList[animationIndex]);    // запуск анимации атаки
            isAttackInQueue = false;                                // сброс флажка очереди атак
            yield return new WaitForSeconds(AttackFrequency);       // ожидание окончания анимации
            UpdateAnimationIndex();     // обновление индекса анимации
            OnAttackStop?.Invoke();     // вызов события конца атаки
        }

        isAttackCoRunning = false;      // флажок запущенной корутины отключен
    }
    Coroutine attackCo; //инициалзация контейнера для корутины
}
