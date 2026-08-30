"""Extract the approved gameplay HUD regions from the three Level 01 references."""

from pathlib import Path

from PIL import Image, ImageFilter


ROOT = Path(__file__).resolve().parents[2]
REFERENCE_ROOT = ROOT / "ArtSource/References/Level01"
OUTPUT_ROOT = ROOT / "Assets/_Project/Art/UI"
OUTPUT_SIZE = (720, 1280)


def keep_ui_pixel(red: int, green: int, blue: int) -> bool:
    dark_fill = max(red, green, blue) < 145
    gold = red > 125 and green > 75 and blue < 105
    return dark_fill or gold


def extract(reference_name: str, output_name: str, regions: list[tuple[int, int, int, int]]) -> None:
    source = Image.open(REFERENCE_ROOT / reference_name).convert("RGB")
    source = source.resize(OUTPUT_SIZE, Image.Resampling.LANCZOS)
    mask = Image.new("L", OUTPUT_SIZE, 0)
    pixels = source.load()
    alpha = mask.load()
    for left, top, right, bottom in regions:
        for y in range(top, min(bottom, OUTPUT_SIZE[1])):
            for x in range(left, min(right, OUTPUT_SIZE[0])):
                if keep_ui_pixel(*pixels[x, y]):
                    alpha[x, y] = 255

    # Preserve antialiased text and outlines immediately around the solid UI.
    mask = mask.filter(ImageFilter.MaxFilter(5)).filter(ImageFilter.GaussianBlur(0.45))
    result = source.convert("RGBA")
    result.putalpha(mask)
    OUTPUT_ROOT.mkdir(parents=True, exist_ok=True)
    result.save(OUTPUT_ROOT / output_name)


TOP_WIDGETS = [(10, 5, 120, 75), (220, 4, 500, 82), (645, 5, 712, 78)]
extract("REF_Level01_Traversal_GateRescue.png", "Level01_GateRescue_HUD.png", TOP_WIDGETS)
extract(
    "REF_Level01_BeachLanding.png",
    "Level01_BeachLanding_HUD.png",
    TOP_WIDGETS + [(205, 72, 505, 130)],
)
