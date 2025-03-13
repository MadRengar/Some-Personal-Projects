# 值迭代
import numpy as np


def calculate_q_value(env, state, action, gamma, value):
    """ 计算 Q 值 """
    return sum(env.p(next_state, state, action) *
               (env.r(next_state, state, action) + gamma * value[next_state])
               for next_state in range(env.n_states))


def value_iteration(env, gamma, theta, max_iterations, value=None):
    """ 通过值迭代找到最优策略 """
    value = np.zeros(env.n_states) if value is None else np.array(value, dtype=np.float64)
    policy = np.zeros(env.n_states, dtype=int)

    for i in range(max_iterations):
        delta = 0
        for state in range(env.n_states):
            q_values = [calculate_q_value(env, state, action, gamma, value) for action in range(env.n_actions)]
            best_value = max(q_values)
            best_action = np.argmax(q_values)
            delta = max(delta, abs(value[state] - best_value))
            value[state] = best_value
            policy[state] = best_action  # 更新策略
        if delta < theta:
            print(f"Value iteration converged after {i + 1} iterations")
            break
    return policy, value
