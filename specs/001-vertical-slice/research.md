# Research: First Playable Vertical Slice

## 1. Engine version

**Decision**: Use Unity 6.3 LTS and the matching released URP package for the production
baseline.

**Rationale**: Unity identifies 6.3 as the current LTS and supports it until December 2027,
which gives the slice a stable production window without adopting short-lived update
releases. Source: [Unity 6 release support](https://unity.com/releases/unity-6/support).

**Alternatives considered**:

- Unity 6.5 Update: newer, but its support window ends when the next update replaces it.
- Unity 2022/2023: compatible with some reference projects, but shorter remaining support
  and less reason to begin a new production there.
- Godot or Unreal: capable engines, but they add migration and tooling cost while the
  available reference code and team plan are Unity-oriented.

## 2. Rendering pipeline

**Decision**: Use URP Forward with separate primary and reduced-effects URP assets.

**Rationale**: The slice needs scalable mobile lighting and material settings. Unity's URP
guidance explicitly supports reducing memory and CPU/GPU cost through depth/opaque texture
choices, shadow distance, light settings, SRP Batcher, mobile depth priming, and simplified
materials. Source: [Configure URP for better performance](https://docs.unity3d.com/6000.0/Documentation/Manual/urp/configure-for-better-performance.html).

**Alternatives considered**:

- HDRP: rejected because it targets higher-end hardware and conflicts with the mobile floor.
- Built-in pipeline: rejected because it weakens the planned quality-tier and Shader Graph
  workflow for a new Unity 6 project.
- Deferred/Forward+: deferred until profiling proves a real need for many dynamic lights.

## 3. Crowd simulation and rendering

**Decision**: Use a hybrid architecture: Burst jobs and NativeArray state for ordinary
agents, a spatial grid for local queries, GPU-instanced rendering, and conventional Unity
objects only for orchestration, bosses, heroes, UI, and authoring.

**Rationale**: Unity's Burst compiler is designed to turn job-compatible code into optimized
native code, while NativeContainers allow safe shared native memory. Unity also provides
`Graphics.RenderMeshInstanced` to render multiple instances more efficiently through GPU
instancing. Sources: [Burst package](https://docs.unity3d.com/6000.0/Manual/com.unity.burst.html),
[NativeContainer guidance](https://docs.unity3d.com/2022.3/Documentation/Manual/JobSystemNativeContainer.html),
and [RenderMeshInstanced](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Graphics.RenderMeshInstanced.html).

**Alternatives considered**:

- One GameObject/Rigidbody/Animator per unit: rejected because multiplication creates the
  highest load at the exact moment the game must look smooth.
- Full Entities/ECS from day one: rejected for the slice because it increases authoring and
  onboarding complexity before the 500-agent benchmark proves it is necessary.
- NavMeshAgent per unit: rejected because the levels use controlled lanes and landing zones;
  a lightweight flow field and spatial grid are sufficient.

## 4. Animation strategy

**Decision**: Use shader-driven phase variation or a small baked pose set for ordinary
crowd units. Use skeletal rigs and Animator only for captains, bosses, and close hero units.

**Rationale**: Crowd readability depends more on synchronized direction, silhouette, hit
response, and variation than on hundreds of independent skeletal state machines. The split
preserves expressive hero animation while keeping the crowd renderable at the required cap.

**Alternatives considered**:

- Full skeletal animation for every unit: rejected unless a measured benchmark later proves
  it fits both device tiers.
- Billboard sprites: fast, but rejected for the primary style because camera push-ins and
  landings need coherent 3D volume. It remains a possible far-distance fallback.

## 5. Art authoring and import

**Decision**: Use the locally installed Blender 5.1.1 for bespoke art during the slice,
pin `.blend` authoring to that version, and export reviewed FBX files plus textures to Unity.
Store raw source files separately from game-ready exports.

**Rationale**: Blender is already available on the workstation, and FBX gives the Unity
project a stable, reviewable interchange artifact. The art contract focuses on in-engine
phone-size results rather than Blender render quality. Blender 4.5 LTS remains the fallback
if a multi-artist team needs longer source-file compatibility. Source:
[Blender 4.5 LTS](https://www.blender.org/releases/4-5/).

**Alternatives considered**:

- Direct `.blend` import into Unity: rejected because it silently depends on a local Blender
  installation and can produce inconsistent imports across machines.
- Purchasing a complete visual identity: rejected because mixed store packs rarely create
  a distinctive, coherent store-facing level. Compatible assets remain useful for greybox
  and secondary props.

## 6. Water and ship motion

**Decision**: Use a stylized low-cost water shader, authored foam/wake meshes, simple height
sampling where needed, and kinematic ship movement along controlled combat lanes.

**Rationale**: The vertical slice camera needs convincing color, surface motion, contact
foam, and wakes, not a general-purpose ocean or physical sailing simulation. This preserves
budget for crowd and boss feedback.

**Alternatives considered**:

- Physical ocean and buoyancy for every craft: rejected as unnecessary scope.
- Static flat plane: acceptable for the first greybox only; it cannot pass the Art Lock Gate.

## 7. Testing and evidence

**Decision**: Use EditMode, PlayMode, Performance tests, plus physical-device capture gates.

**Rationale**: Unity's Test Framework supports EditMode, PlayMode, and target-platform runs,
while the Performance Testing package can record repeatable measurements. Product quality
still requires device footage, profiler evidence, and human visual review. Source:
[Unity automated testing](https://docs.unity3d.com/6000.0/Documentation/Manual/testing-editortestsrunner.html).

**Alternatives considered**:

- Editor-only testing: rejected because mobile GPU, thermal, memory, input, and shader
  behavior cannot be approved from the workstation alone.
- Manual-only testing: rejected because arithmetic, save migration, event order, and retries
  need repeatable regression protection.

## 8. Persistence

**Decision**: Persist a small versioned JSON snapshot locally using atomic write-and-replace.
Keep static gameplay definitions out of the save.

**Rationale**: The slice needs only loadout, ownership, rewards, progress, settings, and save
version. A local schema is sufficient, inspectable, and easy to migrate.

**Alternatives considered**:

- PlayerPrefs for all data: rejected because structured migration and validation become
  fragile.
- Database or cloud save: rejected because no slice requirement justifies a backend.

## 9. Repository and source-size policy

**Decision**: Keep the GitHub repository private and dedicated to this project. Require
every contributor to read `AGENTS.md` and the active plan before editing. Enforce at most
1,000 physical lines for new or changed authored source, freeze legacy oversized files
until split, preserve a 1,500-line absolute ceiling, and target 500 or fewer.

**Rationale**: The user explicitly restricted remote work to this project and requested
small, easy-to-edit files. Generated and vendor code are reported separately because the
team does not own their structure.

**Alternatives considered**:

- A monorepo with unrelated work: rejected by explicit user scope and privacy requirements.
- A lower universal hard cap such as 300 lines: rejected because data-oriented systems and
  custom editor tooling can remain cohesive above that size; 500 remains the target and
  1,000 is the maximum for a changed file.

## Resolved dependencies

- Unity Editor is not currently installed on the workstation. Unity Hub plus Unity 6.3 LTS,
  Android Build Support, SDK, NDK, and OpenJDK are required before implementation begins.
- Blender 5.1.1 is currently installed and suitable for the initial art-source workflow.
- Exact physical Android models are a required M2 input; a device class is defined now so
  implementation can begin without pretending an editor benchmark passes the device gate.
