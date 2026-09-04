using UnityEngine;
using TMPro;

public class BallControl : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private blocks[] allBlocks;

    [Header("Velocidade e Força da Bola")]
    [Tooltip("Força horizontal inicial aplicada na bola")]
    public float forceX = 10.0f;               

    [Tooltip("Força vertical inicial aplicada na bola (valor negativo para ir para baixo)")]
    public float forceY = -8.0f;              

    [Tooltip("Velocidade máxima permitida para a bola")]
    public float maxSpeed = 15.0f;            

    [Tooltip("Velocidade vertical mínima para impedir que a bola fique presa apenas na horizontal")]
    public float minVerticalSpeed = 3.0f;     
    
    [Header("Vidas e Limite da Tela")]
    public int maxLives = 3;                    // Número inicial de vidas
    public int lives = 3;                       // Vidas atuais do jogador
    public bool autoCalculateBottomLimit = true;// Calcula limite inferior automaticamente pela Câmera
    public float bottomLimitY = -6.0f;          // Limite manual Y onde a bola é considerada perdida
    public TextMeshProUGUI livesText;          // Referência opcional para UI de vidas

    [Header("Posição de Renascimento")]
    public Vector2 spawnPosition = new Vector2(0f, -2f); // Posição de renascimento (0 X, -2 Y)
    public float respawnDelay = 1.0f;                     // Delay de 1 segundo para renascer


    [Header("Pontuação")]
    public int score = 0;                       // Placar acumulado
    public TextMeshProUGUI scoreText;          // Referência opcional para texto da interface (UI)
    public int guiFontSize = 36;                // Tamanho da fonte para exibição do placar na tela


    [Header("Efeitos Sonoros")]
    [Tooltip("Som ao destruir um bloco (tocado em volume baixo)")]
    public AudioClip blockExplodeSound;
    [Range(0f, 1f)]
    public float blockExplodeVolume = 0.25f;   // Volume baixo para não ser irritante

    [Tooltip("Som ao cair na parte inferior da tela (perder 1 vida)")]
    public AudioClip lifeLostSound;
    [Range(0f, 1f)]
    public float lifeLostVolume = 0.7f;

    [Tooltip("Som ao perder todas as 3 vidas (Game Over)")]
    public AudioClip gameOverSound;
    [Range(0f, 1f)]
    public float gameOverVolume = 1.0f;

    private AudioSource audioSource;



    void GoBall(){                      
        float rand = Random.Range(0, 2);
        if(rand < 1){
            rb2d.AddForce(new Vector2(forceX, forceY));
        } else {
            rb2d.AddForce(new Vector2(-forceX, forceY));
        }
    }

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>(); // Inicializa o objeto bola
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        lives = maxLives;
        FindAllBlocksInScene();
        UpdateScoreUI();
        ResetBall();
        Invoke("GoBall", respawnDelay);    // Chama a função GoBall após 1 segundo       
    }


    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    void FindAllBlocksInScene()
    {
        allBlocks = Object.FindObjectsByType<blocks>(FindObjectsInactive.Include);
    }


    void ResetAllBlocks()
    {
        if (allBlocks == null || allBlocks.Length == 0)
        {
            FindAllBlocksInScene();
        }

        foreach (blocks block in allBlocks)
        {
            if (block != null)
            {
                block.ResetBlock();
            }
        }
    }

    void OnCollisionEnter2D (Collision2D coll) {
        // Colisão com a raquete do jogador
        if(coll.collider.CompareTag("Player")){
            Vector2 vel;
            vel.x = rb2d.linearVelocity.x;
            vel.y = (rb2d.linearVelocity.y / 2) + (coll.collider.attachedRigidbody.linearVelocity.y / 3);
            rb2d.linearVelocity = vel;
        }
        // Colisão com os blocos ("block")
        else if (coll.gameObject.CompareTag("Block")) {
            // Tenta obter os pontos do bloco atingido
            blocks blockComponent = coll.gameObject.GetComponent<blocks>();
            int pointsEarned = (blockComponent != null) ? blockComponent.points : 1;

            AddScore(pointsEarned);

            // Toca som de destruição do bloco (volume baixo)
            PlaySound(blockExplodeSound, blockExplodeVolume);

            // Destrói / Desativa o bloco
            if (blockComponent != null) {
                blockComponent.DestroyBlock();
            } else {
                coll.gameObject.SetActive(false);
            }
        }
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Pontos: " + score;
        }
        if (livesText != null)
        {
            livesText.text = "Vidas: " + lives;
        }
    }

    // Desenha o placar e vidas diretamente na tela com fonte maior e negrito
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = guiFontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 20, 700, 60), "Pontos: " + score + "   |   Vidas: " + lives, style);
    }

    // Reinicializa a posição e velocidade da bola
    void ResetBall(){
        rb2d.linearVelocity = Vector2.zero;
        transform.position = spawnPosition;
    }

    // Perde uma vida ao cair da tela
    void LoseLife()
    {
        lives--;
        UpdateScoreUI();

        if (lives <= 0)
        {
            // Toca som de Game Over ao perder todas as vidas
            PlaySound(gameOverSound, gameOverVolume);

            // Perdeu todas as vidas -> Reinicia o jogo completo e restaura todos os blocos
            RestartGame();
        }
        else
        {
            // Toca som ao passar da parte inferior (perder 1 vida)
            PlaySound(lifeLostSound, lifeLostVolume);

            // Reinicia apenas a bola
            ResetBall();
            Invoke("GoBall", respawnDelay);
        }
    }

    // Reinicializa o jogo completamente
    void RestartGame(){
        score = 0;
        lives = maxLives;
        ResetAllBlocks();
        UpdateScoreUI();
        ResetBall();
        Invoke("GoBall", respawnDelay);
    }


    // Update is called once per frame
    void Update()
    {
        CheckBottomBoundary();
    }

    void CheckBottomBoundary()
    {
        float limitY = bottomLimitY;
        if (autoCalculateBottomLimit && Camera.main != null && Camera.main.orthographic)
        {
            limitY = -Camera.main.orthographicSize - 1.5f;
        }

        // Se a bola passou do limite inferior da tela
        if (transform.position.y < limitY)
        {
            LoseLife();
        }
    }

    void FixedUpdate()
    {
        if (rb2d != null)
        {
            Vector2 vel = rb2d.linearVelocity;

            // Aplica a correção apenas se a bola já estiver em jogo (se movendo)
            if (vel.magnitude > 0.5f)
            {
                // Impede que a velocidade vertical (Y) fique próxima de 0 (evita presas horizontais nas laterais/topo)
                if (Mathf.Abs(vel.y) < minVerticalSpeed)
                {
                    // Se a velocidade Y for quase 0, direciona para baixo se estiver na metade superior da tela
                    float dirY = (Mathf.Abs(vel.y) < 0.1f) 
                        ? (transform.position.y > 0 ? -1f : 1f) 
                        : Mathf.Sign(vel.y);

                    vel.y = dirY * minVerticalSpeed;
                    rb2d.linearVelocity = vel;
                }

                // Limita a velocidade máxima da bola
                if (maxSpeed > 0 && rb2d.linearVelocity.magnitude > maxSpeed)
                {
                    rb2d.linearVelocity = rb2d.linearVelocity.normalized * maxSpeed;
                }
            }
        }
    }
}




