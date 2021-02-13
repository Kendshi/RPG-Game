using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Vector3 sliderOffset;
    [SerializeField] private Transform enemyPos;
    [SerializeField] private Text enemyName;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Vector3 nameOffset;
    [SerializeField] private string enterName;
    
    private void Start()
    {
        if (enemyData != null)
        {
            enemyName.text = enemyData.enemyName;
        }
        else enemyName.text = enterName;
    }

    private void Update()
    {
        _slider.transform.position = Camera.main.WorldToScreenPoint(enemyPos.position + sliderOffset);
        enemyName.transform.position = Camera.main.WorldToScreenPoint(enemyPos.position + nameOffset);
    }

    public void SetHealthValue(float currentHealth, float maxHealth)
    {
        _slider.gameObject.SetActive(currentHealth < maxHealth);
        _slider.value = currentHealth;
        _slider.maxValue = maxHealth;
    }
}
