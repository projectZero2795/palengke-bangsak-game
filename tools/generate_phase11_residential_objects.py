#!/usr/bin/env python3
"""Generate deterministic Phase 11 residential placeholder sprites.

These assets are replaceable placeholders for barangay-style houses, fences,
and gates. The goal is a stable versioned object set that can later be swapped
per map/component version without breaking Unity references.
"""

from __future__ import annotations

import math
import struct
import uuid
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Residential"
SIZE = 128


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    hex_color = hex_color.lstrip("#")
    return (
        int(hex_color[0:2], 16),
        int(hex_color[2:4], 16),
        int(hex_color[4:6], 16),
        alpha,
    )


OUTLINE = rgba("#0d1320", 235)
SHADOW = rgba("#000000", 88)
ROOF_DARK = rgba("#5a2d24")
ROOF_RED = rgba("#9e4630")
ROOF_BLUE = rgba("#2f557a")
ROOF_LIGHT = rgba("#d6844a")
WALL_BAMBOO = rgba("#b58a4a")
WALL_CREAM = rgba("#d8bd8a")
WALL_CONCRETE = rgba("#8fa0a8")
WINDOW_LIGHT = rgba("#ffd66b")
WINDOW_EDGE = rgba("#3a5269")
WOOD = rgba("#7a5130")
WOOD_LIGHT = rgba("#b57b40")
METAL = rgba("#6e8090")
METAL_LIGHT = rgba("#a6b4bd")
GRASS_SHADE = rgba("#15281a")


def asset_guid(relative_path: Path) -> str:
    return uuid.uuid5(uuid.NAMESPACE_URL, f"palengke-bangsak:{relative_path.as_posix()}").hex


def sprite_id(guid: str) -> str:
    return guid[:16] + "0900000000000000"


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
  spritePivot: {{x: 0.5, y: 0.38}}
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


class Canvas:
    def __init__(self) -> None:
        self.pixels = bytearray(SIZE * SIZE * 4)

    def blend(self, x: int, y: int, color: tuple[int, int, int, int]) -> None:
        if x < 0 or y < 0 or x >= SIZE or y >= SIZE:
            return
        r, g, b, a = color
        if a <= 0:
            return
        idx = (y * SIZE + x) * 4
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

    def ellipse(self, cx: float, cy: float, rx: float, ry: float, color: tuple[int, int, int, int]) -> None:
        for y in range(int(cy - ry) - 2, int(cy + ry) + 3):
            for x in range(int(cx - rx) - 2, int(cx + rx) + 3):
                if ((x - cx) / max(1, rx)) ** 2 + ((y - cy) / max(1, ry)) ** 2 <= 1:
                    self.blend(x, y, color)

    def rect(self, x0: int, y0: int, x1: int, y1: int, color: tuple[int, int, int, int]) -> None:
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.blend(x, y, color)

    def polygon(self, points: list[tuple[float, float]], color: tuple[int, int, int, int]) -> None:
        min_x = int(min(x for x, _ in points)) - 1
        max_x = int(max(x for x, _ in points)) + 1
        min_y = int(min(y for _, y in points)) - 1
        max_y = int(max(y for _, y in points)) + 1
        for y in range(min_y, max_y + 1):
            for x in range(min_x, max_x + 1):
                inside = False
                j = len(points) - 1
                for i in range(len(points)):
                    xi, yi = points[i]
                    xj, yj = points[j]
                    crosses = (yi > y) != (yj > y) and x < (xj - xi) * (y - yi) / max(0.0001, yj - yi) + xi
                    if crosses:
                        inside = not inside
                    j = i
                if inside:
                    self.blend(x, y, color)

    def line(self, x0: float, y0: float, x1: float, y1: float, radius: float, color: tuple[int, int, int, int]) -> None:
        min_x = int(min(x0, x1) - radius - 1)
        max_x = int(max(x0, x1) + radius + 1)
        min_y = int(min(y0, y1) - radius - 1)
        max_y = int(max(y0, y1) + radius + 1)
        dx = x1 - x0
        dy = y1 - y0
        length_sq = max(0.0001, dx * dx + dy * dy)
        for y in range(min_y, max_y + 1):
            for x in range(min_x, max_x + 1):
                t = max(0.0, min(1.0, ((x - x0) * dx + (y - y0) * dy) / length_sq))
                px = x0 + dx * t
                py = y0 + dy * t
                if math.hypot(x - px, y - py) <= radius:
                    self.blend(x, y, color)


def draw_small_house() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 98, 43, 13, SHADOW)
    canvas.polygon([(24, 48), (64, 26), (104, 48), (98, 56), (64, 40), (30, 56)], OUTLINE)
    canvas.polygon([(29, 47), (64, 29), (99, 47), (94, 52), (64, 39), (34, 52)], ROOF_RED)
    for x in range(37, 92, 9):
        canvas.line(x, 48, x + 28, 34, 1.4, ROOF_LIGHT)
    canvas.rect(32, 54, 96, 95, OUTLINE)
    canvas.rect(37, 57, 91, 92, WALL_BAMBOO)
    for x in range(42, 89, 9):
        canvas.line(x, 58, x, 91, 1, rgba("#7e5d36", 190))
    canvas.rect(56, 70, 72, 93, OUTLINE)
    canvas.rect(59, 73, 69, 93, rgba("#52331f"))
    canvas.rect(40, 64, 52, 76, OUTLINE)
    canvas.rect(43, 66, 50, 73, WINDOW_LIGHT)
    canvas.rect(76, 63, 88, 75, OUTLINE)
    canvas.rect(79, 65, 86, 72, WINDOW_LIGHT)
    canvas.ellipse(81, 45, 24, 12, rgba("#ffd66b", 45))
    return bytes(canvas.pixels)


def draw_medium_house() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 100, 48, 14, SHADOW)
    canvas.rect(25, 48, 103, 94, OUTLINE)
    canvas.rect(30, 52, 98, 91, WALL_CONCRETE)
    canvas.polygon([(19, 49), (39, 30), (100, 30), (111, 49)], OUTLINE)
    canvas.polygon([(26, 47), (43, 34), (96, 34), (104, 47)], ROOF_BLUE)
    for x in range(36, 100, 12):
        canvas.line(x, 35, x - 9, 47, 1.5, METAL_LIGHT)
    canvas.rect(55, 68, 74, 93, OUTLINE)
    canvas.rect(59, 71, 71, 93, rgba("#394c5d"))
    canvas.rect(35, 59, 49, 72, OUTLINE)
    canvas.rect(38, 61, 47, 69, WINDOW_LIGHT)
    canvas.rect(81, 58, 94, 72, OUTLINE)
    canvas.rect(84, 61, 92, 69, WINDOW_LIGHT)
    canvas.line(30, 82, 98, 82, 2, rgba("#657780"))
    canvas.ellipse(86, 46, 28, 12, rgba("#ffd66b", 35))
    return bytes(canvas.pixels)


def draw_fence_horizontal() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 78, 48, 7, SHADOW)
    for x in (18, 42, 66, 90, 112):
        canvas.rect(x - 3, 43, x + 3, 82, OUTLINE)
        canvas.rect(x - 2, 45, x + 2, 79, WOOD)
        canvas.rect(x - 1, 45, x, 79, WOOD_LIGHT)
    for y in (54, 69):
        canvas.rect(13, y - 4, 116, y + 4, OUTLINE)
        canvas.rect(16, y - 2, 113, y + 2, WOOD)
        canvas.line(18, y - 1, 112, y - 1, 1, WOOD_LIGHT)
    return bytes(canvas.pixels)


def draw_fence_vertical() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 79, 17, 8, SHADOW)
    for y in (22, 45, 68, 91):
        canvas.rect(45, y - 3, 83, y + 3, OUTLINE)
        canvas.rect(48, y - 2, 80, y + 2, WOOD)
        canvas.line(49, y - 1, 79, y - 1, 1, WOOD_LIGHT)
    for x in (53, 75):
        canvas.rect(x - 4, 16, x + 4, 101, OUTLINE)
        canvas.rect(x - 2, 19, x + 2, 98, WOOD)
    return bytes(canvas.pixels)


def draw_gate() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 81, 33, 8, SHADOW)
    canvas.rect(26, 35, 34, 84, OUTLINE)
    canvas.rect(94, 35, 102, 84, OUTLINE)
    canvas.rect(29, 38, 32, 81, METAL)
    canvas.rect(96, 38, 99, 81, METAL)
    canvas.rect(33, 43, 96, 49, OUTLINE)
    canvas.rect(36, 45, 93, 47, METAL_LIGHT)
    canvas.rect(35, 66, 95, 72, OUTLINE)
    canvas.rect(38, 68, 92, 70, METAL)
    for x in range(43, 90, 11):
        canvas.line(x, 46, x + 7, 70, 2, METAL_LIGHT)
        canvas.line(x + 7, 46, x, 70, 2, METAL)
    canvas.ellipse(64, 43, 25, 11, rgba("#ffd66b", 32))
    return bytes(canvas.pixels)


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    write_folder_meta(OUT_DIR)

    sprites = (
        ("small_house_placeholder.png", draw_small_house()),
        ("medium_house_placeholder.png", draw_medium_house()),
        ("fence_horizontal_placeholder.png", draw_fence_horizontal()),
        ("fence_vertical_placeholder.png", draw_fence_vertical()),
        ("gate_placeholder.png", draw_gate()),
    )

    for name, pixels in sprites:
        path = OUT_DIR / name
        write_png(path, SIZE, SIZE, pixels)
        write_meta(path)
        print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
