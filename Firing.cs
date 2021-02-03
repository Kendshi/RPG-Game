using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// класс для генерации снарядов
public class Firing : MonoBehaviour
{
    public GameObject projectilePrefab; // префаб снаряда
    public float fireRate;              // скорость стрельбы
    public bool isFiring;               // флажок стрельбы

    public float timeToFire { get { return _timeToFire; } }

    private Transform transformComp;    // transform
    private float _timeToFire;          // минимальное время между выстрелами

    void Start()
    {
        transformComp = transform;
    }

    void Update()
    {
        if (isFiring && Time.time >= _timeToFire)
        {
            _timeToFire = Time.time + 1 / fireRate;
            Instantiate(projectilePrefab, transformComp.position, transformComp.rotation);  // спаун снаряда
            isFiring = false;
        }
    }
}
