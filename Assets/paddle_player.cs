using UnityEngine;
using UnityEngine.InputSystem;

public class paddle_player : MonoBehaviour
{
    public Key moveLeft = Key.A;              // Move a raquete para Esquerda (New Input System)
    public Key moveRight = Key.D;            // Move a raquete para Direita (New Input System)
    public float speed = 10.0f;             // Define a velocidade da raquete

    [Header("Limites de Tela")]
    public bool autoCalculateBounds = true; // Calcula automaticamente os limites exatos da tela
    public float boundX = 10f;              // Limite manual em X (usado se autoCalculateBounds for falso)

    private Rigidbody2D rb2d;               // Define o corpo rígido 2D que representa a raquete
    private float calculatedBoundX;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>(); 
        UpdateBounds();
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
        transform.position = pos;               // Atualiza a posição da raquete
    }
}

