import numpy as np
import contextlib
from Environments.environment import Environment


# Configures numpy print options
@contextlib.contextmanager
def _printoptions(*args, **kwargs):
    original = np.get_printoptions()
    np.set_printoptions(*args, **kwargs)
    try:
        yield
    finally:
        np.set_printoptions(**original)


class FrozenLake(Environment):
    def __init__(self, lake, slip, max_steps, seed=None):
        """
        lake: A matrix that represents the lake. For example:
        lake =  [['&', '.', '.', '.'],
                ['.', '#', '.', '#'],
                ['.', '.', '.', '#'],
                ['#', '.', '.', '$']]
        slip: The probability that the agent will slip
        max_steps: The maximum number of time steps in an episode
        seed: A seed to control the random number generator (optional)
        """
        # start (&), frozen (.), hole (#), goal ($)
        self.lake = np.array(lake)
        self.lake_flat = self.lake.reshape(-1)

        self.slip = slip

        n_states = self.lake.size + 1
        n_actions = 4

        pi = np.zeros(n_states, dtype=float)
        pi[np.where(self.lake_flat == '&')[0]] = 1.0

        self.absorbing_state = n_states - 1

        self.ProbAct = np.zeros((n_states, n_states, n_actions))
        n_rows, n_cols = self.lake.shape

        for state in range(n_states - 1):
            if self.lake_flat[state] in ['#', '$']:
                self.ProbAct[self.absorbing_state, state, :] = 1
                continue

            row, col = divmod(state, n_cols)

            for action in range(n_actions):
                next_row, next_col = row, col

                # Determine next state based on action
                if action == 0:  # UP
                    next_row = max(0, row - 1)
                elif action == 1:  # LEFT
                    next_col = max(0, col - 1)
                elif action == 2:  # DOWN
                    next_row = min(n_rows - 1, row + 1)
                elif action == 3:  # RIGHT
                    next_col = min(n_cols - 1, col + 1)

                # Collect valid slip directions
                valid_slip_states = []
                for slip_action in range(n_actions):
                    slip_row, slip_col = row, col

                    if slip_action == 0:  # UP
                        slip_row = max(0, row - 1)
                    elif slip_action == 1:  # LEFT
                        slip_col = max(0, col - 1)
                    elif slip_action == 2:  # DOWN
                        slip_row = min(n_rows - 1, row + 1)
                    elif slip_action == 3:  # RIGHT
                        slip_col = min(n_cols - 1, col + 1)

                    # Add valid directions, including self-loop in corners or edges
                    valid_slip_states.append((slip_row, slip_col))

                # Assign probabilities for active move
                next_state = next_row * n_cols + next_col
                self.ProbAct[next_state, state, action] += 1 - self.slip

                # Assign probabilities for slip directions
                n_valid_slip = len(valid_slip_states)
                for slip_row, slip_col in valid_slip_states:
                    slip_state = slip_row * n_cols + slip_col
                    self.ProbAct[slip_state, state, action] += self.slip / n_valid_slip

        self.ProbAct[self.absorbing_state, self.absorbing_state, :] = 1
        Environment.__init__(self, n_states, n_actions, max_steps, pi, seed=seed)

    def step(self, action):
        state, reward, done = Environment.step(self, action)

        done = (state == self.absorbing_state) or done

        return state, reward, done

    def p(self, next_state, state, action):
        ProbAct = self.ProbAct[next_state, state, action]
        return ProbAct

    def r(self, next_state, state, action):
        if state == self.absorbing_state:
            return 0
        if self.lake_flat[state] == '$':
            return 1
        else:
            return 0

    def render(self, policy=None, value=None):
        if policy is None:
            lake = np.array(self.lake_flat)

            if self.state < self.absorbing_state:
                lake[self.state] = '@'

            print(lake.reshape(self.lake.shape))
        else:
            actions = ['^', '<', '_', '>']

            print('Lake:')
            print(self.lake)

            print('Policy:')
            policy = np.array([actions[a] for a in policy[:-1]])
            print(policy.reshape(self.lake.shape))

            print('Value:')
            with _printoptions(precision=3, suppress=True):
                print(value[:-1].reshape(self.lake.shape))
