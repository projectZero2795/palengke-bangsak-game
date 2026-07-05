#!/usr/bin/env python3
"""Generate deterministic Phase 3 helper sprites.

These are intentionally utilitarian placeholders for movement/collision review:
wall blocks plus mobile joystick base/handle UI art.
"""

from __future__ import annotations

import math
import struct
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Controls"
WALL_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Environment"


def write_png(path: Path, width: int, height: int, pixels: bytes) -> None:
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
        raw.extend(pixels[y * stride : (y + 1) * stride])

    data = b"\x89PNG\r\n\x1a\n"
    data += chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
    data += chunk(b"IDAT", zlib.compress(bytes(raw), 9))
    data += chunk(b"IEND", b"")
    path.write_bytes(data)


def circle_sprite(size: int, fill: tuple[int, int, int, int], outline: tuple[int, int, int, int]) -> bytes:
    pixels = bytearray(size * size * 4)
    center = (size - 1) / 2
    outer = size * 0.46
    inner = size * 0.39
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - center, y - center)
            color = (0, 0, 0, 0)
            if d <= outer:
                color = outline if d > inner else fill
            i = (y * size + x) * 4
            pixels[i : i + 4] = bytes(color)
    return bytes(pixels)


def wall_sprite(size: int = 64) -> bytes:
    pixels = bytearray(size * size * 4)
    dark = (31, 42, 57, 255)
    mid = (61, 75, 96, 255)
    line = (105, 121, 145, 255)
    for y in range(size):
        for x in range(size):
            color = mid if ((x // 16) + (y // 16)) % 2 == 0 else dark
            if x % 16 in (0, 1) or y % 16 in (0, 1):
                color = line
            i = (y * size + x) * 4
            pixels[i : i + 4] = bytes(color)
    return bytes(pixels)


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    WALL_DIR.mkdir(parents=True, exist_ok=True)

    write_png(
        OUT_DIR / "joystick_base_placeholder.png",
        128,
        128,
        circle_sprite(128, (94, 132, 190, 76), (180, 205, 245, 96)),
    )
    write_png(
        OUT_DIR / "joystick_handle_placeholder.png",
        96,
        96,
        circle_sprite(96, (238, 244, 255, 215), (255, 255, 255, 255)),
    )
    write_png(WALL_DIR / "wall_placeholder.png", 64, 64, wall_sprite())


if __name__ == "__main__":
    main()
