using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    //����ģʽ
    public static DialogueUI Instance { get; private set; }

    //ʹ��public��������������ק��ֵ
    //���ʹ��private���ڴ������start�����¸�ֵ
    public TextMeshProUGUI nameText;//����NPC���� 
    public TextMeshProUGUI contentText;//���ڶԻ�����


    public List<string> contentList;//�Ի�����
    public Button continueButton;//������ť

    private int contentIndex = 0;//�Ի���������
    private GameObject uiGameObject;
    private Action OnDialogueEnd;//����Show�����Ļص�

    //����ģʽһ����Awake���ʼ��
    private void Awake()
    {
        //if���жϣ�Instance��Ϊ�գ�����Instance��ʵ����������һ��
        //ԭ��Awake()��Щ����µ��ÿ��ܻ᲻ֹһ�Σ���˻��ж��DialogueUI�ᱻʵ����
        //����Instance != this��˵����Instance�Ѿ����ڣ�����Instance���ǵ�ǰ��Instance
        if (Instance != null && Instance != this)
        {
            //һ�������ظ����þ�����
            //���ԣ��������и���һ��DialogueUI�������е�ʱ��ֻ�ᱣ��һ��
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        //transform.Find���Ի�ȡ���Ӷ���
        //nameText = transform.Find("NameImg/NameText").GetComponent<TextMeshProUGUI>();

        //Ϊ����������ť��ӵ��ʱ��
        //continueButton = transform.Find("ContinueButton").GetComponent<Button>();
        continueButton.onClick.AddListener(this.OnContinueButtonClick);

        uiGameObject = transform.Find("UI").gameObject;//�ҵ�������
        Hide();
    }

    public void Show()
    {
        uiGameObject.SetActive(true);
    }

    public void Show(string name, string[] content, Action OnDialogueEnd = null)
    {
        nameText.text = name;//���ݶԻ�������
        contentList = new List<string>();//һ��Ҫ�½�һ���б�ÿ��NPC�ڵ���Show������ʱ�򶼻ᵥ������һ���Ի����ݵ��б�
        contentList.AddRange(content);
        contentIndex = 0; //����Ի���������
        contentText.text = contentList[0];//���ݶԻ�������
        uiGameObject.SetActive(true);

        this.OnDialogueEnd = OnDialogueEnd;//
    }

    public void Hide()
    {
        uiGameObject.SetActive(false);
    }

    private void OnContinueButtonClick()
    {
        contentIndex++;//�������
        if (contentIndex >= contentList.Count) 
        {
            /*�Ի�����*/
            OnDialogueEnd?.Invoke();
            Hide();
            return;
        }
        contentText.text = contentList[contentIndex];
    }
}
