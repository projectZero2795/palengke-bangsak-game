#!/usr/bin/env python3
"""Generate deterministic Phase 4 directional placeholder player sprites.

The Phase 2 sprites were front-facing only. Phase 4 needs the player to look
where they are moving, including diagonals, while still staying lightweight and
fully reproducible for review. These are intentionally placeholder assets:
clear direction, readable silhouette, easy to replace later.
"""

from __future__ import annotations

import math
import struct
import uuid
import zlib
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_DIR = ROOT / "unity" / "Assets" / "Art" / "Placeholders" / "Players" / "Directional"
SCALE = 4
SIZE = 64

VARIANTS = {
    "red": (214, 57, 46, 255),
    "green": (36, 164, 83, 255),
    "blue": (58, 117, 232, 255),
    "yellow": (242, 196, 67, 255),
}

DIRECTIONS = {
    "down": (0.0, 1.0),
    "down_right": (0.72, 0.7),
    "right": (1.0, 0.0),
    "up_right": (0.72, -0.7),
    "up": (0.0, -1.0),
    "up_left": (-0.72, -0.7),
    "left": (-1.0, 0.0),
    "down_left": (-0.72, 0.7),
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
NOSE = rgba("#7b3f2b")
FACE_GUIDE = rgba("#ffffff", 38)


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
        for i in range(steps + 1):
            t = i / steps
            x = x0 + (x1 - x0) * t
            y = y0 + (y1 - y0) * t
            self.ellipse(x, y, width, width, color)

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
    mipMapsPreserveCoverage: 0
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
  - serializedVersion: 3
    buildTarget: Standalone
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 3
    buildTarget: WebGL
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
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


def draw_face(c: Canvas, direction: str, x: float, y: float, dx: float, dy: float) -> None:
    if direction == "up":
        c.ellipse(x, y - 7, 9, 3.2, HAIR_HI)
        return

    if direction in {"up_left", "up_right"}:
        side = -1 if direction == "up_left" else 1
        c.ellipse(x + side * 3, y - 5, 6, 2.4, HAIR_HI)
        c.ellipse(x + side * 4.5, y + 3, 1.2, 1.5, OUTLINE)
        return

    if direction in {"left", "right"}:
        side = -1 if direction == "left" else 1
        c.ellipse(x + side * 4.5, y + 1.5, 1.3, 1.7, OUTLINE)
        c.ellipse(x + side * 6.2, y + 5.5, 1.2, 0.7, NOSE)
        c.rect(x + side * 2, y + 7.5, x + side * 6, y + 8.3, NOSE)
        return

    # down and lower diagonals
    eye_spread = 4.2
    c.ellipse(x - eye_spread + dx * 2.0, y + 1.0 + dy * 0.8, 1.4, 1.7, OUTLINE)
    c.ellipse(x + eye_spread + dx * 2.0, y + 1.0 + dy * 0.8, 1.4, 1.7, OUTLINE)
    c.ellipse(x + dx * 2.2, y + 5.4 + dy * 0.6, 1.3, 0.7, SKIN_DARK)
    c.ellipse(x + dx * 1.7, y + 7.8, 3.0, 1.1, WHITE)
    c.rect(x - 3 + dx * 1.7, y + 7.2, x + 3 + dx * 1.7, y + 8.0, NOSE)


def draw_player(shirt: tuple[int, int, int, int], direction: str, step: float) -> bytes:
    c = Canvas()
    dx, dy = DIRECTIONS[direction]
    bob = -0.7 if abs(step) > 1.5 else 0.0

    # soft grounding shadow
    c.ellipse(32, 56, 17, 5, SHADOW)

    center_x = 32 + dx * 1.5
    center_y = 32 + bob + dy * 0.6
    head_x = 32 + dx * 3.8
    head_y = 22 + bob + dy * 2.4

    # Walk cycle: feet swing along the travel direction and separate sideways.
    side_x = -dy
    side_y = dx
    foot_a_x = 28 + side_x * 3.0 - dx * step * 0.65
    foot_a_y = 51 + side_y * 3.0 - dy * step * 0.65 + bob
    foot_b_x = 36 - side_x * 3.0 + dx * step * 0.65
    foot_b_y = 51 - side_y * 3.0 + dy * step * 0.65 + bob

    c.line(28 + side_x * 2.2, 42 + side_y * 2.2 + bob, foot_a_x, foot_a_y, 2.4, OUTLINE)
    c.line(36 - side_x * 2.2, 42 - side_y * 2.2 + bob, foot_b_x, foot_b_y, 2.4, OUTLINE)
    c.line(28 + side_x * 2.2, 42 + side_y * 2.2 + bob, foot_a_x, foot_a_y, 1.5, PANTS)
    c.line(36 - side_x * 2.2, 42 - side_y * 2.2 + bob, foot_b_x, foot_b_y, 1.5, PANTS)
    c.ellipse(foot_a_x, foot_a_y + 2.2, 4, 2.5, SHOES)
    c.ellipse(foot_b_x, foot_b_y + 2.2, 4, 2.5, SHOES)

    # arms, drawn behind torso and swinging opposite feet
    arm_a_x = 21 + side_x * 1.5 + dx * step * 0.35
    arm_a_y = 38 + side_y * 1.5 + dy * step * 0.35 + bob
    arm_b_x = 43 - side_x * 1.5 - dx * step * 0.35
    arm_b_y = 38 - side_y * 1.5 - dy * step * 0.35 + bob
    c.line(26 + side_x * 5, 35 + bob, arm_a_x, arm_a_y, 3.2, OUTLINE)
    c.line(38 - side_x * 5, 35 + bob, arm_b_x, arm_b_y, 3.2, OUTLINE)
    c.line(26 + side_x * 5, 35 + bob, arm_a_x, arm_a_y, 2.0, SKIN)
    c.line(38 - side_x * 5, 35 + bob, arm_b_x, arm_b_y, 2.0, SKIN)

    # body
    c.rounded_rect(center_x - 10, center_y - 1, center_x + 10, center_y + 17, 5, OUTLINE)
    c.rounded_rect(center_x - 8, center_y, center_x + 8, center_y + 16, 4, shirt)
    c.ellipse(center_x + dx * 2, center_y + 1, 8, 4, FACE_GUIDE)

    # neck, ears, head
    c.rounded_rect(head_x - 4, head_y + 5, head_x + 4, head_y + 12, 3, SKIN_DARK)
    if direction not in {"up", "up_left", "up_right"}:
        c.ellipse(head_x - 12, head_y, 3.2, 4.5, OUTLINE)
        c.ellipse(head_x + 12, head_y, 3.2, 4.5, OUTLINE)
        c.ellipse(head_x - 12, head_y, 2.1, 3.2, SKIN)
        c.ellipse(head_x + 12, head_y, 2.1, 3.2, SKIN)
    c.ellipse(head_x, head_y, 14, 15, OUTLINE)
    c.ellipse(head_x, head_y + 1, 12, 13, SKIN)

    # hair silhouette changes by direction, making direction readable.
    c.ellipse(head_x - dx * 3, head_y - 8 - dy * 2, 12, 7, HAIR)
    c.ellipse(head_x - 8 + dx * 2, head_y - 2, 5, 8, HAIR)
    c.ellipse(head_x + 8 + dx * 2, head_y - 2, 5, 8, HAIR)
    c.ellipse(head_x - dx * 5, head_y - 10, 4, 2, HAIR_HI)
    if direction == "up":
        c.ellipse(head_x, head_y - 2, 13, 12, HAIR)
        c.ellipse(head_x - 3, head_y - 8, 5, 2.5, HAIR_HI)

    draw_face(c, direction, head_x, head_y, dx, dy)

    return c.downsample()


def main() -> None:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    write_folder_meta(OUT_DIR)

    for variant_name, shirt in VARIANTS.items():
        for direction_name in DIRECTIONS:
            for pose_name, step in POSES.items():
                path = OUT_DIR / f"player_{pose_name}_{direction_name}_{variant_name}.png"
                write_png(path, SIZE, SIZE, draw_player(shirt, direction_name, step))
                write_meta(path)
                print(path.relative_to(ROOT))


if __name__ == "__main__":
    main()
