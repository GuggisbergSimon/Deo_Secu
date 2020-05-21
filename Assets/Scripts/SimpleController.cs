using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SimpleController : NetworkBehaviour
{
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private TextMeshProUGUI _text = null;
    [SerializeField] private TextMeshProUGUI _playerName = null;
    [SerializeField] private TextMeshProUGUI _listName = null;
    [SerializeField] private GameObject _scrollList = null;

    [SerializeField] private GameObject canvas = null;
    [SyncVar] private string _message;
    [SyncVar] private string _name = "New Player";


    private SyncListString _autorizedNameList;

    //private bool _nameChosen = false;
    List<GameObject> _list = new List<GameObject>();

    GameObject _localPlayer;
    private int _playerIndex = 0;
    private bool _inputMode = false;
    private bool _inputNameMode = false;
    private bool _choosePlayerMode = false;
    private string _tmpMessage = "";
    private const int CESAR = 3;

    public override void OnStartLocalPlayer()
    {
        _localPlayer = this.gameObject;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            g.GetComponent<SimpleController>()._localPlayer = _localPlayer;
        }
        _inputNameMode = !_inputNameMode;
    }

    private void Start()
    {
        canvas = GameObject.Find("GameZone");
        foreach (TextMeshProUGUI g in GameObject.FindObjectsOfType<TextMeshProUGUI>())
        {
            if (g.name == "ListTextMesh")
            {
                _listName = Instantiate(GameObject.FindObjectsOfType<TextMeshProUGUI>()[0], Vector3.one, Quaternion.identity);
            }
        }
        //_listName = _scrollList.GetComponent<TextMeshProUGUI>();
    }

    public void OnDestroy()
    {
        Destroy(_listName);
    }

    private void WritePlayerName(GameObject g)
    {
        if (_autorizedNameList.Contains(g.GetComponent<SimpleController>()._name))
        {
            _listName.text += "O-";
        }
        else
        {
            _listName.text += "X-";
        }
        _listName.text += g.GetComponent<SimpleController>()._name + "\n";
    }

    private void SelectPlayer()
    {

        _playerIndex++;
        _playerIndex = _playerIndex % _list.Count;
        

        _listName.text = "";
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (g.GetComponent<SimpleController>()._name == _list[_playerIndex].GetComponent<SimpleController>()._name)
            {
                _listName.text += "->";
            }

            WritePlayerName(g);
        }
    }

    private void OpenClosePlayerList()
    {
        if (_choosePlayerMode)
        {
            _listName.text = "";
        }
        else
        {
            _list.Clear();
            _listName.text = "";
            _listName.transform.SetParent(canvas.transform);
            _listName.rectTransform.localPosition = new Vector3();
            _listName.rectTransform.localScale = Vector3.one;
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {

                _list.Add(g);
                _listName.color = Color.red;
                WritePlayerName(g);
                
            }
        }
        _choosePlayerMode = !_choosePlayerMode;
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (_choosePlayerMode)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    SelectPlayer();
                }
            }
            else if (_inputMode)
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
            else if (_inputNameMode)
            {
                string input = Input.inputString;
                if (!input.Equals("\n") && !input.Equals("\b") && !input.Equals(" ") && _tmpMessage.Trim().Length <=9)
                {
                    _tmpMessage += input;
                }

                if (input.Equals("\b"))
                {
                    RemoveLastChar();
                }

                _playerName.text = _tmpMessage + "_";
            }
            else if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                transform.position += Time.deltaTime * speed *
                                      (Vector3.right * Input.GetAxis("Horizontal") +
                                       Vector3.up * Input.GetAxis("Vertical"));
            }


            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (_choosePlayerMode)
                {
                    if (_list[_playerIndex] != null)
                    {
                        if (!_autorizedNameList.Contains(_list[_playerIndex].GetComponent<SimpleController>()._name))
                        {
                            CmdAddAuthorizationList(_list[_playerIndex].GetComponent<SimpleController>()._name);
                        }
                        else
                        {
                            CmdRemoveAutorizationList(_list[_playerIndex].GetComponent<SimpleController>()._name);
                        }
                        _listName.text = "";
                        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
                        {
                            if (g.GetComponent<SimpleController>()._name == _list[_playerIndex].GetComponent<SimpleController>()._name)
                            {
                                _listName.text += "->";
                            }
                            WritePlayerName(g);
                        }
                    }
                    else
                    {
                        _choosePlayerMode = false;
                        OpenClosePlayerList();
                    }
                }
                else if (_inputMode)
                {
                    _inputMode = !_inputMode;
                    RemoveLastChar();
                    CmdPrintText(Compress(Encrypt(_tmpMessage)));
                    _tmpMessage = "";
                }
                else if (_inputNameMode)
                {
                    if (_tmpMessage.Trim().Length > 1 )
                    {
                        _inputNameMode = !_inputNameMode;
                       // RemoveLastChar();
                        CmdChangeName(_tmpMessage);
                        _tmpMessage = "";
                    }
                }
                else
                {
                    _inputMode = !_inputMode;
                }
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (!_inputMode && !_inputNameMode)
                {
                    OpenClosePlayerList();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            g.GetComponent<SimpleController>()._localPlayer = _localPlayer;
        }
        if (_autorizedNameList.Contains(_localPlayer.GetComponent<SimpleController>()._name) || isLocalPlayer)
        {
            //Debug.Log(_message + ":" + Decompress(_message) + ":" + Decrypt(Decompress(_message)));
            _text.text = Decrypt(Decompress(_message));
        }
        else
        {
            _text.text = _message;
        }
        _playerName.text = _name;
    }

    private void RemoveLastChar()
    {
        if (_tmpMessage.Length > 0)
        {
            _tmpMessage = _tmpMessage.Substring(0, _tmpMessage.Length - 1);
        }
    }

    private string Encrypt(string message)
    {
        return Cesar(message, CESAR);
    }

    private string Decrypt(string message)
    {
        return Cesar(message, -CESAR);
    }

    private string Cesar(string message, int shift)
    {
        if (message == null)
        {
            return "";
        }

        string newMessage = "";
        foreach (var c in message)
        {
            newMessage += Convert.ToChar(c + shift);
        }

        return newMessage;
    }

    private string Compress(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "";
        }

        string newMsg = "";
        int nbrChar = 1;
        char previousChar = message[0];
        for (int i = 1; i < message.Length; i++)
        {
            if (message[i] == previousChar)
            {
                nbrChar++;
            }
            else
            {
                newMsg += (nbrChar <= 1 && !Char.IsNumber(previousChar) ? "" : nbrChar.ToString()) + previousChar;
                nbrChar = 1;
                previousChar = message[i];
            }
        }
        newMsg += (nbrChar <= 1 && !Char.IsNumber(previousChar) ? "" : nbrChar.ToString()) + previousChar;

        return newMsg;
    }

    private string Decompress(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "";
        }

        string newMsg = "";
        bool isReadingChar = false;
        int nbrChar = 1;
        for (int i = 0; i < message.Length; i++)
        {
            //handles first number of duo
            if (nbrChar <= 1 && Char.IsNumber(message[i]) && !isReadingChar)
            {
                nbrChar = message[i] - '0';
                isReadingChar = true;
            }
            else
            {
                newMsg += new string(message[i], nbrChar);
                nbrChar = 1;
                isReadingChar = false;
            }
        }

        return newMsg;
    }

    [Command]
    private void CmdPrintText(string text)
    {
        _message = text;
    }

    [Command]
    private void CmdChangeName(string name)
    {
        _name = name;
    }

    [Command]
    private void CmdAddAuthorizationList(string l)
    {

        _autorizedNameList.Add(l);
    }

    [Command]
    private void CmdRemoveAutorizationList(string l)
    {
        _autorizedNameList.Remove(l);
    }
}