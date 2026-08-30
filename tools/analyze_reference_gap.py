#!/usr/bin/env python3
"""Quantitative reference-vs-render gap analysis (stdlib only, via sips BMP)."""
import struct
import subprocess
import tempfile
from pathlib import Path

ROOT = Path("/Users/apple/Desktop/أسد البحار Lion of the Seas")
REFERENCE = ROOT / "ArtSource/References/Level01/REF_Level01_Opening.png"
RENDER = ROOT / "Artifacts/Local/Blockout/Level01_Opening.png"
SIZE = (720, 1280)


def to_bmp(png: Path, tmp: Path, name: str) -> Path:
    out = tmp / name
    subprocess.run(
        ["sips", "-s", "format", "bmp", "-z", str(SIZE[1]), str(SIZE[0]), str(png), "--out", str(out)],
        check=True, capture_output=True,
    )
    return out


def load(bmp: Path):
    data = bmp.read_bytes()
    offset = struct.unpack_from("<I", data, 10)[0]
    w, h = struct.unpack_from("<ii", data, 18)
    top_down = h < 0
    h = abs(h)
    row = (w * 3 + 3) & ~3

    def pixel(x, y):
        yy = y if top_down else h - 1 - y
        base = offset + yy * row + x * 3
        return data[base + 2], data[base + 1], data[base]  # r,g,b 0-255

    return w, h, pixel


def lum(p):
    return 0.2126 * p[0] + 0.7152 * p[1] + 0.0722 * p[2]


def sat(p):
    mx, mn = max(p), min(p)
    return 0 if mx == 0 else (mx - mn) / mx


tmp = Path(tempfile.mkdtemp(prefix="gap_", dir="/var/folders/k4/vwnz9wys34q__66zz8l82zqm0000gn/T/opencode"))
rw, rh, ref = load(to_bmp(REFERENCE, tmp, "ref.bmp"))
nw, nh, ren = load(to_bmp(RENDER, tmp, "ren.bmp"))
assert (rw, rh) == (nw, nh)

# --- 1) Water gradient: vertical bands in clear side strips (x 2-20% and 80-98%), y 25-92%
print("== water vertical gradient (side strips, median RGB) ==")
print(f"{'band':8s} {'ref':>14s} {'render':>14s}")
for i in range(6):
    y0 = int(rh * (0.25 + i * 0.11))
    y1 = int(rh * (0.25 + (i + 1) * 0.11))
    acc = [[], []]
    for img_i, px in enumerate((ref, ren)):
        for y in range(y0, y1, 4):
            for x in list(range(int(rw * 0.02), int(rw * 0.20), 4)) + list(range(int(rw * 0.80), int(rw * 0.98), 4)):
                acc[img_i].append(px(x, y))
    vals = []
    for pts in acc:
        ch = []
        for c in range(3):
            s = sorted(p[c] for p in pts)
            ch.append(s[len(s) // 2])
        vals.append("#{:02X}{:02X}{:02X}".format(*ch))
    print(f"y{0.25 + i * 0.11:.2f}   {vals[0]:>14s} {vals[1]:>14s}")

# --- 2) Foam coverage: fraction of pixels with luminance > 195, lower half, excluding ships via saturation<0.25 & high lum
print("\n== foam coverage (lum>195, y 55-95%) ==")
for name, px in (("ref", ref), ("render", ren)):
    total = foam = 0
    for y in range(int(rh * 0.55), int(rh * 0.95), 2):
        for x in range(0, rw, 2):
            p = px(x, y)
            total += 1
            if lum(p) > 195 and sat(p) < 0.25:
                foam += 1
    print(f"{name:6s} {foam * 100.0 / total:5.2f}%")

# The opening flagship wake projects to the lower frame, not the cannon-splash region.
# Sample both wake arms while avoiding most of the ship's dark central stern.
print("\n== flagship wake arm coverage (x 30-48% + 62-88%, y 84-92%) ==")
for name, px in (("ref", ref), ("render", ren)):
    samples = []
    x_ranges = ((0.30, 0.48), (0.62, 0.88))
    for y in range(int(rh * 0.84), int(rh * 0.92), 2):
        for start, end in x_ranges:
            for x in range(int(rw * start), int(rw * end), 2):
                samples.append(px(x, y))
    values = []
    for threshold in (110, 140, 170):
        hits = sum(1 for p in samples if lum(p) > threshold and sat(p) < 0.35)
        values.append(f">{threshold}:{hits * 100.0 / len(samples):4.1f}%")
    luminance = sorted(lum(p) for p in samples)
    p95 = luminance[int(len(luminance) * 0.95)]
    p99 = luminance[int(len(luminance) * 0.99)]
    print(f"{name:6s} {'  '.join(values)}  p95:{p95:5.1f}  p99:{p99:5.1f}")

# --- 3) Sky: mean + stddev of luminance (cloud texture) in top band y 6-11%
print("\n== sky cloud texture (y 6-11%) ==")
for name, px in (("ref", ref), ("render", ren)):
    vals = []
    row_stddev = []
    horizontal_delta = []
    for y in range(int(rh * 0.06), int(rh * 0.11), 2):
        row = []
        for x in range(int(rw * 0.1), int(rw * 0.9), 2):
            value = lum(px(x, y))
            vals.append(value)
            row.append(value)
        row_mean = sum(row) / len(row)
        row_stddev.append((sum((v - row_mean) ** 2 for v in row) / len(row)) ** 0.5)
        horizontal_delta.extend(abs(row[i] - row[i - 2]) for i in range(2, len(row)))
    m = sum(vals) / len(vals)
    var = sum((v - m) ** 2 for v in vals) / len(vals)
    print(
        f"{name:6s} mean_lum {m:6.1f}  stddev {var ** 0.5:5.1f}"
        f"  row_std {sum(row_stddev) / len(row_stddev):5.1f}"
        f"  x_delta {sum(horizontal_delta) / len(horizontal_delta):4.1f}"
    )

# --- 4) Sun-sparkle band: high-lum pixels in water y 42-55% (reference has glitter path)
print("\n== sparkle coverage (lum>170, y 42-55%, x 55-95%) ==")
for name, px in (("ref", ref), ("render", ren)):
    total = hits = 0
    for y in range(int(rh * 0.42), int(rh * 0.55), 2):
        for x in range(int(rw * 0.55), int(rw * 0.95), 2):
            p = px(x, y)
            total += 1
            if lum(p) > 170:
                hits += 1
    print(f"{name:6s} {hits * 100.0 / total:5.2f}%")

# --- 5) Water hue saturation in mid water
print("\n== mid-water saturation (y 30-50%, side strips) ==")
for name, px in (("ref", ref), ("render", ren)):
    vals = []
    for y in range(int(rh * 0.30), int(rh * 0.50), 2):
        for x in list(range(int(rw * 0.03), int(rw * 0.22), 3)) + list(range(int(rw * 0.78), int(rw * 0.97), 3)):
            vals.append(sat(px(x, y)))
    print(f"{name:6s} mean_sat {sum(vals) / len(vals):.3f}")
