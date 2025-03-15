# Some-Personal-Projects
Here are some of my daily practice projects

## 1. 基于强化学习的 NPC 行为优化

技术栈：**Python** 

### (1) 项目简介

1. 独立**从零实现**强化学习算法（策略迭代、值迭代、Sarsa、Q-learning、DQN），优化 NPC 行为策略。 
2. 自定义游戏环境，实现状态转移和奖励函数，提高 NPC 适应性。 
3. 构建深度 Q 网络（DQN），采用经验回放（Replay Buffer）与目标网络（Target Network），提高 NPC 决策效率。

### (2) 项目描述

本项目使用 `Frozen Lake` 环境，这是一个离散网格世界，包含以下地形：

- `&` - 起点
- `.` - 冰面（可以安全移动）
- `#` - 洞（掉入后游戏结束）
- `$` - 目标点（达到后获得奖励）

NPC 可以执行 **四个动作**（上、下、左、右），但环境具有一定概率的 **滑动效果**，使得 NPC 可能朝 **随机方向** 移动。

### (3) 项目结构

```
📂 RL_FrozenLake
 ├── 📂 Environments/   # 自定义环境（Frozen Lake）
 │   ├── environment_model.py
 │   ├── environment.py
 │   ├── frozen_lake.py
 │   ├── frozen_lake_image_wrapper.py
 │   ├── linear_wrapper.py
 ├── 📂 Algorithms/     # 各种强化学习算法实现
 │   ├── policy_iteration.py
 │   ├── value_iteration.py
 │   ├── sarsa.py
 │   ├── q_learning.py
 │   ├── linear_sarsa.py
 │   ├── linear_q_learning.py
 │   ├── deep_q_learning.py
 ├── 📂 Models/
 │   ├── deep_q_network.py
 ├── 📂 Utils/
 │   ├── replay_buffer.py
 ├── main.py           # 运行所有算法，测试和可视化


```

本项目实现了一系列强化学习算法，涵盖 **模型驱动（model-based）** 和 **无模型（model-free）** 方法，具体如下：

#### 1. 环境构建

- **自定义 Frozen Lake 环境**
- **状态转移概率计算**
- **奖励函数设计**

#### 2. 基于表的强化学习

- **策略评估（Policy Evaluation）**
- **策略改进（Policy Improvement）**
- **策略迭代（Policy Iteration）**
- **值迭代（Value Iteration）**

#### 3. 基于表的无模型强化学习

- **SARSA 算法**
- **Q-learning 算法**

#### 4. 线性函数逼近强化学习

- **线性 SARSA**
- **线性 Q-learning**

#### 5. 深度强化学习

- **DQN（深度 Q 网络）**
- **经验回放（Replay Buffer）**
- **目标网络（Target Network）**



## 2. 基于 Unity3D 的 RPG 游戏开发

### (1) 项目简介

本项目是一个使用 **Unity3D & C#** 开发的 **RPG 游戏**，支持 **AI 导航、战斗系统、任务系统、物品管理** 等核心功能。游戏采用 **面向对象编程 + 设计模式** 进行架构优化，提升可维护性和扩展性。实现游戏功能：

1. **AI-Navigation** - **鼠标点击角色移动**，与 NPC 交互，并执行 **攻击指令**
2. **战斗系统** - 包含 **攻击动画、碰撞检测、掉落物品拾取机制**
3. **任务系统** - 使用 **事件中心** 记录 NPC 任务进度，支持 **击杀怪物奖励**
4. **UI 管理** - **单例模式** 控制 UI 显示、交互，支持 **动态界面**
5. **道具 & 背包系统** - **ScriptableObject** 管理物品，支持 **物品存取 & 交互**
6. **发布-订阅模式** - 实现 **任务事件触发、UI 刷新**

### (2) 运行&操作

使用**Unity3D-2022**及以上的版本运行。

| 按键    | 功能                                      |
| ------- | ----------------------------------------- |
| L-Mouse | 点击地面移动、与NPC交互、使用背包中的物品 |
| Tab     | 打开人物基本信息属性栏                    |
| B       | 打开背包                                  |
| Space   | 普通攻击                                  |



## 3. 基于 C++的 2D 动作游戏

### (1) 项目简介

本项目是一个使用 **C++** 开发的 **2D 动作游戏**，采用 **SFML** 作为图形渲染库，实现角色控制、物理碰撞、关卡设计、数据驱动等核心功能。该项目展现了 **C++ 游戏开发** 的关键技术，包括 **<u>状态管理、碰撞检测、动画渲染、输入处理 、更新模式、数据结构设计、性能优化</u>**等。

1. 游戏采用 **E-C（Entity-Component）架构** 组织代码，使游戏逻辑 **模块化**，方便扩展和复用。
2. 同时，使用 **智能指针（`std::shared_ptr` / `std::unique_ptr`）** 进行资源管理，避免内存泄漏。
3. 在架构设计上，采用 **命令模式（Command Pattern）** 处理输入，**更新模式（Update Pattern）** 等开发模式管理游戏逻辑，使游戏代码清晰、可维护。
4. 采用adaptiveLoop，实现帧数计算和锁定。
5. 遵循**The Data Locality Pattern**设计模式，优化数据存储分布，提高cpu和ram的交换效率，提高游戏性能。

### (1) 配置：SFML-2.6.2

在git仓库中找到SFML-2.6.2并下载。

转到 C/C++→ General → Configuration Properties（配置属性），并将“include”（包含）目录添加到“Additional Include Directories” （其他包含目录）字段中。例如，如果您将 SFML 文件下载为 “H:/sfml/SFML2.5.1/”

![image-20250316023944035](G:\A-GithubProjects\IMG-Resources\image-1.png)

还需要链接器，以便能够使用 SFML 提供的实用程序找到库。转到配置 → Linker → General 的 Properties 并将 SFML 的 “lib” 目录添加到 “Additional Library Directories”

![image-20250316024106379](G:\A-GithubProjects\IMG-Resources\image-2.png)

上一步指示可以找到库的位置，但与标头不同，并非所有库都会自动包含。我们需要指定要使用的库。在本项目和以下 项目中，我们将静态链接 SFML 库。SFML 文档还介绍了如何动态链接这些库，但在此模块中我们不需要这样做。设置项目以静态地链接库 ，需要做三件事： 

转到 Configuration Properties → Linker → Input 并选择（在顶部的下拉菜单中）**Debug 配置**。将以下库添 加到 Additional Dependencies 列表中（确保它们用分号“;”分隔，并且不要删除此字段中的现有库）：

```
sfml-graphics-s-d.lib;sfml-window-s-d.lib;sfml-system-s-d.lib;opengl32.lib;winmm.lib;gdi32.lib;
```

将 Configuration 切换到 **Release** （如果出现提示，请保存更改），并将相同库的 release 和 static 版本添加 到 Additional Dependencies 字段：

```
sfml-graphics-s.lib;sfml-window-s.lib;sfml-system-s.lib;opengl32.lib;winmm.lib;gdi32.lib;
```

我们需要定义一个预处理器宏，以指示 SFML 将静态链接。这是在 C/C→ Preprocessor → Configuration Properties 中完成的。请注意，需要为所有配置定义此宏，因此请在顶部选择器中选择 “All Configurations”。将字符串 `SFML_STATIC`  添加到字段 “Preprocessor Definitions” 中：

![image-20250316024437163](G:\A-GithubProjects\IMG-Resources\image-3.png)

关闭“属性”窗口，单击“确定”按钮。SFML 的配置现已完成。

### (2) 操作提示

| 按键    | 功能                 |
| ------- | -------------------- |
| A/D     | 左右移动             |
| W/S     | 上下移动             |
| L-SHIFT | 发射火球             |
| Space   | 普通攻击/收集Log资源 |
| Esc     | 暂停游戏             |



