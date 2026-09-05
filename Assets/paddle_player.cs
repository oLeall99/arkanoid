using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class paddle_player : MonoBehaviour
{
    public Key moveLeft = Key.A;              // Move a raquete para Esquerda (New Input System)
    public Key moveRight = Key.D;            // Move a raquete para Direita (New Input System)
    public Key pushKey = Key.Space;          // Tecla para impulsionar a raquete para frente

    public float speed = 10.0f;             // Define a velocidade da raquete
    public float pushDistance = 0.35f;      // Distância Y que a raquete avança
    public float pushDuration = 0.15f;      // Duração da animação do impulso (ida e volta)
    public float speedBoostMultiplier = 1.5f;// Multiplicador de velocidade da bola impulsionada (50% mais rápida)
    public float speedBoostDuration = 2.0f;  // Duração da velocidade turbinada na bola

    [Header("Limites de Tela")]
    public bool autoCalculateBounds = true; // Calcula automaticamente os limites exatos da tela
    public float boundX = 10f;              // Limite manual em X (usado se autoCalculateBounds for falso)

    [Header("Power Up: Aumento da Barra e Velocidade (8s)")]
    public int speedPaddleLevel = 0;        // Nível acumulativo do PowerUp (+0.2 de tamanho na barra por nível)
    public float speedPaddleDuration = 8.0f;// Duração de 8 segundos

    public bool isBoosting = false;         // Indica se o impulso para frente está ativo
    private Rigidbody2D rb2d;               // Define o corpo rígido 2D que representa a raquete
    private float calculatedBoundX;
    private float startY;
    private Vector3 originalPaddleScale = Vector3.one;
    private Coroutine pushCoroutine;
    private Coroutine speedPaddleCoroutine;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
        originalPaddleScale = transform.localScale;
        UpdateBounds();
    }

    public void EnableSpeedPaddle()
    {
        speedPaddleLevel++;

        if (originalPaddleScale == Vector3.zero)
        {
            originalPaddleScale = transform.localScale;
        }

        // Aumenta o tamanho da barra em +0.2 a cada acúmulo
        transform.localScale = new Vector3(originalPaddleScale.x + (speedPaddleLevel * 0.2f), originalPaddleScale.y, originalPaddleScale.z);

        // Aumenta a velocidade da bola em +0.1 a cada acúmulo
        BallControl ball = Object.FindAnyObjectByType<BallControl>();
        if (ball != null)
        {
            ball.EnableSpeedPaddleBonus(speedPaddleLevel);
        }

        if (speedPaddleCoroutine != null)
        {
            StopCoroutine(speedPaddleCoroutine);
        }
        speedPaddleCoroutine = StartCoroutine(SpeedPaddleRoutine());
    }

    private IEnumerator SpeedPaddleRoutine()
    {
        yield return new WaitForSeconds(speedPaddleDuration);

        DisableSpeedPaddle();
    }

    public void DisableSpeedPaddle()
    {
        if (speedPaddleCoroutine != null)
        {
            StopCoroutine(speedPaddleCoroutine);
            speedPaddleCoroutine = null;
        }
        speedPaddleLevel = 0;
        if (originalPaddleScale != Vector3.zero)
        {
            transform.localScale = originalPaddleScale;
        }

        BallControl ball = Object.FindAnyObjectByType<BallControl>();
        if (ball != null)
        {
            ball.DisableSpeedPaddleBonus();
        }
    }

    void UpdateBounds()
    {
        if (autoCalculateBounds && Camera.main != null && Camera.main.orthographic)
        {
            float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            float paddleHalfWidth = 0.5f;

            // Obtém a metade da largura da raquete através do SpriteRenderer ou Collider2D
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                paddleHalfWidth = sprite.bounds.extents.x;
            }
            else
            {
                Collider2D col = GetComponent<Collider2D>();
                if (col != null)
                {
                    paddleHalfWidth = col.bounds.extents.x;
                }
            }

            calculatedBoundX = screenHalfWidth - paddleHalfWidth;
        }
        else
        {
            calculatedBoundX = boundX;
        }
    }

    void Update()
    {
        // Ao pressionar Espaço, executa o impulso para frente
        if (Keyboard.current != null && Keyboard.current[pushKey].wasPressedThisFrame && pushCoroutine == null)
        {
            pushCoroutine = StartCoroutine(PushRoutine());
        }

        var vel = rb2d.linearVelocity;                // Acessa a velocidade da raquete
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current[moveLeft].isPressed) {       // Tecla para Esquerda
                vel.x = -speed;
            }
            else if (Keyboard.current[moveRight].isPressed) { // Tecla para Direita
                vel.x = speed;                    
            }
            else {
                vel.x = 0;                                  // Raquete parada
            }
        }

        rb2d.linearVelocity = vel;                    // Atualiza a velocidade da raquete

        if (autoCalculateBounds)
        {
            UpdateBounds();
        }

        var pos = transform.position;           // Acessa a Posição da raquete
        if (pos.x > calculatedBoundX) {                  
            pos.x = calculatedBoundX;           // Corrige a posição no limite direito
        }
        else if (pos.x < -calculatedBoundX) {
            pos.x = -calculatedBoundX;          // Corrige a posição no limite esquerdo
        }

        // Mantém a posição Y padrão quando não estiver realizando o impulso
        if (!isBoosting && pushCoroutine == null)
        {
            pos.y = startY;
        }

        transform.position = pos;               // Atualiza a posição da raquete
    }

    private IEnumerator PushRoutine()
    {
        isBoosting = true;
        float elapsed = 0f;
        float halfDuration = pushDuration / 2f;

        // Avança para frente (+Y)
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float newY = Mathf.Lerp(startY, startY + pushDistance, elapsed / halfDuration);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }

        // Retorna para a posição inicial
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float newY = Mathf.Lerp(startY + pushDistance, startY, elapsed / halfDuration);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, startY, transform.position.z);
        isBoosting = false;
        pushCoroutine = null;
    }
}

