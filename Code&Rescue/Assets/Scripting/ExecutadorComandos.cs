using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tipos de ações e estruturas de controlo
public enum TipoComando
{
    MoverFrente,     // Cima (Y - 1)
    MoverTras,       // Baixo (Y + 1)
    MoverEsquerda,   // Esquerda (X - 1)
    MoverDireita,    // Direita (X + 1)
    ColocarSaco,     // Coloca saco na célula à frente (Robô)
    IniciarCiclo,    // Ciclo (repetir X vezes)
    EncerrarCiclo,   // Fim do Ciclo
    IniciarSe,       // Se (condição)
    EncerrarSe       // Fim do Se
}

// Tipos de condições para o bloco 'Se'
public enum TipoCondicao
{
    FrenteTemAgua,
    FrenteTemObstaculo,
    FrenteTemVitima
}

// Estrutura de cada bloco adicionado ao terminal
[System.Serializable]
public class BlocoComando
{
    public TipoComando tipo;
    public int valorCiclo = 2; // Quantidade de iterações do ciclo
    public TipoCondicao condicaoSe;

    public BlocoComando(TipoComando tipo, int valor = 1, TipoCondicao cond = TipoCondicao.FrenteTemAgua)
    {
        this.tipo = tipo;
        this.valorCiclo = valor;
        this.condicaoSe = cond;
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

    // Métodos para ligar aos botões do Canvas
    public void AdicionarMoverFrente()   => Inserir(new BlocoComando(TipoComando.MoverFrente));
    public void AdicionarMoverTras()     => Inserir(new BlocoComando(TipoComando.MoverTras));
    public void AdicionarMoverEsquerda() => Inserir(new BlocoComando(TipoComando.MoverEsquerda));
    public void AdicionarMoverDireita()  => Inserir(new BlocoComando(TipoComando.MoverDireita));
    public void AdicionarColocarSaco()   => Inserir(new BlocoComando(TipoComando.ColocarSaco));

    public void AdicionarCiclo(int repeticoes) => Inserir(new BlocoComando(TipoComando.IniciarCiclo, repeticoes));
    public void AdicionarEncerrarCiclo()       => Inserir(new BlocoComando(TipoComando.EncerrarCiclo));

    public void AdicionarSe(TipoCondicao cond) => Inserir(new BlocoComando(TipoComando.IniciarSe, 1, cond));
    public void AdicionarEncerrarSe()          => Inserir(new BlocoComando(TipoComando.EncerrarSe));

    // Insere o bloco na sequência
    private void Inserir(BlocoComando bloco)
    {
        if (emExecucao) return;
        algoritmo.Add(bloco);
        Debug.Log($"Bloco adicionado: {bloco.tipo}. Total: {algoritmo.Count}");
    }

    // Remove todos os blocos do terminal
    public void LimparTerminal()
    {
        if (emExecucao) return;
        algoritmo.Clear();
        Debug.Log("Terminal limpo.");
    }

    // Inicia a execução do algoritmo
    public void IniciarExecucao()
    {
        if (!emExecucao && algoritmo.Count > 0)
        {
            if (PlayerProgressManager.Instance != null)
                PlayerProgressManager.Instance.IniciarMissao();

            StartCoroutine(InterpretarAlgoritmo());
        }
    }

    // Interpretador sequencial com suporte a saltos de Loops e Condições
    private IEnumerator InterpretarAlgoritmo()
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
                        int contagemRestante = pilhaContadoresCiclo.Pop() - 1;
                        int indiceInicio = pilhaIndicesCiclo.Pop();

                        if (contagemRestante > 0)
                        {
                            pilhaContadoresCiclo.Push(contagemRestante);
                            pilhaIndicesCiclo.Push(indiceInicio);
                            ponteiro = indiceInicio; // Volta para dentro do ciclo
                        }
                    }
                    break;

                case TipoComando.IniciarSe:
                    bool condicaoVerdadeira = AvaliarCondicao(atual.condicaoSe);
                    if (!condicaoVerdadeira)
                    {
                        // Salta para a instrução a seguir ao EncerrarSe correspondente
                        ponteiro = EncontrarFimSe(ponteiro);
                    }
                    break;

                case TipoComando.EncerrarSe:
                    // Apenas marcador de fecho do bloco
                    break;
            }

            ponteiro++;
            yield return new WaitForSeconds(0.2f);
        }

        emExecucao = false;
    }

    // Avalia a condição do bloco 'Se'
    private bool AvaliarCondicao(TipoCondicao condicao)
    {
        switch (condicao)
        {
            case TipoCondicao.FrenteTemAgua:
                return VeiculoAgente.Instance.TemAguaAFrente();
            case TipoCondicao.FrenteTemObstaculo:
                return VeiculoAgente.Instance.TemObstaculoAFrente();
            case TipoCondicao.FrenteTemVitima:
                return VeiculoAgente.Instance.TemVitimaAFrente();
            default:
                return false;
        }
    }

    // Localiza o bloco EncerrarSe correspondente no caso de condição falsa
    private int EncontrarFimSe(int indiceInicio)
    {
        int profundidade = 0;
        for (int i = indiceInicio; i < algoritmo.Count; i++)
        {
            if (algoritmo[i].tipo == TipoComando.IniciarSe) profundidade++;
            else if (algoritmo[i].tipo == TipoComando.EncerrarSe)
            {
                profundidade--;
                if (profundidade == 0) return i;
            }
        }
        return algoritmo.Count - 1;
    }
}