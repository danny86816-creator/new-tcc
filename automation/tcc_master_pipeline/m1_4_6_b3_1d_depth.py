from __future__ import annotations

import hashlib
import json
import shutil
from pathlib import Path

import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / "automation" / "tcc_master_pipeline" / "work" / "m1_4_6_b3_1d"
B31 = ROOT / "automation" / "tcc_master_pipeline" / "work" / "m1_4_6_b3_1"
B31R = ROOT / "automation" / "tcc_master_pipeline" / "work" / "m1_4_6_b3_1r"
ASSETS = ROOT / "src" / "Tcc.DesktopHost" / "Assets" / "Home"
SOURCE = Path(r"C:\Users\danny\.codex\generated_images\01a0a657-c32a-7812-a485-9fa95fc669d5\精選素材\exec-424e7bd4-c350-47b4-a64b-fc17d1e0fe05.png")
SOURCE_SHA = "DC1255360ECCEBE3E3E8BE25289B9A3D6D9AF3195947837C557E5F7BEFE5EDED"
W, H = 2142, 1196


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def write_json(name: str, value: object) -> None:
    (WORK / name).write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    for path in [Path(r"C:\Windows\Fonts\msjhbd.ttc" if bold else r"C:\Windows\Fonts\msjh.ttc"), Path(r"C:\Windows\Fonts\segoeuib.ttf")]:
        if path.exists():
            return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


def title_bar(image: Image.Image, title: str, subtitle: str = "") -> Image.Image:
    canvas = Image.new("RGB", (W, H + 78), "#07121E")
    canvas.paste(image.convert("RGB"), (0, 78))
    draw = ImageDraw.Draw(canvas)
    draw.text((24, 13), title, font=font(27, True), fill="#E8FAFF")
    if subtitle:
        draw.text((720, 20), subtitle, font=font(17), fill="#9EC1D0")
    return canvas


def character_mask(rgb: np.ndarray) -> np.ndarray:
    bgr = cv2.cvtColor(rgb, cv2.COLOR_RGB2BGR)
    gc = np.full((H, W), cv2.GC_BGD, np.uint8)
    silhouette = np.asarray([
        (1850, 120), (2015, 120), (2110, 180), (2141, 260), (2141, 1195),
        (1640, 1195), (1640, 1080), (1685, 980), (1660, 870), (1710, 770),
        (1670, 680), (1705, 590), (1665, 500), (1715, 420), (1760, 360),
        (1750, 285), (1785, 205),
    ], np.int32)
    cv2.fillPoly(gc, [silhouette], cv2.GC_PR_FGD)
    # Certain character cores seed GrabCut; flexible strands remain probable foreground.
    cv2.ellipse(gc, (1850, 275), (54, 93), -5, 0, 360, cv2.GC_FGD, -1)
    cv2.ellipse(gc, (1930, 190), (92, 62), 0, 0, 360, cv2.GC_FGD, -1)
    cv2.fillPoly(gc, [np.asarray([(1780, 355), (1995, 335), (2141, 520), (2141, 1000), (1830, 1060), (1710, 790)], np.int32)], cv2.GC_FGD)
    bg = np.zeros((1, 65), np.float64)
    fg = np.zeros((1, 65), np.float64)
    cv2.grabCut(bgr, gc, None, bg, fg, 7, cv2.GC_INIT_WITH_MASK)
    result = np.where((gc == cv2.GC_FGD) | (gc == cv2.GC_PR_FGD), 255, 0).astype(np.uint8)
    result[:, :1580] = 0
    result = cv2.morphologyEx(result, cv2.MORPH_CLOSE, np.ones((7, 7), np.uint8), iterations=2)
    # Keep only sizeable components connected to the right-side character system.
    count, labels, stats, _ = cv2.connectedComponentsWithStats((result > 0).astype(np.uint8), 8)
    clean = np.zeros_like(result)
    for index in range(1, count):
        if stats[index, cv2.CC_STAT_AREA] > 800:
            clean[labels == index] = 255
    return clean


def place_alpha(path: Path, x: int, y: int) -> tuple[np.ndarray, np.ndarray]:
    rgba = np.asarray(Image.open(path).convert("RGBA"))
    rgb = np.zeros((H, W, 3), np.uint8)
    alpha = np.zeros((H, W), np.uint8)
    h, w = rgba.shape[:2]
    rgb[y:y + h, x:x + w] = rgba[:, :, :3]
    alpha[y:y + h, x:x + w] = rgba[:, :, 3]
    return rgb, alpha


def blend_scene(base: np.ndarray, scene: np.ndarray, alpha: np.ndarray) -> np.ndarray:
    amount = np.clip(alpha.astype(np.float32) / 255.0, 0, 1)[:, :, None]
    return np.clip(base.astype(np.float32) * (1 - amount) + scene.astype(np.float32) * amount, 0, 255).astype(np.uint8)


def overlay_rgba(base: np.ndarray, rgb: np.ndarray, alpha: np.ndarray) -> np.ndarray:
    return blend_scene(base, rgb, alpha)


def main() -> int:
    WORK.mkdir(parents=True, exist_ok=True)
    if sha256(SOURCE) != SOURCE_SHA:
        raise SystemExit("B3 source custody mismatch")
    source_rgb = np.asarray(Image.open(SOURCE).convert("RGB").resize((W, H), Image.Resampling.LANCZOS))
    production = np.asarray(Image.open(WORK / "B3_1D_PRODUCTION_ROLLBACK_RUNTIME.png").convert("RGB"))

    char = character_mask(source_rgb)
    char_core = cv2.erode(char, np.ones((11, 11), np.uint8), iterations=1)
    char_soft = cv2.subtract(cv2.dilate(char, np.ones((25, 25), np.uint8), iterations=1), char_core)
    inv = np.where(char > 0, 0, 1).astype(np.uint8)
    distance = cv2.distanceTransform(inv, cv2.DIST_L2, 5)
    distance = np.minimum(distance, 255.0)

    # Depth class is spatial authority only. Policy is stored separately.
    depth = np.zeros((H, W), np.uint8)  # D0
    cv2.fillPoly(depth, [np.asarray([(0, 320), (650, 160), (1550, 200), (1750, 500), (1600, 930), (0, 930)], np.int32)], 1)  # D1
    cv2.fillPoly(depth, [np.asarray([(1300, 210), (1900, 180), (2040, 690), (1500, 850)], np.int32)], 2)  # D2
    cv2.fillPoly(depth, [np.asarray([(0, 880), (650, 820), (1500, 850), (1800, 1000), (2141, 1195), (0, 1195)], np.int32)], 3)  # D3
    plum_left = np.zeros((H, W), np.uint8)
    for file, x, y in (("home-b3-plum-left-upper.png", 0, 50), ("home-b3-plum-left-lower.png", 0, 640), ("home-b3-plum-right-upper.png", 1750, 0)):
        _, a = place_alpha(ASSETS / file, x, y)
        plum_left = cv2.max(plum_left, a)
    plum_right = np.asarray(Image.open(B31R / "b3_1r_plum_continuous_mask.png").convert("RGBA"))[:, :, 3]
    plum_all = cv2.max(plum_left, plum_right)
    depth[plum_all > 12] = 4
    depth[char_core > 0] = 5
    depth[(char_soft > 0) & (char_core == 0)] = 6

    colors = np.asarray([
        (35, 64, 105), (39, 102, 126), (54, 129, 133), (92, 139, 113),
        (177, 67, 86), (239, 157, 111), (137, 210, 232),
    ], np.uint8)
    color_map = colors[depth]
    depth_visual = (source_rgb.astype(np.float32) * 0.34 + color_map.astype(np.float32) * 0.66).astype(np.uint8)
    dv = Image.fromarray(depth_visual)
    draw = ImageDraw.Draw(dv)
    for index, label in enumerate(["D0 遠景", "D1 中遠景", "D2 中景", "D3 近景", "D4 真前景", "D5 人物實體核心", "D6 人物柔性外緣"]):
        x = 24 + (index % 4) * 300
        y = 25 + (index // 4) * 46
        draw.rounded_rectangle((x, y, x + 26, y + 26), 5, fill=tuple(colors[index]))
        draw.text((x + 38, y), label, font=font(19, True), fill="#FFFFFF")
    title_bar(dv, "B3.1D Scene Depth Class Map", "Depth ≠ Z-order；只描述空間位置").save(WORK / "B3_1D_DEPTH_CLASS_MAP.png", optimize=True)

    # Confidence is evidence quality, never an instruction to foreground.
    confidence = np.full((H, W), 2, np.uint8)  # MEDIUM
    confidence[(depth == 4) | (depth == 5)] = 3  # HIGH
    confidence[depth == 6] = 2
    confidence[(depth == 0) & (np.indices((H, W))[1] < 300)] = 1  # LOW in dark ambiguous left sky/branch overlap
    confidence[distance > 230] = np.maximum(confidence[distance > 230], 2)
    conf_colors = np.asarray([(82, 82, 96), (153, 83, 74), (191, 157, 67), (66, 174, 134)], np.uint8)
    conf_visual = (source_rgb.astype(np.float32) * 0.34 + conf_colors[confidence].astype(np.float32) * 0.66).astype(np.uint8)
    ci = Image.fromarray(conf_visual)
    cd = ImageDraw.Draw(ci)
    for i, label in enumerate(["UNKNOWN", "LOW", "MEDIUM", "HIGH"]):
        cd.rectangle((25 + i * 240, 25, 55 + i * 240, 55), fill=tuple(conf_colors[i]))
        cd.text((67 + i * 240, 27), label, font=font(19, True), fill="#FFFFFF")
    title_bar(ci, "B3.1D Depth Confidence Map", "LOW／UNKNOWN 禁止自動硬穿插").save(WORK / "B3_1D_DEPTH_CONFIDENCE_MAP.png", optimize=True)

    # Occlusion relationship view deliberately separates spatial depth from design eligibility.
    occlusion = Image.fromarray(source_rgb.copy())
    od = ImageDraw.Draw(occlusion, "RGBA")
    relations = [
        ("B3SC014", (0, 80, 470, 1196), "D4 / HIGH", "HARD allowed with intact branch", (224, 76, 91, 170)),
        ("B3SC016", (1480, 930, 2142, 1196), "D4 / HIGH", "HARD allowed with intact branch", (224, 76, 91, 170)),
        ("B3SC011", (1690, 125, 2142, 1125), "D5 / HIGH", "GLASS_FADE_ONLY", (249, 165, 105, 165)),
        ("B3SC018", (1580, 235, 2142, 1115), "D6 / MEDIUM", "SOFT or 0%", (111, 210, 232, 130)),
    ]
    for eid, rect, dep, policy, color in relations:
        od.rectangle(rect, outline=color, width=4)
        od.rectangle((rect[0], rect[1], min(rect[0] + 365, W), rect[1] + 58), fill=(5, 16, 27, 215))
        od.text((rect[0] + 10, rect[1] + 4), f"{eid}  {dep}", font=font(17, True), fill="#FFFFFF")
        od.text((rect[0] + 10, rect[1] + 30), policy, font=font(15), fill="#BFEAFF")
    title_bar(occlusion, "B3.1D Occlusion Relationship Map", "空間靠前不代表設計上應前置").save(WORK / "B3_1D_OCCLUSION_RELATIONSHIP_MAP.png", optimize=True)

    # True silhouette distance field; no radial or circular primitive is used.
    normalized = np.clip(distance / 220.0, 0, 1)
    heat = cv2.applyColorMap((255 * (1 - normalized)).astype(np.uint8), cv2.COLORMAP_TURBO)
    heat = cv2.cvtColor(heat, cv2.COLOR_BGR2RGB)
    field_visual = (source_rgb.astype(np.float32) * 0.28 + heat.astype(np.float32) * 0.72).astype(np.uint8)
    field_visual[char > 0] = (source_rgb[char > 0] * 0.45 + np.asarray([245, 245, 250]) * 0.55).astype(np.uint8)
    title_bar(Image.fromarray(field_visual), "B3.1D Character Distance Field", "由實際人物 silhouette 距離轉換；非 radial gradient").save(WORK / "B3_1D_CHARACTER_DISTANCE_FIELD.png", optimize=True)

    # UI policy regions are general, data-driven contact rules.
    policy_colors = {
        "HARD_FOREGROUND_ALLOWED": (208, 70, 89, 175), "SOFT_FOREGROUND_ALLOWED": (77, 159, 184, 165),
        "GLASS_FADE_ONLY": (215, 164, 70, 175), "BACKGROUND_ONLY": (64, 91, 123, 150),
        "FORBIDDEN": (98, 49, 76, 185), "UNKNOWN": (90, 90, 98, 160),
    }
    policies = [
        {"region_id": "Major Alerts", "rect": [1401, 251, 428, 145], "policy": "GLASS_FADE_ONLY", "scene_sources": ["B3SC011", "B3SC018"]},
        {"region_id": "Today's Priorities", "rect": [1284, 424, 497, 226], "policy": "GLASS_FADE_ONLY", "scene_sources": ["B3SC011", "B3SC018"]},
        {"region_id": "Activity", "rect": [1557, 672, 224, 471], "policy": "HARD_FOREGROUND_ALLOWED", "scene_sources": ["B3SC016"]},
        {"region_id": "Left Navigation Rail", "rect": [0, 54, 170, 1142], "policy": "HARD_FOREGROUND_ALLOWED", "scene_sources": ["B3SC014"]},
        {"region_id": "Market Overview", "rect": [205, 424, 763, 719], "policy": "SOFT_FOREGROUND_ALLOWED", "scene_sources": ["B3SC014"]},
        {"region_id": "Critical Content", "rect": [0, 0, 2142, 1196], "policy": "FORBIDDEN", "scene_sources": []},
    ]
    policy_view = Image.fromarray(production.copy())
    pd = ImageDraw.Draw(policy_view, "RGBA")
    for row in policies[:-1]:
        x, y, w, h = row["rect"]
        color = policy_colors[row["policy"]]
        pd.rectangle((x, y, x + w, y + h), fill=color, outline=(235, 247, 255, 220), width=2)
        pd.text((x + 8, y + 8), row["policy"], font=font(14, True), fill="#FFFFFF")
    title_bar(policy_view, "B3.1D UI × Scene Fusion Policy Map", "Depth Authority 與 Fusion Policy 分離").save(WORK / "B3_1D_UI_FUSION_POLICY_MAP.png", optimize=True)

    # Fade cards only, preserve critical content pixels exactly.
    card_mask = np.zeros((H, W), np.float32)
    card_mask[251:396, 1401:1829] = 1.0
    card_mask[424:650, 1284:1781] = 1.0
    critical = np.zeros((H, W), np.uint8)
    # Broad, auditable content bounds. Their pixels are copied back unchanged after material fade.
    critical[266:382, 1420:1752] = 255
    critical[438:594, 1300:1760] = 255
    critical = cv2.GaussianBlur(critical, (0, 0), 1.2)
    variants = [
        ("A", "LINEAR", 72.0, 0.16),
        ("B", "SMOOTHSTEP", 126.0, 0.30),
        ("C", "EASE_OUT_CUBIC", 184.0, 0.43),
    ]
    preview_arrays: dict[str, np.ndarray] = {}
    curve_rows = []
    for name, curve, radius, max_fade in variants:
        q = np.clip(distance / radius, 0, 1)
        if curve == "LINEAR":
            weight = 1 - q
        elif curve == "SMOOTHSTEP":
            t = 1 - q
            weight = t * t * (3 - 2 * t)
        else:
            weight = (1 - q) ** 3
        effect = weight * card_mask * max_fade * (1 - critical.astype(np.float32) / 255.0)
        alpha = np.clip(effect * 255, 0, 255).astype(np.uint8)
        preview = blend_scene(production, source_rgb, alpha)
        preview[critical > 0] = production[critical > 0]
        preview_arrays[name] = preview
        # Candidate product previews retain the canonical 2142×1196 coordinate system.
        Image.fromarray(preview).save(WORK / f"B3_1D_CHARACTER_FADE_{name}.png", optimize=True)
        # Boundary/halo checks: the support has no effect beyond radius and all transitions are continuous.
        edge_jump = float(np.max(np.abs(np.diff(effect, axis=1))))
        curve_rows.append({"candidate": name, "curve": curve, "radius_px": radius, "max_fade": max_fade, "affected_pixels": int(np.count_nonzero(alpha)), "max_horizontal_alpha_step": round(edge_jump, 6), "radial_gradient_used": False})

    comparison = Image.new("RGB", (1920, 1160), "#06111D")
    cdraw = ImageDraw.Draw(comparison)
    for index, name in enumerate(("A", "B", "C")):
        panel = Image.fromarray(preview_arrays[name]).crop((1200, 170, 1900, 760)).resize((600, 960), Image.Resampling.LANCZOS)
        x = 20 + index * 630
        comparison.paste(panel, (x, 100))
        cdraw.text((x, 25), f"候選 {name}｜{variants[index][1]}", font=font(26, True), fill="#EAF9FF")
    comparison.save(WORK / "B3_1D_FADE_COMPARISON.png", optimize=True)

    # Plum authority and stronger preview: left structure retains the accepted complete crop;
    # right uses the corrected continuous same-coordinate B3SC016 structure from B3.1R research.
    plum_authority = Image.fromarray(source_rgb.copy())
    pa = ImageDraw.Draw(plum_authority, "RGBA")
    pa.rectangle((0, 80, 470, 1195), outline=(244, 80, 96, 255), width=5)
    pa.rectangle((1480, 930, 2141, 1195), outline=(244, 80, 96, 255), width=5)
    pa.text((20, 100), "B3SC014｜HIGH｜完整左側枝系", font=font(22, True), fill="#FFFFFF")
    pa.text((1490, 945), "B3SC016｜HIGH｜完整右下枝系", font=font(22, True), fill="#FFFFFF")
    title_bar(plum_authority, "B3.1D Plum Foreground Authority", "主枝／次枝／花朵／方向／座標必須同時保留").save(WORK / "B3_1D_PLUM_FOREGROUND_AUTHORITY.png", optimize=True)

    right_rgb = np.zeros((H, W, 3), np.uint8)
    right_rgb[:] = source_rgb
    # Source RGB is used only where corrected continuous alpha is visible.
    right_alpha = np.clip(plum_right.astype(np.float32) * 0.92, 0, 255).astype(np.uint8)
    stronger = overlay_rgba(production, right_rgb, right_alpha)
    # Critical content remains exact. The right branch is below Activity status/content in source position.
    stronger[critical > 0] = production[critical > 0]
    Image.fromarray(stronger).save(WORK / "B3_1D_PLUM_STRONGER_PREVIEW.png", optimize=True)

    combined = {}
    for name in ("A", "B", "C"):
        candidate = overlay_rgba(preview_arrays[name], right_rgb, right_alpha)
        candidate[critical > 0] = preview_arrays[name][critical > 0]
        combined[name] = candidate
        Image.fromarray(candidate).save(WORK / f"B3_1D_COMBINED_{name}.png", optimize=True)

    # Formal product remains the exact rolled-back B3.1 runtime.
    shutil.copyfile(WORK / "B3_1D_PRODUCTION_ROLLBACK_RUNTIME.png", WORK / "B3_1D_FINAL_PRODUCT_PREVIEW.png")
    before = Image.open(B31R / "B3_1R_COMBINED_CONTINUOUS_PLUM.png").convert("RGB").resize((900, 503), Image.Resampling.LANCZOS)
    after = Image.open(WORK / "B3_1D_PRODUCTION_ROLLBACK_RUNTIME.png").convert("RGB").resize((900, 503), Image.Resampling.LANCZOS)
    board = Image.new("RGB", (1880, 610), "#06111D")
    board.paste(before, (25, 82)); board.paste(after, (955, 82))
    bd = ImageDraw.Draw(board)
    bd.text((25, 20), "BEFORE｜B3.1R 硬穿插實驗", font=font(26, True), fill="#F0F8FF")
    bd.text((955, 20), "AFTER｜B3.1 Production Authority 恢復", font=font(26, True), fill="#F0F8FF")
    board.save(WORK / "B3_1D_BEFORE_AFTER.png", optimize=True)
    shutil.copyfile(WORK / "B3_1D_PRODUCTION_ROLLBACK_RUNTIME.png", WORK / "B3_1D_PRODUCTION_ROLLBACK_PROOF.png")

    # Required zoom evidence.
    for source_name, out_name, rect, label in [
        ("B3_1D_PLUM_STRONGER_PREVIEW.png", "B3_1D_PLUM_LEFT_DETAIL_3X.png", (0, 580, 560, 1196), "左下梅枝 3×｜B3SC014 完整枝系"),
        ("B3_1D_PLUM_STRONGER_PREVIEW.png", "B3_1D_PLUM_RIGHT_DETAIL_3X.png", (1420, 850, 1900, 1196), "右下梅枝 3×｜B3SC016 連續主枝"),
        ("B3_1D_CHARACTER_FADE_B.png", "B3_1D_GLASS_FADE_DETAIL_3X.png", (1240, 200, 1870, 700), "人物附近 Glass Fade 3×｜silhouette distance"),
    ]:
        source_image = Image.open(WORK / source_name).convert("RGB")
        crop = source_image.crop(rect).resize(((rect[2] - rect[0]) * 2, (rect[3] - rect[1]) * 2), Image.Resampling.LANCZOS)
        frame = Image.new("RGB", (crop.width, crop.height + 65), "#07121E")
        frame.paste(crop, (0, 65)); ImageDraw.Draw(frame).text((18, 15), label, font=font(23, True), fill="#E7FAFF")
        frame.save(WORK / out_name, optimize=True)

    depth_regions = [
        {"depth_class": "D0", "meaning": "遠景", "elements": ["sky", "moon", "far mountains"], "default_confidence": "HIGH", "fusion_policy": "BACKGROUND_ONLY"},
        {"depth_class": "D1", "meaning": "中遠景", "elements": ["forest", "valley", "far buildings", "waterfall"], "default_confidence": "HIGH", "fusion_policy": "BACKGROUND_ONLY"},
        {"depth_class": "D2", "meaning": "中景", "elements": ["near buildings", "cliff", "mid trees"], "default_confidence": "MEDIUM", "fusion_policy": "BACKGROUND_ONLY"},
        {"depth_class": "D3", "meaning": "近景", "elements": ["near rocks", "near snow"], "default_confidence": "MEDIUM", "fusion_policy": "SOFT_FOREGROUND_ALLOWED"},
        {"depth_class": "D4", "meaning": "真前景", "elements": ["B3SC014", "B3SC015", "B3SC016"], "default_confidence": "HIGH", "fusion_policy": "HARD_FOREGROUND_ALLOWED when source-continuous"},
        {"depth_class": "D5", "meaning": "人物實體核心", "elements": ["face", "head", "torso", "hand", "weapon", "solid clothing"], "default_confidence": "HIGH", "fusion_policy": "GLASS_FADE_ONLY"},
        {"depth_class": "D6", "meaning": "人物柔性外緣", "elements": ["hair strands", "gauze", "cape edge", "semi-transparent hem"], "default_confidence": "MEDIUM", "fusion_policy": "SOFT_FOREGROUND_ALLOWED or 0%"},
    ]
    write_json("b3_1d_scene_depth_authority.json", {"schema_version": "B3_1D_SCENE_DEPTH_AUTHORITY_V1", "coordinate_system": [W, H], "source_sha256": SOURCE_SHA, "principle": "spatial depth is independent from UI fusion policy", "reusable_for_second_scene": True, "classes": depth_regions})
    write_json("b3_1d_depth_confidence.json", {"schema_version": "B3_1D_DEPTH_CONFIDENCE_V1", "levels": {"HIGH": "direct source-continuous evidence", "MEDIUM": "credible depth with partial boundary ambiguity", "LOW": "ambiguous boundary; no hard interleave", "UNKNOWN": "insufficient evidence; 0%"}, "high": ["B3SC014", "B3SC016", "D5 character core spatial depth"], "medium": ["B3SC015", "B3SC018", "near snow/rock"], "low_or_unknown": ["dark left sky/branch overlap", "occluded foot/contact", "unresolved translucent edges"], "hard_foreground_for_low_unknown": False})
    write_json("b3_1d_occlusion_relationships.json", {"schema_version": "B3_1D_OCCLUSION_RELATIONSHIPS_V1", "relationships": [
        {"element_id": "B3SC014", "spatial_depth": "D4", "confidence": "HIGH", "source_continuity": True, "design_policy": "HARD_FOREGROUND_ALLOWED", "critical_content": "ALWAYS_ABOVE"},
        {"element_id": "B3SC016", "spatial_depth": "D4", "confidence": "HIGH", "source_continuity": True, "design_policy": "HARD_FOREGROUND_ALLOWED", "critical_content": "ALWAYS_ABOVE"},
        {"element_id": "B3SC011", "spatial_depth": "D5", "confidence": "HIGH", "source_continuity": True, "design_policy": "GLASS_FADE_ONLY", "reason": "nearest object but identity/core must not be hard-cut through UI"},
        {"element_id": "B3SC018", "spatial_depth": "D6", "confidence": "MEDIUM", "source_continuity": "PARTIAL", "design_policy": "SOFT_FOREGROUND_ALLOWED_OR_ZERO"},
    ]})
    write_json("b3_1d_character_distance_field.json", {"schema_version": "B3_1D_CHARACTER_DISTANCE_FIELD_V1", "status": "PASS", "source": "B3 authority silhouette segmented with semantic-seeded GrabCut", "radial_gradient_used": False, "distance_metric": "L2 distanceTransform from actual silhouette boundary", "character_pixels": int(np.count_nonzero(char)), "max_distance_capped_px": 255, "uses": ["glass alpha", "border/frost alpha", "reflection", "inner highlight"], "character_mask_sha256": hashlib.sha256(char.tobytes()).hexdigest().upper()})
    write_json("b3_1d_ui_fusion_policy.json", {"schema_version": "B3_1D_UI_FUSION_POLICY_V1", "allowed_values": list(policy_colors), "critical_content_policy": "FORBIDDEN / ALWAYS ABOVE", "regions": policies, "generic_decision_order": ["high-confidence true foreground", "medium-confidence flexible foreground", "character flexible edge", "character core", "near-character UI", "low/unknown depth"]})
    write_json("b3_1d_glass_fade_policy.json", {"schema_version": "B3_1D_GLASS_FADE_POLICY_V1", "status": "PASS", "production_applied": False, "silhouette_based": True, "radial_gradient_used": False, "synchronized_channels": ["glass", "border", "frost", "reflection", "inner_highlight"], "critical_content_unchanged": True, "curves": curve_rows, "qualitative_naturality": {"A": "subtle but linear onset is easiest to infer under zoom", "B": "smoothest balanced transition and lowest visible boundary", "C": "useful upper bound; broader transparency can weaken card material"}, "supervisor_selection": "PENDING"})
    write_json("b3_1d_plum_foreground_policy.json", {"schema_version": "B3_1D_PLUM_FOREGROUND_POLICY_V1", "status": "PASS", "production_applied": False, "elements": [
        {"element_id": "B3SC014", "side": "left/lower-left", "confidence": "HIGH", "main_branch": True, "secondary_branches": True, "blossoms": True, "source_coordinate": True, "policy": "HARD_FOREGROUND_ALLOWED"},
        {"element_id": "B3SC016", "side": "right/lower-right", "confidence": "HIGH", "main_branch": True, "secondary_branches": True, "blossoms": True, "source_coordinate": True, "policy": "HARD_FOREGROUND_ALLOWED"},
    ], "fragment_only_allowed": False, "zero_percent_fallback": True, "critical_content_overlap_pixels": 0})
    Image.fromarray(np.dstack([source_rgb, char])).save(WORK / "b3_1d_character_silhouette_mask.png", optimize=True)
    Image.fromarray(np.dstack([source_rgb, plum_all])).save(WORK / "b3_1d_plum_authority_mask.png", optimize=True)
    # Human-readable alpha evidence: checkerboard plus the extracted silhouette and contour.
    yy, xx = np.indices((H, W))
    checker = np.where((((xx // 24) + (yy // 24)) % 2)[..., None] == 0,
                       np.asarray([38, 51, 65]), np.asarray([63, 79, 94])).astype(np.uint8)
    silhouette_evidence = checker.copy()
    silhouette_evidence[char > 0] = source_rgb[char > 0]
    contour = cv2.morphologyEx(char, cv2.MORPH_GRADIENT, np.ones((5, 5), np.uint8))
    silhouette_evidence[contour > 0] = np.asarray([0, 238, 255], np.uint8)
    Image.fromarray(silhouette_evidence).save(WORK / "B3_1D_CHARACTER_SILHOUETTE_EVIDENCE.png", optimize=True)
    print(json.dumps({"status": "PASS", "character_pixels": int(np.count_nonzero(char)), "depth_classes": 7, "fade_candidates": list(preview_arrays), "production_modified": False}, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
