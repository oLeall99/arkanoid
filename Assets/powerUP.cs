using UnityEngine;

public enum PowerUpType
{
    Duplicator,
    Penetration,
    Bigger
}

public class powerUP : MonoBehaviour
{
    [Header("Configurações do Power Up")]
    public PowerUpType type;
    public float fallSpeed = 8.0f;
    private bool isTypeSet = false;

    [Header("Sprites dos Power Ups (3 Sprites Diferentes)")]
    public Sprite duplicatorSprite;
    public Sprite penetrationSprite;
    public Sprite biggerSprite;

    [Header("Prefab de Power Up (Opcional)")]
    public static GameObject powerUpPrefabStatic;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        try
        {
            gameObject.tag = "Power";
        }
        catch (System.Exception)
        {
            // Tag "Power" pode não estar registrada no TagManager da Unity ainda
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.isTrigger = true;
            circleCol.radius = 0.4f;
        }
        else
        {
            col.isTrigger = true;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void Start()
    {
        if (!isTypeSet)
        {
            SetupRandomType();
        }
        else
        {
            UpdateSprite();
        }
    }

    public void SetupType(PowerUpType newType)
    {
        type = newType;
        isTypeSet = true;
        UpdateSprite();
    }

    public void SetupRandomType()
    {
        isTypeSet = true;
        PowerUpType[] allTypes = (PowerUpType[])System.Enum.GetValues(typeof(PowerUpType));
        type = allTypes[Random.Range(0, allTypes.Length)];
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer == null) return;

        switch (type)
        {
            case PowerUpType.Duplicator:
                if (duplicatorSprite != null) spriteRenderer.sprite = duplicatorSprite;
                spriteRenderer.color = Color.green;
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case PowerUpType.Penetration:
                if (penetrationSprite != null) spriteRenderer.sprite = penetrationSprite;
                spriteRenderer.color = new Color(1.0f, 0.2f, 0.8f); // Rosa / Magenta
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case PowerUpType.Bigger:
                if (biggerSprite != null) spriteRenderer.sprite = biggerSprite;
                spriteRenderer.color = Color.yellow;
                transform.localScale = new Vector3(1.4f, 1.4f, 1.4f); // Maior visualmente
                break;
        }
    }

    private void Update()
    {
        // Cai verticalmente com velocidade de 8
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Destrói o power up se cair abaixo da tela
        float bottomLimit = -6.0f;
        if (Camera.main != null && Camera.main.orthographic)
        {
            bottomLimit = -Camera.main.orthographicSize - 2.0f;
        }

        if (transform.position.y < bottomLimit)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<paddle_player>() != null)
        {
            ActivatePowerUp();
            Destroy(gameObject);
        }
    }

    private void ActivatePowerUp()
    {
        BallControl ball = Object.FindAnyObjectByType<BallControl>();

        if (ball == null) return;

        switch (type)
        {
            case PowerUpType.Duplicator:
                ball.SpawnExtraBall();
                break;

            case PowerUpType.Penetration:
                ball.EnablePenetration();
                break;

            case PowerUpType.Bigger:
                ball.EnableBiggerBall();
                break;
        }
    }

    /// <summary>
    /// Método estático utilitário para instanciar um PowerUp na posição informada.
    /// </summary>
    public static powerUP SpawnPowerUp(Vector3 position, GameObject prefab = null)
    {
        GameObject go;
        if (prefab != null)
        {
            go = Instantiate(prefab, position, Quaternion.identity);
        }
        else if (powerUpPrefabStatic != null)
        {
            go = Instantiate(powerUpPrefabStatic, position, Quaternion.identity);
        }
        else
        {
            go = new GameObject("PowerUp");
            go.transform.position = position;
            powerUP p = go.AddComponent<powerUP>();
            p.SetupRandomType();
            return p;
        }

        powerUP script = go.GetComponent<powerUP>();
        if (script == null)
        {
            script = go.AddComponent<powerUP>();
        }
        script.SetupRandomType();
        return script;
    }
}

