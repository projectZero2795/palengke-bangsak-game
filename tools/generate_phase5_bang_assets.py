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
DEFAULT_SIZE = 64
DEFAULT_SCALE = 4
RANGE_SIZE = 256
RANGE_SCALE = 3


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
RANGE = rgba("#ef4444", 74)
RANGE_EDGE = rgba("#fb8a7e", 170)
GOLD = rgba("#ffd166")
BROWN = rgba("#5b2f1c")
DARK_BROWN = rgba("#2b1710")
SANDAL_SOLE = rgba("#e28e4e")
SANDAL_FOOTBED = rgba("#f7b76a")
SANDAL_STRAP = rgba("#cd3034")
SANDAL_STRAP_DARK = rgba("#6f171c")
BUTTON_DARK = rgba("#1c2433")
BUTTON_TOP = rgba("#3c4b66")
BUTTON_RING = rgba("#ffd45a")
TRANSPARENT = (0, 0, 0, 0)


class Canvas:
    def __init__(self, size: int = DEFAULT_SIZE, scale: int = DEFAULT_SCALE) -> None:
        self.size = size
        self.scale = scale
        self.w = size * scale
        self.h = size * scale
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
        cx *= self.scale
        cy *= self.scale
        rx *= self.scale
        ry *= self.scale
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
        cx *= self.scale
        cy *= self.scale
        outer = radius * self.scale
        inner = max(0.0, (radius - width) * self.scale)
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
        scaled = [(x * self.scale, y * self.scale) for x, y in points]
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
        steps = max(1, int(math.hypot(x1 - x0, y1 - y0) * self.scale))
        for index in range(steps + 1):
            t = index / steps
            x = x0 + (x1 - x0) * t
            y = y0 + (y1 - y0) * t
            self.ellipse(x, y, width, width, color)

    def rect(self, x0: float, y0: float, x1: float, y1: float, color: tuple[int, int, int, int]) -> None:
        for y in range(math.floor(y0 * self.scale), math.ceil(y1 * self.scale)):
            for x in range(math.floor(x0 * self.scale), math.ceil(x1 * self.scale)):
                self.blend(x, y, color)

    def downsample(self) -> bytes:
        out = bytearray(self.size * self.size * 4)
        for y in range(self.size):
            for x in range(self.size):
                totals = [0, 0, 0, 0]
                for sy in range(self.scale):
                    for sx in range(self.scale):
                        idx = ((y * self.scale + sy) * self.w + (x * self.scale + sx)) * 4
                        for i in range(4):
                            totals[i] += self.pixels[idx + i]
                dst = (y * self.size + x) * 4
                count = self.scale * self.scale
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


def write_meta(path: Path, pixels_per_unit: int = DEFAULT_SIZE, filter_mode: int = 0) -> None:
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
    filterMode: {filter_mode}
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
  spritePixelsToUnits: {pixels_per_unit}
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


def draw_tsinelas_marker() -> bytes:
    c = Canvas()
    # The marker points to the right at zero rotation. The controller rotates it
    # to match the player's facing direction.
    c.ellipse(30, 40, 24, 8, (0, 0, 0, 42))
    outer = [
        (7, 34),
        (11, 25),
        (20, 18),
        (37, 14),
        (53, 21),
        (58, 33),
        (51, 44),
        (33, 51),
        (16, 47),
    ]
    c.polygon(outer, DARK_BROWN)
    rim = [
        (9, 34),
        (13, 27),
        (21, 20),
        (37, 17),
        (51, 23),
        (55, 33),
        (49, 42),
        (33, 48),
        (18, 45),
    ]
    c.polygon(rim, SANDAL_SOLE)
    footbed = [
        (14, 34),
        (18, 28),
        (25, 24),
        (38, 22),
        (48, 26),
        (51, 33),
        (46, 39),
        (33, 43),
        (22, 41),
    ]
    c.polygon(footbed, SANDAL_FOOTBED)
    for x, y in ((22, 38), (29, 41), (39, 39), (47, 33), (25, 27), (38, 24)):
        c.ellipse(x, y, 1.1, 1.1, (87, 45, 24, 96))
    toe = (39, 33)
    c.line(23, 25, toe[0], toe[1], 2.2, SANDAL_STRAP_DARK)
    c.line(toe[0], toe[1], 28, 43, 2.2, SANDAL_STRAP_DARK)
    c.line(toe[0], toe[1], 49, 28, 2.2, SANDAL_STRAP_DARK)
    c.line(23, 25, toe[0], toe[1], 1.45, SANDAL_STRAP)
    c.line(toe[0], toe[1], 28, 43, 1.45, SANDAL_STRAP)
    c.line(toe[0], toe[1], 49, 28, 1.45, SANDAL_STRAP)
    c.ellipse(toe[0], toe[1], 3.1, 3.1, SANDAL_STRAP_DARK)
    c.ellipse(toe[0], toe[1], 1.25, 1.25, GOLD)
    return c.downsample()


def draw_range_cone() -> bytes:
    c = Canvas(RANGE_SIZE, RANGE_SCALE)
    # Cone points up at zero rotation. The controller rotates it to the
    # player's facing direction and scales it to the configured range. This
    # asset is intentionally larger and bilinear-filtered so the range UI
    # stays smooth when scaled in-world, unlike the pixel-art player sprites.
    arc = []
    center = RANGE_SIZE / 2
    radius = RANGE_SIZE * 0.46
    half_angle = 34
    samples = 96
    for index in range(samples + 1):
        angle = math.radians(-90 - half_angle + index * (half_angle * 2) / samples)
        arc.append((center + math.cos(angle) * radius, center + math.sin(angle) * radius))
    cone = [(center, center)] + arc
    c.polygon(cone, rgba("#ef4444", 68))
    c.line(center, center, arc[0][0], arc[0][1], 1.7, rgba("#ff8a80", 150))
    c.line(center, center, arc[-1][0], arc[-1][1], 1.7, rgba("#ff8a80", 150))
    for index in range(len(arc) - 1):
        c.line(arc[index][0], arc[index][1], arc[index + 1][0], arc[index + 1][1], 1.3, RANGE_EDGE)
    return c.downsample()


def draw_impact_burst() -> bytes:
    c = Canvas()
    points = []
    for index in range(22):
        angle = -math.pi / 2 + index * math.tau / 22
        radius = 25 if index % 2 == 0 else 12
        points.append((32 + math.cos(angle) * radius, 32 + math.sin(angle) * radius))
    c.polygon(points, rgba("#ffd54a", 220))
    inner = []
    for index in range(18):
        angle = -math.pi / 2 + index * math.tau / 18
        radius = 15 if index % 2 == 0 else 8
        inner.append((32 + math.cos(angle) * radius, 32 + math.sin(angle) * radius))
    c.polygon(inner, rgba("#ff7a3d", 220))
    c.ring(32, 32, 28, 2.0, rgba("#ffe58a", 135))
    for angle in range(0, 360, 45):
        radians = math.radians(angle)
        c.line(
            32 + math.cos(radians) * 10,
            32 + math.sin(radians) * 10,
            32 + math.cos(radians) * 28,
            32 + math.sin(radians) * 28,
            0.9,
            rgba("#fff0a8", 160),
        )
    return c.downsample()


def draw_button_background() -> bytes:
    c = Canvas()
    c.ellipse(34, 36, 27, 27, (0, 0, 0, 92))
    c.ellipse(32, 32, 29, 29, DARK_BROWN)
    c.ring(32, 32, 29, 3.0, BUTTON_RING)
    c.ellipse(32, 32, 23, 23, BUTTON_DARK)
    c.ellipse(32, 24, 20, 13, BUTTON_TOP)
    c.ring(32, 32, 23, 1.2, rgba("#ffffff", 42))
    return c.downsample()


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    write_folder_meta(OUT_DIR)

    for name, size, image, pixels_per_unit, filter_mode in (
        ("bang_marker_placeholder.png", DEFAULT_SIZE, draw_tsinelas_marker(), DEFAULT_SIZE, 0),
        ("bang_range_placeholder.png", RANGE_SIZE, draw_range_cone(), RANGE_SIZE, 1),
        ("bang_impact_placeholder.png", DEFAULT_SIZE, draw_impact_burst(), DEFAULT_SIZE, 0),
        ("bang_button_dark_placeholder.png", DEFAULT_SIZE, draw_button_background(), DEFAULT_SIZE, 0),
    ):
        path = OUT_DIR / name
        write_png(path, size, size, image)
        write_meta(path, pixels_per_unit=pixels_per_unit, filter_mode=filter_mode)
        print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
