using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    
    private void Awake()
    {
        if(CompareTag("Player") && PlayerData.Instance != null)
        {
            maxHealth = PlayerData.Instance.GetMaxHealth();
            currentHealth = PlayerData.Instance.currentHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    private void Start()
    {
        // Player면 playerData에서 최대 체력 가져오기
        if(CompareTag("Player") &&  PlayerData.Instance != null)
        {
            maxHealth = PlayerData.Instance.GetMaxHealth();
        }
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = damage;

        // Player만 방어력 적용
        if(CompareTag("Player") && PlayerData.Instance != null)
        {
            finalDamage = PlayerData.Instance.CalculateDamage(damage);
            Debug.Log("원래 데미지 : " + damage + "  실제 데미지 : " + finalDamage);
        }

        currentHealth -= finalDamage;
        Debug.Log(gameObject.name + " 체력 : " + currentHealth);
        
        if(CompareTag("Player") && PlayerData.Instance != null)
        {
            PlayerData.Instance.currentHealth = currentHealth;
        }

        if(currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log(gameObject.name + " 사망!");

        // 드랍 처리
        EnemyDrops drop = GetComponent<EnemyDrops>();
        if(drop != null)
        {
            drop.DropLoot();
        }


        // 태그로 플레이어인지 자동 확인
        if(CompareTag("Player"))
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnPlayerDeath();
            }

            gameObject.SetActive(false);
            return;
        }

        Destroy(gameObject);
    }

    public void SetCurrentHealth(float health)
    {
        currentHealth = health;

        // PlayerData 동기화
        if(CompareTag("Player") && PlayerData.Instance != null)
        {
            PlayerData.Instance.currentHealth = currentHealth;
        }
    }

    public float GetMaxHealth()
    {
        if (CompareTag("Player") && PlayerData.Instance != null)
        {
            return PlayerData.Instance.GetMaxHealth();
        }
        return maxHealth;
    }

    public float GetCurrentHealth() => currentHealth;

    public void RefreshMaxHealth()
    {
        if(CompareTag("Player") && PlayerData.Instance != null)
        {
            maxHealth = PlayerData.Instance.GetMaxHealth();
        }
    }
}
