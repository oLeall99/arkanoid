using System.Collections.Generic;
using UnityEngine;

public class blockSpawnerPattern : MonoBehaviour
{
    [Header("Configurações do Desenho")]
    [Tooltip("Desenhe o mapa de blocos. '1', '2', '3' definem a cor/sprite. '0' ou ' ' (espaço) deixam o espaço vazio.")]
    [TextArea(7, 15)]
    public string[] pattern = new string[]
    {
        "  111111  ",
        " 11111111 ",
        "11 2112 11",
        "11 2112 11",
        "1111111111",
        "1311111131",
        "112    211",
        "11 2222 11",
        " 11111111 ",
        "  111111  "
    };

    [Header("Espaçamento e Posição")]
    public Vector2 spacing = new Vector2(1.1f, 0.55f); // Distância entre os blocos
    public float topMargin = 1.5f;                    // Distância do topo da tela

    [Header("Referências")]
    public GameObject blockPrefab;                     // Prefab do bloco
    public Sprite[] blockSprites;                      // Lista de Sprites/Cores para os números ('1', '2', etc.)

    void Start()
    {
        GeneratePatternGrid();
    }

    void GeneratePatternGrid()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("blockSpawnerPattern: Nenhum blockPrefab foi atribuído no Inspector!");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("blockSpawnerPattern: Câmera principal (Camera.main) não encontrada!");
            return;
        }

        if (pattern == null || pattern.Length == 0)
        {
            Debug.LogWarning("blockSpawnerPattern: Nenhum padrão/desenho foi definido!");
            return;
        }

        List<GameObject> spawnedBlocks = new List<GameObject>();

        // Ponto de início temporário para instanciar os blocos
        Vector2 tempStartPos = (Vector2)transform.position;

        for (int r = 0; r < pattern.Length; r++)
        {
            string rowStr = pattern[r];

            for (int c = 0; c < rowStr.Length; c++)
            {
                char ch = rowStr[c];

                // Pula espaços vazios
                if (ch == ' ' || ch == '0')
                {
                    continue;
                }

                // Calcula a posição temporária do bloco
                Vector2 spawnPos = new Vector2(
                    tempStartPos.x + (c * spacing.x),
                    tempStartPos.y - (r * spacing.y)
                );

                GameObject newBlock = Instantiate(blockPrefab, spawnPos, Quaternion.identity, transform);

                // Garante que o bloco tenha a tag Block
                if (!newBlock.CompareTag("Block"))
                {
                    newBlock.tag = "Block";
                }

                // Se o caractere for um dígito (ex: '1', '2', '3'), escolhe o sprite correspondente
                if (char.IsDigit(ch) && blockSprites != null && blockSprites.Length > 0)
                {
                    int spriteIndex = (ch - '1'); // '1' vira 0, '2' vira 1, etc.
                    if (spriteIndex >= 0 && spriteIndex < blockSprites.Length)
                    {
                        SpriteRenderer sr = newBlock.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sprite = blockSprites[spriteIndex];
                        }
                    }
                }

                spawnedBlocks.Add(newBlock);
            }
        }

        // Centralização Matemática Perfeita com base no Bounding Box dos blocos gerados
        if (spawnedBlocks.Count > 0)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (GameObject b in spawnedBlocks)
            {
                float posX = b.transform.position.x;
                float posY = b.transform.position.y;

                if (posX < minX) minX = posX;
                if (posX > maxX) maxX = posX;
                if (posY > maxY) maxY = posY;
            }

            // Calcula a diferença exata para centralizar na câmera (X) e encostar no topo (Y)
            float currentCenterX = (minX + maxX) / 2.0f;
            float targetCenterX = cam.transform.position.x;
            float shiftX = targetCenterX - currentCenterX;

            float targetTopY = cam.transform.position.y + cam.orthographicSize - topMargin;
            float shiftY = targetTopY - maxY;

            // Aplica o deslocamento perfeito a todos os blocos
            Vector3 offset = new Vector3(shiftX, shiftY, 0f);
            foreach (GameObject b in spawnedBlocks)
            {
                b.transform.position += offset;
            }
        }
    }
}
