using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CodeLobby : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI code;

    private void Start()
    {
        code.text = "CODE: " + RelayServer.staticCode;
    }
}
