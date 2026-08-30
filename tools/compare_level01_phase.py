#!/usr/bin/env python3
"""Create a deterministic local comparison report for a Level 01 phase."""

import argparse
import json
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageFont, ImageStat


CAPTURE_SIZE = (720, 1280)


def resized_rgb(path: Path) -> Image.Image:
    return Image.open(path).convert("RGB").resize(CAPTURE_SIZE, Image.Resampling.LANCZOS)


def mean_rgb(image: Image.Image) -> list[float]:
    return [round(value, 3) for value in ImageStat.Stat(image).mean]


def luminance(rgb: list[float]) -> float:
    return round(0.2126 * rgb[0] + 0.7152 * rgb[1] + 0.0722 * rgb[2], 3)


def edge_density(image: Image.Image) -> float:
    edges = image.convert("L").filter(ImageFilter.FIND_EDGES)
    histogram = edges.histogram()
    strong = sum(histogram[48:])
    return round(strong * 100.0 / (CAPTURE_SIZE[0] * CAPTURE_SIZE[1]), 3)


def region_metrics(reference: Image.Image, render: Image.Image, box: tuple[int, int, int, int]) -> dict:
    ref_crop = reference.crop(box)
    render_crop = render.crop(box)
    diff = ImageChops.difference(ref_crop, render_crop)
    ref_rgb = mean_rgb(ref_crop)
    render_rgb = mean_rgb(render_crop)
    return {
        "mae": round(sum(ImageStat.Stat(diff).mean) / 3.0, 3),
        "reference_mean_rgb": ref_rgb,
        "render_mean_rgb": render_rgb,
        "reference_luminance": luminance(ref_rgb),
        "render_luminance": luminance(render_rgb),
        "reference_edge_density_percent": edge_density(ref_crop.resize(CAPTURE_SIZE)),
        "render_edge_density_percent": edge_density(render_crop.resize(CAPTURE_SIZE)),
    }


def make_board(reference: Image.Image, render: Image.Image, output: Path, phase: str, mae: float) -> None:
    difference = ImageChops.difference(reference, render)
    header = 58
    board = Image.new("RGB", (CAPTURE_SIZE[0] * 3, CAPTURE_SIZE[1] + header), (8, 14, 20))
    board.paste(reference, (0, header))
    board.paste(render, (CAPTURE_SIZE[0], header))
    board.paste(difference, (CAPTURE_SIZE[0] * 2, header))
    draw = ImageDraw.Draw(board)
    font = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial Bold.ttf", 24)
    small = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial.ttf", 18)
    draw.text((20, 10), "REFERENCE", font=font, fill=(90, 230, 235))
    draw.text((CAPTURE_SIZE[0] + 20, 10), "UNITY RENDER", font=font, fill=(245, 210, 120))
    draw.text((CAPTURE_SIZE[0] * 2 + 20, 10), "PIXEL DIFFERENCE", font=font, fill=(255, 115, 90))
    draw.text((CAPTURE_SIZE[0] * 2 + 260, 16), f"{phase} | MAE {mae:.2f}", font=small, fill=(235, 235, 235))
    output.parent.mkdir(parents=True, exist_ok=True)
    board.save(output)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--phase", required=True)
    parser.add_argument("--reference", type=Path, required=True)
    parser.add_argument("--render", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--metrics", type=Path, required=True)
    args = parser.parse_args()

    reference = resized_rgb(args.reference)
    render = resized_rgb(args.render)
    full_diff = ImageChops.difference(reference, render)
    full_mae = sum(ImageStat.Stat(full_diff).mean) / 3.0
    width, height = CAPTURE_SIZE
    regions = {
        "sky_and_horizon": (0, 0, width, int(height * 0.33)),
        "gameplay_focus": (0, int(height * 0.25), width, int(height * 0.72)),
        "foreground_water": (0, int(height * 0.62), width, height),
        "center_lane": (int(width * 0.22), int(height * 0.18), int(width * 0.78), height),
    }
    payload = {
        "phase": args.phase,
        "reference": str(args.reference),
        "render": str(args.render),
        "size": list(CAPTURE_SIZE),
        "full_frame_mae": round(full_mae, 3),
        "full_reference_edge_density_percent": edge_density(reference),
        "full_render_edge_density_percent": edge_density(render),
        "regions": {name: region_metrics(reference, render, box) for name, box in regions.items()},
    }
    args.metrics.parent.mkdir(parents=True, exist_ok=True)
    args.metrics.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")
    make_board(reference, render, args.output, args.phase, full_mae)
    print(json.dumps(payload, indent=2))


if __name__ == "__main__":
    main()
