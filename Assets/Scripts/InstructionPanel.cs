using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InstructionPanel : MonoBehaviour
{
    [SerializeField]Button playBtn;
    // Start is called before the first frame update
    void Start()
    {

        //playBtn.onClick.AddListener(GlobaGameManager.Instance.ResumeTimer);
    }
  

    // Update is called once per frame
    public void DisableGameObject(GameObject go)
    {
        go.SetActive(false);
    }
}
