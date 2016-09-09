using UnityEngine;
using System.Collections;
using System.IO.Ports;
using UnityEngine.SceneManagement;
using System;

public class StartList : MonoBehaviour
{   string[] st;
    bool yes;
    HumanBodyBones i;
    public static SerialPort sp;
    public GameObject Window;
    public GameObject Window1;
    GameObject PortList;
    GameObject PortLabel;
    GameObject BaudRateLabel;
    string portName;
    int baudRate;
    Parity parity = Parity.None;
    int dataBits = 8;
    StopBits stopBits = StopBits.One;
    // Use this for initialization
    void Start()
    {
        PortList = GameObject.Find("PortList");
        PortLabel = GameObject.Find("PortLabel");
        BaudRateLabel = GameObject.FindWithTag("BaudRate");
        //获取当前可用串口
        st = SerialPort.GetPortNames();
        //判断当前有无可用串口，若有则全部添加到下拉列表并指定默认端口
        if (st.Length == 0)
        {
            PortLabel.GetComponent<UILabel>().text = "无可用端口";
        }
        else
        {
            for (int i = 0; i < st.Length; ++i)
            {
                PortList.GetComponent<UIPopupList>().items.Add(st[i]);
            }
            PortLabel.GetComponent<UILabel>().text = PortList.GetComponent<UIPopupList>().items[0];
        }
        //指定默认波特率
        BaudRateLabel.GetComponent<UILabel>().text = "115200";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    //确定按钮的回调函数
    public void ButtonC()
    {
        if (PortLabel.GetComponent<UILabel>().text == "无可用端口")
        {
            Window.SetActive(true);
            return;
        }
        portName = PortLabel.GetComponent<UILabel>().text;
        baudRate = Convert.ToInt32(BaudRateLabel.GetComponent<UILabel>().text);
        OpenPort();
        if (yes)
        {
            SceneManager.LoadSceneAsync(1);
        }
        else
        {
            Window1.SetActive(true);
        }
    }
    //打开串口
    public void OpenPort()
    {
        try
        {
            sp = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            sp.ReadTimeout = 100;
            sp.Open();
            if (sp.IsOpen)
            {
                Debug.Log("打开" + portName + "成功！");
                yes = true;
            }
        }
        catch (Exception ex)
        {
            Debug.Log("打开" + portName + "失败，请选择正确的端口！\t" + ex.Message);
            yes = false;
        }
    }
    //未正确配置端口或波特率时错误弹窗回调函数
    public void Reset()
    {
        Window.GetComponent<TweenAlpha>().ResetToBeginning();
        Window.GetComponent<TweenAlpha>().enabled = true;
        Window.SetActive(false);
    }
    //端口已被占用时错误弹窗回调函数
    public void Reset1()
    {
        Window1.GetComponent<TweenAlpha>().ResetToBeginning();
        Window1.GetComponent<TweenAlpha>().enabled = true;
        Window1.SetActive(false);
    }
}