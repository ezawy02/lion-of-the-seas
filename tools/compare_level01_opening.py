#!/usr/bin/env python3
"""Create a deterministic reference/render/difference board for Level 01 opening."""

from pathlib import Path
from PIL import Image, ImageChops, ImageDraw, ImageFont, ImageStat


ROOT = Path(__file__).resolve().parents[1]
REFERENCE = ROOT / "ArtSource/References/Level01/REF_Level01_Opening.png"
RENDER = ROOT / "Artifacts/Local/Blockout/Level01_Opening.png"
OUTPUT = ROOT / "Artifacts/Local/Blockout/Level01_Opening_Comparison.png"
SIZE = (720, 1280)


def main():
    reference = Image.open(REFERENCE).convert("RGB").resize(SIZE, Image.Resampling.LANCZOS)
    render = Image.open(RENDER).convert("RGB")
    if render.size != SIZE:
        raise SystemExit(f"Unexpected Unity capture size: {render.size}")
    difference = ImageChops.difference(reference, render)
    stat = ImageStat.Stat(difference)
    mean_error = sum(stat.mean) / 3.0
    exact_pixels = sum(1 for pixel in difference.getdata() if pixel == (0, 0, 0))
    exact_percent = exact_pixels * 100.0 / (SIZE[0] * SIZE[1])

    header = 58
    board = Image.new("RGB", (SIZE[0] * 3, SIZE[1] + header), (8, 14, 20))
    board.paste(reference, (0, header))
    board.paste(render, (SIZE[0], header))
    board.paste(difference, (SIZE[0] * 2, header))
    draw = ImageDraw.Draw(board)
    font = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial Bold.ttf", 24)
    small = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial.ttf", 18)
    draw.text((20, 10), "REFERENCE", font=font, fill=(90, 230, 235))
    draw.text((SIZE[0] + 20, 10), "UNITY RENDER", font=font, fill=(245, 210, 120))
    draw.text((SIZE[0] * 2 + 20, 10), "PIXEL DIFFERENCE", font=font, fill=(255, 115, 90))
    draw.text((SIZE[0] * 2 + 255, 16), f"mean error {mean_error:.2f} | exact pixels {exact_percent:.4f}%", font=small, fill=(235, 235, 235))
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    board.save(OUTPUT)
    print(f"mean_error={mean_error:.4f}")
    print(f"exact_pixel_percent={exact_percent:.6f}")
    print(OUTPUT)


if __name__ == "__main__":
    main()
