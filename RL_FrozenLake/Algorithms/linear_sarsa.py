import numpy as np


def linear_sarsa(env, max_episodes, eta, gamma, epsilon, seed=None):
    """
    线性函数逼近的 SARSA 算法
    :param env: 线性特征封装的环境
    :param max_episodes: 最大训练回合数
    :param eta: 初始学习率
    :param gamma: 折扣因子
    :param epsilon: ϵ-贪心策略的探索率
    :param seed: 随机种子
    :return: 训练好的参数 theta 和每回合的累计奖励
    """
    random_state = np.random.RandomState(seed)
    eta = np.linspace(eta, 0, max_episodes)
    epsilon = np.linspace(epsilon, 0, max_episodes)
    theta = np.zeros(env.n_features)
    episode_returns = []

    for i in range(max_episodes):
        features = env.reset()
        q = features.dot(theta)

        # 选择动作
        if random_state.rand() < epsilon[i]:
            a = random_state.choice(env.n_actions)
        else:
            max_actions = np.flatnonzero(q == q.max())
            a = random_state.choice(max_actions)

        done = False
        episode_return = 0

        while not done:
            next_features, reward, done = env.step(a)
            next_q = next_features.dot(theta)
            episode_return += reward

            # 选择下一动作
            if random_state.rand() < epsilon[i]:
                next_a = random_state.choice(env.n_actions)
            else:
                next_max_actions = np.flatnonzero(next_q == next_q.max())
                next_a = random_state.choice(next_max_actions)

            # 更新 theta
            delta = reward + gamma * next_q[next_a] - q[a]
            theta += eta[i] * delta * features[a]

            features, q, a = next_features, next_q, next_a

        episode_returns.append(episode_return)

    return theta, episode_returns
