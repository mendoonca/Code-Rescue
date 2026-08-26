using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoComando
{
    MoverFrente,     // Cima (Y - 1)
    MoverTras,       // Baixo (Y + 1)
    MoverEsquerda,   // Esquerda (X - 1)
    MoverDireita,    // Direita (X + 1)
    ColocarSaco,     // Ação do Robô
    IniciarCiclo,    // Ciclo (repetir X vezes)
    EncerrarCiclo,   // Fim do Ciclo
    IniciarSe,       // Se (condição)
    EncerrarSe       // Fim do Se
}

public enum TipoCondicao
{
    FrenteTemAgua,
    FrenteTemObstaculo,
    FrenteTemVitima
}

[System.Serializable]
public class BlocoComando
{
    public TipoComando tipo;
    public int valorCiclo = 2;
    public TipoCondicao condicao;

    public BlocoComando(TipoComando tipo, int valor = 1, TipoCondicao cond = TipoCondicao.FrenteTemAgua)
    {
        this.tipo = tipo;
        this.valorCiclo = valor;
        this.condicao = cond;
    }
}

public class ExecutadorComandos : MonoBehaviour
{
    public static ExecutadorComandos Instance { get; private set; }

    private List<BlocoComando> algoritmo = new List<BlocoComando>();
    private bool emExecucao = false;

    private void Awake()
    {
        Instance = this;
    }

    // Métodos para ligar aos botões UI
    public void AdicionarMoverFrente()   => Inserir(new BlocoComando(TipoComando.MoverFrente));
    public void AdicionarMoverTras()     => Inserir(new BlocoComando(TipoComando.MoverTras));
    public void AdicionarMoverEsquerda() => Inserir(new BlocoComando(TipoComando.MoverEsquerda));
    public void AdicionarMoverDireita()  => Inserir(new BlocoComando(TipoComando.MoverDireita));
    public void AdicionarColocarSaco()   => Inserir(new BlocoComando(TipoComando.ColocarSaco));

    public void AdicionarCiclo(int repeticoes) => Inserir(new BlocoComando(TipoComando.IniciarCiclo, repeticoes));
    public void AdicionarEncerrarCiclo()       => Inserir(new BlocoComando(TipoComando.EncerrarCiclo));

    public void AdicionarSe(TipoCondicao cond) => Inserir(new BlocoComando(TipoComando.IniciarSe, 1, cond));
    public void AdicionarEncerrarSe()          => Inserir(new BlocoComando(TipoComando.EncerrarSe));

    private void Inserir(BlocoComando bloco)
    {
        if (emExecucao) return;
        algoritmo.Add(bloco);
        Debug.Log($"Bloco inserido: {bloco.tipo}. Total: {algoritmo.Count}");
    }

    public void LimparTerminal()
    {
        if (emExecucao) return;
        algoritmo.Clear();
        Debug.Log("Terminal limpo.");
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

    // Executa as instruções com suporte a Loops e Ifs
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

                case TipoComando.ColocarSaco:
                    VeiculoAgente.Instance.ExecutarColocarSacoAreia();
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
                            ponteiro = inicio; // Volta para repetir o loop
                        }
                    }
                    break;

                case TipoComando.IniciarSe:
                    bool condicaoVerdadeira = AvaliarCondicao(atual.condicao);
                    if (!condicaoVerdadeira)
                    {
                        ponteiro = EncontrarFimSe(ponteiro); // Salta o bloco 'Se'
                    }
                    break;

                case TipoComando.EncerrarSe:
                    break;
            }

            ponteiro++;
            yield return new WaitForSeconds(0.2f);
        }

        emExecucao = false;
    }

    private bool AvaliarCondicao(TipoCondicao cond)
    {
        switch (cond)
        {
            case TipoCondicao.FrenteTemAgua: return VeiculoAgente.Instance.TemAguaAFrente();
            case TipoCondicao.FrenteTemObstaculo: return VeiculoAgente.Instance.TemObstaculoAFrente();
            case TipoCondicao.FrenteTemVitima: return VeiculoAgente.Instance.TemVitimaAFrente();
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
}