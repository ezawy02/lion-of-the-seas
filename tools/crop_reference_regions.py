#!/usr/bin/env python3
"""Crop regions of interest from ref/render BMPs into PNGs for visual verification."""
import struct
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path("/Users/apple/Desktop/أسد البحار Lion of the Seas")
OUT = Path("/var/folders/k4/vwnz9wys34q__66zz8l82zqm0000gn/T/opencode/crops")
OUT.mkdir(exist_ok=True)
SIZE = (720, 1280)

SOURCES = {
    "ref": ROOT / "ArtSource/References/Level01/REF_Level01_Opening.png",
    "ren": ROOT / "Artifacts/Local/Blockout/Level01_Opening.png",
}

# name -> (source, fx, fy, fw, fh) fractions of the 720x1280 frame
CROPS = {
    "enemy_left_ship": ("ren", 0.24, 0.35, 0.30, 0.18),
    "enemy_ships_ref": ("ref", 0.30, 0.20, 0.55, 0.18),
    "wall_right_red": ("ren", 0.58, 0.20, 0.30, 0.14),
    "hero_stern_render": ("ren", 0.22, 0.52, 0.56, 0.44),
    "hero_stern_ref": ("ref", 0.10, 0.30, 0.60, 0.60),
    "left_cliff_render": ("ren", 0.0, 0.14, 0.28, 0.30),
    "left_cliff_ref": ("ref", 0.0, 0.08, 0.28, 0.40),
    "right_cliff_render": ("ren", 0.72, 0.16, 0.28, 0.28),
    "right_cliff_ref": ("ref", 0.72, 0.08, 0.28, 0.34),
}


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
        return data[base + 2], data[base + 1], data[base]

    return w, h, pixel


def main():
    tmp = Path(tempfile.mkdtemp(prefix="crop_", dir="/var/folders/k4/vwnz9wys34q__66zz8l82zqm0000gn/T/opencode"))
    imgs = {}
    for key, src in SOURCES.items():
        bmp = tmp / f"{key}.bmp"
        subprocess.run(["sips", "-s", "format", "bmp", "-z", str(SIZE[1]), str(SIZE[0]), str(src), "--out", str(bmp)],
                       check=True, capture_output=True)
        imgs[key] = load(bmp)

    for name, (src, fx, fy, fw, fh) in CROPS.items():
        w, h, px = imgs[src]
        x0, y0 = int(fx * w), int(fy * h)
        cw, ch = int(fw * w), int(fh * h)
        out_bmp = OUT / f"{name}.bmp"
        row_size = (cw * 3 + 3) & ~3
        header = bytearray(54)
        struct.pack_into("<H", header, 0, 0x4D42)
        struct.pack_into("<I", header, 2, 54 + row_size * ch)
        struct.pack_into("<I", header, 10, 54)
        struct.pack_into("<I", header, 14, 40)
        struct.pack_into("<ii", header, 18, cw, ch)
        struct.pack_into("<H", header, 26, 1)
        struct.pack_into("<H", header, 28, 24)
        with open(out_bmp, "wb") as fh_out:
            fh_out.write(header)
            for y in range(y0 + ch - 1, y0 - 1, -1):
                row = bytearray(row_size)
                for x in range(cw):
                    r, g, b = px(x0 + x, y)
                    row[x * 3] = b
                    row[x * 3 + 1] = g
                    row[x * 3 + 2] = r
                fh_out.write(row)
        png = OUT / f"{name}.png"
        subprocess.run(["sips", "-s", "format", "png", str(out_bmp), "--out", str(png)],
                       check=True, capture_output=True)
        print(png)


if __name__ == "__main__":
    sys.exit(main())
