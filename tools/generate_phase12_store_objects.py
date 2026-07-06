#!/usr/bin/env python3
"""Generate deterministic Phase 12 marketplace placeholder sprites.

These are replaceable Filipino/palengke-themed store props. They establish a
stable versioned object set for Phase 12 without pretending to be final art.
"""

from __future__ import annotations

import math
import struct
import uuid
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Stores"
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
SHADOW = rgba("#000000", 86)
WOOD = rgba("#7a5130")
WOOD_LIGHT = rgba("#b77b3d")
ROOF_RED = rgba("#a74830")
ROOF_GREEN = rgba("#348350")
ROOF_BLUE = rgba("#2f5d87")
TARP_YELLOW = rgba("#f0c24a")
TARP_ORANGE = rgba("#d97933")
WALL = rgba("#c69a58")
WINDOW_LIGHT = rgba("#ffd66b")
FRUIT_RED = rgba("#d34b3f")
FRUIT_GREEN = rgba("#67a856")
FRUIT_YELLOW = rgba("#e6b63d")
BASKET = rgba("#a86b38")
BASKET_DARK = rgba("#5b3925")
METAL = rgba("#6f8090")
SIGN_BLUE = rgba("#284d79")
SIGN_TEXT = rgba("#f4df86")


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


def draw_sari_sari_store() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 100, 43, 13, SHADOW)
    canvas.rect(28, 53, 100, 93, OUTLINE)
    canvas.rect(33, 57, 95, 90, WALL)
    canvas.polygon([(23, 54), (38, 34), (98, 34), (108, 54)], OUTLINE)
    canvas.polygon([(30, 51), (43, 38), (94, 38), (101, 51)], ROOF_RED)
    canvas.rect(38, 43, 89, 54, OUTLINE)
    canvas.rect(41, 45, 86, 52, SIGN_BLUE)
    # Pixel text hint: SARI
    for x in (45, 55, 65, 75):
        canvas.rect(x, 47, x + 4, 49, SIGN_TEXT)
    canvas.rect(39, 64, 57, 83, OUTLINE)
    canvas.rect(42, 67, 54, 80, WINDOW_LIGHT)
    canvas.rect(67, 66, 85, 90, OUTLINE)
    canvas.rect(70, 69, 82, 90, rgba("#5b3925"))
    for y in (67, 73, 79):
        canvas.line(42, y, 54, y, 1, rgba("#8a5a32", 180))
    canvas.ellipse(85, 49, 24, 11, rgba("#ffd66b", 40))
    return bytes(canvas.pixels)


def draw_palengke_stall() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 95, 45, 11, SHADOW)
    canvas.rect(26, 56, 102, 89, OUTLINE)
    canvas.rect(31, 62, 97, 86, WOOD)
    canvas.rect(24, 44, 104, 60, OUTLINE)
    for i, color in enumerate((ROOF_GREEN, TARP_YELLOW, ROOF_GREEN, TARP_YELLOW, ROOF_GREEN)):
        x0 = 29 + i * 14
        canvas.polygon([(x0, 47), (x0 + 15, 47), (x0 + 12, 58), (x0 + 2, 58)], color)
    canvas.rect(36, 66, 90, 80, rgba("#47311f"))
    for cx, cy, color in (
        (42, 69, FRUIT_RED),
        (51, 71, FRUIT_YELLOW),
        (61, 69, FRUIT_GREEN),
        (72, 72, FRUIT_RED),
        (82, 69, FRUIT_GREEN),
    ):
        canvas.ellipse(cx, cy, 5, 4, OUTLINE)
        canvas.ellipse(cx, cy, 3, 3, color)
    canvas.rect(34, 82, 94, 90, OUTLINE)
    canvas.rect(38, 83, 90, 87, WOOD_LIGHT)
    return bytes(canvas.pixels)


def draw_food_stall() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 95, 36, 10, SHADOW)
    canvas.rect(35, 57, 93, 89, OUTLINE)
    canvas.rect(39, 62, 89, 86, rgba("#8f4f32"))
    canvas.rect(31, 43, 97, 59, OUTLINE)
    canvas.polygon([(35, 45), (62, 35), (94, 45), (89, 57), (40, 57)], TARP_ORANGE)
    canvas.rect(43, 50, 82, 57, rgba("#f1b044"))
    canvas.rect(45, 65, 83, 75, OUTLINE)
    canvas.rect(48, 67, 80, 72, rgba("#f6d171"))
    for cx in (51, 60, 70, 78):
        canvas.ellipse(cx, 67, 4, 3, rgba("#783623"))
    canvas.line(42, 59, 42, 88, 2, METAL)
    canvas.line(86, 59, 86, 88, 2, METAL)
    canvas.ellipse(80, 47, 19, 9, rgba("#ffd66b", 38))
    return bytes(canvas.pixels)


def draw_signboard_sari() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 86, 19, 6, SHADOW)
    canvas.line(50, 75, 50, 97, 3, OUTLINE)
    canvas.line(78, 75, 78, 97, 3, OUTLINE)
    canvas.line(50, 77, 50, 96, 1.6, WOOD_LIGHT)
    canvas.line(78, 77, 78, 96, 1.6, WOOD_LIGHT)
    canvas.rect(35, 47, 93, 76, OUTLINE)
    canvas.rect(39, 51, 89, 72, SIGN_BLUE)
    # Pixel label hint: SARI
    for x in (44, 56, 68, 80):
        canvas.rect(x, 57, x + 5, 61, SIGN_TEXT)
        canvas.rect(x, 65, x + 5, 67, SIGN_TEXT)
    canvas.ellipse(74, 45, 20, 8, rgba("#ffd66b", 34))
    return bytes(canvas.pixels)


def draw_crates_baskets() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 91, 32, 8, SHADOW)
    for cx, cy, rx, ry in ((47, 69, 15, 12), (67, 73, 17, 13), (85, 68, 14, 11)):
        canvas.ellipse(cx, cy, rx + 2, ry + 2, OUTLINE)
        canvas.ellipse(cx, cy, rx, ry, BASKET)
        canvas.line(cx - rx + 4, cy, cx + rx - 4, cy, 1, BASKET_DARK)
        canvas.line(cx, cy - ry + 3, cx, cy + ry - 2, 1, BASKET_DARK)
    for cx, cy, color in (
        (43, 65, FRUIT_RED),
        (52, 67, FRUIT_GREEN),
        (64, 70, FRUIT_YELLOW),
        (73, 72, FRUIT_RED),
        (84, 66, FRUIT_GREEN),
    ):
        canvas.ellipse(cx, cy, 4, 3, OUTLINE)
        canvas.ellipse(cx, cy, 2, 2, color)
    canvas.rect(36, 82, 91, 95, OUTLINE)
    canvas.rect(40, 84, 87, 92, WOOD)
    for x in range(43, 88, 10):
        canvas.line(x, 84, x, 92, 1, WOOD_LIGHT)
    return bytes(canvas.pixels)


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    write_folder_meta(OUT_DIR)

    sprites = (
        ("sari_sari_store_placeholder.png", draw_sari_sari_store()),
        ("palengke_stall_placeholder.png", draw_palengke_stall()),
        ("food_stall_placeholder.png", draw_food_stall()),
        ("signboard_sari_placeholder.png", draw_signboard_sari()),
        ("crates_baskets_placeholder.png", draw_crates_baskets()),
    )

    for name, pixels in sprites:
        path = OUT_DIR / name
        write_png(path, SIZE, SIZE, pixels)
        write_meta(path)
        print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
