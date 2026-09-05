using System.Collections.Generic;
using UnityEngine;

public class blockSpawner1 : MonoBehaviour
{
    [Header("Configurações do Grid")]
    public int rows = 4;                           // Quantidade de linhas
    public int columns = 8;                        // Quantidade de colunas
    public Vector2 spacing = new Vector2(1.2f, 0.6f); // Distância entre cada bloco
    public float topMargin = 1.5f;                 // Margem do topo da tela

    [Header("Níveis dos Blocos (Opcional)")]
    [Tooltip("Define a vida/resistência dos blocos por linha (ex: 6, 5, 4, 3, 2, 1). Se vazio, usa a vida padrão do prefab.")]
    public int[] rowHealth;

    [Header("Referências")]
    public GameObject blockPrefab;                 // Seu Prefab de bloco
    public Sprite[] blockSprites;                  // Sprites de cores diferentes (um por linha)

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("blockSpawner1: Nenhum blockPrefab foi atribuído no Inspector!");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("blockSpawner1: Câmera principal (Camera.main) não encontrada!");
            return;
        }

        List<GameObject> spawnedBlocks = new List<GameObject>();
        Vector2 tempStartPos = (Vector2)transform.position;

        for (int r = 0; r < rows; r++)
        {
            Sprite rowSprite = null;
            if (blockSprites != null && blockSprites.Length > 0)
            {
                rowSprite = blockSprites[r % blockSprites.Length];
            }

            for (int c = 0; c < columns; c++)
            {
                Vector2 spawnPos = new Vector2(
                    tempStartPos.x + (c * spacing.x),
                    tempStartPos.y - (r * spacing.y)
                );

                GameObject newBlock = Instantiate(blockPrefab, spawnPos, Quaternion.identity, transform);

                if (!newBlock.CompareTag("Block"))
                {
                    newBlock.tag = "Block";
                }

                if (rowSprite != null)
                {
                    SpriteRenderer sr = newBlock.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = rowSprite;
                    }
                }

                blocks blockComp = newBlock.GetComponent<blocks>();
                if (blockComp != null && rowHealth != null && rowHealth.Length > 0)
                {
                    int health = rowHealth[r % rowHealth.Length];
                    blockComp.SetHealth(health);
                }

                spawnedBlocks.Add(newBlock);
            }
        }

        // Centralização Matemática Perfeita com Bounding Box
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

            float currentCenterX = (minX + maxX) / 2.0f;
            float targetCenterX = cam.transform.position.x;
            float shiftX = targetCenterX - currentCenterX;

            float targetTopY = cam.transform.position.y + cam.orthographicSize - topMargin;
            float shiftY = targetTopY - maxY;

            Vector3 offset = new Vector3(shiftX, shiftY, 0f);
            foreach (GameObject b in spawnedBlocks)
            {
                b.transform.position += offset;
            }
        }
    }
}