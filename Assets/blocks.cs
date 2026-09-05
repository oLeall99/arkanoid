using UnityEngine;

public class blocks : MonoBehaviour
{
    [Header("Resistência do Bloco")]
    [Tooltip("Nível de destruição / Vida do bloco (1 = quebra instantaneamente, 6 = precisa de 6 acertos: 6->5->4->3->2->1->quebra)")]
    public int maxHealth = 1;

    [Tooltip("Nível de vida atual durante a partida")]
    public int currentHealth = 1;

    [Header("Pontuação")]
    public int points = 1; // Pontos concedidos por acerto ao interagir com este bloco

    [Header("Visual e Sprites por Nível (Opcional)")]
    [Tooltip("Sprites para cada nível de vida (índice 0 = 1 de vida, índice 5 = 6 de vida). Se preenchido, o sprite muda dinamicamente.")]
    public Sprite[] healthSprites;

    [Tooltip("Se verdadeiro, altera a opacidade do bloco conforme perde vida quando não houver sprites por nível.")]
    public bool changeColorOnHit = true;

    [Header("Power Up")]
    [Tooltip("Prefab do Power Up (opcional). Se não configurado, o powerUP gerará um automaticamente.")]
    public GameObject powerUpPrefab;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = Mathf.Max(1, maxHealth);
    }

    private void Start()
    {
        UpdateVisuals();
    }

    /// <summary>
    /// Define a vida/nível máximo do bloco dinamicamente.
    /// </summary>
    public void SetHealth(int health)
    {
        maxHealth = Mathf.Max(1, health);
        currentHealth = maxHealth;
        UpdateVisuals();
    }

    /// <summary>
    /// Processa o impacto da bola no bloco com base no dano causado.
    /// Reduz o nível de destruição com base no dano (padrão = 1).
    /// Retorna TRUE se o bloco foi destruído, ou FALSE se ainda tem vida restante.
    /// </summary>
    public bool TakeHit(int damage = 1)
    {
        currentHealth -= Mathf.Max(1, damage);

        if (currentHealth <= 0)
        {
            DestroyBlock();
            return true;
        }
        else
        {
            UpdateVisuals();
            return false;
        }
    }

    // Desativa o bloco em vez de destruí-lo permanentemente (permite reiniciar o jogo) e solta 10% das vezes um PowerUp
    public void DestroyBlock()
    {
        TryDropPowerUp();
        gameObject.SetActive(false);
    }

    private void TryDropPowerUp()
    {
        // 10% de chance de soltar um PowerUp
        if (Random.value <= 0.20f)
        {
            powerUP.SpawnPowerUp(transform.position, powerUpPrefab);
        }
    }

    // Reativa o bloco e restaura sua vida total para reiniciar a partida
    public void ResetBlock()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        UpdateVisuals();
    }

    /// <summary>
    /// Atualiza o visual do bloco (Sprite ou Opacidade da Cor) com base na vida atual.
    /// </summary>
    public void UpdateVisuals()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null) return;

        // Se houver array de sprites por nível de vida
        if (healthSprites != null && healthSprites.Length > 0)
        {
            int index = Mathf.Clamp(currentHealth - 1, 0, healthSprites.Length - 1);
            if (healthSprites[index] != null)
            {
                spriteRenderer.sprite = healthSprites[index];
            }
        }
        // Caso contrário, ajusta a opacidade da cor conforme a vida restante (se ativado)
        else if (changeColorOnHit && maxHealth > 1)
        {
            float ratio = (float)currentHealth / maxHealth;
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0.35f, 1.0f, ratio);
            spriteRenderer.color = color;
        }
    }
}



