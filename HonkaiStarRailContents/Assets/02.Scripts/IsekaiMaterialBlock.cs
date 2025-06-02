using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IsekaiMaterialBlock : MonoBehaviour
{
    [SerializeField] Material visitedMat;

    private bool visited = false;

    //------------------------------------------

    public bool Visited { get { return visited; }}

    //------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (visited == true)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }


        if (other.gameObject.tag == GroupConst.player)
        {
            visited = true;
            gameObject.GetComponent<MeshRenderer>().material = visitedMat;
        }
    }
}
