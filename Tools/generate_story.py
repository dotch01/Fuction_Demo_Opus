"""
generate_story.py — Excel → StoryConfig.json 轉換工具

Excel 格式（Sheet1）：
  A: chapter_id      (例如 "chapter_0")
  B: triggerAfterQuest (整數, 0=遊戲開始)
  C: speaker          (說話者名稱)
  D: portrait         (立繪檔名，空白=無)
  E: text             (對話內容)
  F: onComplete       (只在該章最後一行填寫: openStarMap / returnToShip)

同一 chapter_id 的連續行會合併為同一章的多句對話。
onComplete 取該章最後一行的值。

使用方式:
  pip install openpyxl
  python generate_story.py story.xlsx ../Assets/StreamingAssets/StoryConfig.json
"""

import json
import sys
from collections import OrderedDict

try:
    import openpyxl
except ImportError:
    print("請先安裝 openpyxl: pip install openpyxl")
    sys.exit(1)


def main():
    if len(sys.argv) < 3:
        print(f"用法: python {sys.argv[0]} <input.xlsx> <output.json>")
        sys.exit(1)

    xlsx_path = sys.argv[1]
    json_path = sys.argv[2]

    wb = openpyxl.load_workbook(xlsx_path, read_only=True)
    ws = wb.active

    chapters = OrderedDict()

    for row in ws.iter_rows(min_row=2, values_only=True):
        chapter_id = str(row[0] or "").strip()
        if not chapter_id:
            continue

        trigger = int(row[1] or 0)
        speaker = str(row[2] or "").strip()
        portrait = str(row[3] or "").strip()
        text = str(row[4] or "").strip()
        on_complete = str(row[5] or "").strip()

        if chapter_id not in chapters:
            chapters[chapter_id] = {
                "id": chapter_id,
                "triggerAfterQuest": trigger,
                "dialogues": [],
                "onComplete": "",
            }

        if text:
            chapters[chapter_id]["dialogues"].append(
                {"speaker": speaker, "portrait": portrait, "text": text}
            )

        if on_complete:
            chapters[chapter_id]["onComplete"] = on_complete

    wb.close()

    output = {"chapters": list(chapters.values())}

    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(output, f, ensure_ascii=False, indent=2)

    print(f"完成: {len(chapters)} 章節 → {json_path}")


if __name__ == "__main__":
    main()
