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

#### 实现方法

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



## 3. 基于 C++的 2D 动作游戏
