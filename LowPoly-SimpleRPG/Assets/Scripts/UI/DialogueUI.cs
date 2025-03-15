using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    //单例模式
    public static DialogueUI Instance { get; private set; }

    //使用public可以在引擎中拖拽赋值
    //如果使用private则在代码里的start方法下赋值
    public TextMeshProUGUI nameText;//窗口NPC名字 
    public TextMeshProUGUI contentText;//窗口对话内容


    public List<string> contentList;//对话数组
    public Button continueButton;//继续按钮

    private int contentIndex = 0;//对话内容索引
    private GameObject uiGameObject;
    private Action OnDialogueEnd;//负责Show方法的回调

    //单例模式一般在Awake里初始化
    private void Awake()
    {
        //if的判断：Instance不为空；并且Instance的实例化不大于一个
        //原因：Awake()有些情况下调用可能会不止一次，因此会有多个DialogueUI会被实例化
        //所以Instance != this就说明：Instance已经存在，但是Instance不是当前的Instance
        if (Instance != null && Instance != this)
        {
            //一旦发现重复调用就消除
            //测试：在引擎中复制一个DialogueUI，在运行的时候只会保留一个
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        //transform.Find可以获取到子对象
        //nameText = transform.Find("NameImg/NameText").GetComponent<TextMeshProUGUI>();

        //为‘继续’按钮添加点击时间
        //continueButton = transform.Find("ContinueButton").GetComponent<Button>();
        continueButton.onClick.AddListener(this.OnContinueButtonClick);

        uiGameObject = transform.Find("UI").gameObject;//找到子物体
        Hide();
    }

    public void Show()
    {
        uiGameObject.SetActive(true);
    }

    public void Show(string name, string[] content, Action OnDialogueEnd = null)
    {
        nameText.text = name;//传递对话者名字
        contentList = new List<string>();//一定要新建一个列表，每个NPC在调用Show方法的时候都会单独创建一个对话内容的列表
        contentList.AddRange(content);
        contentIndex = 0; //归零对话内容索引
        contentText.text = contentList[0];//传递对话者内容
        uiGameObject.SetActive(true);

        this.OnDialogueEnd = OnDialogueEnd;//
    }

    public void Hide()
    {
        uiGameObject.SetActive(false);
    }

    private void OnContinueButtonClick()
    {
        contentIndex++;//点击继续
        if (contentIndex >= contentList.Count) 
        {
            /*对话结束*/
            OnDialogueEnd?.Invoke();
            Hide();
            return;
        }
        contentText.text = contentList[contentIndex];
    }
}
