using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class RoboNiveisManager : MonoBehaviour
{

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            Voltar();
        }
    }

    // Botão 1: Incêndio Florestal (Nível 1 - 5x5)
    public void JogarIncendio()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.NivelSelecionado = 1;

        SceneManager.LoadScene("MapaJogo");
    }

    // Botão 2: Inundação Urbana (Nível 2 - 7x7)
    public void JogarInundacao()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.NivelSelecionado = 2;

        SceneManager.LoadScene("MapaJogo");
    }

    // Botão 3: Sismo / Terramoto (Nível 3 - 9x9)
    public void JogarSismo()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.NivelSelecionado = 3;

        SceneManager.LoadScene("MapaJogo");
    }

    // Botão Voltar: regressa à escolha de equipamento
    public void Voltar()
    {
        SceneManager.LoadScene("EscolhaEquipamento");
    }
    
}