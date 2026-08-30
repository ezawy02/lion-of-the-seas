#!/usr/bin/env python3
"""Generate deterministic local VFX textures for the Level 01 opening shot."""

from pathlib import Path
import random

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Assets/_Project/Art/VFX/L01_CannonSplash.png"
WAKE_OUTPUT = ROOT / "Assets/_Project/Art/VFX/L01_FlagshipWake.png"


def main():
    random.seed(101)
    size = 512
    alpha = Image.new("L", (size, size), 0)
    draw = ImageDraw.Draw(alpha)

    # Layered tapered spray body.
    draw.polygon([(210, 470), (238, 185), (256, 55), (276, 190), (304, 470)], fill=175)
    draw.polygon([(170, 470), (225, 260), (242, 140), (245, 355), (268, 470)], fill=125)
    draw.polygon([(268, 470), (285, 300), (324, 210), (302, 390), (344, 470)], fill=105)
    for _ in range(95):
        y = random.randint(55, 445)
        spread = int((470 - y) * 0.42 + 18)
        x = 256 + random.randint(-spread, spread)
        radius = random.randint(2, 8)
        strength = random.randint(110, 235)
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=strength)
    for _ in range(35):
        x = 256 + random.randint(-135, 135)
        y = random.randint(50, 280)
        radius = random.randint(2, 5)
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=random.randint(155, 255))

    soft = alpha.filter(ImageFilter.GaussianBlur(7))
    core = alpha.filter(ImageFilter.GaussianBlur(1.4))
    combined = Image.new("L", (size, size), 0)
    combined = Image.blend(soft, core, 0.68)
    image = Image.new("RGBA", (size, size), (210, 250, 255, 0))
    image.putalpha(combined)
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    image.save(OUTPUT)
    print(OUTPUT)

    wake_size = 1024
    wake_alpha = Image.new("L", (wake_size, wake_size), 0)
    wake_draw = ImageDraw.Draw(wake_alpha)
    for side in (-1, 1):
        points = []
        for step in range(32):
            t = step / 31.0
            x = 512 + side * (105 + 260 * (t ** 0.72)) + random.randint(-13, 13)
            y = 80 + t * 870
            points.append((int(x), int(y)))
        wake_draw.line(points, fill=205, width=46, joint="curve")
        wake_draw.line(points, fill=105, width=94, joint="curve")
        wake_draw.line(points, fill=225, width=24, joint="curve")
    for _ in range(420):
        y = random.randint(75, 980)
        t = (y - 75) / 905.0
        side = random.choice((-1, 1))
        center = 512 + side * (105 + 260 * (t ** 0.72))
        x = int(center + random.gauss(0, 54 + 42 * t))
        radius = random.randint(2, 12)
        wake_draw.ellipse((x-radius, y-radius, x+radius, y+radius), fill=random.randint(75, 220))
    # Turbulence immediately behind the stern.
    for _ in range(180):
        x = int(random.gauss(512, 105))
        y = random.randint(80, 470)
        radius = random.randint(3, 15)
        wake_draw.ellipse((x-radius, y-radius, x+radius, y+radius), fill=random.randint(60, 180))
    wake_alpha = Image.blend(wake_alpha.filter(ImageFilter.GaussianBlur(5)), wake_alpha, 0.64)
    wake = Image.new("RGBA", (wake_size, wake_size), (225, 252, 250, 0))
    wake.putalpha(wake_alpha)
    wake.save(WAKE_OUTPUT)
    print(WAKE_OUTPUT)


if __name__ == "__main__":
    main()
