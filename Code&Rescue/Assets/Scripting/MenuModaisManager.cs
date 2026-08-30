using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuModaisManager : MonoBehaviour
{
    public static MenuModaisManager Instance { get; private set; }

    [Header("Navegação Principal")]
    public List<Button> botoesNavegacao = new List<Button>();
    private int indiceBotaoAtual = 0;

    [Header("Paineis Popups")]
    public GameObject popupCiclo;
    public GameObject popupAcao;
    public GameObject popupIf;

    [Header("Paineis Fim de Jogo")]
    public GameObject painelVitoria;
    public GameObject painelDerrota;

    [Header("Botoes Exclusivos no Popup de Acao")]
    public GameObject btnColocarSaco;
    public GameObject btnResgatarPessoa;
    public GameObject btnLargarKitMedico;
    public GameObject btnApagarFogo;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FecharTodosPopups();
        if (painelVitoria != null) painelVitoria.SetActive(false);
        if (painelDerrota != null) painelDerrota.SetActive(false);

        FocarPrimeiroDisponivel();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // 1. Tecla TAB: Alterna entre botões do contexto ativo (Popup ou Ecrã Principal)
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            AvancarSelecaoBotao();
        }

        // 2. Tecla BACKSPACE: Remove a última linha apenas se nenhum popup estiver aberto
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            if (!TemPopupAberto() && ExecutadorComandos.Instance != null)
            {
                ExecutadorComandos.Instance.RemoverUltimoComando();
            }
        }
    }

    private List<Button> ObterBotoesAtivosContexto()
    {
        GameObject popupAtivo = ObterPopupAtivo();

        if (popupAtivo != null)
        {
            // Se houver um popup aberto, o Tab navega apenas pelos botões dentro dele
            Button[] botoesNoPopup = popupAtivo.GetComponentsInChildren<Button>(false);
            List<Button> listaPopup = new List<Button>();
            foreach (var b in botoesNoPopup)
            {
                if (b.interactable && b.gameObject.activeInHierarchy)
                    listaPopup.Add(b);
            }
            return listaPopup;
        }

        // Caso contrário, navega pela lista principal
        return botoesNavegacao.FindAll(b => b != null && b.gameObject.activeInHierarchy && b.interactable);
    }

    private GameObject ObterPopupAtivo()
    {
        if (popupCiclo != null && popupCiclo.activeInHierarchy) return popupCiclo;
        if (popupAcao != null && popupAcao.activeInHierarchy) return popupAcao;
        if (popupIf != null && popupIf.activeInHierarchy) return popupIf;
        if (painelVitoria != null && painelVitoria.activeInHierarchy) return painelVitoria;
        if (painelDerrota != null && painelDerrota.activeInHierarchy) return painelDerrota;
        return null;
    }

    private bool TemPopupAberto()
    {
        return ObterPopupAtivo() != null;
    }

    private void AvancarSelecaoBotao()
    {
        List<Button> botoes = ObterBotoesAtivosContexto();
        if (botoes == null || botoes.Count == 0) return;

        // Localiza o botão atualmente selecionado na lista ativa
        GameObject selecionado = EventSystem.current.currentSelectedGameObject;
        int index = -1;
        if (selecionado != null)
        {
            index = botoes.FindIndex(b => b.gameObject == selecionado);
        }

        indiceBotaoAtual = (index + 1) % botoes.Count;
        botoes[indiceBotaoAtual].Select();
        EventSystem.current.SetSelectedGameObject(botoes[indiceBotaoAtual].gameObject);
    }

    private void FocarPrimeiroDisponivel()
    {
        List<Button> botoes = ObterBotoesAtivosContexto();
        if (botoes != null && botoes.Count > 0 && botoes[0] != null)
        {
            indiceBotaoAtual = 0;
            botoes[0].Select();
            EventSystem.current.SetSelectedGameObject(botoes[0].gameObject);
        }
    }

    // --- POPUPS DE PROGRAMAÇÃO ---
    public void AbrirPopupCiclo()
    {
        FecharTodosPopups();
        if (popupCiclo != null) popupCiclo.SetActive(true);
        FocarPrimeiroDisponivel();
    }

    public void AbrirPopupAcao()
    {
        FecharTodosPopups();
        if (popupAcao != null) popupAcao.SetActive(true);

        bool isDrone = (GameManager.Instance == null || GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone);

        if (btnApagarFogo != null) btnApagarFogo.SetActive(true);
        if (btnLargarKitMedico != null) btnLargarKitMedico.SetActive(isDrone);
        if (btnColocarSaco != null) btnColocarSaco.SetActive(!isDrone);
        if (btnResgatarPessoa != null) btnResgatarPessoa.SetActive(!isDrone);

        FocarPrimeiroDisponivel();
    }

    public void AbrirPopupIf()
    {
        FecharTodosPopups();
        if (popupIf != null) popupIf.SetActive(true);
        FocarPrimeiroDisponivel();
    }

    public void FecharTodosPopups()
    {
        if (popupCiclo != null) popupCiclo.SetActive(false);
        if (popupAcao != null) popupAcao.SetActive(false);
        if (popupIf != null) popupIf.SetActive(false);

        // Ao fechar, devolve o foco para a grelha principal de comandos
        FocarPrimeiroDisponivel();
    }

    // --- SELEÇÕES CICLO ---
    public void SelecionarCiclo(int repeticoes)
    {
        ExecutadorComandos.Instance.InserirCiclo(repeticoes);
        FecharTodosPopups();
    }

    // --- SELEÇÕES AÇÃO ---
    public void SelecionarApagarFogo()    { ExecutadorComandos.Instance.InserirAcao(TipoComando.ApagarFogo); FecharTodosPopups(); }
    public void SelecionarColocarSaco()   { ExecutadorComandos.Instance.InserirAcao(TipoComando.ColocarSaco); FecharTodosPopups(); }
    public void SelecionarResgatarPessoa(){ ExecutadorComandos.Instance.InserirAcao(TipoComando.ResgatarPessoa); FecharTodosPopups(); }
    public void SelecionarLargarKit()     { ExecutadorComandos.Instance.InserirAcao(TipoComando.LargarKitMedico); FecharTodosPopups(); }

    // --- SELEÇÕES IF ---
    public void SelecionarIfFrenteFogo()       { ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemFogo); FecharTodosPopups(); }
    public void SelecionarIfFrenteAgua()       { ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemAgua); FecharTodosPopups(); }
    public void SelecionarIfFrentePessoa()     { ExecutadorComandos.Instance.InserirSe(TipoCondicao.FrenteTemPessoa); FecharTodosPopups(); }
    public void SelecionarIfCaminhoBloqueado() { ExecutadorComandos.Instance.InserirSe(TipoCondicao.CaminhoBloqueado); FecharTodosPopups(); }

    // --- TELAS DE FIM DE JOGO ---
    public void MostrarVitoria()
    {
        FecharTodosPopups();
        if (painelVitoria != null) painelVitoria.SetActive(true);
        FocarPrimeiroDisponivel();
    }

    public void MostrarDerrota()
    {
        FecharTodosPopups();
        if (painelDerrota != null) painelDerrota.SetActive(true);
        FocarPrimeiroDisponivel();
    }

    public void ReiniciarMissao()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarMenuNiveis()
    {
        if (GameManager.Instance != null && GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone)
            SceneManager.LoadScene("DroneNiveis");
        else
            SceneManager.LoadScene("RoboNiveis");
    }
}