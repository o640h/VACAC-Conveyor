# VACAC Modular Conveyor System

A small Unity prototype created for the VACAC Graduate Software Engineer
technical assessment. It lets the user assemble a conveyor line at runtime,
then run products across the connected layout.

**Repository:** [github.com/o640h/VACAC-Conveyor](https://github.com/o640h/VACAC-Conveyor)

## Features

- Four placeable conveyor types: long, short, incline, and decline
- Connector-based snapping from either end, including elevated connections
- Placement feedback for free, valid, and occupied positions
- Continuous product movement across the connected conveyor graph
- Alternating box and can products
- Separate Build and Run modes
- Adjustable conveyor speed and product spawn interval
- Live active, completed, and throughput figures
- Free-fly camera and runtime layout reset

## Controls

| Input | Action |
|---|---|
| `1` / `2` / `3` / `4` | Select long, short, incline, or decline conveyor |
| Left mouse button | Place conveyor |
| `R` | Rotate preview |
| `Esc` | Cancel placement |
| Right mouse button + mouse | Look around |
| `WASD` | Move camera |
| `Q` / `E` | Move down / up |
| `Shift` | Move faster |

Use the on-screen **Build**, **Run**, and **Reset** controls to switch modes,
start the simulation, or clear products and conveyors added during the session.

## Running the Project

1. Open the project folder in Unity.
2. Open `Assets/_Project/Scenes/Main.unity`.
3. Enter Play mode.
4. Build a layout, switch to Run, and adjust the simulation controls as needed.

## Technical Approach

- `ConveyorSegment` stores input/output connectors, movement points, and explicit
  previous/next links.
- `ConveyorPlacementManager` handles previews, rotation, connector occupancy,
  snapping, and runtime cleanup.
- `ConveyorProduct` follows each segment's path before transferring to the next
  linked segment.
- `ProductSpawner` finds the true start of the current conveyor chain and
  alternates the supplied product types.
- `SimulationController` coordinates Build/Run state, reset behaviour, speed,
  spawning, and production figures.

### Placement Design

Snapping uses the distance between compatible conveyor connectors rather than
requiring the cursor to hit a small screen-space target. This is quicker to use
and remains predictable from different camera angles. Near an elevated
connector, the preview uses a temporary placement plane at the required height,
avoiding the perspective offset caused by projecting everything onto the
ground. Occupied connectors are rejected to prevent overlapping branches.

## Automated Tests

Two Edit Mode tests cover the core conveyor graph behaviour:

- Reconnecting a segment clears stale links and preserves a valid two-way link.
- Any segment in a connected chain can find the first conveyor.

## Tools used

- Unity with Universal Render Pipeline
- C# and the Unity Input System
- Unity UI (uGUI)
- Unity Test Framework / NUnit
- Visual Studio and Visual Studio Code
- Git and GitHub
