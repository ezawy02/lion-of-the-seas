# 3D Conversion Batch — 2026-08-26

Each PNG contains exactly one isolated asset. Keep the asset IDs unchanged when returning
the 3D sources and exports. These images are modeling references only and remain REVIEW
until the corresponding model is inspected and explicitly approved by the user in Unity.

| Asset ID | Image | Level | Target category |
|---|---|---:|---|
| L01-CHR-004 | `L01-CHR-004_Harbor_Guardian_Single_REVIEW.png` | 1 | Boss character |
| L01-CHR-005 | `L01-CHR-005_Enemy_Commander_Single_REVIEW.png` | 1 | Enemy commander |
| L02-SHP-001 | `L02-SHP-001_Armored_Warship_Boss_Single_REVIEW.png` | 2 | Boss warship |
| L02-PRP-001 | `L02-PRP-001_Floating_Naval_Mine_Single_REVIEW.png` | 2 | Gameplay prop |
| L02-PRP-002 | `L02-PRP-002_Heavy_Chain_Link_Unit_Single_REVIEW.png` | 2 | Modular gameplay prop |
| L03-SHP-001 | `L03-SHP-001_Gunpowder_Skiff_Single_REVIEW.png` | 3 | Small enemy craft |
| L03-PRP-001 | `L03-PRP-001_Gunpowder_Barrel_Cluster_Single_REVIEW.png` | 3 | Gameplay prop cluster |
| L03-CHR-001 | `L03-CHR-001_Storm_Fortress_Commander_Single_REVIEW.png` | 3 | Final commander boss |

## Return requirements

- Preserve believable depth and a clean silhouette on every axis; no flat card meshes.
- Keep character rigs humanoid and retain separate weapon/accessory objects where practical.
- Keep ship masts, sails, oars, rudders, weapons, and damageable armor as separate named parts.
- Keep the heavy chain link modular so repeated copies interlock without visible gaps.
- Use meters and a consistent world scale; apply transforms before export.
- Provide editable source plus FBX export and embedded or adjacent textures.
- Do not combine all materials into one opaque placeholder. Preserve the documented palette.
- Do not label any result final. Every returned model remains REVIEW until approved in Unity.
