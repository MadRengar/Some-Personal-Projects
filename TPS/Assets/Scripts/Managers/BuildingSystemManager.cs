using PlayerControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSystemManager : MonoBehaviour
{
    [Header("Building System")]
    public GameObject[] buildablePrefabs; // 可建造的建筑物预制体
    public LayerMask groundLayer = 1; // 地面层
    public Material previewMaterial; // 预览材质

    private PlayerInputSystem _playerInputs;
    private Camera _camera;
    private GameObject _currentPreview; // 当前预览的建筑物
    private int _selectedBuildingIndex = 0;

    // UI 相关
    [Header("UI")]
    public GameObject buildMenuUI; // 建筑菜单UI

    private void Awake()
    {
        _playerInputs = FindObjectOfType<PlayerInputSystem>();
        _camera = Camera.main;
    }

    private void Start()
    {
        // 订阅模式切换事件
        PlayerInputSystem.OnModeChanged += OnPlayerModeChanged;
    }

    private void OnDestroy()
    {
        PlayerInputSystem.OnModeChanged -= OnPlayerModeChanged;
    }

    private void Update()
    {
        switch (_playerInputs.currentMode)
        {
            case PlayerInputSystem.PlayerMode.BuildMenu:
                HandleBuildMenuInput();
                break;
            case PlayerInputSystem.PlayerMode.Placing:
                HandlePlacingInput();
                break;
        }
    }

    private void OnPlayerModeChanged(PlayerInputSystem.PlayerMode newMode)
    {
        switch (newMode)
        {
            case PlayerInputSystem.PlayerMode.Combat:
                HideBuildMenu();
                DestroyPreview();
                break;
            case PlayerInputSystem.PlayerMode.BuildMenu:
                ShowBuildMenu();
                break;
            case PlayerInputSystem.PlayerMode.Placing:
                HideBuildMenu();
                CreatePreview();
                break;
        }
    }

    private void HandleBuildMenuInput()
    {
        // 在这里处理建筑菜单的输入
        // 例如选择不同的建筑物类型
    }

    private void HandlePlacingInput()
    {
        UpdatePreviewPosition();

        // 左键放置建筑物
        if (_playerInputs.shootPressed)
        {
            PlaceBuilding();
        }

        // 右键或ESC取消放置
        if (Input.GetMouseButtonDown(1) || _playerInputs.cancelPressed)
        {
            _playerInputs.EnterBuildMenu();
        }
    }

    private void UpdatePreviewPosition()
    {
        if (_currentPreview == null) return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            _currentPreview.transform.position = hit.point;
        }
    }

    private void PlaceBuilding()
    {
        if (_currentPreview == null) return;

        // 实例化真正的建筑物
        GameObject building = Instantiate(buildablePrefabs[_selectedBuildingIndex],
                                        _currentPreview.transform.position,
                                        _currentPreview.transform.rotation);

        // 可以在这里添加建筑物放置的逻辑，比如检查资源、记录建筑物等

        Debug.Log($"Placed building: {building.name} at {building.transform.position}");

        // 放置后返回建筑菜单或战斗模式
        _playerInputs.EnterBuildMenu(); // 或者 _playerInputs.EnterCombatMode();
    }

    private void CreatePreview()
    {
        if (buildablePrefabs.Length > 0 && _selectedBuildingIndex < buildablePrefabs.Length)
        {
            _currentPreview = Instantiate(buildablePrefabs[_selectedBuildingIndex]);

            // 设置预览状态
            SetupPreview(_currentPreview);
        }
    }

    private void SetupPreview(GameObject preview)
    {
        // 禁用所有碰撞器
        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // 设置预览材质
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = previewMaterial;
            }
            renderer.materials = materials;
        }

        // 添加预览标签
        preview.tag = "Preview";
    }

    private void DestroyPreview()
    {
        if (_currentPreview != null)
        {
            Destroy(_currentPreview);
            _currentPreview = null;
        }
    }

    private void ShowBuildMenu()
    {
        if (buildMenuUI != null)
        {
            buildMenuUI.SetActive(true);
        }
    }

    private void HideBuildMenu()
    {
        if (buildMenuUI != null)
        {
            buildMenuUI.SetActive(false);
        }
    }

    // 公共方法供UI调用
    public void SelectBuilding(int index)
    {
        if (index >= 0 && index < buildablePrefabs.Length)
        {
            _selectedBuildingIndex = index;
            _playerInputs.EnterPlacingMode();
        }
    }
}
