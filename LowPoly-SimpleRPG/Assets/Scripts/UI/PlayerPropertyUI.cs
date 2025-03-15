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
        UpdatePlayerPropertyUI();//游戏开始时更新

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

        ClearGrid();//更新之前先清空Grid

        AddProperty("饥饿值:" + pp.enengyValue);
        AddProperty("精神值:" + pp.mentalValue);

        //遍历字典
        foreach (var item in pp.propertyDict)
        {
            string PropertyName = "";
            switch (item.Key)
            {
                case PropertyType.HPValue:
                    PropertyName = "生命值:";
                    break;
                case PropertyType.EnergyValue:
                    PropertyName = "饥饿值:";
                    break;
                case PropertyType.MentalValue:
                    PropertyName = "精神值:";
                    break;
                case PropertyType.SpeedValue:
                    PropertyName = "速度:";
                    break;
                case PropertyType.AttackValue:
                    PropertyName = "攻击力:";
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
            //判断自身是否处于激活状态
            if (child.gameObject.activeSelf)
            {
                Destroy(child.gameObject);
            }
        }
    }

    //将一个属性模板放在Grid中
    private void AddProperty(string propertyStr)
    {
        GameObject go = GameObject.Instantiate(propertyTemplate);//实例化一个propertyTemplate(属性模板)
        go.SetActive(true);
        //go.transform.parent = propertyGrid.transform;//将新初始化的属性模板挂载到PropertyGrid下
        go.transform.SetParent(propertyGrid.transform);
        go.transform.Find("Property").GetComponent<TextMeshProUGUI>().text = propertyStr;//赋值
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
