import numpy as np


class LinearWrapper:
    """
    用于线性函数逼近的环境封装类，将离散状态转换为特征向量
    """

    def __init__(self, env):
        self.env = env
        self.n_actions = self.env.n_actions
        self.n_states = self.env.n_states
        self.n_features = self.n_actions * self.n_states  # 线性特征数

    def encode_state(self, s):
        """ 将状态 s 编码为特征向量 """
        features = np.zeros((self.n_actions, self.n_features))
        for a in range(self.n_actions):
            i = np.ravel_multi_index((s, a), (self.n_states, self.n_actions))
            features[a, i] = 1.0
        return features

    def decode_policy(self, theta):
        """ 从参数 theta 解码策略 """
        policy = np.zeros(self.env.n_states, dtype=int)
        value = np.zeros(self.env.n_states)
        for s in range(self.n_states):
            features = self.encode_state(s)
            q = features.dot(theta)
            policy[s] = np.argmax(q)
            value[s] = np.max(q)
        return policy, value

    def reset(self):
        """ 重置环境并返回初始特征 """
        return self.encode_state(self.env.reset())

    def step(self, action):
        """ 执行动作，并返回 (特征, 奖励, 是否结束) """
        state, reward, done = self.env.step(action)
        return self.encode_state(state), reward, done

    def render(self, policy=None, value=None):
        """ 可视化环境 """
        self.env.render(policy, value)
