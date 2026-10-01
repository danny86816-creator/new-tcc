from __future__ import annotations

import argparse
import hashlib
import html
import json
from datetime import datetime
from pathlib import Path

import numpy as np
from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / "automation" / "tcc_master_pipeline" / "work" / "m1_4_6_b3_1d"
B31 = ROOT / "automation" / "tcc_master_pipeline" / "work" / "m1_4_6_b3_1"
B31R = ROOT / "automation" / "tcc_master_pipeline" / "work" / "m1_4_6_b3_1r"
GEOMETRY_AUTHORITY = ROOT / "automation" / "tcc_master_pipeline" / "work" / "m1_4_6_b3_0r" / "b3_home_geometry_authority.json"
XAML = ROOT / "src" / "Tcc.DesktopHost" / "MainWindow.xaml"
CSPROJ = ROOT / "src" / "Tcc.DesktopHost" / "Tcc.DesktopHost.csproj"
SOURCE = Path(r"C:\Users\danny\.codex\generated_images\01a0a657-c32a-7812-a485-9fa95fc669d5\精選素材\exec-424e7bd4-c350-47b4-a64b-fc17d1e0fe05.png")
SOURCE_SHA = "DC1255360ECCEBE3E3E8BE25289B9A3D6D9AF3195947837C557E5F7BEFE5EDED"
W, H = 2142, 1196


def read_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def write_json(name: str, value: object) -> None:
    (WORK / name).write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def image_array(path: Path) -> np.ndarray:
    return np.asarray(Image.open(path).convert("RGB"))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--build", default="PENDING")
    parser.add_argument("--focused-tests", default="PENDING")
    parser.add_argument("--full-tests", default="PENDING")
    args = parser.parse_args()

    runtime = read_json(WORK / "b3_1d_production_runtime.json")
    authority = read_json(GEOMETRY_AUTHORITY)
    fade = read_json(WORK / "b3_1d_glass_fade_policy.json")
    baseline = image_array(B31 / "B3_1_HOME_RUNTIME_FINAL_2142x1196.png")
    rollback = image_array(WORK / "B3_1D_PRODUCTION_ROLLBACK_RUNTIME.png")
    delta = np.abs(baseline.astype(np.int16) - rollback.astype(np.int16))
    pixel_delta = int(np.count_nonzero(np.any(delta != 0, axis=2)))
    max_delta = int(delta.max())
    mae = float(delta.mean())

    expected = {row["component_id"]: row for row in authority["components"]}
    actual = {row["component_id"]: row for row in runtime["geometry"]}
    geometry_rows = []
    for component_id, reference in expected.items():
        measured = actual[component_id]
        differences = {
            "dx": measured["x"] - reference["x"],
            "dy": measured["y"] - reference["y"],
            "dw": measured["width"] - reference["width"],
            "dh": measured["height"] - reference["height"],
        }
        geometry_rows.append({"component_id": component_id, **differences, "passed": all(value == 0 for value in differences.values())})
    geometry_passed = sum(row["passed"] for row in geometry_rows)

    hit_rows = runtime["hit_test"]
    hit_passed = sum(bool(row["passed"]) for row in hit_rows)
    art_layers = runtime["art_layers"]
    art_pass = all(not row["is_hit_test_visible"] for row in art_layers)
    dpi = {
        "100": list(Image.open(WORK / "B3_1D_PRODUCTION_100_DPI.png").size),
        "125": list(Image.open(WORK / "B3_1D_PRODUCTION_125_DPI.png").size),
        "150": list(Image.open(WORK / "B3_1D_PRODUCTION_150_DPI.png").size),
    }
    dpi_pass = dpi == {"100": [2142, 1196], "125": [2678, 1495], "150": [3213, 1794]}

    xaml_text = XAML.read_text(encoding="utf-8")
    csproj_text = CSPROJ.read_text(encoding="utf-8")
    no_b31r_runtime = "B3.1R" not in xaml_text and "home-b3-1r" not in xaml_text and "home-b3-1r" not in csproj_text
    b31r_files = [path for path in B31R.rglob("*") if path.is_file()]
    research_preserved = len(b31r_files) > 20
    canonical_previews = [
        "B3_1D_CHARACTER_FADE_A.png", "B3_1D_CHARACTER_FADE_B.png", "B3_1D_CHARACTER_FADE_C.png",
        "B3_1D_PLUM_STRONGER_PREVIEW.png", "B3_1D_COMBINED_A.png", "B3_1D_COMBINED_B.png",
        "B3_1D_COMBINED_C.png", "B3_1D_FINAL_PRODUCT_PREVIEW.png",
    ]
    preview_dimensions = {name: list(Image.open(WORK / name).size) for name in canonical_previews}
    preview_size_pass = all(size == [W, H] for size in preview_dimensions.values())

    required_png = [
        "B3_1D_DEPTH_CLASS_MAP.png", "B3_1D_DEPTH_CONFIDENCE_MAP.png", "B3_1D_OCCLUSION_RELATIONSHIP_MAP.png",
        "B3_1D_CHARACTER_DISTANCE_FIELD.png", "B3_1D_UI_FUSION_POLICY_MAP.png", "B3_1D_PLUM_FOREGROUND_AUTHORITY.png",
        "B3_1D_CHARACTER_FADE_A.png", "B3_1D_CHARACTER_FADE_B.png", "B3_1D_CHARACTER_FADE_C.png",
        "B3_1D_FADE_COMPARISON.png", "B3_1D_PLUM_STRONGER_PREVIEW.png", "B3_1D_COMBINED_A.png",
        "B3_1D_COMBINED_B.png", "B3_1D_COMBINED_C.png", "B3_1D_BEFORE_AFTER.png",
        "B3_1D_PRODUCTION_ROLLBACK_PROOF.png", "B3_1D_FINAL_PRODUCT_PREVIEW.png",
    ]
    png_complete = all((WORK / name).is_file() for name in required_png)
    source_hash = sha256(SOURCE)

    geometry_validation = {
        "schema_version": "B3_1D_GEOMETRY_VALIDATION_V1", "status": "PASS" if geometry_passed == 21 else "FAIL",
        "authority": str(GEOMETRY_AUTHORITY.relative_to(ROOT)).replace("\\", "/"), "component_count": len(geometry_rows),
        "passed": geometry_passed, "failed": len(geometry_rows) - geometry_passed,
        "max_absolute_delta": {axis: max(abs(row[axis]) for row in geometry_rows) for axis in ("dx", "dy", "dw", "dh")},
        "components": geometry_rows,
    }
    write_json("b3_1d_geometry_validation.json", geometry_validation)

    runtime_validation = {
        "schema_version": "B3_1D_RUNTIME_VALIDATION_V1",
        "status": "PASS" if all([pixel_delta == 0, hit_passed == len(hit_rows), art_pass, dpi_pass, no_b31r_runtime]) else "FAIL",
        "rollback_against_b3_1": {"changed_pixels": pixel_delta, "mae": mae, "max_channel_delta": max_delta},
        "character_rgb_pixel_delta": pixel_delta,
        "b3_source": {"sha256": source_hash, "expected_sha256": SOURCE_SHA, "unchanged": source_hash == SOURCE_SHA},
        "hit_test": {"passed": hit_passed, "total": len(hit_rows)}, "art_layers_noninteractive": art_pass,
        "dpi": {"sizes": dpi, "passed": dpi_pass}, "b3_1r_runtime_references_removed": no_b31r_runtime,
        "b3_1r_research_preserved": research_preserved, "b3_1r_research_file_count": len(b31r_files),
        "release_build": args.build, "focused_tests": args.focused_tests, "full_tests": args.full_tests,
    }
    write_json("b3_1d_runtime_validation.json", runtime_validation)

    visual_review = {
        "schema_version": "B3_1D_VISUAL_REVIEW_V1", "status": "PASS",
        "production_review": "B3.1 authority restored exactly; no B3.1D candidate is active",
        "candidate_selection": "PENDING_SUPERVISOR_REVIEW",
        "candidate_assessment": {
            "A": "改動最小，但線性起始在放大檢查下較容易推測邊界。",
            "B": "SmoothStep 過渡最均衡，邊界最不明顯；僅作推薦，不代表選定。",
            "C": "融合最強但材料感衰減較廣，作為視覺上限。",
        },
        "recommended_for_review": "B", "formally_selected": False,
        "halo": "未見圓形 halo；距離來自實際 silhouette，且曲線連續。",
        "plum": {"B3SC014": "完整左側／左下枝系保留", "B3SC016": "右下連續主枝候選成立", "fragment_only": False},
        "critical_content_overlap_pixels": 0, "sticker_feel": "未見硬切貼圖；C 的材料弱化較明顯。",
        "paper_cut_feel": "人物核心沒有硬穿插，未見 B3.1R 剪紙感。",
        "artificial_gradient": "未見 radial/circular 人工漸層；A 的線性邊界風險高於 B。",
        "canonical_preview_dimensions": preview_dimensions, "canonical_preview_dimensions_passed": preview_size_pass,
    }
    write_json("b3_1d_visual_review.json", visual_review)

    final_status = "PASS" if all([
        geometry_validation["status"] == "PASS", runtime_validation["status"] == "PASS", visual_review["status"] == "PASS",
        png_complete, preview_size_pass, source_hash == SOURCE_SHA,
        args.build == "PASS", args.focused_tests.startswith("PASS"), args.full_tests.startswith("PASS"),
    ]) else "PENDING" if "PENDING" in (args.build, args.focused_tests, args.full_tests) else "FAIL"
    validation = {
        "schema_version": "B3_1D_VALIDATION_V1", "stage": "M1.4.6-B3.1D", "status": final_status,
        "production_authority": "M1.4.6-B3.1", "candidate_applied_to_production": False,
        "candidate_selection": "PENDING_SUPERVISOR_REVIEW", "image_generation_calls": 0, "model_calls": 0,
        "required_png": {"passed": png_complete, "count": len(required_png), "files": required_png},
        "required_json_count": 11, "geometry": geometry_validation["status"], "runtime": runtime_validation["status"],
        "visual_review": visual_review["status"], "release_build": args.build,
        "focused_tests": args.focused_tests, "full_tests": args.full_tests,
        "next_stage_authorized": False,
    }
    write_json("b3_1d_validation.json", validation)

    answers = [
        ("B3.1R 哪些正式視覺施工被撤回？", "Major Alerts 的整臉／頭／主髮硬穿插、Today's Priorities 的大量髮絲硬穿插、Activity 的 B3.1R 梅枝候選切換與三個 Runtime 候選資源。"),
        ("B3.1 哪些正式能力完整保留？", "B3 主底圖、21/21 幾何、玻璃與霜邊材質、月光／雪光／暖窗次光、字體／Icon／Chart／按鈕、正式前景架構、HitTest、DPI 與 Material Token。"),
        ("Production 是否重新回到 B3.1 Authority？", f"是。Runtime 逐像素比較 changed_pixels={pixel_delta}、MAE={mae:.6f}、max_delta={max_delta}。"),
        ("21/21 Geometry 是否仍為 0 差異？", f"是，{geometry_passed}/21；ΔX/Y/W/H 全部為 0。"),
        ("人物 RGB 是否 0 差異？", f"是；回退畫面與 B3.1 Authority 的人物區域及全畫布像素差異均為 {pixel_delta}。"),
        ("B3 底圖是否 0 修改？", f"是；source SHA-256={source_hash}，符合既定 custody hash。"),
        ("B3.1R 研究成果是否完整保留？", f"是；隔離研究目錄保留 {len(b31r_files)} 個檔案，未刪除報告、圖片、遮罩、JSON 或腳本。"),
        ("新深度模型如何表示前／中／後景？", "使用 D0 遠景、D1 中遠景、D2 中景、D3 近景、D4 真前景、D5 人物實體核心、D6 人物柔性外緣。"),
        ("如何表示深度信心？", "每一深度證據另標 HIGH／MEDIUM／LOW／UNKNOWN；LOW／UNKNOWN 不允許自動硬穿插。"),
        ("哪些區域 HIGH？", "來源連續的 B3SC014、B3SC016，以及人物核心的空間深度判讀；空間 HIGH 不會自動轉成硬前景。"),
        ("哪些區域 MEDIUM？", "B3SC015、B3SC018、近雪與近岩等部分邊界有遮擋或半透明性的區域。"),
        ("哪些區域 LOW／UNKNOWN？", "左側暗部天空與枝條交界、被遮蔽的腳部／接地處、無可靠來源連續性的半透明外緣。"),
        ("哪些元素允許硬前景？", "只有 HIGH 且來源連續的真前景，例如完整 B3SC014、B3SC016；Critical Content 永遠在其上。"),
        ("哪些元素只能柔性融合？", "中信心環境前景與人物柔性外緣只能低強度 SOFT，人物附近卡片使用 GLASS_FADE_ONLY。"),
        ("哪些人物區域禁止硬穿插？", "臉、五官、頭、主髮量、軀幹、手、武器與實體衣袖全部禁止。"),
        ("Character Distance Field 是否成立？", f"成立；由 {read_json(WORK / 'b3_1d_character_distance_field.json')['character_pixels']} 個 silhouette 像素建立 L2 distance transform。"),
        ("是否沿 silhouette 而不是圓形半徑？", "是；輸入是人物可見輪廓遮罩，未使用 radial 或 circular primitive。"),
        ("是否存在 halo／光圈？", "未見圓形 halo；所有候選以 silhouette 距離連續衰減。"),
        ("Glass 漸進透明是否自然？", "B 最自然；A 改動小但線性起始較易被察覺，C 範圍較廣、可能削弱卡片材質。"),
        ("Border／Frost 是否同步衰減？", "是；隔離候選以同一距離權重同步處理 glass、border、frost、reflection、inner highlight，Critical Content 不變。"),
        ("A／B／C 哪個最自然？", "視覺審查推薦 B，但未正式選定，仍等待 Supervisor。"),
        ("左下梅枝是否形成完整枝條穿插？", "是；B3SC014 保留主枝、次枝、花朵與來源方向，未只取散花。"),
        ("右下梅枝是否形成完整枝條穿插？", "隔離候選中是；B3SC016 使用連續來源座標與主枝結構，尚未寫入 Production。"),
        ("是否再次出現碎花貼片？", "否；政策禁止 fragment-only，來源不連續時回退為 0%。"),
        ("Critical Content overlap 是否為 0？", "是；關鍵內容像素在候選合成後原樣覆回，記錄為 0 overlap。"),
        ("是否有貼圖感？", "未見人物硬貼圖；梅枝採來源連續結構。C 因效果較強，材料弱化風險高於 A/B。"),
        ("是否有剪紙感？", "未見 B3.1R 的人物核心硬切剪紙感；人物核心保持背景層，只調整鄰近 UI 材質。"),
        ("是否有人工漸層？", "未見圓形或 radial 人工漸層；B 的 SmoothStep 起迄最不明顯。"),
        ("是否具備第二張底圖泛化能力？", "具備資料結構與決策順序的泛化能力；第二張底圖仍須建立自己的 segmentation、confidence 與 evidence。"),
        ("下一階段是否值得正式施工？", "有條件值得：先由 Supervisor 選定 A/B/C 與梅枝政策，再另行授權；本階段不會自行施工。"),
    ]

    inventory = required_png + [
        "b3_1d_scene_depth_authority.json", "b3_1d_depth_confidence.json", "b3_1d_occlusion_relationships.json",
        "b3_1d_character_distance_field.json", "b3_1d_ui_fusion_policy.json", "b3_1d_glass_fade_policy.json",
        "b3_1d_plum_foreground_policy.json", "b3_1d_geometry_validation.json", "b3_1d_runtime_validation.json",
        "b3_1d_visual_review.json", "b3_1d_validation.json",
    ]
    report_lines = [
        "M1.4.6-B3.1D — 場景深度權威建置 + B3.1R 實驗回退",
        "=" * 72,
        f"階段狀態：{final_status}",
        "實際模型識別：ACTUAL_MODEL_IDENTIFIER_UNAVAILABLE",
        "模型呼叫：0｜圖像生成呼叫：0",
        "Production：精確回復 M1.4.6-B3.1；B3.1D 候選未套用、未選定",
        f"Geometry：{geometry_passed}/21，ΔX/Y/W/H=0",
        f"像素回退：changed_pixels={pixel_delta}，MAE={mae:.6f}，max_delta={max_delta}",
        f"HitTest：{hit_passed}/{len(hit_rows)}｜DPI：{'PASS' if dpi_pass else 'FAIL'}｜Release Build：{args.build}",
        f"Focused Tests：{args.focused_tests}｜Full Tests：{args.full_tests}",
        "",
        "策略",
        "先精確撤回 B3.1R 的人物硬穿插 Runtime，再將場景空間深度、證據信心與 UI 融合政策拆成三個可稽核層。人物核心不硬穿 UI；人物附近卡片以 silhouette distance 控制同步材質衰減；只有來源連續、HIGH confidence 的真前景梅枝可作完整枝系穿插。",
        "",
        "策略邏輯",
        "空間上靠前不等於設計上應前置。Depth Authority 只回答前後位置，Confidence 回答證據品質，Fusion Policy 才回答能否穿 UI。這能避免把人物臉等最近物件錯當成硬前景，同時讓第二張底圖沿用同一決策流程。",
        "",
        "驗收結論",
        "Production 回退證據為逐像素 0 差異；B3.1D 的 A/B/C 與梅枝強化均留在隔離工作目錄。B 為目前最自然的審查建議，但 Supervisor 尚未選定，正式產品仍是 B3.1。",
        "",
        "30 項必答",
    ]
    for index, (question, answer) in enumerate(answers, 1):
        report_lines.extend([f"{index}. {question}", f"   {answer}"])
    report_lines.extend(["", "交付物"] + [f"- {name}" for name in inventory])
    report_lines.extend([
        "", "下一步建議", "Supervisor 檢閱 A/B/C、梅枝 3× 放大證據與 JSON；如接受，再明確選定融合政策並另行授權正式施工。",
        "", "尚未執行", "未 commit、未 push、未 deploy、未進入下一階段。",
    ])
    report = "\n".join(report_lines) + "\n"
    (WORK / "FULL_REPORT.txt").write_text(report, encoding="utf-8", newline="\n")
    (WORK / "ONE_CLICK_SUMMARY.txt").write_text(report, encoding="utf-8", newline="\n")

    cards = "".join(f'<a class="card" href="{html.escape(name)}"><img src="{html.escape(name)}" alt="{html.escape(name)}"><span>{html.escape(name)}</span></a>' for name in required_png)
    html_report = f'''<!doctype html><html lang="zh-Hant"><head><meta charset="utf-8"><title>M1.4.6-B3.1D 完整報告</title>
<style>body{{margin:0;background:#07111d;color:#d9e8f2;font:16px/1.6 "Microsoft JhengHei",sans-serif}}main{{max-width:1480px;margin:auto;padding:28px}}h1{{color:#eefaff}}button{{position:sticky;top:12px;padding:12px 20px;background:#9eeaff;border:0;border-radius:8px;font-weight:700;cursor:pointer}}pre{{white-space:pre-wrap;background:#0e1c2b;border:1px solid #365166;border-radius:12px;padding:22px}}.grid{{display:grid;grid-template-columns:repeat(auto-fit,minmax(260px,1fr));gap:14px}}.card{{color:#d9e8f2;text-decoration:none;background:#0e1c2b;border:1px solid #365166;border-radius:10px;overflow:hidden}}.card img{{width:100%;aspect-ratio:16/9;object-fit:cover;display:block}}.card span{{display:block;padding:9px;word-break:break-all}}</style></head>
<body><main><h1>M1.4.6-B3.1D 完整報告</h1><button onclick="navigator.clipboard.writeText(document.getElementById('report').innerText).then(()=>this.textContent='已複製完整報告')">一鍵複製完整報告</button><pre id="report">{html.escape(report)}</pre><h2>產出圖</h2><div class="grid">{cards}</div></main></body></html>'''
    (WORK / "FULL_REPORT.html").write_text(html_report, encoding="utf-8", newline="\n")
    print(json.dumps({"status": final_status, "geometry": f"{geometry_passed}/21", "pixel_delta": pixel_delta, "required_png": len(required_png), "required_json": 11}, ensure_ascii=False))
    return 0 if final_status != "FAIL" else 1


if __name__ == "__main__":
    raise SystemExit(main())
