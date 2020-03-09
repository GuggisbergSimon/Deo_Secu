using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleController : NetworkBehaviour
{
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private TextMeshProUGUI _text = null;
    [SyncVar] private string _message;
    private bool _inputMode = false;
    private string _tmpMessage = "";


    private void Update()
    {
        if (isLocalPlayer)
        {
            if (_inputMode)
            {
                string input = Input.inputString;
                if (!input.Equals("\n") && !input.Equals("\b"))
                {
                    _tmpMessage += input;
                }

                if (input.Equals("\b"))
                {
                    RemoveLastChar();
                }

                _text.text = _tmpMessage + "_";
            }
            else if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                transform.position += Time.deltaTime * speed *
                                      (Vector3.right * Input.GetAxis("Horizontal") +
                                       Vector3.up * Input.GetAxis("Vertical"));
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                RemoveLastChar();
                CmdPrintText(_tmpMessage);
                _tmpMessage = "";
                _inputMode = !_inputMode;
            }
        }
    }

    private void FixedUpdate()
    {
        _text.text = _message;
    }

    private void RemoveLastChar()
    {
        if (_tmpMessage.Length > 0)
        {
            _tmpMessage = _tmpMessage.Substring(0, _tmpMessage.Length - 1);
        }
    }

    [Command]
    private void CmdPrintText(string text)
    {
        _message = text;
        _text.text = _message;
    }
}