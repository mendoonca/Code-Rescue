using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TipoComando
{
    MoverFrente,
    MoverTras,
    MoverEsquerda,
    MoverDireita,
    ApagarFogo,
    ColocarSaco,
    ResgatarPessoa,
    LargarKitMedico,
    IniciarCiclo,
    EncerrarCiclo,
    IniciarSe,
    EncerrarSe
}

public enum TipoCondicao
{
    FrenteTemFogo,
    FrenteTemAgua,
    FrenteTemPessoa,
    CaminhoBloqueado
}

[System.Serializable]
public class BlocoComando
{
    public TipoComando tipo;
    public int valorCiclo = 2;
    public TipoCondicao condicao;

    public BlocoComando(TipoComando tipo, int valor = 2, TipoCondicao cond = TipoCondicao.FrenteTemFogo)
    {
        this.tipo = tipo;
        this.valorCiclo = valor;
        this.condicao = cond;
    }

    public override string ToString()
    {
        switch (tipo)
        {
            case TipoComando.MoverFrente: return "Mover_Cima()";
            case TipoComando.MoverTras:   return "Mover_Baixo()";
            case TipoComando.MoverEsquerda: return "Mover_Esquerda()";
            case TipoComando.MoverDireita: return "Mover_Direita()";
            case TipoComando.ApagarFogo: return "Apagar_Fogo()";
            case TipoComando.ColocarSaco: return "Colocar_Saco()";
            case TipoComando.ResgatarPessoa: return "Resgatar_Pessoa()";
            case TipoComando.LargarKitMedico: return "Largar_Kit_Medico()";
            case TipoComando.IniciarCiclo: return $"Ciclo({valorCiclo}x) {{";
            case TipoComando.EncerrarCiclo: return "}";
            case TipoComando.IniciarSe: return $"If ({condicao}) {{";
            case TipoComando.EncerrarSe: return "}";
            default: return tipo.ToString();
        }
    }
}

public class ExecutadorComandos : MonoBehaviour
{
    public static ExecutadorComandos Instance { get; private set; }

    [Header("UI - Visualização de Linhas de Código")]
    public Transform containerAlgoritmo;
    public GameObject prefabLinhaCodigo;
    public TextMeshProUGUI textoContadorLinhas;
    public ScrollRect algoritmoScrollRect;

    private List<BlocoComando> algoritmo = new List<BlocoComando>();
    private bool emExecucao = false;

    private void Awake()
    {
        Instance = this;
    }

    // Comandos de Movimento Direto
    public void AdicionarMoverFrente()   => Inserir(new BlocoComando(TipoComando.MoverFrente));
    public void AdicionarMoverTras()     => Inserir(new BlocoComando(TipoComando.MoverTras));
    public void AdicionarMoverEsquerda() => Inserir(new BlocoComando(TipoComando.MoverEsquerda));
    public void AdicionarMoverDireita()  => Inserir(new BlocoComando(TipoComando.MoverDireita));

    // Comandos Selecionados via Popups
    public void InserirCiclo(int repeticoes) => Inserir(new BlocoComando(TipoComando.IniciarCiclo, repeticoes));
    public void AdicionarEncerrarCiclo()     => Inserir(new BlocoComando(TipoComando.EncerrarCiclo));

    public void InserirAcao(TipoComando acao) => Inserir(new BlocoComando(acao));

    public void InserirSe(TipoCondicao condicao) => Inserir(new BlocoComando(TipoComando.IniciarSe, 1, condicao));
    public void AdicionarEncerrarSe()            => Inserir(new BlocoComando(TipoComando.EncerrarSe));

    private void Inserir(BlocoComando bloco)
    {
        if (emExecucao) return;
        algoritmo.Add(bloco);
        AtualizarUI();
    }

    public void LimparTerminal()
    {
        if (emExecucao) return;
        algoritmo.Clear();
        AtualizarUI();
    }

    private void AtualizarUI()
    {
        if (textoContadorLinhas != null)
            textoContadorLinhas.text = $"Algoritmo ({algoritmo.Count})";

        if (containerAlgoritmo != null && prefabLinhaCodigo != null)
        {
            foreach (Transform child in containerAlgoritmo)
                Destroy(child.gameObject);

            int tabulacao = 0;
            for (int i = 0; i < algoritmo.Count; i++)
            {
                BlocoComando cmd = algoritmo[i];
                if (cmd.tipo == TipoComando.EncerrarCiclo || cmd.tipo == TipoComando.EncerrarSe)
                    tabulacao = Mathf.Max(0, tabulacao - 1);

                GameObject linha = Instantiate(prefabLinhaCodigo, containerAlgoritmo);
                TextMeshProUGUI txt = linha.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    string espacos = new string(' ', tabulacao * 4);
                    txt.text = $"{i + 1}.  {espacos}{cmd}";
                }

                if (cmd.tipo == TipoComando.IniciarCiclo || cmd.tipo == TipoComando.IniciarSe)
                    tabulacao++;
            }
        }
        StartCoroutine(DescerScrollAteFim());
    }

    public void RemoverUltimoComando()
    {
        if (emExecucao || algoritmo.Count == 0) return;

        // 1. Remove o comando da lista lógica
        algoritmo.RemoveAt(algoritmo.Count - 1);

        // 2. Destrói visualmente o último texto instanciado no Content
        if (containerAlgoritmo != null && containerAlgoritmo.childCount > 0)
        {
            Transform ultimoFilho = containerAlgoritmo.GetChild(containerAlgoritmo.childCount - 1);
            Destroy(ultimoFilho.gameObject);
        }

        // 3. Atualiza o texto do contador se existir
        if (textoContadorLinhas != null)
        {
            textoContadorLinhas.text = $"Algoritmo ({algoritmo.Count})";
        }

        Debug.Log("Última linha de comando removida via Backspace.");
    }

    public void IniciarExecucao()
    {
        if (!emExecucao && algoritmo.Count > 0)
        {
            if (PlayerProgressManager.Instance != null)
                PlayerProgressManager.Instance.IniciarMissao();

            StartCoroutine(ProcessarAlgoritmo());
        }
    }

    private IEnumerator ProcessarAlgoritmo()
    {
        emExecucao = true;
        Stack<int> pilhaIndicesCiclo = new Stack<int>();
        Stack<int> pilhaContadoresCiclo = new Stack<int>();
        int ponteiro = 0;

        while (ponteiro < algoritmo.Count)
        {
            BlocoComando atual = algoritmo[ponteiro];

            switch (atual.tipo)
            {
                case TipoComando.MoverFrente:
                    yield return StartCoroutine(VeiculoAgente.Instance.MoverUmBloco(new Vector2Int(0, -1)));
                    break;
                case TipoComando.MoverTras:
                    yield return StartCoroutine(VeiculoAgente.Instance.MoverUmBloco(new Vector2Int(0, 1)));
                    break;
                case TipoComando.MoverEsquerda:
                    yield return StartCoroutine(VeiculoAgente.Instance.MoverUmBloco(new Vector2Int(-1, 0)));
                    break;
                case TipoComando.MoverDireita:
                    yield return StartCoroutine(VeiculoAgente.Instance.MoverUmBloco(new Vector2Int(1, 0)));
                    break;

                case TipoComando.ApagarFogo:
                    VeiculoAgente.Instance.ExecutarApagarFogo();
                    break;
                case TipoComando.ColocarSaco:
                    VeiculoAgente.Instance.ExecutarColocarSacoAreia();
                    break;
                case TipoComando.ResgatarPessoa:
                    VeiculoAgente.Instance.ExecutarResgatarPessoa();
                    break;
                case TipoComando.LargarKitMedico:
                    VeiculoAgente.Instance.ExecutarLargarKit();
                    break;

                case TipoComando.IniciarCiclo:
                    pilhaIndicesCiclo.Push(ponteiro);
                    pilhaContadoresCiclo.Push(atual.valorCiclo);
                    break;

                case TipoComando.EncerrarCiclo:
                    if (pilhaContadoresCiclo.Count > 0)
                    {
                        int restante = pilhaContadoresCiclo.Pop() - 1;
                        int inicio = pilhaIndicesCiclo.Pop();

                        if (restante > 0)
                        {
                            pilhaContadoresCiclo.Push(restante);
                            pilhaIndicesCiclo.Push(inicio);
                            ponteiro = inicio;
                        }
                    }
                    break;

                case TipoComando.IniciarSe:
                    if (!AvaliarCondicao(atual.condicao))
                    {
                        ponteiro = EncontrarFimSe(ponteiro);
                    }
                    break;

                case TipoComando.EncerrarSe:
                    break;
            }

            // Se o comando acabou de ser executado e o robô está em perigo sem ter apagado/bloqueado:
            if (VeiculoAgente.Instance.EstaEmPerigoMortal())
            {
                // Verifica se o comando seguinte neutraliza a ameaça
                bool proximoSalva = false;
                if (ponteiro + 1 < algoritmo.Count)
                {
                    TipoComando prox = algoritmo[ponteiro + 1].tipo;
                    TipoElemento perigoAtual = GridManager.Instance.ObterTipoElemento(VeiculoAgente.Instance.PosicaoAtual);

                    if (perigoAtual == TipoElemento.Chama && prox == TipoComando.ApagarFogo)
                        proximoSalva = true;
                    else if (perigoAtual == TipoElemento.AguaInundacao && prox == TipoComando.ColocarSaco)
                        proximoSalva = true;
                }

                if (!proximoSalva)
                {
                    yield return new WaitForSeconds(0.4f);
                    Debug.LogWarning("O jogador entrou no perigo e não usou a ação necessária a seguir!");
                    VeiculoAgente.Instance.ExecutarDerrota();
                    emExecucao = false;
                    yield break;
                }
            }

            ponteiro++;
            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(0.4f);

        if (VeiculoAgente.Instance != null && !VeiculoAgente.Instance.MissaoConcluida && !VeiculoAgente.Instance.TeveDerrota)
        {
            VeiculoAgente.Instance.ReporPosicaoInicial();
        }

        emExecucao = false;
    }

    private bool AvaliarCondicao(TipoCondicao cond)
    {
        switch (cond)
        {
            case TipoCondicao.FrenteTemFogo: return VeiculoAgente.Instance.TemFogoAFrente();
            case TipoCondicao.FrenteTemAgua: return VeiculoAgente.Instance.TemAguaAFrente();
            case TipoCondicao.FrenteTemPessoa: return VeiculoAgente.Instance.TemVitimaAFrente();
            case TipoCondicao.CaminhoBloqueado: return VeiculoAgente.Instance.TemObstaculoAFrente();
            default: return false;
        }
    }

    private int EncontrarFimSe(int inicio)
    {
        int nivel = 0;
        for (int i = inicio; i < algoritmo.Count; i++)
        {
            if (algoritmo[i].tipo == TipoComando.IniciarSe) nivel++;
            else if (algoritmo[i].tipo == TipoComando.EncerrarSe)
            {
                nivel--;
                if (nivel == 0) return i;
            }
        }
        return algoritmo.Count - 1;
    }

    private IEnumerator DescerScrollAteFim()
    {
        yield return null; // Espera a destruição/instanciação dos GameObjects
        yield return new WaitForEndOfFrame(); // Espera a frame de renderização terminar

        if (algoritmoScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            
            // Força a atualização da altura física do Content
            if (containerAlgoritmo != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerAlgoritmo.GetComponent<RectTransform>());
            }

            Canvas.ForceUpdateCanvases();
            algoritmoScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}