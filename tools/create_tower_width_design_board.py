#!/usr/bin/env python3
"""Create a local-only tower-width decision board without changing Unity assets."""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[1]
REFERENCE = ROOT / "ArtSource/References/Level01/REF_Level01_BeachLanding.png"
TOWER = ROOT / (
    "Artifacts/Local/Approval/Level01FortressModules/"
    "Tower_TripoV31_R2_Optimized_REVIEW.png"
)
OUTPUT = ROOT / (
    "Artifacts/Local/Approval/Level01FortressModules/"
    "Tower_Width_Proposal_R5_PREVIEW_ONLY.png"
)

CANVAS = (1920, 1120)
BACKGROUND = (24, 31, 34, 255)
PANEL = (35, 44, 47, 255)
TEXT = (235, 226, 205, 255)
MUTED = (170, 178, 171, 255)
ACCENT = (222, 158, 74, 255)
GRID = (67, 79, 79, 255)


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    name = "Arial Bold.ttf" if bold else "Arial.ttf"
    path = Path("/System/Library/Fonts/Supplemental") / name
    return ImageFont.truetype(str(path), size)


def fit(image: Image.Image, box: tuple[int, int]) -> Image.Image:
    scale = min(box[0] / image.width, box[1] / image.height)
    size = (round(image.width * scale), round(image.height * scale))
    return image.resize(size, Image.Resampling.LANCZOS)


def alpha_crop(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    bounds = rgba.getchannel("A").getbbox()
    return rgba.crop(bounds) if bounds else rgba


def draw_panel(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int]) -> None:
    draw.rounded_rectangle(box, radius=22, fill=PANEL, outline=GRID, width=2)


def dimension_arrow(
    draw: ImageDraw.ImageDraw,
    left: int,
    right: int,
    y: int,
    label: str,
) -> None:
    draw.line((left, y, right, y), fill=ACCENT, width=4)
    draw.polygon([(left, y), (left + 16, y - 9), (left + 16, y + 9)], fill=ACCENT)
    draw.polygon([(right, y), (right - 16, y - 9), (right - 16, y + 9)], fill=ACCENT)
    label_box = draw.textbbox((0, 0), label, font=font(28, True))
    label_width = label_box[2] - label_box[0]
    draw.rounded_rectangle(
        ((left + right - label_width) // 2 - 12, y - 48,
         (left + right + label_width) // 2 + 12, y - 12),
        radius=8,
        fill=BACKGROUND,
    )
    draw.text(((left + right - label_width) // 2, y - 45), label, font=font(28, True), fill=TEXT)


def place_tower(
    canvas: Image.Image,
    draw: ImageDraw.ImageDraw,
    tower: Image.Image,
    panel_box: tuple[int, int, int, int],
    width_scale: float,
    width_label: str,
) -> None:
    left, top, right, bottom = panel_box
    base = fit(tower, (390, 700))
    widened = base.resize(
        (round(base.width * width_scale), base.height),
        Image.Resampling.LANCZOS,
    )
    x = (left + right - widened.width) // 2
    y = bottom - 160 - widened.height
    canvas.alpha_composite(widened, (x, y))
    draw.line((left + 45, bottom - 155, right - 45, bottom - 155), fill=GRID, width=3)
    dimension_arrow(draw, x, x + widened.width, bottom - 105, width_label)


def main() -> None:
    reference = Image.open(REFERENCE).convert("RGBA")
    tower = alpha_crop(Image.open(TOWER))
    canvas = Image.new("RGBA", CANVAS, BACKGROUND)
    draw = ImageDraw.Draw(canvas)

    draw.text((60, 38), "FORTRESS TOWER WIDTH STUDY", font=font(46, True), fill=TEXT)
    draw.text(
        (60, 94),
        "PREVIEW ONLY - NO UNITY OR ASSET CHANGE",
        font=font(25, True),
        fill=ACCENT,
    )

    reference_panel = (45, 155, 585, 1050)
    current_panel = (620, 155, 1240, 1050)
    proposed_panel = (1275, 155, 1895, 1050)
    for panel in (reference_panel, current_panel, proposed_panel):
        draw_panel(draw, panel)

    ref_fit = fit(reference, (480, 780))
    ref_x = (reference_panel[0] + reference_panel[2] - ref_fit.width) // 2
    ref_y = reference_panel[1] + 75
    canvas.alpha_composite(ref_fit, (ref_x, ref_y))
    draw.text((reference_panel[0] + 28, reference_panel[1] + 22), "01  APPROACH REFERENCE", font=font(28, True), fill=TEXT)

    draw.text((current_panel[0] + 28, current_panel[1] + 22), "02  CURRENT TOWER", font=font(28, True), fill=TEXT)
    place_tower(canvas, draw, tower, current_panel, 1.0, "8.56 m")
    draw.text((current_panel[0] + 48, current_panel[3] - 52), "Height: 14.55 m (locked)", font=font(24), fill=MUTED)

    draw.text((proposed_panel[0] + 28, proposed_panel[1] + 22), "03  PROPOSED +18%", font=font(28, True), fill=ACCENT)
    place_tower(canvas, draw, tower, proposed_panel, 1.18, "10.10 m")
    draw.text((proposed_panel[0] + 48, proposed_panel[3] - 82), "Height: 14.55 m (locked)", font=font(24), fill=MUTED)
    draw.text((proposed_panel[0] + 48, proposed_panel[3] - 49), "Footprint only; cannon and ground pivot stay fixed", font=font(21), fill=MUTED)

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    canvas.convert("RGB").save(OUTPUT, quality=95)
    print(OUTPUT)


if __name__ == "__main__":
    main()
