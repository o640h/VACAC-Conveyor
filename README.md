# VACAC Modular Conveyor System

A modular conveyor-building prototype created in Unity 6.6 for the VACAC Graduate Software Engineer technical task.

## Repository

[Repository Link](https://github.com/o640h/VACAC-Conveyor)

## Current features

- Four modular conveyor types: long, short, incline, and decline
- Runtime placement from either end of an existing conveyor
- Automatic connector alignment across flat and elevated sections
- Continuous product movement across the connected conveyor graph
- Alternating box and can products
- Build/run modes with speed, spawn-rate, and throughput controls
- Free-fly inspection camera

## Design decisions

### Connector-proximity snapping

Snapping is based on the distance between the preview conveyor's connector
and compatible connectors in the scene, rather than requiring the cursor to
hit a small screen-space snap point. This reduces precision demands, feels
consistent from different camera angles, and lets the user focus on arranging
the equipment itself. The comparison is performed horizontally so a conveyor
placed on the ground plane can still snap cleanly to an elevated incline end.
The preview remains centred beneath the cursor, while its initial rotation is
derived from the prefab's input-to-output direction. This normalizes imported
models whose local forward directions differ without adding prefab-specific
rotation rules.
Occupied connectors are rejected to prevent overlapping conveyor branches.
When approaching an elevated connector, the preview temporarily uses a
horizontal placement plane derived from that connector's height. This avoids
the perspective offset caused by projecting an elevated conveyor onto the
ground while keeping the model centred beneath the cursor.

## Controls

- `1` — Select generic conveyor
- `2` — Select short conveyor
- `3` — Select incline conveyor
- `4` — Select decline conveyor
- `R` — Rotate preview
- `Esc` — Cancel placement
- Left mouse button — Place conveyor
- Right mouse button + mouse — Look around
- `WASD` — Move camera
- `Q` / `E` — Move camera down/up
- `Shift` — Move camera faster

## Unity version

Unity 6.6
