#!/usr/bin/env python3
"""Generate local transparent HUD overlays for Level 01 boss and victory captures."""

from pathlib import Path
import subprocess
import tempfile
from PIL import Image, ImageDraw, ImageFont, ImageOps


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/_Project/Art/UI"
SIZE = (720, 1280)
NAVY = (5, 34, 49, 242)
GOLD = (208, 151, 54, 255)
CYAN = (78, 224, 229, 255)
WHITE = (248, 248, 237, 255)
FONT = "/System/Library/Fonts/Supplemental/Arial Bold.ttf"
SHIP = ROOT / "ArtSource/References/Level01/L01-SHP-004_Hero_Flagship_Reference.png"


def arabic(text, size, color):
    with tempfile.TemporaryDirectory() as temp:
        target = Path(temp) / "text.png"
        subprocess.run(["pango-view", "--no-display", "--background=transparent", "--pixels",
                        "--rtl", "--single-par", "--align=center", "--margin=3",
                        f"--font=SF Arabic Bold {size}", f"--foreground={color}",
                        f"--text={text}", f"--output={target}"], check=True)
        return Image.open(target).convert("RGBA")


def center(base, layer, x, y):
    base.alpha_composite(layer, (x - layer.width // 2, y - layer.height // 2))


def panel(draw, box, radius=10):
    draw.rounded_rectangle(box, radius=radius, fill=NAVY, outline=GOLD, width=2)


def common(image):
    draw = ImageDraw.Draw(image)
    panel(draw, (17, 15, 126, 67), 9)
    draw.ellipse((31, 27, 46, 42), fill=CYAN)
    draw.rounded_rectangle((27, 41, 50, 57), 7, fill=CYAN)
    draw.text((59, 25), "1,236", font=ImageFont.truetype(FONT, 25), fill=WHITE)
    panel(draw, (654, 15, 704, 66), 11)
    draw.rounded_rectangle((670, 27, 677, 54), 2, fill=WHITE)
    draw.rounded_rectangle((684, 27, 691, 54), 2, fill=WHITE)
    return draw


def boss():
    image = Image.new("RGBA", SIZE, (0, 0, 0, 0))
    draw = common(image)
    draw.rounded_rectangle((185, 24, 620, 68), 14, fill=NAVY, outline=GOLD, width=3)
    draw.rounded_rectangle((199, 48, 606, 62), 6, fill=(35, 20, 17, 245), outline=GOLD, width=1)
    draw.rounded_rectangle((201, 50, 520, 60), 5, fill=(191, 42, 37, 255))
    center(image, arabic("حارس الميناء", 20, "#F5E5B5"), 402, 36)
    image.save(OUT / "Level01_BossBattle_HUD.png")


def victory():
    image = Image.new("RGBA", SIZE, (0, 0, 0, 0))
    draw = common(image)
    for x, scale in ((304, 18), (360, 25), (416, 18)):
        points = []
        for index in range(10):
            import math
            angle = -math.pi / 2 + index * math.pi / 5
            radius = scale if index % 2 == 0 else scale * 0.45
            points.append((x + math.cos(angle) * radius, 85 + math.sin(angle) * radius))
        draw.polygon(points, fill=(242, 177, 35, 255), outline=(116, 67, 8, 255))
    draw.rounded_rectangle((145, 112, 575, 190), 22, fill=NAVY, outline=GOLD, width=4)
    center(image, arabic("نصر مئة شراع", 38, "#E8B954"), 360, 151)
    glow = Image.new("RGBA", SIZE, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow)
    for spread, alpha in ((28, 28), (18, 46), (9, 82)):
        glow_draw.rounded_rectangle((265-spread, 695-spread, 455+spread, 920+spread),
                                    12+spread, fill=(255, 190, 55, alpha))
    image = Image.alpha_composite(image, glow)
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle((265, 695, 455, 920), 10, fill=(16, 78, 119, 238), outline=GOLD, width=5)
    draw.line((282, 725, 438, 725), fill=(115, 196, 221, 150), width=1)
    draw.line((282, 890, 438, 890), fill=(115, 196, 221, 150), width=1)
    ship = Image.open(SHIP).convert("RGBA")
    alpha = ship.getchannel("A")
    blueprint = ImageOps.colorize(ImageOps.grayscale(ship), (40, 130, 170), (235, 251, 245))
    blueprint.putalpha(alpha.point(lambda value: int(value * 0.93)))
    blueprint.thumbnail((168, 206), Image.Resampling.LANCZOS)
    center(image, blueprint, 360, 807)
    panel(draw, (245, 940, 475, 992), 10)
    center(image, arabic("مخطط السفينة", 25, "#E8B954"), 360, 966)
    image.save(OUT / "Level01_VictoryReward_HUD.png")


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    boss()
    victory()
