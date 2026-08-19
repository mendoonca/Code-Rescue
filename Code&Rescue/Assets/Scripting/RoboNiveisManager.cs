using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class RoboNiveisManager : MonoBehaviour
{

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            VoltarAoMenuEquipamento();
        }
    }

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
