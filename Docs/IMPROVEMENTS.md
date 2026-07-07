Antz — Prioritized Improvement Plan
Date: 2026-07-04

Overview: prioritized, actionable improvements split into High / Medium / Low priority. Each item includes brief rationale, action summary, and target files.

High Priority (1–6) — immediate stability / performance / correctness
1. Replace singletons with injectable managers — High
- Rationale: improves testability, reduces hidden coupling.
- Action: introduce interfaces (e.g., `IPheromoneMap`), inject managers into `Ant`/`AntColony` at spawn.
- Targets: Assets/Scripts/Pheromones/PheromoneMap.cs, Assets/Scripts/Ants/Ant.cs, Assets/Scripts/Ants/AntColony.cs

2. Spatial data structure for pheromones (grid / spatial hash) — High
- Rationale: current cubic search scales poorly and GC-heavy.
- Action: implement fixed 3D grid or spatial hash with cell buckets and radius queries.
- Targets: Assets/Scripts/Pheromones/PheromoneMap.cs, pheromone model

3. Object pooling for frequent Instantiate/Destroy — High
- Rationale: reduces GC spikes and improves frame stability.
- Action: add pooling for Ants, Food, pheromone markers; replace Instantiate/Destroy with Get/Release.
- Targets: Assets/Scripts/Ants/AntColony.cs, Assets/Scripts/Ants/MovingEntity.cs, Food generator

4. Event unsubscription & lifecycle safety — High
- Rationale: avoid callbacks to destroyed objects and memory leaks.
- Action: add `OnDestroy()` to unsubscribe all delegates added in `Init` and elsewhere; prefer explicit unsubscribe.
- Targets: Assets/Scripts/Ants/Ant.cs, Assets/Scripts/Ants/MovingEntity.cs, UI listeners

5. Fix Genome initialization & null-safety — High
- Rationale: current `Genome` may be incomplete and `Cross` assumes non-null traits.
- Action: enforce `Genome.InitializeTraitDefinitions()` at startup; make constructor create defaults if missing; add null checks in `Cross`.
- Targets: Assets/Scripts/Genome/Genome.cs, Assets/Scripts/SceneManagers/GameManager.cs

6. Reduce allocations and use squared distances in hot loops — High
- Rationale: `GetPheromones` and `FindNearest` allocate heavily and call `Vector3.Distance`.
- Action: use squared-magnitude checks, reuse pooled lists/buffers, avoid creating arrays per call.
- Targets: Assets/Scripts/Pheromones/PheromoneMap.cs, Assets/Scripts/Ants/MovingEntity.cs

Medium Priority (7–14) — robustness, maintainability, and medium-effort gains
7. Remove heavy logging from editor gizmos — Medium
- Rationale: `Debug.Log` in `OnDrawGizmosSelected` spams the editor and slows iteration.
- Action: remove or gate Debug.Log and heavy work inside gizmo callbacks.
- Targets: Assets/Scripts/Ants/Ant.cs, Assets/Scripts/Ants/MovingEntity.cs

8. Centralize config into ScriptableObjects — Medium
- Rationale: tunables should be editable, versionable, and decoupled from code.
- Action: create `AntzConfig` (ACO settings, decay factors, thresholds) and `GenomeTraitDefinition` assets.
- Targets: Assets/Scripts/Genome/Genome.cs, Assets/Scripts/Pheromones/PheromoneMap.cs, movement code

9. State machine reuse / factory — Medium
- Rationale: creating state objects on every transition causes allocations and complexity.
- Action: implement a `StateFactory` or reuse immutable state instances and separate state data from behavior.
- Targets: Assets/Scripts/AntStates/*, Assets/Scripts/Ants/Ant.cs

10. Movement & physics improvements — Medium
- Rationale: direct transform position changes may conflict with Rigidbodies.
- Action: if using physics, use cached Rigidbody + `MovePosition`; compute scalar deltas correctly.
- Targets: Assets/Scripts/Ants/MovingEntity.cs and relevant prefabs

11. Cache GetComponent and add null-guards — Medium
- Rationale: repeated GetComponent calls are costly and may NRE if component missing.
- Action: cache MeshRenderer, Rigidbody, Collider in Awake/Init and guard on use.
- Targets: Assets/Scripts/Ants/Ant.cs, Assets/Scripts/Ants/MovingEntity.cs

12. Centralize RNG for determinism/testing — Medium
- Rationale: sprinkled `Random` makes behavior non-deterministic and hard to test.
- Action: add `IRandomProvider` injected into systems; support seed-based runs.
- Targets: movement, spawning, genome crossover

13. Naming and API consistency — Medium
- Rationale: clearer naming improves readability and reduces bugs.
- Action: rename `queen` -> `Queen` property, private fields `_queenPrefab`, PascalCase public members.
- Targets: Assets/Scripts/Ants/AntColony.cs, other public APIs

14. Replace Destroy-based Drop with world manager/pool — Medium
- Rationale: gameplay should return items to pool/world manager, not destroy.
- Action: route dropped items to `FoodPool` or `WorldManager` for reuse.
- Targets: Assets/Scripts/Ants/MovingEntity.cs, food logic

Low Priority (15–20) — polish, testing, and long-term improvements
15. Add XML docs and method summaries — Low
- Rationale: improves onboarding and IDE help.
- Action: add summaries for public classes and APIs.
- Targets: Ant, AntColony, PheromoneMap, Genome

16. Add unit tests and CI skeleton — Low
- Rationale: prevent regressions in core deterministic logic.
- Action: add test assembly for `Genome.Cross`, pheromone grid math, basic state transitions.
- Targets: new Tests/ folder and test runner config

17. Profiling instrumentation for hot paths — Low
- Rationale: measure before/after changes.
- Action: add lightweight counters and Unity Profiler markers around pheromone queries and FixedUpdate.
- Targets: PheromoneMap, Ant.FixedUpdate

18. Plan DOTS/Jobs migration for large-scale sims — Low
- Rationale: needed if sim scales to thousands of agents.
- Action: create incremental migration plan: isolate logic, prototype Jobs implementations, measure.
- Targets: architecture doc + prototypes

19. Improve fail-fast error handling — Low
- Rationale: avoid silent partial initialization.
- Action: throw clear exceptions or provide robust fallbacks at startup.
- Targets: Genome, startup code

20. UI/UX debug overlays — Low
- Rationale: better runtime debugging of pheromone densities and ant metrics.
- Action: add debug toggles, aggregated metrics window, and toggleable pheromone visuals.
- Targets: Assets/Scripts/Interface/*, Pheromone map renderer

Quick Wins (apply first)
- Implement items 4 (event unsubscriptions), 7 (remove gizmo logging), and 5 (Genome null-safety) — small edits with immediate stability improvements.
- Replace `Vector3.Distance` with squared distance in hot loops — quick perf win (item 6).
- Fix singleton assignment pattern in `PheromoneMap` (ensure only one instance, or explicit destroy/throw) — stability win.

Estimated order of work
- Phase A (1–2 weeks): Items 4, 5, 6, 7, singleton fix, caching components.
- Phase B (2–4 weeks): Implement pooling, spatial hash grid, and manager injection.
- Phase C (4+ weeks): State factory, ScriptableObject config, RNG provider.
- Phase D (optional): Tests, profiling, DOTS/Jobs migration.

Notes & Risks
- Large refactors (spatial grid, DOTS) affect many systems — add integration tests and benchmark before/after.
- Injection approach requires update to spawning flow (AntColony/QueenAnt) to pass managers.
- Object pooling requires careful prefab reset logic to avoid stale state.

Next steps (choose one)
- I can generate small code patches for quick wins (4, 5, 6, singleton fix).
- Or produce a more detailed design doc for spatial grid + pooling with code skeletons.

Which next step do you want me to do?
