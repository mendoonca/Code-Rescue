using UnityEngine;

public enum TipoEquipamento
{
    Drone,
    Robo
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TipoEquipamento EquipamentoSelecionado { get; set; } = TipoEquipamento.Drone;
    
    // Guarda o nível escolhido (1 = Incêndio, 2 = Inundação, 3 = Sismo/Incêndio)
    public int NivelSelecionado { get; set; } = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}