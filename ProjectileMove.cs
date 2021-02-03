using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// класс движение снаряда
public class ProjectileMove : MonoBehaviour
{
    public float speed;                 // скорость снаряда
    public GameObject muzzlePrefab;     // префаб эффекта выстрела
    public GameObject hitPrefab;        // префаб эффекта попадания;

    private Transform transformComp;    // transform
    private float maxDistance = 100;    // максимальная дистанция полёта снаряда
    private Vector3 startPosition;      // начальная позиция снаряда

    void Start()
    {
        transformComp = transform;
        if (muzzlePrefab != null)
        {
            GameObject muzzleObj = Instantiate(muzzlePrefab, transformComp.position, Quaternion.identity);  // спаун эффекта выстрела
            muzzleObj.transform.forward = gameObject.transform.forward;
            ParticleSystem psMuzzle = muzzleObj.GetComponent<ParticleSystem>();
            if (psMuzzle != null)
            {
                Destroy(muzzleObj, psMuzzle.main.duration);
            }
            else
            {
                var psChild = muzzleObj.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleObj, psChild.main.duration);
            }
        }
        startPosition = transformComp.position;
    }

    void Update()
    {
        if (speed != 0)
        {
            transformComp.Translate(Vector3.forward * Time.deltaTime * speed);  // перемещение снаряда
            if (Vector3.Distance(transformComp.position, startPosition) > maxDistance)
            {
                Destroy(gameObject);    // уничтожение объекта снаряда при превышении максимальной дистанции от стартовой точки
            }
        }
        else
        {
            Debug.Log("BulletMove (Script): zero projectile speed");
        }
        
    }

    // уничтожение при коллизии
    private void OnCollisionEnter(Collision collision)
    {
        speed = 0;
        if (hitPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];  // точка контакта
            GameObject hitObj = Instantiate(hitPrefab, contact.point, Quaternion.FromToRotation(Vector3.up, contact.normal));   // спаун эффекта попадания
            ParticleSystem psHit = hitObj.GetComponent<ParticleSystem>();
            if (psHit != null)
            {
                Destroy(hitObj, psHit.main.duration);
            }
            else
            {
                ParticleSystem psChild = hitObj.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitObj, psChild.main.duration);
            }
        }
        Destroy(gameObject);
    }
}
