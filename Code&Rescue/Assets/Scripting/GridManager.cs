using System.Collections.Generic;
using UnityEngine;

// Tipos de blocos que podem existir no mapa
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

    [Header("Sprites de Base (Chão)")]
    public Sprite spriteCelulaBase; // Chão/Tile vazio (opcional)

    [Header("Sprites Individuais")]
    public Sprite spriteAguaInundacao;
    public Sprite spriteCasa;
    public Sprite spriteCasaIncendio;
    public Sprite spriteCasaInundacao;
    public Sprite spriteChama;
    public Sprite spriteDestrocos;
    public Sprite spriteHospital;
    public Sprite spriteMuroSacoAreia;
    public Sprite spritePredioSismoTerramoto;

    [Header("Sprites com Variações / Skins Aleatórias")]
    public Sprite[] spritesArvores;            // Várias skins de árvores
    public Sprite[] spritesPessoas;            // Várias skins de pessoas
    public Sprite[] spritesPessoasInundacao;   // Várias skins de pessoas na água

    private TipoElemento[,] matrizElementos;
    private GameObject[,] matrizObjetosInstanciados;

    public Vector2Int PosicaoInicialJogador { get; private set; } = new Vector2Int(0, 0);

    // Configuração do Singleton
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Inicializa o mapa ao carregar a cena
    private void Start()
    {
        InicializarGrelha();
    }

    // Cria as matrizes de memória e instancia o chão individual célula a célula
    public void InicializarGrelha()
    {
        matrizElementos = new TipoElemento[largura, altura];
        matrizObjetosInstanciados = new GameObject[largura, altura];

        // 1. Cria uma célula de chão para cada coordenada (X, Y)
        for (int x = 0; x < largura; x++)
        {
            for (int y = 0; y < altura; y++)
            {
                matrizElementos[x, y] = TipoElemento.Vazio;
                if (spriteCelulaBase != null)
                {
                    CriarObjetoVisual(new Vector2Int(x, y), $"Chao_{x}_{y}", spriteCelulaBase, -1);
                }
            }
        }

        // 2. Carrega os elementos sobre as respetivas células
        CarregarCenarioTeste();
    }

    // Configura o mapa de teste inicial respeitando a escolha entre Drone e Robô
    private void CarregarCenarioTeste()
    {
        bool isDrone = (GameManager.Instance != null && GameManager.Instance.EquipamentoSelecionado == TipoEquipamento.Drone);

        // Obstáculos fixos
        DefinirElemento(new Vector2Int(1, 1), TipoElemento.Casa);
        DefinirElemento(new Vector2Int(2, 3), TipoElemento.Arvores);
        DefinirElemento(new Vector2Int(3, 1), TipoElemento.Destrocos);

        // Água de inundação
        DefinirElemento(new Vector2Int(1, 2), TipoElemento.AguaInundacao);

        // Vítima a resgatar (escolhe automaticamente uma skin aleatória)
        DefinirElemento(new Vector2Int(3, 2), TipoElemento.Pessoas);

        // O Hospital só aparece na missão do Robô
        if (!isDrone)
        {
            DefinirElemento(new Vector2Int(4, 4), TipoElemento.Hospital);
        }
    }

    // Define um elemento na grelha e seleciona a skin adequada
    public void DefinirElemento(Vector2Int coord, TipoElemento tipo)
    {
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura) return;

        // Remove o elemento anterior se existir nessa coordenada
        if (matrizObjetosInstanciados[coord.x, coord.y] != null)
        {
            Destroy(matrizObjetosInstanciados[coord.x, coord.y]);
        }

        matrizElementos[coord.x, coord.y] = tipo;

        Sprite spriteEscolhido = ObterSpritePorTipo(tipo);
        if (spriteEscolhido != null)
        {
            GameObject obj = CriarObjetoVisual(coord, tipo.ToString(), spriteEscolhido, 0);
            matrizObjetosInstanciados[coord.x, coord.y] = obj;
        }
    }

    // Escolhe o Sprite correto, aplicando sorteio aleatório se tiver múltiplas skins
    private Sprite ObterSpritePorTipo(TipoElemento tipo)
    {
        switch (tipo)
        {
            case TipoElemento.AguaInundacao: return spriteAguaInundacao;
            case TipoElemento.Casa: return spriteCasa;
            case TipoElemento.CasaIncendio: return spriteCasaIncendio;
            case TipoElemento.CasaInundacao: return spriteCasaInundacao;
            case TipoElemento.Chama: return spriteChama;
            case TipoElemento.Destrocos: return spriteDestrocos;
            case TipoElemento.Hospital: return spriteHospital;
            case TipoElemento.MuroSacoAreia: return spriteMuroSacoAreia;
            case TipoElemento.PredioSismoTerramoto: return spritePredioSismoTerramoto;

            // Seleção aleatória de skin para as árvores
            case TipoElemento.Arvores:
                if (spritesArvores != null && spritesArvores.Length > 0)
                    return spritesArvores[Random.Range(0, spritesArvores.Length)];
                break;

            // Seleção aleatória de skin para pessoas
            case TipoElemento.Pessoas:
                if (spritesPessoas != null && spritesPessoas.Length > 0)
                    return spritesPessoas[Random.Range(0, spritesPessoas.Length)];
                break;

            // Seleção aleatória de skin para pessoas em inundação
            case TipoElemento.PessoasInundacao:
                if (spritesPessoasInundacao != null && spritesPessoasInundacao.Length > 0)
                    return spritesPessoasInundacao[Random.Range(0, spritesPessoasInundacao.Length)];
                break;
        }
        return null;
    }

    // Cria o GameObject no mundo e ajusta a escala para preencher exatamente 1 célula
    private GameObject CriarObjetoVisual(Vector2Int coord, string nome, Sprite sprite, int sortingOrder)
    {
        GameObject obj = new GameObject(nome);
        obj.transform.parent = transform;
        obj.transform.position = ObterPosicaoMundo(coord);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        if (sprite != null)
        {
            float larguraSprite = sprite.bounds.size.x;
            float alturaSprite = sprite.bounds.size.y;

            if (larguraSprite > 0 && alturaSprite > 0)
            {
                float escalaX = tamanhoCelula / larguraSprite;
                float escalaY = tamanhoCelula / alturaSprite;
                obj.transform.localScale = new Vector3(escalaX, escalaY, 1f);
            }
        }

        return obj;
    }

    // Converte coordenada da matriz (X, Y) para posição no mundo Unity
    public Vector3 ObterPosicaoMundo(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tamanhoCelula, -gridPos.y * tamanhoCelula, 0);
    }

    // Valida se o veículo pode passar para a célula
    public bool PodeMoverPara(Vector2Int coord, bool isDrone)
    {
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura)
            return false;

        TipoElemento elemento = matrizElementos[coord.x, coord.y];

        // Obstáculos que bloqueiam sempre
        if (elemento == TipoElemento.Casa ||
            elemento == TipoElemento.CasaIncendio ||
            elemento == TipoElemento.CasaInundacao ||
            elemento == TipoElemento.PredioSismoTerramoto ||
            elemento == TipoElemento.Destrocos ||
            elemento == TipoElemento.Arvores)
        {
            return false;
        }

        // Água de inundação
        if (elemento == TipoElemento.AguaInundacao)
        {
            return isDrone; // Drone voa por cima, Robô só passa se já tiver saco de areia
        }

        return true;
    }

    // Transforma a célula de água em saco de areia transitável
    public bool ColocarSacoDeAreia(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura)
            return false;

        if (matrizElementos[coord.x, coord.y] == TipoElemento.AguaInundacao)
        {
            DefinirElemento(coord, TipoElemento.MuroSacoAreia);
            return true;
        }

        return false;
    }

    // Consulta o elemento presente numa coordenada
    public TipoElemento ObterTipoElemento(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= largura || coord.y < 0 || coord.y >= altura)
            return TipoElemento.Vazio;

        return matrizElementos[coord.x, coord.y];
    }

    // Limpa a célula (usado após resgate da vítima)
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