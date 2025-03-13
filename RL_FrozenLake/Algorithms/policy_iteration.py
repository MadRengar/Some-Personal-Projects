# 策略评估、策略改进、策略迭代
import numpy as np


def policy_evaluation(env, policy, gamma, theta, max_iterations):
    """ 评估当前策略的状态价值 """
    value = np.zeros(env.n_states, dtype=float)
    for iteration in range(max_iterations):
        delta = 0
        for state in range(env.n_states):
            value_new = 0
            action = policy[state]
            for next_state in range(env.n_states):
                prob = env.p(next_state, state, action)
                reward = env.r(next_state, state, action)
                value_new += prob * (reward + gamma * value[next_state])
            delta = max(delta, abs(value[state] - value_new))
            value[state] = value_new
        if delta < theta:
            break
    return value


def policy_improvement(env, value, gamma):
    """ 计算基于当前状态价值的新策略 """
    policy = np.zeros(env.n_states, dtype=int)
    for state in range(env.n_states):
        best_action = None
        best_q_value = float('-inf')
        for action in range(env.n_actions):
            q_value = sum(env.p(next_state, state, action) *
                          (env.r(next_state, state, action) + gamma * value[next_state])
                          for next_state in range(env.n_states))
            if q_value > best_q_value:
                best_q_value = q_value
                best_action = action
        policy[state] = best_action
    return policy


def policy_iteration(env, gamma, theta, max_iterations, policy=None):
    """ 通过策略评估和策略改进反复迭代，找到最优策略 """
    if policy is None:
        policy = np.zeros(env.n_states, dtype=int)
    else:
        policy = np.array(policy, dtype=int)

    for i in range(max_iterations):
        value = policy_evaluation(env, policy, gamma, theta, max_iterations)
        new_policy = policy_improvement(env, value, gamma)
        if np.array_equal(policy, new_policy):
            print(f"Policy converged after {i + 1} iterations")
            break
        policy = new_policy
    return policy, value
