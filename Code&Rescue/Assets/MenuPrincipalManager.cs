using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private string nomeDoNivelDeJogo;
    [SerializeField] private GameObject painelMenuPrincipal;
    [SerializeField] private GameObject painelInstrucoes;

    void Update()
    {
        // Se carregar no Backspace, volta no menu
        if (painelInstrucoes != null && painelInstrucoes.activeSelf)
        {
            if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                voltarAoMenuPrincipal();
            }
        }
    }

    public void jogar()
    {
        SceneManager.LoadScene("nomeDoNivelDeJogo");
        
    }

    public void progresso()
    {
        
    }

    public void instrucoes()
    {
        painelMenuPrincipal.SetActive(false);
        painelInstrucoes.SetActive(true);
    }

    public void voltarAoMenuPrincipal()
    {
        painelInstrucoes.SetActive(false);
        painelMenuPrincipal.SetActive(true);
    }

    public void sair()
    {  
        Debug.Log("Sair do jogo");
        Application.Quit();
    }
}
