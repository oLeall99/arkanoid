using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class BallControl : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private blocks[] allBlocks;

    [Header("Velocidade e Controle da Bola")]
    [Tooltip("Velocidade fixa e constante da bola (padrão 8.0f para movimento suave e controlado)")]
    public float ballSpeed = 8.0f;             

    [Header("Transição de Fases")]
    public string[] levelSequence = new string[] { "level01", "level02", "level03", "SampleScene" };
    public int countdownTime = 3;               // Contagem regressiva em segundos
    private bool isLevelCompleting = false;
    private int currentCountdown = 3;

    [Header("Telas Especiais")]
    private bool isTutorialActive = false;      // Exibido no level01
    private bool isVictoryActive = false;       // Exibido no SampleScene
    private int consecutiveBlockHits = 0;       // Contador de acertos em blocos sem tocar na raquete





    
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
    public static int score = 0;               // Placar acumulado total de todas as fases
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

    [Header("Efeitos de Power Up")]
    public bool isPenetrating = false;
    public bool isBigger = false;
    public bool isExtraBall = false; // Indica se esta instância é uma bola extra duplicada
    public int biggerLevel = 0; // Nível acumulativo do Bigger (cada nível adiciona +1 de dano e tamanho)
    public float powerUpDuration = 5.0f; // Duração dos efeitos em segundos (5s)

    public int BallDamage => 1 + biggerLevel; // Dano base = 1, cada Bigger aumenta o dano em +1

    private Coroutine penetrationCoroutine;
    private Coroutine biggerCoroutine;
    private Vector3 originalScale = Vector3.one;

    void GoBall(){                      
        float rand = Random.Range(0, 2);
        Vector2 dir = new Vector2(rand < 1 ? 0.7f : -0.7f, -1.0f).normalized;
        rb2d.linearVelocity = dir * ballSpeed;
    }

    void Start()
    {
        originalScale = transform.localScale;
        rb2d = GetComponent<Rigidbody2D>(); // Inicializa o objeto bola
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Se for uma bola extra duplicada, não reinicia as vidas nem executa tutoriais/telas
        if (isExtraBall)
        {
            return;
        }

        lives = maxLives;
        FindAllBlocksInScene();
        UpdateScoreUI();
        ResetBall();

        string currentScene = SceneManager.GetActiveScene().name;

        // Se estiver no SampleScene -> ativa a tela de Vitória final
        if (currentScene.Equals("SampleScene", System.StringComparison.OrdinalIgnoreCase))
        {
            isVictoryActive = true;
        }
        // Se estiver no level01 -> ativa a tela de Tutorial
        else if (currentScene.Equals("level01", System.StringComparison.OrdinalIgnoreCase))
        {
            isTutorialActive = true;
        }
        else
        {
            Invoke("GoBall", respawnDelay);
        }
    }

    public void EnablePenetration()
    {
        if (penetrationCoroutine != null)
        {
            StopCoroutine(penetrationCoroutine);
        }
        penetrationCoroutine = StartCoroutine(PenetrationRoutine());
    }

    private IEnumerator PenetrationRoutine()
    {
        isPenetrating = true;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        yield return new WaitForSeconds(powerUpDuration);

        DisablePenetration();
    }

    public void DisablePenetration()
    {
        if (penetrationCoroutine != null)
        {
            StopCoroutine(penetrationCoroutine);
            penetrationCoroutine = null;
        }
        isPenetrating = false;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    public void EnableBiggerBall()
    {
        biggerLevel++;
        if (originalScale == Vector3.zero)
        {
            originalScale = Vector3.one;
        }

        // Crescimento acumulativo (1x -> 2x -> 3x -> 4x...)
        transform.localScale = originalScale * (1f + biggerLevel);

        if (biggerCoroutine != null)
        {
            StopCoroutine(biggerCoroutine);
        }
        biggerCoroutine = StartCoroutine(BiggerBallRoutine());
    }

    private IEnumerator BiggerBallRoutine()
    {
        isBigger = true;

        yield return new WaitForSeconds(powerUpDuration);

        DisableBiggerBall();
    }

    public void DisableBiggerBall()
    {
        if (biggerCoroutine != null)
        {
            StopCoroutine(biggerCoroutine);
            biggerCoroutine = null;
        }
        isBigger = false;
        biggerLevel = 0;
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Spawna uma bola extra que parte do spawnPosition para CIMA e dura 5 segundos.
    /// </summary>
    public void SpawnExtraBall()
    {
        GameObject extraGo = Instantiate(gameObject, spawnPosition, Quaternion.identity);
        BallControl extraBall = extraGo.GetComponent<BallControl>();

        if (extraBall != null)
        {
            extraBall.isExtraBall = true;
            extraBall.isTutorialActive = false;
            extraBall.isVictoryActive = false;
            extraBall.isLevelCompleting = false;

            // Inicia o movimento da bola extra para CIMA com variação suave no eixo X
            Vector2 upDir = new Vector2(Random.Range(-0.4f, 0.4f), 1.0f).normalized;
            Rigidbody2D extraRb = extraGo.GetComponent<Rigidbody2D>();
            if (extraRb != null)
            {
                extraRb.linearVelocity = upDir * ballSpeed;
            }

            extraBall.StartCoroutine(extraBall.ExtraBallLifetime(5.0f));
        }
    }

    private IEnumerator ExtraBallLifetime(float duration)
    {
        yield return new WaitForSeconds(duration);
        // Após 5 segundos, se a bola extra ainda estiver na tela, ela desaparece sem tirar vida
        if (gameObject != null && isExtraBall)
        {
            Destroy(gameObject);
        }
    }

    private void SyncLives(int newLives)
    {
        BallControl[] balls = Object.FindObjectsByType<BallControl>(FindObjectsInactive.Include);
        foreach (BallControl b in balls)
        {
            if (b != null)
            {
                b.lives = newLives;
                b.UpdateScoreUI();
            }
        }
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
        blocks[] sceneBlocks = Object.FindObjectsByType<blocks>(FindObjectsInactive.Include);
        foreach (blocks block in sceneBlocks)
        {
            if (block != null)
            {
                block.ResetBlock();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isPenetrating) return;

        // Se tocar na raquete do jogador enquanto penetrando -> Rebate mantendo a penetração ativa nos 5s
        if (other.CompareTag("Player") || other.GetComponent<paddle_player>() != null)
        {
            consecutiveBlockHits = 0;

            float hitFactor = (transform.position.x - other.transform.position.x) / other.bounds.size.x;
            Vector2 dir = new Vector2(hitFactor * 1.2f, 1f).normalized;
            rb2d.linearVelocity = dir * ballSpeed;
        }
        // Se tocar em um bloco enquanto penetrando -> Atravessa causando dano a todos os blocos no caminho
        else if (other.CompareTag("Block") || other.GetComponent<blocks>() != null)
        {
            consecutiveBlockHits++;

            int comboMultiplier = (consecutiveBlockHits > 1) ? 2 : 1;
            blocks blockComponent = other.GetComponent<blocks>();
            int basePoints = (blockComponent != null) ? blockComponent.points : 1;
            int totalPoints = basePoints * comboMultiplier;

            AddScore(totalPoints);
            PlaySound(blockExplodeSound, blockExplodeVolume);

            bool isDestroyed = true;
            if (blockComponent != null)
            {
                isDestroyed = blockComponent.TakeHit(BallDamage);
            }
            else
            {
                other.gameObject.SetActive(false);
            }

            if (isDestroyed)
            {
                CheckLevelCompleted();
            }
        }
        // Se tocar nas Paredes ou Limites enquanto penetrando -> Rebate mantendo a penetração ativa
        else if (!other.CompareTag("Power"))
        {
            Vector2 ballPos = transform.position;
            Vector2 closestPoint = other.ClosestPoint(ballPos);
            Vector2 normal = (ballPos - closestPoint).normalized;

            if (normal == Vector2.zero)
            {
                Vector2 currentDir = rb2d.linearVelocity.normalized;
                if (Mathf.Abs(currentDir.x) > Mathf.Abs(currentDir.y))
                {
                    normal = new Vector2(-Mathf.Sign(currentDir.x), 0);
                }
                else
                {
                    normal = new Vector2(0, -Mathf.Sign(currentDir.y));
                }
            }

            Vector2 inVelocity = rb2d.linearVelocity;
            Vector2 reflectedVelocity = Vector2.Reflect(inVelocity, normal);

            if (reflectedVelocity == Vector2.zero)
            {
                reflectedVelocity = -inVelocity;
            }

            rb2d.linearVelocity = reflectedVelocity.normalized * ballSpeed;
        }
    }

    void OnCollisionEnter2D (Collision2D coll) {
        // Colisão com a raquete do jogador (calcula a nova direção com velocidade constante ballSpeed)
        if(coll.collider.CompareTag("Player")){
            consecutiveBlockHits = 0; // Reseta o combo ao tocar na raquete
            float hitFactor = (transform.position.x - coll.transform.position.x) / coll.collider.bounds.size.x;
            Vector2 dir = new Vector2(hitFactor * 1.2f, 1f).normalized;
            rb2d.linearVelocity = dir * ballSpeed;
        }
        // Colisão com os blocos ("Block")
        else if (coll.gameObject.CompareTag("Block")) {
            consecutiveBlockHits++; // Incrementa os acertos consecutivos em blocos

            // Regra de Pontuação: 1 ponto no 1º bloco após a raquete; 2 pontos para blocos atingidos em sequência
            int comboMultiplier = (consecutiveBlockHits > 1) ? 2 : 1;

            blocks blockComponent = coll.gameObject.GetComponent<blocks>();
            int basePoints = (blockComponent != null) ? blockComponent.points : 1;
            int totalPoints = basePoints * comboMultiplier;

            AddScore(totalPoints);

            // Toca som de destruição do bloco (volume baixo)
            PlaySound(blockExplodeSound, blockExplodeVolume);

            bool isDestroyed = true;

            // Processa o dano / destruição do bloco com o dano atual da bola
            if (blockComponent != null) {
                isDestroyed = blockComponent.TakeHit(BallDamage);
            } else {
                coll.gameObject.SetActive(false);
            }

            // Verifica se todos os blocos foram destruídos para avançar de fase (somente quando um bloco é destruído)
            if (isDestroyed) {
                CheckLevelCompleted();
            }
        }
    }

    void CheckLevelCompleted()
    {
        if (isLevelCompleting) return;

        // Busca apenas os blocos que estão ATIVOS no momento na cena
        blocks[] activeBlocks = Object.FindObjectsByType<blocks>(FindObjectsInactive.Exclude);

        // Se não resta nenhum bloco ativo na cena, inicia a transição de fase
        if (activeBlocks == null || activeBlocks.Length == 0)
        {
            StartCoroutine(LevelCompletedRoutine());
        }
    }

    IEnumerator LevelCompletedRoutine()
    {
        isLevelCompleting = true;
        rb2d.linearVelocity = Vector2.zero;
        transform.position = spawnPosition;

        currentCountdown = countdownTime;
        while (currentCountdown > 0)
        {
            yield return new WaitForSeconds(1.0f);
            currentCountdown--;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = GetNextSceneName(currentScene);

        SceneManager.LoadScene(nextScene);
    }

    string GetNextSceneName(string current)
    {
        for (int i = 0; i < levelSequence.Length; i++)
        {
            if (levelSequence[i].Equals(current, System.StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < levelSequence.Length)
                {
                    return levelSequence[i + 1];
                }
                else
                {
                    return levelSequence[0]; // Retorna para a primeira fase ao concluir level03
                }
            }
        }

        return "level01";
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

    // Desenha o placar, tutorial do level01, tela de vitória em SampleScene e contagem de transição
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = guiFontSize;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        // 1. Tela de Vitória em SampleScene (Cobre 100% da tela)
        if (isVictoryActive)
        {
            Texture2D darkTex = new Texture2D(1, 1);
            darkTex.SetPixel(0, 0, new Color(0.05f, 0.05f, 0.1f, 0.95f));
            darkTex.Apply();

            GUIStyle fullBg = new GUIStyle();
            fullBg.normal.background = darkTex;
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", fullBg);

            GUIStyle victoryTitleStyle = new GUIStyle();
            victoryTitleStyle.fontSize = guiFontSize + 16;
            victoryTitleStyle.fontStyle = FontStyle.Bold;
            victoryTitleStyle.alignment = TextAnchor.MiddleCenter;
            victoryTitleStyle.normal.textColor = Color.yellow;

            GUIStyle victorySubStyle = new GUIStyle();
            victorySubStyle.fontSize = guiFontSize + 2;
            victorySubStyle.alignment = TextAnchor.MiddleCenter;
            victorySubStyle.normal.textColor = Color.white;

            float vBoxWidth = 700f;
            float vBoxHeight = 300f;
            float vBoxX = (Screen.width - vBoxWidth) / 2f;
            float vBoxY = (Screen.height - vBoxHeight) / 2f;

            string titleText = "PARABÉNS! 🎉\nVOCÊ COMPLETOU O JOGO!";
            string subText = "Pontuação Final: " + score + "\n\n[ Pressione R para Reiniciar ]";

            GUI.Label(new Rect(vBoxX, vBoxY - 40, vBoxWidth, 100), titleText, victoryTitleStyle);
            GUI.Label(new Rect(vBoxX, vBoxY + 80, vBoxWidth, 150), subText, victorySubStyle);
            return;
        }

        // Placar padrão de pontos e vidas
        GUI.Label(new Rect(20, 20, 700, 60), "Pontos: " + score + "   |   Vidas: " + lives, style);

        // 2. Tela de Tutorial no level01
        if (isTutorialActive)
        {
            GUIStyle boxStyle = new GUIStyle();
            Texture2D boxTex = new Texture2D(1, 1);
            boxTex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.92f));
            boxTex.Apply();
            boxStyle.normal.background = boxTex;

            GUIStyle tutTitleStyle = new GUIStyle();
            tutTitleStyle.fontSize = guiFontSize + 14;
            tutTitleStyle.fontStyle = FontStyle.Bold;
            tutTitleStyle.alignment = TextAnchor.MiddleCenter;
            tutTitleStyle.normal.textColor = Color.yellow;

            GUIStyle tutBodyStyle = new GUIStyle();
            tutBodyStyle.fontSize = guiFontSize - 2;
            tutBodyStyle.fontStyle = FontStyle.Bold;
            tutBodyStyle.wordWrap = true;
            tutBodyStyle.alignment = TextAnchor.MiddleLeft;
            tutBodyStyle.normal.textColor = Color.white;

            float tWidth = 960f;
            float tHeight = 520f;
            float tX = (Screen.width - tWidth) / 2f;
            float tY = (Screen.height - tHeight) / 2f;

            GUI.Box(new Rect(tX, tY, tWidth, tHeight), "", boxStyle);

            GUI.Label(new Rect(tX, tY + 25, tWidth, 60), "COMO JOGAR", tutTitleStyle);

            string bodyText = " • Mova a raquete usando as teclas [A] e [D].\n\n" +
                              " • Objetivo: Destrua todos os blocos com a bola.\n\n" +
                              " • Não deixe a bola cair na parte inferior da tela!\n\n" +
                              " • Pontuação: 1 ponto no 1º bloco após a raquete;\n" +
                              "   2 pontos para cada bloco atingido em sequência!\n\n" +
                              "                [ Pressione ESPAÇO para Começar ]";

            GUI.Label(new Rect(tX + 45, tY + 95, tWidth - 90, tHeight - 110), bodyText, tutBodyStyle);
        }

        // 3. Display de Contagem Regressiva ao vencer uma fase
        if (isLevelCompleting)
        {
            GUIStyle bannerStyle = new GUIStyle();
            bannerStyle.fontSize = guiFontSize + 12;
            bannerStyle.fontStyle = FontStyle.Bold;
            bannerStyle.alignment = TextAnchor.MiddleCenter;
            bannerStyle.normal.textColor = Color.yellow;

            float boxWidth = 600f;
            float boxHeight = 160f;
            float boxX = (Screen.width - boxWidth) / 2f;
            float boxY = (Screen.height - boxHeight) / 2f;

            GUI.Box(new Rect(boxX, boxY, boxWidth, boxHeight), "");

            string text = (currentCountdown > 0)
                ? "FASE CONCLUÍDA!\nPróxima fase em: " + currentCountdown
                : "CARREGANDO FASE...";

            GUI.Label(new Rect(boxX, boxY, boxWidth, boxHeight), text, bannerStyle);
        }
    }

    // Reinicializa a posição e velocidade da bola
    void ResetBall(){
        consecutiveBlockHits = 0;
        rb2d.linearVelocity = Vector2.zero;
        transform.position = spawnPosition;
        DisablePenetration();
        DisableBiggerBall();
    }


    // Perde uma vida ao cair da tela
    void LoseLife()
    {
        int newLives = lives - 1;
        SyncLives(newLives);

        if (newLives <= 0)
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

            if (isExtraBall)
            {
                // Se for bola extra, ela é destruída sem interromper a bola principal
                Destroy(gameObject);
            }
            else
            {
                // Reinicia apenas a bola principal
                ResetBall();
                Invoke("GoBall", respawnDelay);
            }
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
        // Se estiver na tela de Vitória (SampleScene), aguarda tecla R para reiniciar
        if (isVictoryActive)
        {
            if (Keyboard.current != null && Keyboard.current[Key.R].wasPressedThisFrame)
            {
                score = 0;
                lives = maxLives;
                isVictoryActive = false;
                SceneManager.LoadScene("level01");
            }
            return;
        }

        // Se estiver no Tutorial do level01, aguarda tecla ESPAÇO para iniciar
        if (isTutorialActive)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current[Key.Space].wasPressedThisFrame ||
                    Keyboard.current[Key.Enter].wasPressedThisFrame ||
                    Keyboard.current[Key.A].wasPressedThisFrame ||
                    Keyboard.current[Key.D].wasPressedThisFrame)
                {
                    isTutorialActive = false;
                    Invoke("GoBall", respawnDelay);
                }
            }
            return;
        }

        CheckBottomBoundary();
    }



    void CheckBottomBoundary()
    {
        float limitY = bottomLimitY;
        if (autoCalculateBottomLimit && Camera.main != null && Camera.main.orthographic)
        {
            float orthoHeight = Camera.main.orthographicSize;
            float orthoWidth = orthoHeight * Camera.main.aspect;

            limitY = -orthoHeight - 1.5f;

            // Se estiver em modo penetração, garante o rebate nas paredes de topo e laterais se a bola chegar nas bordas da tela
            if (isPenetrating)
            {
                Vector2 vel = rb2d.linearVelocity;
                Vector3 pos = transform.position;

                // Parede Direita
                if (pos.x > orthoWidth - 0.5f && vel.x > 0)
                {
                    vel.x = -Mathf.Abs(vel.x);
                }
                // Parede Esquerda
                else if (pos.x < -orthoWidth + 0.5f && vel.x < 0)
                {
                    vel.x = Mathf.Abs(vel.x);
                }

                // Parede do Topo
                if (pos.y > orthoHeight - 0.5f && vel.y > 0)
                {
                    vel.y = -Mathf.Abs(vel.y);
                }

                rb2d.linearVelocity = vel.normalized * ballSpeed;
            }
        }

        // Se a bola passou do limite inferior da tela
        if (transform.position.y < limitY)
        {
            LoseLife();
        }
    }

    void FixedUpdate()
    {
        if (rb2d != null && rb2d.linearVelocity.magnitude > 0.5f)
        {
            Vector2 dir = rb2d.linearVelocity.normalized;

            // Corrige se o componente Y estiver plano demais (evita ficar presa no topo)
            if (Mathf.Abs(dir.y) < 0.25f)
            {
                dir.y = (transform.position.y > 0 ? -0.35f : 0.35f);
                dir = dir.normalized;
            }

            // Corrige se o componente X estiver plano demais (evita ficar presa nas paredes laterais)
            if (Mathf.Abs(dir.x) < 0.25f)
            {
                dir.x = (transform.position.x > 0 ? -0.35f : 0.35f);
                dir = dir.normalized;
            }

            // Mantém a velocidade 100% constante no valor de ballSpeed
            rb2d.linearVelocity = dir * ballSpeed;
        }
    }
}






