using UnityEngine;
using UnityEngine.SceneManagement;

public class RoboNiveisManager : MonoBehaviour
{

    public void jogarIncendio()
    {
        SceneManager.LoadScene("RoboIncendio");
    }

    public void jogarInundacao()
    {
        SceneManager.LoadScene("RoboInundacao");
    }

    public void jogarSismoTerramoto()
    {
        SceneManager.LoadScene("RoboSismoTerramoto");
    }

    public void VoltarAoMenuEquipamento()
    {
        SceneManager.LoadScene("EscolhaEquipamento");
    }
    
}
