"""Build local side-by-side evidence sheets for the three execution references."""

from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
REFERENCE_ROOT = ROOT / "ArtSource/References/Level01"
CAPTURE_ROOT = ROOT / "Artifacts/Local/Blockout"
SIZE = (720, 1280)
HEADER = 34

PAIRS = {
    "Opening": ("REF_Level01_Opening.png", "Level01_Opening.png"),
    "GateRescue": ("REF_Level01_Traversal_GateRescue.png", "Level01_GateRescue.png"),
    "BeachLanding": ("REF_Level01_BeachLanding.png", "Level01_BeachLanding.png"),
    "BossBattle": ("REF_Level01_BossBattle.png", "Level01_BossBattle.png"),
    "VictoryReward": ("REF_Level01_VictoryReward.png", "Level01_VictoryReward.png"),
}


for phase, (reference_name, capture_name) in PAIRS.items():
    reference = Image.open(REFERENCE_ROOT / reference_name).convert("RGB").resize(SIZE, Image.Resampling.LANCZOS)
    capture = Image.open(CAPTURE_ROOT / capture_name).convert("RGB")
    sheet = Image.new("RGB", (SIZE[0] * 2, SIZE[1] + HEADER), (8, 18, 25))
    sheet.paste(reference, (0, HEADER))
    sheet.paste(capture, (SIZE[0], HEADER))
    draw = ImageDraw.Draw(sheet)
    draw.text((12, 10), "EXECUTION REFERENCE", fill=(100, 232, 239))
    draw.text((SIZE[0] + 12, 10), "UNITY BLOCKOUT REVIEW", fill=(238, 183, 75))
    draw.line((SIZE[0], 0, SIZE[0], SIZE[1] + HEADER), fill=(238, 183, 75), width=2)
    sheet.save(CAPTURE_ROOT / f"Level01_Comparison_{phase}.png", optimize=True)
