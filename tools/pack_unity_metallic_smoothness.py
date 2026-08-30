#!/usr/bin/env python3
"""Convert glTF metallic/roughness textures to Unity metallic/smoothness masks.

glTF stores roughness in green and metallic in blue. URP Lit expects metallic in red
and smoothness in alpha, so directly assigning the glTF texture produces flat materials.
"""

from pathlib import Path
import sys

from PIL import Image


def pack(source: Path) -> Path:
    image = Image.open(source).convert("RGBA")
    _red, roughness, metallic, _alpha = image.split()
    smoothness = roughness.point(lambda value: 255 - value)
    zero = Image.new("L", image.size, 0)
    output = Image.merge("RGBA", (metallic, zero, zero, smoothness))
    destination = source.with_name(source.name.replace("_MetallicRoughness", "_MetallicSmoothness"))
    output.save(destination, optimize=True)
    return destination


def main() -> None:
    root = Path(sys.argv[1] if len(sys.argv) > 1 else "Assets/_Project/Art/Textures/Level01")
    sources = sorted(root.glob("*_MetallicRoughness.png"))
    if not sources:
        raise SystemExit(f"No metallic/roughness maps found in {root}")
    for source in sources:
        print(pack(source))


if __name__ == "__main__":
    main()
