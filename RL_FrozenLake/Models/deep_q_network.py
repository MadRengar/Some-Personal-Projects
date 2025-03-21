import torch
import torch.nn.functional as F
import numpy as np


class DeepQNetwork(torch.nn.Module):
    def __init__(self, env, learning_rate, kernel_size, conv_out_channels,
                 fc_out_features, seed):
        torch.nn.Module.__init__(self)
        torch.manual_seed(seed)
        self.conv_layer = torch.nn.Conv2d(in_channels=env.state_shape[0],
                                          out_channels=conv_out_channels,
                                          kernel_size=kernel_size, stride=1)

        h = env.state_shape[1] - kernel_size + 1
        w = env.state_shape[2] - kernel_size + 1
        self.fc_layer = torch.nn.Linear(in_features=h * w * conv_out_channels,
                                        out_features=fc_out_features)
        self.output_layer = torch.nn.Linear(in_features=fc_out_features,
                                            out_features=env.n_actions)

        self.optimizer = torch.optim.Adam(self.parameters(), lr=learning_rate)

    def forward(self, x):
        x = torch.tensor(x, dtype=torch.float)
        x = self.conv_layer(x)
        x = F.relu(x)  # Applying ReLU activation function after convolution
        # Flatten the output for the fully-connected layer
        x = x.view(x.size(0), -1)  # Flatten the tensor
        # Apply the first fully connected layer
        x = self.fc_layer(x)
        x = F.relu(x)  # Applying ReLU activation function after fully connected layer
        # Output layer
        x = self.output_layer(x)

        return x

    def train_step(self, transitions, gamma, tdqn):
        states = np.array([transition[0] for transition in transitions])
        actions = np.array([transition[1] for transition in transitions])
        rewards = np.array([transition[2] for transition in transitions])
        next_states = np.array([transition[3] for transition in transitions])
        dones = np.array([transition[4] for transition in transitions])
        q = self(states)
        q = q.gather(1, torch.Tensor(actions).view(len(transitions), 1).long())
        q = q.view(len(transitions))
        with torch.no_grad():
            next_q = tdqn(next_states).max(dim=1)[0] * (1 - dones)
        target = torch.Tensor(rewards) + gamma * next_q
        # TODO: the loss is the mean squared error between q and target
        q = q.float()
        target = target.float()
        loss = F.mse_loss(q, target)

        self.optimizer.zero_grad()
        loss.backward()
        self.optimizer.step()
