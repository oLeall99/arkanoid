using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("Configurações do Grid")]
    public int rows = 4;           // Quantidade de linhas
    public int columns = 8;        // Quantidade de colunas
    public Vector2 spacing = new Vector2(1.2f, 0.6f); // Distância entre cada bloco

    [Header("Referências")]
    public GameObject blockPrefab; // Seu Prefab de bloco
    public Sprite[] blockSprites;  // Sprites de cores diferentes (um por linha)

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Ponto inicial relativo à posição do objeto pai (blocks)
        Vector2 startPos = (Vector2)transform.position;

        for (int r = 0; r < rows; r++)
        {
            // Escolhe o sprite da linha atual (se houver sprites configurados)
            Sprite rowSprite = null;
            if (blockSprites.Length > 0)
            {
                rowSprite = blockSprites[r % blockSprites.Length];
            }

            for (int c = 0; c < columns; c++)
            {
                // Calcula a posição (deslocamento X e Y negativo para descer as linhas)
                Vector2 spawnPos = new Vector2(
                    startPos.x + (c * spacing.x),
                    startPos.y - (r * spacing.y)
                );

                // Cria o bloco e define o objeto pai
                GameObject newBlock = Instantiate(blockPrefab, spawnPos, Quaternion.identity, transform);

                // Troca a imagem/sprite do bloco
                if (rowSprite != null)
                {
                    newBlock.GetComponent<SpriteRenderer>().sprite = rowSprite;
                }
            }
        }
    }
}