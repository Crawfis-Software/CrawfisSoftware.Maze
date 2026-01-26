# CrawfisSoftware.Maze
Maze generation and maze *crafting* utilities.

This library centers around building a grid-based maze by setting *which neighboring cells are connected* using `Direction` flags. You can:

- Randomly generate mazes using the included algorithms.
- Craft mazes by explicitly carving passages, placing walls, and composing hand-authored regions with algorithmic generation.

## Key types

### `MazeBuilder<N, E>`

`MazeBuilder<N, E>` implements `IMazeBuilder<N, E>` and is the main “editable maze” type.

- Holds an underlying `Grid<N, E>` (`mazeBuilder.Grid`).
- Maintains a 2D array of `Direction` flags (one per cell) representing which exits are open.
- Can be edited cell-by-cell and then converted to a read-only `Maze<N, E>` via `mazeBuilder.GetMaze()`.

### `IMazeBuilder<N, E>`

The editing surface for maze creation. Most crafting APIs are available here, including:

- `SetCell(column, row, dirs, preserveExistingCells)`
- `CarvePassage(...)` / `AddWall(...)`
- `GetDirection(column, row)`
- `Clear()`
- `RemoveUndefines()`
- `MakeBidirectionallyConsistent(...)`

### `Maze<N, E>`

An immutable view of a maze that implements graph interfaces (`IIndexedGraph<N,E>`). A `Maze` filters the edges of the underlying grid based on the stored `Direction` flags.

Useful things you can do with a `Maze`:

- Query connectivity via `ContainsEdge(from, to)`, `Neighbors(node)`.
- Print an ASCII view with `maze.ToString()`.

## Coordinate system and cell indices

The builder and the maze use *columns* and *rows* (integer grid coordinates), plus a flattened *cell index*.

- `(column, row)` are 0-based.
- A *cell index* is computed as:

  `cellIndex = row * Width + column`

Cell indices therefore increase left-to-right across a row, then bottom-to-top.

## Understanding `Direction` flags

Each cell stores a set of flags indicating which directions are open:

- `Direction.N`, `Direction.E`, `Direction.S`, `Direction.W`
- `Direction.None` means “no open exits” (a blocked/empty cell)
- `Direction.Undefined` is special: it marks the cell as *not finalized yet* (see the crafting workflow below)

When you display a maze with `Maze.ToString()`, cells that are `Direction.Undefined` or `Direction.None` are rendered as filled (`...`) to make unfinished/blocked cells obvious.

## The MazeBuilder workflow (random generation vs crafting)

### Random generation (typical)

1. Create a `MazeBuilder<N, E>`.
2. Run a generator such as `MazeBuilderRecursiveDivision<N, E>.CarveMaze()`.
3. Optionally post-process with extension methods/modifiers.
4. Call `GetMaze()`.

### Crafting (hand-authored shapes + optional generation)

Crafting is about explicitly editing the builder’s `Direction` array.

Recommended workflow:

1. **Create a builder**
2. **Prepare the canvas** (clear, open regions, wall boundaries)
3. **Carve** corridors/rooms/doors (passages) and **place walls**
4. **Freeze** the parts you want to keep
5. **Fill the rest** with a generator while preserving your frozen work
6. **Fix consistency**, then **finalize** (remove `Undefined`)
7. **Materialize** a `Maze` via `GetMaze()`

The sections below describe each phase.

## Step-by-step: crafting APIs

### 1) Create a builder

Most users can use `object`/`int` placeholders for labels if they don’t care about node/edge labels.

```csharp
using CrawfisSoftware.Maze;

var builder = new MazeBuilder<object, int>(width: 20, height: 10);
builder.StartCell = 0;
builder.EndCell = builder.Width * builder.Height - 1;
```

### 2) Prepare the canvas

You can either start from a blank slate (`Clear`) or explicitly open/block regions.

- `builder.Clear()` sets every cell to `Direction.Undefined`.
- `OpenRegion(lowerLeftCell, upperRightCell, markAsUndefined: true)` opens all internal neighbor connections in a rectangle.
- `BlockRegion(lowerLeftCell, upperRightCell)` removes all exits in a rectangle.
- `WallBoundary()` removes exits that lead outside the maze.

Example: open everything, then enforce outer walls:

```csharp
builder.OpenRegion(0, builder.Width * builder.Height - 1);
builder.WallBoundary();
```

### 3) Carve passages and add walls

You can work in either coordinate space:

- By `(column,row)` via `CarvePassage(currentColumn, currentRow, selectedColumn, selectedRow)`
- By *cell index* via the extension `CarvePassage(currentCell, targetCell)`

There are also directional helpers:

- `CarveDirectionally(column, row, Direction.E)`
- `BlockDirectionally(column, row, Direction.N)`
- `CarveHorizontalSpan(row, col1, col2, preserveExistingCells)`
- `CarveVerticalSpan(column, row1, row2, preserveExistingCells)`

Example: carve a main horizontal corridor and two vertical branches:

```csharp
// A main corridor on row 2 from column 1 to 18
builder.CarveHorizontalSpan(row: 2, column1: 1, column2: 18, preserveExistingCells: false);

// Two vertical branches off that corridor
builder.CarveVerticalSpan(column: 5, row1: 2, row2: 8, preserveExistingCells: false);
builder.CarveVerticalSpan(column: 14, row1: 2, row2: 7, preserveExistingCells: false);
```

Example: create a blocked “solid” room area inside the maze:

```csharp
// Block a 4x3 rectangle (remember: indices are row*Width+col)
int lowerLeft = 6 + 4 * builder.Width;
int upperRight = 9 + 6 * builder.Width;
builder.BlockRegion(lowerLeft, upperRight);
```

### 4) Freezing vs `preserveExistingCells`

Many algorithms and helper methods accept `preserveExistingCells`.

When `preserveExistingCells` is `true`, operations only modify cells that are still marked as `Direction.Undefined`. This is how you “lock in” crafted regions and let algorithms fill the rest.

To freeze authored work, remove `Undefined` from the cells you want to protect:

- `FreezeCellIfUndefined(row, column)` freezes a single cell *if it has been defined*.
- `FreezeDefinedCells()` freezes all cells that aren’t still purely `Undefined`.

Example: craft corridors, then freeze them:

```csharp
builder.CarveHorizontalSpan(row: 2, column1: 1, column2: 18, preserveExistingCells: false);
builder.FreezeDefinedCells();
```

### 5) Fill remaining space with a generator (optional)

Once your key structures are frozen, you can run any generator on the remaining `Undefined` cells by passing `preserveExistingCells: true`.

Example: combine handcrafted corridors with Recursive Division:

```csharp
builder.OpenRegion(0, builder.Width * builder.Height - 1);
builder.WallBoundary();

// Hand craft: a guaranteed corridor
builder.CarveHorizontalSpan(row: 2, column1: 1, column2: 18, preserveExistingCells: false);
builder.FreezeDefinedCells();

// Fill the rest without touching frozen cells
var division = new MazeBuilderRecursiveDivision<object, int>(builder);
division.CarveMaze(preserveExistingCells: true);
```

You can repeat this pattern for other algorithms in the library that accept an `IMazeBuilder` and a `preserveExistingCells` flag.

### 6) Ensure bidirectional consistency

Since crafting can edit individual cells, it’s possible to accidentally create one-way passages (cell A says it connects to cell B, but cell B doesn’t connect back).

To repair this:

- `builder.MakeBidirectionallyConsistent(carvingMissingPassages: true)` will open the missing reverse passages.
- `builder.MakeBidirectionallyConsistent(carvingMissingPassages: false)` will instead wall off inconsistent edges.

In most cases, `carvingMissingPassages: true` is what you want after carving.

### 7) Finalize: remove `Undefined`

If you want the final maze to have no “unfinished” cells, call:

```csharp
builder.RemoveUndefines();
```

This clears the marker bit but does not otherwise change connectivity.

### 8) Build the final `Maze`

```csharp
var maze = builder.GetMaze();
Console.WriteLine(maze.ToString());
```

## Troubleshooting and common pitfalls

### My passages look one-way / solver can’t traverse

Use `MakeBidirectionallyConsistent(...)` after manual editing (especially if you used `SetCell` or removed directions explicitly).

### My ASCII output shows `...` cells

Those cells are either:

- `Direction.Undefined` (not finalized / still editable), or
- `Direction.None` (intentionally blocked)

If you intended a complete maze, ensure you filled all cells and call `RemoveUndefines()`.

### Off-by-one and out-of-range neighbors

Directional carving helpers assume the neighbor exists. Prefer the higher-level span helpers (`CarveHorizontalSpan`, `CarveVerticalSpan`) or check bounds before carving to avoid targeting invalid neighbors.

## Additional utilities

- `MazeBuilderModifiers.TrimDeadEnds(...)` can post-process a maze using computed metrics.
- `MazeBuilderExtensions` contains many region and carving helpers (`OpenRegion`, `BlockRegion`, `WallBoundary`, `InvertDirections`, etc.).
