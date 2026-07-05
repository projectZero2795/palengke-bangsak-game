#!/usr/bin/env python3
"""Generate deterministic Phase 5 safe Bang placeholder assets."""

from __future__ import annotations

import math
import struct
import uuid
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Bang"
SCALE = 4
SIZE = 64


def rgba(hex_color: str, alpha: int = 255) -> tuple[int, int, int, int]:
    hex_color = hex_color.lstrip("#")
    return (
        int(hex_color[0:2], 16),
        int(hex_color[2:4], 16),
        int(hex_color[4:6], 16),
        alpha,
    )


YELLOW = rgba("#ffd166")
ORANGE = rgba("#f77f00")
RED = rgba("#d62828")
BLUE = rgba("#4dabf7")
WHITE = rgba("#fff7d6")
INK = rgba("#1b2430")
RANGE = rgba("#ffd166", 84)
TRANSPARENT = (0, 0, 0, 0)


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

    def ring(
        self,
        cx: float,
        cy: float,
        radius: float,
        width: float,
        color: tuple[int, int, int, int],
    ) -> None:
        cx *= SCALE
        cy *= SCALE
        outer = radius * SCALE
        inner = max(0.0, (radius - width) * SCALE)
        min_x = math.floor(cx - outer)
        max_x = math.ceil(cx + outer)
        min_y = math.floor(cy - outer)
        max_y = math.ceil(cy + outer)
        for y in range(min_y, max_y + 1):
            for x in range(min_x, max_x + 1):
                dist = math.hypot(x + 0.5 - cx, y + 0.5 - cy)
                if inner <= dist <= outer:
                    self.blend(x, y, color)

    def polygon(self, points: list[tuple[float, float]], color: tuple[int, int, int, int]) -> None:
        scaled = [(x * SCALE, y * SCALE) for x, y in points]
        min_x = math.floor(min(x for x, _ in scaled))
        max_x = math.ceil(max(x for x, _ in scaled))
        min_y = math.floor(min(y for _, y in scaled))
        max_y = math.ceil(max(y for _, y in scaled))
        for y in range(min_y, max_y + 1):
            for x in range(min_x, max_x + 1):
                if point_in_polygon(x + 0.5, y + 0.5, scaled):
                    self.blend(x, y, color)

    def line(
        self,
        x0: float,
        y0: float,
        x1: float,
        y1: float,
        width: float,
        color: tuple[int, int, int, int],
    ) -> None:
        steps = max(1, int(math.hypot(x1 - x0, y1 - y0) * SCALE))
        for index in range(steps + 1):
            t = index / steps
            x = x0 + (x1 - x0) * t
            y = y0 + (y1 - y0) * t
            self.ellipse(x, y, width, width, color)

    def rect(self, x0: float, y0: float, x1: float, y1: float, color: tuple[int, int, int, int]) -> None:
        for y in range(math.floor(y0 * SCALE), math.ceil(y1 * SCALE)):
            for x in range(math.floor(x0 * SCALE), math.ceil(x1 * SCALE)):
                self.blend(x, y, color)

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


def point_in_polygon(x: float, y: float, points: list[tuple[float, float]]) -> bool:
    inside = False
    j = len(points) - 1
    for i, point in enumerate(points):
        xi, yi = point
        xj, yj = points[j]
        if (yi > y) != (yj > y):
            x_intersect = (xj - xi) * (y - yi) / (yj - yi + 0.000001) + xi
            if x < x_intersect:
                inside = not inside
        j = i
    return inside


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


def asset_guid(relative_path: Path) -> str:
    return uuid.uuid5(uuid.NAMESPACE_URL, f"palengke-bangsak:{relative_path.as_posix()}").hex


def sprite_id(guid: str) -> str:
    return guid[:16] + "0800000000000000"


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
  spritePixelsToUnits: 64
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


def draw_bang_marker() -> bytes:
    c = Canvas()
    points = []
    for index in range(24):
        angle = -math.pi / 2 + index * math.tau / 24
        radius = 28 if index % 2 == 0 else 18
        points.append((32 + math.cos(angle) * radius, 32 + math.sin(angle) * radius))
    c.polygon(points, ORANGE)
    inner = []
    for index in range(20):
        angle = -math.pi / 2 + index * math.tau / 20
        radius = 22 if index % 2 == 0 else 15
        inner.append((32 + math.cos(angle) * radius, 32 + math.sin(angle) * radius))
    c.polygon(inner, YELLOW)
    c.ellipse(32, 32, 13, 10, WHITE)
    c.line(21, 32, 43, 32, 2.4, INK)
    c.line(21, 26, 21, 38, 2.0, INK)
    c.line(30, 25, 30, 39, 2.0, INK)
    c.line(39, 26, 43, 38, 1.8, INK)
    c.line(43, 26, 39, 38, 1.8, INK)
    c.line(47, 23, 51, 35, 1.8, RED)
    c.ellipse(52, 40, 2.2, 2.2, RED)
    return c.downsample()


def draw_range_ring() -> bytes:
    c = Canvas()
    c.ellipse(32, 32, 28, 28, (255, 209, 102, 24))
    c.ring(32, 32, 28, 2.5, RANGE)
    c.ring(32, 32, 18, 1.2, (77, 171, 247, 68))
    for angle in (0, math.pi / 2, math.pi, math.pi * 3 / 2):
        c.line(
            32 + math.cos(angle) * 22,
            32 + math.sin(angle) * 22,
            32 + math.cos(angle) * 28,
            32 + math.sin(angle) * 28,
            1.1,
            BLUE,
        )
    return c.downsample()


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    write_folder_meta(OUT_DIR)

    for name, image in {
        "bang_marker_placeholder.png": draw_bang_marker(),
        "bang_range_placeholder.png": draw_range_ring(),
    }.items():
        path = OUT_DIR / name
        write_png(path, SIZE, SIZE, image)
        write_meta(path)
        print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
