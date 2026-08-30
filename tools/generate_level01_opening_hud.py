#!/usr/bin/env python3
"""Generate the transparent Level 01 opening HUD atlas locally."""

from pathlib import Path
import subprocess
import tempfile

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Assets/_Project/Art/UI/Level01_Opening_HUD.png"
SIZE = (720, 1280)


def rounded_panel(draw, box, radius=12, fill=(5, 39, 54, 235), outline=(216, 169, 80, 255), width=2):
    draw.rounded_rectangle(box, radius=radius, fill=fill, outline=outline, width=width)


def arabic_layer(text: str, font_size: int, color: str) -> Image.Image:
    with tempfile.TemporaryDirectory() as temp:
        path = Path(temp) / "text.png"
        subprocess.run(
            [
                "pango-view", "--no-display", "--background=transparent", "--pixels",
                "--rtl", "--single-par", "--align=center", "--margin=3",
                f"--font=SF Arabic Bold {font_size}", f"--foreground={color}",
                f"--text={text}", f"--output={path}",
            ],
            check=True,
        )
        return Image.open(path).convert("RGBA")


def centered(base: Image.Image, layer: Image.Image, center_x: int, center_y: int):
    base.alpha_composite(layer, (center_x - layer.width // 2, center_y - layer.height // 2))


def main():
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    image = Image.new("RGBA", SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    # Population counter.
    rounded_panel(draw, (18, 16, 112, 66), radius=8, outline=(20, 91, 99, 255), width=2)
    draw.ellipse((35, 28, 47, 40), fill=(54, 237, 235, 255))
    draw.rounded_rectangle((31, 40, 51, 55), radius=7, fill=(54, 237, 235, 255))
    number_font = "/System/Library/Fonts/Supplemental/Arial Bold.ttf"
    from PIL import ImageFont
    draw.text((64, 24), "24", font=ImageFont.truetype(number_font, 27), fill=(247, 251, 249, 255))

    # Mission title plate.
    rounded_panel(draw, (235, 14, 486, 65), radius=12, fill=(6, 42, 58, 238), outline=(216, 169, 80, 255), width=2)
    draw.polygon([(235, 27), (225, 39), (235, 51)], fill=(6, 42, 58, 238), outline=(216, 169, 80, 255))
    draw.polygon([(486, 27), (496, 39), (486, 51)], fill=(6, 42, 58, 238), outline=(216, 169, 80, 255))
    centered(image, arabic_layer("مئة شراع", 30, "#6BE3E5"), 361, 39)

    # Pause control.
    rounded_panel(draw, (654, 15, 704, 66), radius=11, fill=(6, 42, 58, 238), outline=(216, 169, 80, 255), width=2)
    draw.rounded_rectangle((670, 27, 677, 54), radius=2, fill=(250, 249, 236, 255))
    draw.rounded_rectangle((684, 27, 691, 54), radius=2, fill=(250, 249, 236, 255))

    # Movement instruction and cyan drag arrow.
    instruction = arabic_layer("اسحب للتحرك", 29, "#F4FAF4")
    # Dark shadow keeps the copy readable against white wake foam.
    shadow = arabic_layer("اسحب للتحرك", 29, "#073443")
    centered(image, shadow, 362, 1190)
    centered(image, instruction, 360, 1187)
    draw.line((285, 1230, 435, 1230), fill=(69, 225, 231, 255), width=10)
    draw.polygon([(270, 1230), (292, 1215), (292, 1245)], fill=(69, 225, 231, 255))
    draw.polygon([(450, 1230), (428, 1215), (428, 1245)], fill=(196, 250, 250, 255))

    image.save(OUTPUT)
    print(OUTPUT)


if __name__ == "__main__":
    main()
