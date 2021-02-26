# Game Of Life
[Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life)

## Demo:

```c#
      // Glider hits tub
      GameOfLifeGeneration game = GameOfLifeGeneration.Parse(
        @"...........
          ....O......
          ...O.......
          ...OOO.....
          ...........
          .O.........
          O.O........
          .O.........
        ");

      // Move 100 generations ahead
      game.Next(100);
      
      // Print the result
      Console.Write(game);
```

## Outcome:

```
.O.
O.O
O.O
.O.
```
