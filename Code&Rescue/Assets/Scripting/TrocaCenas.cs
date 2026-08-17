using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocaCenas : MonoBehaviour
{
    public void VoltarAoMenuPrincipal()
    {
        SceneManager.LoadScene("Menu");
    }
}