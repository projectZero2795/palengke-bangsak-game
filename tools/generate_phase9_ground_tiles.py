#!/usr/bin/env python3
"""Generate deterministic Phase 9 ground tile placeholder sprites.

The art direction is the approved "bottom-center" option from the review:
a nighttime Filipino street-market plaza floor with muted colorful painted
borders, small triangle markings, warm lamp pools, and open playable space.
It stays tilemap-friendly and keeps Phase 9 focused on ground only.
"""

from __future__ import annotations

import math
import random
import struct
import uuid
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Ground"
SIZE = 128


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    hex_color = hex_color.lstrip("#")
    return (
        int(hex_color[0:2], 16),
        int(hex_color[2:4], 16),
        int(hex_color[4:6], 16),
        alpha,
    )


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


def asset_guid(relative_path: Path) -> str:
    return uuid.uuid5(uuid.NAMESPACE_URL, f"palengke-bangsak:{relative_path.as_posix()}").hex


def sprite_id(guid: str) -> str:
    return guid[:16] + "0900000000000000"


def write_folder_meta(path: Path) -> None:
    relative = path.relative_to(ROOT)
    path.with_suffix(".meta").write_text(
        f"""fileFormatVersion: 2
guid: {asset_guid(relative)}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
""",
        encoding="utf-8",
    )


def write_meta(path: Path) -> None:
    relative = path.relative_to(ROOT)
    guid = asset_guid(relative)
    sid = sprite_id(guid)
    path.with_suffix(path.suffix + ".meta").write_text(
        f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 1
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {SIZE}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: {sid}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
""",
        encoding="utf-8",
    )


def lerp(a: int, b: int, t: float) -> int:
    return int(a + (b - a) * t)


def mix(a: tuple[int, int, int, int], b: tuple[int, int, int, int], t: float) -> tuple[int, int, int, int]:
    return (
        lerp(a[0], b[0], t),
        lerp(a[1], b[1], t),
        lerp(a[2], b[2], t),
        lerp(a[3], b[3], t),
    )


def clamp(value: float, low: int = 0, high: int = 255) -> int:
    return max(low, min(high, int(value)))


def speckle(
    color: tuple[int, int, int, int],
    target: tuple[int, int, int, int],
    rng: random.Random,
    chance: float,
    strength: float,
) -> tuple[int, int, int, int]:
    if rng.random() >= chance:
        return color

    return mix(color, target, rng.uniform(strength * 0.35, strength))


def distance_to_line(x: int, y: int, slope: float, offset: float) -> float:
    return abs(y - (slope * x + offset)) / math.sqrt((slope * slope) + 1)


def apply_market_paint(
    color: tuple[int, int, int, int],
    x: int,
    y: int,
    seed: int,
    strength: float,
) -> tuple[int, int, int, int]:
    """Add subtle painted Filipino market-floor accents without overpowering play readability."""

    stripes = (
        (0.18, 18 + seed % 13, rgba("#a94a3e")),
        (-0.12, 46 + seed % 19, rgba("#2d6e82")),
        (0.10, 78 + seed % 17, rgba("#b88432")),
    )

    for slope, offset, paint in stripes:
        if distance_to_line(x, y, slope, offset) < 1.15:
            color = mix(color, paint, strength)

    return color


def inside_triangle(x: int, y: int, cx: int, cy: int, radius: int) -> bool:
    local_x = abs(x - cx)
    local_y = y - cy
    return 0 <= local_y <= radius and local_x <= (radius - local_y) * 0.75


def apply_triangle_markers(
    color: tuple[int, int, int, int],
    x: int,
    y: int,
    seed: int,
    alpha: float,
) -> tuple[int, int, int, int]:
    markers = (
        (25 + seed % 11, 34, rgba("#a84438")),
        (70, 23 + seed % 17, rgba("#266e83")),
        (96, 77, rgba("#bd8a31")),
        (42, 96, rgba("#485f9e")),
    )

    for cx, cy, marker in markers:
        if inside_triangle(x, y, cx, cy, 7):
            color = mix(color, marker, alpha)

    return color


def apply_market_border(
    color: tuple[int, int, int, int],
    x: int,
    y: int,
    inset: int,
    strength: float,
) -> tuple[int, int, int, int]:
    border_colors = (
        rgba("#a84438"),
        rgba("#b98732"),
        rgba("#287485"),
        rgba("#465d95"),
    )

    on_border = (
        inset <= x <= SIZE - inset and abs(y - inset) <= 2
        or inset <= x <= SIZE - inset and abs(y - (SIZE - inset)) <= 2
        or inset <= y <= SIZE - inset and abs(x - inset) <= 2
        or inset <= y <= SIZE - inset and abs(x - (SIZE - inset)) <= 2
    )

    if on_border:
        color = mix(color, border_colors[((x // 18) + (y // 18)) % len(border_colors)], strength)

    return color


def apply_night_lamplight(
    color: tuple[int, int, int, int],
    x: int,
    y: int,
    seed: int,
    strength: float,
) -> tuple[int, int, int, int]:
    """Add soft warm sari-sari/store-light pools over the cool nighttime floor."""

    lamp_points = (
        (28 + seed % 9, 36, 24),
        (91, 78 + seed % 11, 28),
        (69, 17 + seed % 7, 18),
    )

    for cx, cy, radius in lamp_points:
        distance = math.hypot(x - cx, y - cy)
        if distance < radius:
            falloff = (1 - distance / radius) ** 1.8
            color = mix(color, rgba("#d2974d"), strength * falloff)

    return color


def apply_cool_night_shadow(
    color: tuple[int, int, int, int],
    x: int,
    y: int,
    strength: float,
) -> tuple[int, int, int, int]:
    edge_distance = min(x, y, SIZE - 1 - x, SIZE - 1 - y)
    edge_shadow = max(0.0, (16 - edge_distance) / 16)
    diagonal_shadow = (math.sin((x * 0.08) + (y * 0.06)) + 1) * 0.5
    return mix(color, rgba("#172132"), strength * (0.35 + edge_shadow * 0.45 + diagonal_shadow * 0.20))


def tile(base: tuple[int, int, int, int], accent: tuple[int, int, int, int], seed: int, mode: str) -> bytes:
    rng = random.Random(seed)
    pixels = bytearray(SIZE * SIZE * 4)

    for y in range(SIZE):
        for x in range(SIZE):
            wave = (math.sin((x + seed) * 0.095) + math.cos((y - seed) * 0.105)) * 0.5
            fine_wave = math.sin((x + y + seed) * 0.41) * 0.04
            noise = rng.random() * 0.10
            t = max(0.0, min(1.0, 0.24 + wave * 0.12 + fine_wave + noise))
            color = mix(base, accent, t)
            color = apply_cool_night_shadow(color, x, y, 0.22)

            if mode == "soil":
                color = speckle(color, rgba("#221a16"), rng, 0.030, 0.20)
                color = speckle(color, rgba("#a9793f"), rng, 0.018, 0.20)
                color = apply_market_paint(color, x, y, seed, 0.20)
                color = apply_triangle_markers(color, x, y, seed, 0.26)
                if 24 < x < 104 and 24 < y < 104:
                    color = apply_market_border(color, x, y, 25, 0.28)
                if 58 < x < 67 or 58 < y < 67:
                    color = mix(color, rgba("#3f332c"), 0.08)

            if mode == "road":
                worn_track = abs(y - (SIZE / 2 + math.sin((x + seed) * 0.08) * 6))
                if worn_track < 14:
                    color = mix(color, rgba("#8f623b"), 0.14)
                for stripe_x, stripe_y, stripe_color in (
                    (20, 26, rgba("#a84438")),
                    (34, 30, rgba("#b98732")),
                    (49, 34, rgba("#287485")),
                    (83, 91, rgba("#465d95")),
                    (98, 95, rgba("#b98732")),
                    (112, 99, rgba("#a84438")),
                ):
                    if abs(x - stripe_x) <= 7 and abs(y - stripe_y) <= 2:
                        color = mix(color, stripe_color, 0.50)
                color = apply_triangle_markers(color, x, y, seed + 5, 0.28)
                color = speckle(color, rgba("#2a2019"), rng, 0.040, 0.22)
                color = speckle(color, rgba("#bb8b46"), rng, 0.012, 0.30)

            if mode == "grass":
                color = speckle(color, rgba("#4a7f45"), rng, 0.110, 0.34)
                color = speckle(color, rgba("#102218"), rng, 0.080, 0.42)
                leaf_band = math.sin((x * 0.18) + (y * 0.11) + seed) > 0.92
                if leaf_band:
                    color = mix(color, rgba("#355f38"), 0.34)

            if mode == "concrete":
                grout = (x % 32 <= 1) or (y % 32 <= 1)
                if grout:
                    color = mix(color, rgba("#1d2833"), 0.40)
                if (x + y + seed) % 41 == 0:
                    color = mix(color, rgba("#9c8550"), 0.18)
                color = apply_market_paint(color, x, y, seed + 17, 0.16)
                color = apply_market_border(color, x, y, 18, 0.24)

            color = apply_night_lamplight(color, x, y, seed, 0.24)

            i = (y * SIZE + x) * 4
            pixels[i : i + 4] = bytes(
                (
                    clamp(color[0]),
                    clamp(color[1]),
                    clamp(color[2]),
                    clamp(color[3]),
                )
            )

    return bytes(pixels)


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    write_folder_meta(OUT_DIR)

    specs = (
        ("soil_tile_placeholder.png", rgba("#49372b"), rgba("#765334"), 901, "soil"),
        ("road_tile_placeholder.png", rgba("#463323"), rgba("#735034"), 902, "road"),
        ("grass_tile_placeholder.png", rgba("#17331f"), rgba("#315d35"), 903, "grass"),
        ("concrete_tile_placeholder.png", rgba("#333a43"), rgba("#596169"), 904, "concrete"),
    )

    for name, base, accent, seed, mode in specs:
        path = OUT_DIR / name
        write_png(path, SIZE, SIZE, tile(base, accent, seed, mode))
        write_meta(path)
        print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
