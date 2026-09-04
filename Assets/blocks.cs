using UnityEngine;

public class blocks : MonoBehaviour
{
    public int points = 1; // Pontos concedidos ao destruir este bloco

    // Desativa o bloco em vez de destruí-lo permanentemente (permite reiniciar o jogo)
    public void DestroyBlock()
    {
        gameObject.SetActive(false);
    }

    // Reativa o bloco para reiniciar a partida
    public void ResetBlock()
    {
        gameObject.SetActive(true);
    }
}


