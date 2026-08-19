using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private string nomeDoNivelDeJogo;
    [SerializeField] private GameObject painelMenuPrincipal;
    [SerializeField] private GameObject painelInstrucoes;
    [SerializeField] private GameObject painelProgresso;

    void Update()
    {
        // Se carregar no Backspace, volta ao menu principal se as instruções OU o progresso estiverem abertos
        bool painelAberto = (painelInstrucoes != null && painelInstrucoes.activeSelf) || 
                            (painelProgresso != null && painelProgresso.activeSelf);

        if (painelAberto)
        {
            if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                voltarAoMenuPrincipal();
            }
        }
    }

    public void jogar()
    {
        SceneManager.LoadScene(nomeDoNivelDeJogo);
        
    }

    public void progresso()
    {
        painelMenuPrincipal.SetActive(false);
        painelProgresso.SetActive(true);
    }

    public void instrucoes()
    {
        painelMenuPrincipal.SetActive(false);
        painelInstrucoes.SetActive(true);
    }

    public void voltarAoMenuPrincipal()
    {
        painelInstrucoes.SetActive(false);
        painelProgresso.SetActive(false);
        painelMenuPrincipal.SetActive(true);
    }

    public void sair()
    {  
        Debug.Log("Sair do jogo");
        Application.Quit();
    }
}
