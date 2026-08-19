using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DroneNiveisManager : MonoBehaviour
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
