#!/usr/bin/env python3
"""Generate deterministic Phase 2 placeholder player sprites.

The sprites are intentionally simple: cute, readable, Filipino-inspired chibi
top-down placeholders with no gameplay behavior. They are good enough for
Phase 2 player-design review and can be replaced by polished art later.
"""

from __future__ import annotations

import math
import struct
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Players"
SCALE = 4
SIZE = 64


VARIANTS = {
    "red": (214, 57, 46, 255),
    "green": (36, 164, 83, 255),
    "blue": (58, 117, 232, 255),
    "yellow": (242, 196, 67, 255),
}


POSES = {
    "idle": 0.0,
    "walk_01": -2.2,
    "walk_02": -0.8,
    "walk_03": 2.2,
    "walk_04": 0.8,
}


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    hex_color = hex_color.lstrip("#")
    return (
        int(hex_color[0:2], 16),
        int(hex_color[2:4], 16),
        int(hex_color[4:6], 16),
        alpha,
    )


SKIN = rgba("#d99568")
SKIN_DARK = rgba("#9b5839")
HAIR = rgba("#1d1714")
HAIR_HI = rgba("#3b2a22")
OUTLINE = rgba("#1b2430")
PANTS = rgba("#24364f")
SHOES = rgba("#161b24")
WHITE = rgba("#fff3dd")
SHADOW = rgba("#000000", 54)


class Canvas:
    def __init__(self) -> None:
        self.w = SIZE * SCALE
        self.h = SIZE * SCALE
        self.pixels = bytearray(self.w * self.h * 4)

    def blend(self, x: int, y: int, color: tuple[int, int, int, int]) -> None:
        if x < 0 or y < 0 or x >= self.w or y >= self.h:
            return
        r, g, b, a = color
        if a <= 0:
            return
        idx = (y * self.w + x) * 4
        dr, dg, db, da = self.pixels[idx : idx + 4]
        src = a / 255.0
        dst = da / 255.0
        out_a = src + dst * (1 - src)
        if out_a <= 0:
            return
        out_r = int((r * src + dr * dst * (1 - src)) / out_a)
        out_g = int((g * src + dg * dst * (1 - src)) / out_a)
        out_b = int((b * src + db * dst * (1 - src)) / out_a)
        self.pixels[idx : idx + 4] = bytes((out_r, out_g, out_b, int(out_a * 255)))

    def ellipse(
        self,
        cx: float,
        cy: float,
        rx: float,
        ry: float,
        color: tuple[int, int, int, int],
    ) -> None:
        cx *= SCALE
        cy *= SCALE
        rx *= SCALE
        ry *= SCALE
        min_x = math.floor(cx - rx)
        max_x = math.ceil(cx + rx)
        min_y = math.floor(cy - ry)
        max_y = math.ceil(cy + ry)
        for y in range(min_y, max_y + 1):
            for x in range(min_x, max_x + 1):
                dx = (x + 0.5 - cx) / rx
                dy = (y + 0.5 - cy) / ry
                if dx * dx + dy * dy <= 1:
                    self.blend(x, y, color)

    def rect(
        self,
        x0: float,
        y0: float,
        x1: float,
        y1: float,
        color: tuple[int, int, int, int],
    ) -> None:
        for y in range(math.floor(y0 * SCALE), math.ceil(y1 * SCALE)):
            for x in range(math.floor(x0 * SCALE), math.ceil(x1 * SCALE)):
                self.blend(x, y, color)

    def rounded_rect(
        self,
        x0: float,
        y0: float,
        x1: float,
        y1: float,
        radius: float,
        color: tuple[int, int, int, int],
    ) -> None:
        self.rect(x0 + radius, y0, x1 - radius, y1, color)
        self.rect(x0, y0 + radius, x1, y1 - radius, color)
        self.ellipse(x0 + radius, y0 + radius, radius, radius, color)
        self.ellipse(x1 - radius, y0 + radius, radius, radius, color)
        self.ellipse(x0 + radius, y1 - radius, radius, radius, color)
        self.ellipse(x1 - radius, y1 - radius, radius, radius, color)

    def downsample(self) -> bytes:
        out = bytearray(SIZE * SIZE * 4)
        for y in range(SIZE):
            for x in range(SIZE):
                totals = [0, 0, 0, 0]
                for sy in range(SCALE):
                    for sx in range(SCALE):
                        idx = ((y * SCALE + sy) * self.w + (x * SCALE + sx)) * 4
                        for i in range(4):
                            totals[i] += self.pixels[idx + i]
                dst = (y * SIZE + x) * 4
                count = SCALE * SCALE
                out[dst : dst + 4] = bytes(v // count for v in totals)
        return bytes(out)


def write_png(path: Path, width: int, height: int, data: bytes) -> None:
    def chunk(kind: bytes, payload: bytes) -> bytes:
        return (
            struct.pack(">I", len(payload))
            + kind
            + payload
            + struct.pack(">I", zlib.crc32(kind + payload) & 0xFFFFFFFF)
        )

    raw = bytearray()
    stride = width * 4
    for y in range(height):
        raw.append(0)
        raw.extend(data[y * stride : (y + 1) * stride])
    png = b"\x89PNG\r\n\x1a\n"
    png += chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
    png += chunk(b"IDAT", zlib.compress(bytes(raw), 9))
    png += chunk(b"IEND", b"")
    path.write_bytes(png)


def draw_player(shirt: tuple[int, int, int, int], step: float) -> bytes:
    c = Canvas()
    bob = -0.7 if abs(step) > 1.5 else 0.0

    # soft grounding shadow
    c.ellipse(32, 56, 17, 5, SHADOW)

    # legs and shoes
    c.rounded_rect(24 - step, 42 + bob, 30 - step, 53 + bob, 2, OUTLINE)
    c.rounded_rect(34 + step, 42 + bob, 40 + step, 53 + bob, 2, OUTLINE)
    c.rounded_rect(25 - step, 42 + bob, 29 - step, 52 + bob, 1.7, PANTS)
    c.rounded_rect(35 + step, 42 + bob, 39 + step, 52 + bob, 1.7, PANTS)
    c.ellipse(26 - step, 54 + bob, 4, 2.5, SHOES)
    c.ellipse(38 + step, 54 + bob, 4, 2.5, SHOES)

    # arms, drawn behind torso
    c.ellipse(20 + step, 37 + bob, 4, 9, OUTLINE)
    c.ellipse(44 - step, 37 + bob, 4, 9, OUTLINE)
    c.ellipse(20 + step, 37 + bob, 2.8, 7.5, SKIN)
    c.ellipse(44 - step, 37 + bob, 2.8, 7.5, SKIN)

    # shirt/body
    c.rounded_rect(22, 31 + bob, 42, 49 + bob, 5, OUTLINE)
    c.rounded_rect(24, 32 + bob, 40, 48 + bob, 4, shirt)
    c.ellipse(32, 32 + bob, 8, 4, rgba("#ffffff", 38))

    # neck, ears, head
    c.rounded_rect(28, 27 + bob, 36, 34 + bob, 3, SKIN_DARK)
    c.ellipse(19.5, 22 + bob, 3.2, 4.5, OUTLINE)
    c.ellipse(44.5, 22 + bob, 3.2, 4.5, OUTLINE)
    c.ellipse(19.5, 22 + bob, 2.1, 3.2, SKIN)
    c.ellipse(44.5, 22 + bob, 2.1, 3.2, SKIN)
    c.ellipse(32, 22 + bob, 14, 15, OUTLINE)
    c.ellipse(32, 23 + bob, 12, 13, SKIN)

    # hair cap and highlights
    c.ellipse(32, 14 + bob, 12, 7, HAIR)
    c.ellipse(23.5, 19 + bob, 5, 8, HAIR)
    c.ellipse(40.5, 19 + bob, 5, 8, HAIR)
    c.ellipse(28, 12 + bob, 4, 2, HAIR_HI)

    # face details
    c.ellipse(27.5, 24 + bob, 1.4, 1.7, OUTLINE)
    c.ellipse(36.5, 24 + bob, 1.4, 1.7, OUTLINE)
    c.ellipse(32, 28.5 + bob, 1.3, 0.7, SKIN_DARK)
    c.ellipse(32, 31 + bob, 3, 1.1, WHITE)
    c.rect(29, 30.4 + bob, 35, 31.2 + bob, rgba("#7b3f2b"))

    return c.downsample()


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    for name, shirt in VARIANTS.items():
        for pose, step in POSES.items():
            path = OUT_DIR / f"player_{pose}_{name}.png"
            write_png(path, SIZE, SIZE, draw_player(shirt, step))
            print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
