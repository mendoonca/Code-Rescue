using UnityEngine;
using UnityEngine.SceneManagement;

public class DroneNiveisManager : MonoBehaviour
{

    public void jogarIncendio()
    {
        SceneManager.LoadScene("DroneIncendio");
    }

    public void jogarInundacao()
    {
        SceneManager.LoadScene("DroneInundacao");
    }

    public void jogarSismoTerramoto()
    {
        SceneManager.LoadScene("DroneSismoTerramoto");
    }

    public void VoltarAoMenuEquipamento()
    {
        SceneManager.LoadScene("EscolhaEquipamento");
    }
    
}
