import random
from collections import deque


class ReplayBuffer:
    def __init__(self, buffer_size, random_state):
        self.buffer = deque(maxlen=buffer_size)
        self.random_state = random_state

    def __len__(self):
        return len(self.buffer)

    def append(self, transition):
        self.buffer.append(transition)

    def draw(self, batch_size):
        samples = min(len(self.buffer), batch_size)
        batch = random.sample(self.buffer, samples)
        return batch
