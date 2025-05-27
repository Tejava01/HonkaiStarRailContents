using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IsekaiMaterialSimulator : MonoBehaviour
{
    [SerializeField] IsekaiMaterialBlockEnd endBlock;
    [SerializeField] IsekaiMaterialBlock[] Block;

    //------------------------------------------------
    private void Awake()
    {
        SettingArriveEvent();
    }

    //------------------------------------------------

    private void SettingArriveEvent()
    {
        endBlock.onPlayerArrive += CheckArrive;
    }

    private void CheckArrive()
    {
        foreach (var block in Block)
        {
            if (block.Visited == false)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(currentScene.name);
            }   
        }
        Debug.Log("성공");
    }
}
