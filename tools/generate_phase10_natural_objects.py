#!/usr/bin/env python3
"""Generate deterministic Phase 10 natural-object placeholder sprites.

These are intentionally replaceable placeholder assets. They establish the
Phase 10 object set and versioned asset contract without pretending to be final
art: Filipino-night-market natural objects such as banana/coconut/bamboo shapes,
bougainvillea-style bushes, and potted tropical plants.
"""

from __future__ import annotations

import math
import struct
import uuid
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Natural"
SIZE = 128


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    hex_color = hex_color.lstrip("#")
    return (
        int(hex_color[0:2], 16),
        int(hex_color[2:4], 16),
        int(hex_color[4:6], 16),
        alpha,
    )


TRANSPARENT = (0, 0, 0, 0)
OUTLINE = rgba("#101722", 230)
NIGHT_LEAF = rgba("#18351f")
LEAF = rgba("#265b31")
LEAF_LIGHT = rgba("#4f8747")
LEAF_WARM = rgba("#8aa34d")
TRUNK = rgba("#4f3424")
TRUNK_LIGHT = rgba("#8b623d")
POT = rgba("#8f4f32")
POT_LIGHT = rgba("#c17a45")
LAMP_EDGE = rgba("#dca44d", 80)
SHADOW = rgba("#000000", 80)


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
  spritePivot: {{x: 0.5, y: 0.42}}
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

    def leaf_blob(self, cx: float, cy: float, rx: float, ry: float, color: tuple[int, int, int, int]) -> None:
        self.ellipse(cx, cy, rx + 2, ry + 2, OUTLINE)
        self.ellipse(cx, cy, rx, ry, color)
        self.ellipse(cx - rx * 0.25, cy - ry * 0.28, rx * 0.35, ry * 0.24, LEAF_LIGHT)
        self.ellipse(cx + rx * 0.18, cy + ry * 0.10, rx * 0.28, ry * 0.18, NIGHT_LEAF)
        self.ellipse(cx - rx * 0.04, cy - ry * 0.04, rx * 0.55, ry * 0.38, rgba("#000000", 18))


def draw_tree() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 101, 30, 11, SHADOW)
    # Coconut/banana-like trunk and crown: common Filipino street/barangay silhouette.
    canvas.line(63, 103, 58, 62, 7, OUTLINE)
    canvas.line(63, 103, 58, 62, 4, TRUNK)
    canvas.line(62, 100, 56, 66, 1.5, TRUNK_LIGHT)
    canvas.line(57, 62, 52, 51, 5, OUTLINE)
    canvas.line(57, 62, 52, 51, 3, TRUNK_LIGHT)
    for angle in (-75, -45, -18, 18, 45, 75):
        radians = math.radians(angle)
        tip_x = 58 + math.sin(radians) * 40
        tip_y = 48 - math.cos(radians) * 22
        canvas.line(58, 52, tip_x, tip_y, 10, OUTLINE)
        canvas.line(58, 52, tip_x, tip_y, 7, LEAF)
        canvas.line(58, 52, tip_x, tip_y, 2, LEAF_LIGHT)
    for cx, cy in ((51, 55), (62, 54), (56, 45)):
        canvas.ellipse(cx, cy, 5, 7, rgba("#6f4a2f"))
    canvas.ellipse(35, 34, 18, 11, LAMP_EDGE)
    return bytes(canvas.pixels)


def draw_bush() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 84, 32, 10, SHADOW)
    # Bougainvillea-style bush: leafy base with small pink/purple flowers.
    for cx, cy, rx, ry in (
        (42, 68, 22, 18),
        (61, 58, 28, 22),
        (83, 69, 23, 19),
        (62, 76, 35, 18),
    ):
        canvas.leaf_blob(cx, cy, rx, ry, LEAF)
    for cx, cy in ((42, 61), (55, 52), (76, 58), (86, 72), (61, 77), (49, 74)):
        canvas.ellipse(cx, cy, 4, 3, rgba("#d05b9d"))
        canvas.ellipse(cx + 2, cy - 1, 3, 3, rgba("#e184b7"))
    canvas.ellipse(75, 45, 16, 10, LAMP_EDGE)
    return bytes(canvas.pixels)


def draw_plant_pot() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 91, 18, 7, SHADOW)
    # Potted tropical plant often seen outside Filipino homes/sari-sari stores.
    for angle in (-62, -34, -10, 16, 42, 66):
        radians = math.radians(angle)
        canvas.line(64, 57, 64 + math.sin(radians) * 28, 43 - math.cos(radians) * 20, 5, OUTLINE)
        canvas.line(64, 57, 64 + math.sin(radians) * 28, 43 - math.cos(radians) * 20, 3, LEAF_LIGHT)
    canvas.ellipse(64, 57, 14, 10, LEAF)
    canvas.ellipse(64, 75, 21, 9, OUTLINE)
    canvas.rect(45, 72, 83, 91, OUTLINE)
    canvas.ellipse(64, 72, 18, 7, POT_LIGHT)
    canvas.rect(49, 73, 79, 89, POT)
    canvas.ellipse(64, 88, 16, 5, rgba("#5b3124"))
    return bytes(canvas.pixels)


def draw_plant_cluster() -> bytes:
    canvas = Canvas()
    canvas.ellipse(64, 93, 41, 10, SHADOW)
    # Bamboo/banana plant cluster silhouette for Filipino barangay corners.
    for cx, cy, angle in ((35, 72, -18), (48, 59, -8), (64, 54, 0), (80, 60, 10), (94, 76, 20)):
        canvas.line(cx, 91, cx + math.sin(math.radians(angle)) * 10, cy - 12, 4, OUTLINE)
        canvas.line(cx, 91, cx + math.sin(math.radians(angle)) * 10, cy - 12, 2, rgba("#6f8a47"))
        canvas.ellipse(cx + math.sin(math.radians(angle)) * 12, cy, 12, 31, OUTLINE)
        canvas.ellipse(cx + math.sin(math.radians(angle)) * 12, cy, 8, 27, LEAF)
        canvas.line(cx + math.sin(math.radians(angle)) * 12, cy + 21, cx + math.sin(math.radians(angle)) * 12, cy - 20, 1.2, LEAF_LIGHT)
    canvas.ellipse(64, 83, 34, 14, rgba("#1f4428"))
    canvas.ellipse(83, 51, 22, 13, LAMP_EDGE)
    return bytes(canvas.pixels)


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    write_folder_meta(OUT_DIR)

    sprites = (
        ("tree_placeholder.png", draw_tree()),
        ("bush_placeholder.png", draw_bush()),
        ("plant_pot_placeholder.png", draw_plant_pot()),
        ("plant_cluster_placeholder.png", draw_plant_cluster()),
    )

    for name, pixels in sprites:
        path = OUT_DIR / name
        write_png(path, SIZE, SIZE, pixels)
        write_meta(path)
        print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
