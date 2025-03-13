import numpy as np


class FrozenLakeImageWrapper:
    def __init__(self, env):
        self.env = env
        lake = self.env.lake
        self.n_actions = self.env.n_actions
        self.state_shape = (4, lake.shape[0], lake.shape[1])
        lake_image = [(lake == c).astype(float) for c in ['&', '#', '$']]
        self.state_image = {
            self.env.absorbing_state: np.stack([np.zeros(lake.shape)] + lake_image)}  ## change to self.env
        for state in range(lake.size):
            position = np.unravel_index(state, lake.shape)
            state_img = np.zeros((4, lake.shape[0], lake.shape[1]))
            state_img[0, position[0], position[1]] = 1
            state_img[1, :, :] = lake_image[0]
            state_img[2, :, :] = lake_image[1]
            state_img[3, :, :] = lake_image[2]
            self.state_image[state] = state_img

    def encode_state(self, state):
        return self.state_image[state]

    def decode_policy(self, dqn):
        states = np.array([self.encode_state(s) for s in range(self.env.n_states)])
        q = dqn(states).detach().numpy()  # torch.no_grad omitted to avoid import
        policy = q.argmax(axis=1)
        value = q.max(axis=1)
        return policy, value

    def reset(self):
        return self.encode_state(self.env.reset())

    def step(self, action):
        state, reward, done = self.env.step(action)
        return self.encode_state(state), reward, done

    def render(self, policy=None, value=None):
        self.env.render(policy, value)
