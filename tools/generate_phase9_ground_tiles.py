#!/usr/bin/env python3
"""Generate deterministic Phase 9 ground tile placeholder sprites.

The art direction is the approved "bottom-center" option from the review:
a warm Filipino street-market plaza floor with colorful painted borders,
small triangle markings, and open playable space. It stays tilemap-friendly
and keeps Phase 9 focused on ground only.
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
        (0.18, 18 + seed % 13, rgba("#d85f4c")),
        (-0.12, 46 + seed % 19, rgba("#3c92a6")),
        (0.10, 78 + seed % 17, rgba("#e3b64b")),
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
        (25 + seed % 11, 34, rgba("#c44f3d")),
        (70, 23 + seed % 17, rgba("#2d8aa0")),
        (96, 77, rgba("#dfb044")),
        (42, 96, rgba("#5f73bd")),
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
        rgba("#d04f3e"),
        rgba("#e0ad37"),
        rgba("#2d8ca4"),
        rgba("#466aa8"),
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

            if mode == "soil":
                color = speckle(color, rgba("#5a3828"), rng, 0.030, 0.20)
                color = speckle(color, rgba("#e1b66c"), rng, 0.018, 0.23)
                color = apply_market_paint(color, x, y, seed, 0.16)
                color = apply_triangle_markers(color, x, y, seed, 0.24)
                if 24 < x < 104 and 24 < y < 104:
                    color = apply_market_border(color, x, y, 25, 0.22)
                if 58 < x < 67 or 58 < y < 67:
                    color = mix(color, rgba("#6e5543"), 0.06)

            if mode == "road":
                worn_track = abs(y - (SIZE / 2 + math.sin((x + seed) * 0.08) * 6))
                if worn_track < 14:
                    color = mix(color, rgba("#c08a57"), 0.18)
                for stripe_x, stripe_y, stripe_color in (
                    (20, 26, rgba("#d04f3e")),
                    (34, 30, rgba("#e3b447")),
                    (49, 34, rgba("#2b899c")),
                    (83, 91, rgba("#466aa8")),
                    (98, 95, rgba("#e0ad37")),
                    (112, 99, rgba("#c74d3d")),
                ):
                    if abs(x - stripe_x) <= 7 and abs(y - stripe_y) <= 2:
                        color = mix(color, stripe_color, 0.58)
                color = apply_triangle_markers(color, x, y, seed + 5, 0.30)
                color = speckle(color, rgba("#5c3725"), rng, 0.040, 0.22)
                color = speckle(color, rgba("#f3cd74"), rng, 0.012, 0.35)

            if mode == "grass":
                color = speckle(color, rgba("#8ed768"), rng, 0.110, 0.45)
                color = speckle(color, rgba("#1e4629"), rng, 0.080, 0.32)
                leaf_band = math.sin((x * 0.18) + (y * 0.11) + seed) > 0.92
                if leaf_band:
                    color = mix(color, rgba("#74bd56"), 0.38)

            if mode == "concrete":
                grout = (x % 32 <= 1) or (y % 32 <= 1)
                if grout:
                    color = mix(color, rgba("#4c3f35"), 0.34)
                if (x + y + seed) % 41 == 0:
                    color = mix(color, rgba("#e0d4a3"), 0.26)
                color = apply_market_paint(color, x, y, seed + 17, 0.12)
                color = apply_market_border(color, x, y, 18, 0.18)

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
        ("soil_tile_placeholder.png", rgba("#8f6843"), rgba("#c28d58"), 901, "soil"),
        ("road_tile_placeholder.png", rgba("#8d6541"), rgba("#c88e55"), 902, "road"),
        ("grass_tile_placeholder.png", rgba("#2f6639"), rgba("#669f48"), 903, "grass"),
        ("concrete_tile_placeholder.png", rgba("#766b5f"), rgba("#aaa092"), 904, "concrete"),
    )

    for name, base, accent, seed, mode in specs:
        path = OUT_DIR / name
        write_png(path, SIZE, SIZE, tile(base, accent, seed, mode))
        write_meta(path)
        print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
