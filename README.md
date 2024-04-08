# Doodle Jump with AI training

## About
This is the source code for a Doodle Jump-like game project in Unity. The game includes options for training the in-game AI, which uses a Neural Network selected by Genetic Algorithm operations.

This project is part to the Nature Inspired Computing course at Innopolis University, Spring 2024.

## AI logic & training

#### Neural Network
The in-game AI uses a shallow neural network for its architecture. This choice was made in order to create a continuous neural network, as it will be easier to work with the game on Unity.

The input layer receives player's speed and position, as well as each visible platform's position, type, and size. The output layer sends a number from -1 to 1 inclusive, which is the player's movement strength to the left or to the right.

![Neural network architecture graphic](/images/doodle-nn-architecture.png)

#### Training/learning approach
The user may watch in-game AI train, which is done using a genetic algorithm (neuroevolution). The training uses GA operations on neural networks and iterates on best performing individuals.

1. A population of AI players (pre-determined amount of individuals) is assigned to neural networks with random parameters
2. Each individual attempts to play a randomly generated game, and the algorithm waits until all individuals have failed or reached a pre-determined threshold
3. Each individual's final score is recorded as the fitness metric
4. The population is updated:
  ```
  5. Sort the current population by fitness, from highest to lowest;
  6. Add top 50% directly to the next population ("winners");
  7. Add 10% to the next population by performing uniform crossover on top 20%;
  8. Add 20% to the next population by performing uniform crossover on a random 40% of winners (20% of the whole population);
  9. Add 20% to the next population by copying a random 40% of winners (20% of the whole population);
  10. Perform mutation by writing random values to a random 30% of parameters;
  ```
11. Repeat from step 2

## Build & implementation info

The game is built in Unity 2022.3. The project can be imported in Unity Hub from the [doodle-jump-src](doodle-jump-src) directory.

There are currently no complete builds, but you can check on overall progress for requirements below:
- ✅ Game logic (game system, controls)
- ❌ AI & training
- ❌ Visual training

Possible additions
- Hyperparameter options for neural networks or the genetic algorithm
- "Game juice" additions: more platform types, enemies
