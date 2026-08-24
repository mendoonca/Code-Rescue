using System.Collections.Generic;
using UnityEngine;

// Tipos de elementos/blocos que podem existir em cada célula do mapa
public enum TipoElemento
{
    Vazio,
    AguaInundacao,
    Arvores,
    Casa,
    CasaIncendio,
    CasaInundacao,
    Chama,
    Destrocos,
    Hospital,
    MuroSacoAreia,
    Pessoas,
    PessoasInundacao,
    PredioSismoTerramoto
}

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Dimensões e Espaçamento da Grelha")]
    public int largura = 5;
    public int altura = 5;
    public float tamanhoCelula = 1.2f;

    [Header("Prefabs de Terreno e Elementos")]
    public GameObject prefabCelulaBase; // Chão/Tile vazio de fundo
    public GameObject prefabAguaInundacao;
    public GameObject prefabArvores;
    public GameObject prefabCasa;
    public GameObject prefabCasaIncendio;
    public GameObject prefabCasaInundacao;
    public GameObject prefabChama;
    public GameObject prefabDestrocos;
    public GameObject prefabHospital;
    public GameObject prefabMuroSacoAreia;
    public GameObject prefabPessoas;
    public GameObject prefabPessoasInundacao;
    public GameObject prefabPredioSismoTerramoto;

    // Matrizes para guardar a lógica e as referências aos objetos instanciados
    private TipoElemento[,] matrizElementos;
    private GameObject[,] matrizObjetosInstanciados;

    // Posição onde o jogador começa a simulação
    public Vector2Int PosicaoInicialJogador { get; private set; } = new Vector2Int(0, 0);

    // Inicialização do Singleton para acesso fácil através de outros scripts
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Inicializa a grelha ao iniciar a cena
    private void Start()
    {
        InicializarGrelha();
    }

    // Aloca a memória das matrizes e gera um nível base de teste
    public void InicializarGrelha()
    {
        matrizElementos = new TipoElemento[largura, altura];
        matrizObjetosInstanciados = new GameObject[largura, altura];

        // 1. Cria a base visual de células vazias
        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                matrizElementos[x, y] = TipoElemento.Vazio;
                if (prefabCelulaBase != null)
                {
                    Instantiate(prefabCelulaBase, ObterPosicaoMundo(new Vector2Int(x, y)), Quaternion.identity, transform);
                }
            }
        }

        // 2. Carrega os elementos do cenário de exemplo
        CarregarCenarioTeste();
    }

    // Configura o mapa de teste inicial respeitando a escolha entre Drone e Robô
    private void CarregarCenarioTeste()
    {
        bool isDrone = (GameManager.Instance != null && GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone);

        // Define obstáculos fixos
        DefinirElemento(new Vector2Int(1, 1), TipoElemento.Casa, prefabCasa);
        DefinirElemento(new Vector2Int(2, 3), TipoElemento.Arvores, prefabArvores);
        DefinirElemento(new Vector2Int(3, 1), TipoElemento.Destrocos, prefabDestrocos);

        // Água de inundação no caminho
        DefinirElemento(new Vector2Int(1, 2), TipoElemento.AguaInundacao, prefabAguaInundacao);

        // Vítima a resgatar
        DefinirElemento(new Vector2Int(3, 2), TipoElemento.Pessoas, prefabPessoas);

        // O Hospital só é instanciado para o Robô (pois o Drone só entrega o kit no local)
        if (!isDrone)
        {
            DefinirElemento(new Vector2Int(4, 4), TipoElemento.Hospital, prefabHospital);
        }
    }

    // Instancia e regista um elemento específico numa coordenada da matriz
    public void DefinirElemento(Vector2Int coord, TipoElemento tipo, GameObject prefab)
    {
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura) return;

        // Remove objeto anterior se existir nessa coordenada
        if (matrizObjetosInstanciados[coord.x, coord.y] != null)
        {
            Destroy(matrizObjetosInstanciados[coord.x, coord.y]);
        }

        matrizElementos[coord.x, coord.y] = tipo;

        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, ObterPosicaoMundo(coord), Quaternion.identity, transform);
            matrizObjetosInstanciados[coord.x, coord.y] = obj;
        }
    }

    // Converte as coordenadas da grelha (x, y) em coordenadas do mundo Unity (Vector3)
    public Vector3 ObterPosicaoMundo(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tamanhoCelula, -gridPos.y * tamanhoCelula, 0);
    }

    // Valida se o veículo (Drone ou Robô) pode mover-se para a célula de destino
    public bool PodeMoverPara(Vector2Int coord, bool isDrone)
    {
        // 1. Fora dos limites da grelha
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura)
            return false;

        TipoElemento elemento = matrizElementos[coord.x, coord.y];

        // 2. Obstáculos intransponíveis para ambos
        if (elemento == TipoElemento.Casa ||
            elemento == TipoElemento.CasaIncendio ||
            elemento == TipoElemento.CasaInundacao ||
            elemento == TipoElemento.PredioSismoTerramoto ||
            elemento == TipoElemento.Destrocos ||
            elemento == TipoElemento.Arvores)
        {
            return false;
        }

        // 3. Água de inundação: Drone passa, Robô só passa se já tiver MuroSacoAreia
        if (elemento == TipoElemento.AguaInundacao)
        {
            return isDrone; // True para Drone, False para Robô
        }

        // Células Vazias, Vítimas, Hospital e MuroSacoAreia são sempre transitáveis
        return true;
    }

    // Ação do Robô: coloca saco de areia sobre a água para criar uma passagem segura
    public bool ColocarSacoDeAreia(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura)
            return false;

        if (matrizElementos[coord.x, coord.y] == TipoElemento.AguaInundacao)
        {
            DefinirElemento(coord, TipoElemento.MuroSacoAreia, prefabMuroSacoAreia);
            return true;
        }

        return false;
    }

    // Retorna o tipo de elemento presente numa determinada coordenada
    public TipoElemento ObterTipoElemento(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura)
            return TipoElemento.Vazio;

        return matrizElementos[coord.x, coord.y];
    }

    // Limpa o elemento de uma célula após o resgate da vítima
    public void LimparCelula(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura) return;

        if (matrizObjetosInstanciados[coord.x, coord.y] != null)
        {
            Destroy(matrizObjetosInstanciados[coord.x, coord.y]);
        }

        matrizElementos[coord.x, coord.y] = TipoElemento.Vazio;
    }
}