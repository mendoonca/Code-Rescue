using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuPrincipalManager : MonoBehaviour
{
    public static MenuPrincipalManager Instance { get; private set; }

    [Header("Painéis")]
    public GameObject painelMenuPrincipal;
    public GameObject painelProgresso;
    public GameObject painelInstrucoes;

    [Header("Botões de Foco Inicial")]
    public Button primeiroBotaoMenu; // Arrastar o IniciarMissaoButton aqui
    public Button botaoVoltarProgresso;
    public Button botaoVoltarInstrucoes;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MostrarMenuPrincipal();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Atalho com o novo Input System para voltar ao menu com Backspace
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            if ((painelProgresso != null && painelProgresso.activeInHierarchy) ||
                (painelInstrucoes != null && painelInstrucoes.activeInHierarchy))
            {
                MostrarMenuPrincipal();
            }
        }
    }

    public void MostrarMenuPrincipal()
    {
        if (painelMenuPrincipal != null) painelMenuPrincipal.SetActive(true);
        if (painelProgresso != null) painelProgresso.SetActive(false);
        if (painelInstrucoes != null) painelInstrucoes.SetActive(false);

        // Foca o botão inicial do menu principal
        FocarBotao(primeiroBotaoMenu);
    }

    public void AbrirProgresso()
    {
        if (painelMenuPrincipal != null) painelMenuPrincipal.SetActive(false);
        if (painelInstrucoes != null) painelInstrucoes.SetActive(false);
        
        if (painelProgresso != null)
        {
            painelProgresso.SetActive(true);
            MenuProgressoUI progUI = painelProgresso.GetComponent<MenuProgressoUI>();
            if (progUI != null) progUI.AtualizarInterfaceProgresso();
        }

        FocarBotao(botaoVoltarProgresso);
    }

    public void AbrirInstrucoes()
    {
        if (painelMenuPrincipal != null) painelMenuPrincipal.SetActive(false);
        if (painelProgresso != null) painelProgresso.SetActive(false);
        
        if (painelInstrucoes != null)
        {
            painelInstrucoes.SetActive(true);
        }

        FocarBotao(botaoVoltarInstrucoes);
    }

    private void FocarBotao(Button btn)
    {
        if (btn != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            btn.Select();
            EventSystem.current.SetSelectedGameObject(btn.gameObject);
        }
    }

    public void IniciarMissao()
    {
        SceneManager.LoadScene("EscolhaEquipamento");
    }

    public void SairJogo()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}