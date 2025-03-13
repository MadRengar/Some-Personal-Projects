# Extracted from CW2.ipynb

import numpy as np
import torch
from collections import deque
import torch.nn.functional as F
import random
import matplotlib.pyplot as plt

from Environments.frozen_lake import FrozenLake
from Algorithms.policy_iteration import policy_iteration
from Algorithms.value_iteration import value_iteration
from Algorithms.sarsa import sarsa
from Algorithms.q_learning import q_learning
from Environments.linear_wrapper import LinearWrapper
from Algorithms.linear_sarsa import linear_sarsa
from Algorithms.linear_q_learning import linear_q_learning
from Environments.frozen_lake_image_wrapper import FrozenLakeImageWrapper
from Algorithms.deep_q_learning import deep_q_network_learning


def plot_returns(returns, algorithm_name, window):
    window = window

    moving_avg = np.convolve(returns, np.ones(window) / window, mode='valid')

    # Plot the moving average
    plt.figure(figsize=(10, 6))
    plt.plot(moving_avg)
    plt.title(f"{algorithm_name} - Moving Average of Returns")
    plt.xlabel("Episode")
    plt.ylabel("Moving Average of Returns")
    plt.grid()
    plt.show()


def drawEtaEpsilon(max_episodes):
    eta_test = np.linspace(0.5, 0, max_episodes)  # 学习率
    epsilon_test = np.linspace(0.5, 0, max_episodes)  # 探索率

    plt.plot(eta_test, label='Learning Rate (eta)')
    plt.plot(epsilon_test, label='Exploration Rate (epsilon)')
    plt.xlabel('Episode')
    plt.ylabel('Value')
    plt.legend()
    plt.title('Decay of Learning and Exploration Rates')
    plt.show()


def play(env):
    actions = ['w', 'a', 's', 'd']
    state = env.reset()
    env.render()
    done = False
    while not done:
        c = input('\nMove: ')
        if c not in actions:
            raise Exception('Invalid action')

        state, r, done = env.step(actions.index(c))

        env.render()
        print('Reward: {0}.'.format(r))


def main():
    seed = 0

    # Big lake
    BigLake = [['&', '.', '.', '.', '.', '.', '.', '.'],
               ['.', '.', '.', '.', '.', '.', '.', '.'],
               ['.', '.', '.', '#', '.', '.', '.', '.'],
               ['.', '.', '.', '.', '.', '#', '.', '.'],
               ['.', '.', '.', '#', '.', '.', '.', '.'],
               ['.', '#', '#', '.', '.', '.', '#', '.'],
               ['.', '#', '.', '.', '#', '.', '#', '.'],
               ['.', '.', '.', '#', '.', '.', '.', '$']]

    # Small lake
    lake = [['&', '.', '.', '.'],
            ['.', '#', '.', '#'],
            ['.', '.', '.', '#'],
            ['#', '.', '.', '$']]

    env = FrozenLake(lake, slip=0.1, max_steps=64, seed=seed)
    gamma = 0.9
    max_episodes = 5000

    print('# Model-based algorithms')

    print('')

    print('## Policy iteration')
    policy, value = policy_iteration(env, gamma, theta=0.001, max_iterations=128)
    env.render(policy, value)

    print('')

    print('## Value iteration')
    policy, value = value_iteration(env, gamma, theta=0.001, max_iterations=128)
    env.render(policy, value)

    print('')

    print('# Model-free algorithms')

    print('')

    print('## Sarsa')
    policy, value = sarsa(env, max_episodes, eta=0.5, gamma=gamma,
                          epsilon=0.5, seed=seed)
    env.render(policy, value)

    print('')

    print('## Q-learning')
    policy, value = q_learning(env, max_episodes, eta=0.5, gamma=gamma,
                               epsilon=0.5, seed=seed)
    env.render(policy, value)

    print('')

    linear_env = LinearWrapper(env)

    print('## Linear Sarsa')

    # parameters = linear_sarsa(linear_env, max_episodes, eta=0.5, gamma=gamma,
    #                           epsilon=0.5, seed=seed)
    # policy, value = linear_env.decode_policy(parameters)
    # linear_env.render(policy, value)

    theta_sarsa, returns_sarsa = linear_sarsa(linear_env, max_episodes, eta=0.5, gamma=gamma,
                                              epsilon=0.5, seed=seed)

    # 使用 theta 解码策略和值函数
    policy_sarsa, value_sarsa = linear_env.decode_policy(theta_sarsa)

    # 渲染策略和值函数
    linear_env.render(policy_sarsa, value_sarsa)

    # 绘制移动平均回报曲线
    plot_returns(returns_sarsa, "Linear Sarsa - Moving Average of Returns", 20)

    print('')

    print('## Linear Q-learning')

    # parameters = linear_q_learning(linear_env, max_episodes, eta=0.5, gamma=gamma,
    #                                epsilon=0.5, seed=seed)
    # policy, value = linear_env.decode_policy(parameters)
    # linear_env.render(policy, value)
    theta_q_learning, returns_q_learning = linear_q_learning(linear_env, max_episodes, eta=0.5, gamma=gamma,
                                                             epsilon=0.5, seed=seed)

    # 使用 theta 解码策略和值函数
    policy_q_learning, value_q_learning = linear_env.decode_policy(theta_q_learning)

    # 渲染策略和值函数
    linear_env.render(policy_q_learning, value_q_learning)

    # 绘制移动平均回报曲线
    plot_returns(returns_q_learning, "Linear Q-Learning - Moving Average of Returns", 20)

    print('')

    image_env = FrozenLakeImageWrapper(env)

    print('## Deep Q-network learning')

    dqn = deep_q_network_learning(image_env, max_episodes, learning_rate=0.001,
                                  gamma=gamma, epsilon=0.2, batch_size=32,
                                  target_update_frequency=4, buffer_size=256,
                                  kernel_size=3, conv_out_channels=4,
                                  fc_out_features=8, seed=4)
    policy, value = image_env.decode_policy(dqn)
    image_env.render(policy, value)


if __name__ == "__main__":
    main()
# play(env)
