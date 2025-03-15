using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPropertyUI : MonoBehaviour
{
    public static PlayerPropertyUI Instance { get; private set; }

    private GameObject uiGameObject;

    private Image hpProgressBar;
    private TextMeshProUGUI hpText;

    private Image levelProgressBar;
    private TextMeshProUGUI levelText;

    private GameObject propertyGrid;
    private GameObject propertyTemplate;

    private Image weaponIcon;

    private PlayerProperty pp;
    private PlayerAttack pa;


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this; 
    }
    void Start()
    {
        uiGameObject = transform.Find("UI").gameObject;

        hpProgressBar = transform.Find("UI/HPProgressBar/ProgressBar").GetComponent<Image>();
        hpText = transform.Find("UI/HPProgressBar/HPValueText").GetComponent<TextMeshProUGUI>();

        levelProgressBar = transform.Find("UI/LevelProgressBar/ProgressBar").GetComponent<Image>();
        levelText = transform.Find("UI/LevelProgressBar/LevelValueText").GetComponent<TextMeshProUGUI>();

        propertyGrid = transform.Find("UI/PropertyGrid").gameObject;
        propertyTemplate = transform.Find("UI/PropertyGrid/PropertyTemplate").gameObject;

        weaponIcon = transform.Find("UI/WeaponIcon").GetComponent<Image>();

        propertyTemplate.SetActive(false);

        GameObject player =  GameObject.FindGameObjectWithTag(Tag.PLAYER);

        pp = player.GetComponent<PlayerProperty>();
        pa = player.GetComponent<PlayerAttack>();
        UpdatePlayerPropertyUI();//��Ϸ��ʼʱ����

        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        { 
            if(uiGameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }

    public void UpdatePlayerPropertyUI()
    {
        hpProgressBar.fillAmount = pp.hpValue / 100.0f;
        hpText.text = pp.hpValue + "/100";

        levelProgressBar.fillAmount = pp.currentExp * 1.0f / (pp.level * 30);
        levelText.text = pp.level.ToString();

        ClearGrid();//����֮ǰ�����Grid

        AddProperty("����ֵ:" + pp.enengyValue);
        AddProperty("����ֵ:" + pp.mentalValue);

        //�����ֵ�
        foreach (var item in pp.propertyDict)
        {
            string PropertyName = "";
            switch (item.Key)
            {
                case PropertyType.HPValue:
                    PropertyName = "����ֵ:";
                    break;
                case PropertyType.EnergyValue:
                    PropertyName = "����ֵ:";
                    break;
                case PropertyType.MentalValue:
                    PropertyName = "����ֵ:";
                    break;
                case PropertyType.SpeedValue:
                    PropertyName = "�ٶ�:";
                    break;
                case PropertyType.AttackValue:
                    PropertyName = "������:";
                    break;
            }

            int sum = 0;
            foreach (var item1 in item.Value)
            {
                sum += item1.value;
            }
            AddProperty(PropertyName + sum);
        }

        if(weaponIcon != null)
        {
            weaponIcon.sprite = pa.weaponIcon;
        }
    }

    private void ClearGrid()
    {
        foreach (Transform child in propertyGrid.transform)
        {
            //�ж������Ƿ��ڼ���״̬
            if (child.gameObject.activeSelf)
            {
                Destroy(child.gameObject);
            }
        }
    }

    //��һ������ģ�����Grid��
    private void AddProperty(string propertyStr)
    {
        GameObject go = GameObject.Instantiate(propertyTemplate);//ʵ����һ��propertyTemplate(����ģ��)
        go.SetActive(true);
        //go.transform.parent = propertyGrid.transform;//���³�ʼ��������ģ����ص�PropertyGrid��
        go.transform.SetParent(propertyGrid.transform);
        go.transform.Find("Property").GetComponent<TextMeshProUGUI>().text = propertyStr;//��ֵ
    }

    private void Show()
    {
        uiGameObject.SetActive(true);
    }

    private void Hide()
    {
        uiGameObject.SetActive(false);
    }


}
