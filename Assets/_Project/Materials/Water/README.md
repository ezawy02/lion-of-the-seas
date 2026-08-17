# T048 water benchmark family

`SeaLionWater.shader` is a single URP transparent shader family for the benchmark water
surface and pooled water effects. It uses two cheap analytic wave terms and vertex-color foam;
there are no texture samples, scene-depth reads, or per-particle allocations.

| Asset | Use | Primary / Reduced intent |
| --- | --- | --- |
| `SeaLion_Water_Primary.mat` | 18x18 `WaterSurface` grid | Fuller wave and foam motion |
| `SeaLion_Water_Reduced.mat` | Reduced surface | Lower wave/foam cost and opacity |
| `SeaLion_Foam_Primary.mat` | Wake, landing, hit, boss rings | High-readability ivory foam |
| `SeaLion_Foam_Reduced.mat` | Reduced pooled effects | Lower intensity/opacity |

The palette is deep navy and turquoise water with ivory foam. `WaterSurface.SetQuality` and
`WaterVfxEffect.SetQuality` are presentation-only controls; they do not alter simulation values.
