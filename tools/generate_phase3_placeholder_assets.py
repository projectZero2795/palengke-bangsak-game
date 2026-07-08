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


def clamp(value: float, low: float = 0.0, high: float = 1.0) -> float:
    return max(low, min(high, value))


def smoothstep(edge0: float, edge1: float, value: float) -> float:
    if edge0 == edge1:
        return 1.0 if value >= edge1 else 0.0

    t = clamp((value - edge0) / (edge1 - edge0))
    return t * t * (3.0 - 2.0 * t)


def lerp_color(
    a: tuple[int, int, int, int],
    b: tuple[int, int, int, int],
    t: float,
) -> tuple[int, int, int, int]:
    t = clamp(t)
    return tuple(round(a[i] + (b[i] - a[i]) * t) for i in range(4))


def over(
    dst: tuple[int, int, int, int],
    src: tuple[int, int, int, int],
) -> tuple[int, int, int, int]:
    sa = src[3] / 255
    da = dst[3] / 255
    out_a = sa + da * (1 - sa)

    if out_a <= 0:
        return (0, 0, 0, 0)

    out_rgb = []
    for i in range(3):
        out_rgb.append(round((src[i] * sa + dst[i] * da * (1 - sa)) / out_a))

    return (out_rgb[0], out_rgb[1], out_rgb[2], round(out_a * 255))


def ring_mask(distance: float, center_radius: float, width: float, feather: float) -> float:
    half = width * 0.5
    outer = 1.0 - smoothstep(center_radius + half - feather, center_radius + half, distance)
    inner = smoothstep(center_radius - half, center_radius - half + feather, distance)
    return clamp(outer * inner)


def circle_mask(distance: float, radius: float, feather: float) -> float:
    return 1.0 - smoothstep(radius - feather, radius, distance)


def joystick_base_sprite(size: int = 192) -> bytes:
    pixels = bytearray(size * size * 4)
    center = (size - 1) / 2
    radius = size * 0.47
    fill_radius = size * 0.39
    outer_ring_radius = size * 0.43
    inner_ring_radius = size * 0.31

    for y in range(size):
        for x in range(size):
            dx = x - center
            dy = y - center
            distance = math.hypot(dx, dy)
            angle = math.atan2(dy, dx)
            radial = clamp(distance / radius)
            color = (0, 0, 0, 0)

            shadow = circle_mask(math.hypot(dx - 3, dy + 4), radius, 7) * 0.18
            if shadow:
                color = over(color, (0, 0, 0, round(255 * shadow)))

            fill_alpha = circle_mask(distance, fill_radius, 6) * 0.42
            if fill_alpha:
                fill = lerp_color((18, 29, 41, 140), (50, 68, 73, 122), radial)
                color = over(color, (*fill[:3], round(fill[3] * fill_alpha)))

            inner_glass = ring_mask(distance, inner_ring_radius, size * 0.025, 5) * 0.65
            if inner_glass:
                color = over(color, (193, 216, 226, round(110 * inner_glass)))

            woven = ring_mask(distance, outer_ring_radius, size * 0.105, 7)
            if woven:
                weave_wave = 0.5 + 0.5 * math.sin(angle * 24 + radial * 38)
                cross_wave = 0.5 + 0.5 * math.sin((x - y) * 0.18)
                weave_t = clamp(weave_wave * 0.7 + cross_wave * 0.3)
                base = lerp_color((116, 80, 39, 235), (228, 178, 83, 242), weave_t)
                color = over(color, (*base[:3], round(base[3] * woven)))

            outer_highlight = ring_mask(distance, radius * 0.91, size * 0.02, 4) * 0.85
            if outer_highlight:
                color = over(color, (255, 226, 140, round(180 * outer_highlight)))

            inner_shadow = ring_mask(distance, radius * 0.72, size * 0.03, 4) * 0.35
            if inner_shadow:
                color = over(color, (34, 23, 13, round(180 * inner_shadow)))

            # Soft top-left specular shine keeps the control feeling like a clean HUD element.
            shine = circle_mask(math.hypot(dx + radius * 0.28, dy + radius * 0.32), radius * 0.22, 9)
            shine *= circle_mask(distance, fill_radius, 7) * 0.22
            if shine:
                color = over(color, (255, 247, 213, round(95 * shine)))

            i = (y * size + x) * 4
            pixels[i : i + 4] = bytes(color)

    return bytes(pixels)


def joystick_handle_sprite(size: int = 128) -> bytes:
    pixels = bytearray(size * size * 4)
    center = (size - 1) / 2
    radius = size * 0.45
    ring_radius = size * 0.38

    for y in range(size):
        for x in range(size):
            dx = x - center
            dy = y - center
            distance = math.hypot(dx, dy)
            angle = math.atan2(dy, dx)
            radial = clamp(distance / radius)
            color = (0, 0, 0, 0)

            shadow = circle_mask(math.hypot(dx - 2, dy + 4), radius, 5) * 0.2
            if shadow:
                color = over(color, (0, 0, 0, round(255 * shadow)))

            fill_alpha = circle_mask(distance, radius * 0.84, 5)
            if fill_alpha:
                fill = lerp_color((248, 240, 218, 238), (190, 132, 61, 226), radial)
                color = over(color, (*fill[:3], round(fill[3] * fill_alpha)))

            woven = ring_mask(distance, ring_radius, size * 0.105, 5)
            if woven:
                weave = 0.5 + 0.5 * math.sin(angle * 18 + (x + y) * 0.08)
                base = lerp_color((109, 70, 34, 248), (255, 213, 122, 248), weave)
                color = over(color, (*base[:3], round(base[3] * woven)))

            highlight = circle_mask(math.hypot(dx + radius * 0.24, dy + radius * 0.28), radius * 0.28, 7)
            highlight *= circle_mask(distance, radius * 0.8, 5) * 0.34
            if highlight:
                color = over(color, (255, 255, 240, round(130 * highlight)))

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
        192,
        192,
        joystick_base_sprite(192),
    )
    write_png(
        OUT_DIR / "joystick_handle_placeholder.png",
        128,
        128,
        joystick_handle_sprite(128),
    )
    write_png(WALL_DIR / "wall_placeholder.png", 64, 64, wall_sprite())


if __name__ == "__main__":
    main()
