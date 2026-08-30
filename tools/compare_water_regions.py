#!/usr/bin/env python3
"""Compare the Unity opening capture against the reference using stdlib only.

Converts both PNGs to BMP via macOS sips, then reports mean error and the
median colour of key water regions in each image plus the delta.
"""
import struct
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path("/Users/apple/Desktop/أسد البحار Lion of the Seas")
REFERENCE = ROOT / "ArtSource/References/Level01/REF_Level01_Opening.png"
RENDER = ROOT / "Artifacts/Local/Blockout/Level01_Opening.png"
SIZE = (720, 1280)

REGIONS = {
    "sky": (0.15, 0.06, 0.85, 0.12),
    "far_water": (0.25, 0.20, 0.65, 0.235),
    "mid_water_left": (0.05, 0.37, 0.25, 0.45),
    "mid_water_right": (0.78, 0.37, 0.96, 0.45),
    "shallow_left": (0.02, 0.57, 0.14, 0.69),
    "foreground": (0.15, 0.90, 0.45, 0.95),
    "foam_wake": (0.63, 0.68, 0.80, 0.80),
}


def to_bmp(png: Path, tmp: Path) -> Path:
    out = tmp / (png.stem + ".bmp")
    subprocess.run(
        ["sips", "-s", "format", "bmp", "-z", str(SIZE[1]), str(SIZE[0]), str(png), "--out", str(out)],
        check=True, capture_output=True,
    )
    return out


def load(bmp: Path):
    data = bmp.read_bytes()
    offset = struct.unpack_from("<I", data, 10)[0]
    w, h = struct.unpack_from("<ii", data, 18)
    bpp = struct.unpack_from("<H", data, 28)[0]
    assert bpp == 24, bpp
    top_down = h < 0
    h = abs(h)
    row = (w * 3 + 3) & ~3

    def pixel(x, y):
        yy = y if top_down else h - 1 - y
        base = offset + yy * row + x * 3
        b, g, r = data[base], data[base + 1], data[base + 2]
        return r / 255.0, g / 255.0, b / 255.0

    return w, h, pixel


def median_color(pixel, x0, y0, x1, y1):
    rs, gs, bs = [], [], []
    for y in range(y0, y1, 2):
        for x in range(x0, x1, 2):
            r, g, b = pixel(x, y)
            rs.append(r)
            gs.append(g)
            bs.append(b)
    rs.sort()
    gs.sort()
    bs.sort()
    m = len(rs) // 2
    return rs[m], gs[m], bs[m]


def main():
    tmp = Path(tempfile.mkdtemp(prefix="watercmp_", dir="/var/folders/k4/vwnz9wys34q__66zz8l82zqm0000gn/T/opencode"))
    ref = load(to_bmp(REFERENCE, tmp))
    ren = load(to_bmp(RENDER, tmp))
    if ref[0] != ren[0] or ref[1] != ren[1]:
        sys.exit(f"size mismatch: ref {ref[:2]} render {ren[:2]}")

    total_err = 0.0
    n = 0
    for y in range(0, ref[1], 3):
        for x in range(0, ref[0], 3):
            a = ref[2](x, y)
            b = ren[2](x, y)
            total_err += sum(abs(p - q) for p, q in zip(a, b)) / 3.0
            n += 1
    print(f"mean abs error (0-255 scale): {total_err / n * 255:.2f}")

    # Water-only masked error: open-sea side strips, away from ships/land/UI.
    wx = list(range(int(SIZE[0] * 0.02), int(SIZE[0] * 0.20), 3)) + \
         list(range(int(SIZE[0] * 0.80), int(SIZE[0] * 0.98), 3))
    wy = list(range(int(SIZE[1] * 0.25), int(SIZE[1] * 0.92), 3))
    werr = wn = 0.0
    for y in wy:
        for x in wx:
            a = ref[2](x, y)
            b = ren[2](x, y)
            werr += sum(abs(p - q) for p, q in zip(a, b)) / 3.0
            wn += 1
    water_mean = werr / wn * 255
    print(f"WATER-ONLY mean abs error: {water_mean:.2f}  (threshold <= 10)")
    print(f"WATER-ONLY verdict: {'PASS' if water_mean <= 10 else 'FAIL'}")

    print(f"\n{'region':16s} {'reference':>16s} {'render':>16s} {'maxdCh':>7s}  verdict(<=8)")
    for name, (fx0, fy0, fx1, fy1) in REGIONS.items():
        x0, y0 = int(fx0 * SIZE[0]), int(fy0 * SIZE[1])
        x1, y1 = int(fx1 * SIZE[0]), int(fy1 * SIZE[1])
        a = median_color(ref[2], x0, y0, x1, y1)
        b = median_color(ren[2], x0, y0, x1, y1)
        fa = "#{:02X}{:02X}{:02X}".format(*(int(c * 255) for c in a))
        fb = "#{:02X}{:02X}{:02X}".format(*(int(c * 255) for c in b))
        maxd = max(abs(p - q) for p, q in zip(a, b))
        verdict = "PASS" if maxd * 255 <= 8 else "FAIL"
        print(f"{name:16s} {fa:>16s} {fb:>16s} {maxd * 255:7.1f}  {verdict}")


if __name__ == "__main__":
    main()
