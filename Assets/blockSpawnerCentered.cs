using System.Collections.Generic;
using UnityEngine;

public class blockSpawnerCentered : MonoBehaviour
{
    [Header("Configurações do Grid")]
    public int rows = 5;                           // Quantidade de linhas
    public int columns = 8;                        // Quantidade de colunas
    public Vector2 spacing = new Vector2(1.2f, 0.5f); // Distância entre cada bloco (X e Y)
    public float topMargin = 1.5f;                 // Distância em relação ao topo da câmera

    [Header("Referências")]
    public GameObject blockPrefab;                 // Prefab do bloco
    public Sprite[] blockSprites;                  // Sprites para alternar por linha (opcional)

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("blockSpawnerCentered: Nenhum blockPrefab foi atribuído no Inspector!");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("blockSpawnerCentered: Câmera principal (Camera.main) não encontrada!");
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
