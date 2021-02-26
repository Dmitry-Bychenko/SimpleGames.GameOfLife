# Game Of Life
[Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life)

## Demo:

```c#
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

      game.Next(100);
      
      Console.Write(game);
```

## Outcome:

```
.O.
O.O
O.O
.O.
```
