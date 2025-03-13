import numpy as np

def q_learning(env, max_episodes, eta, gamma, epsilon, seed=None):
    """
    Q-Learning 算法：基于最优策略学习 Q 值
    :param env: 交互环境
    :param max_episodes: 最大训练回合数
    :param eta: 初始学习率
    :param gamma: 折扣因子
    :param epsilon: ϵ-贪心策略的探索率
    :param seed: 随机种子
    :return: 最优策略和价值函数
    """
    random_state = np.random.RandomState(seed)
    eta = np.linspace(eta, 0, max_episodes)
    epsilon = np.linspace(epsilon, 0, max_episodes)
    q = np.zeros((env.n_states, env.n_actions))

    for i in range(max_episodes):
        s = env.reset()
        done = False
        while not done:
            a = (random_state.randint(env.n_actions) if random_state.rand() < epsilon[i]
                 else np.random.choice(np.flatnonzero(q[s] == q[s].max())))
            next_s, reward, done = env.step(a)
            q[s, a] += eta[i] * (reward + gamma * np.max(q[next_s]) - q[s, a])
            s = next_s

    return q.argmax(axis=1), q.max(axis=1)
