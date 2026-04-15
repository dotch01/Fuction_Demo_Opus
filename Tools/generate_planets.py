"""
generate_planets.py
批量產生 PlanetConfig.json，企劃可直接修改輸出的 JSON 微調。
用法: python generate_planets.py
輸出: ../Assets/StreamingAssets/PlanetConfig.json
"""
import json, random, os

STAR_SYSTEMS = [
    {"name": "GEVURAH",  "quadrant": 1, "center": {"x": 25, "y": 75}, "radius": 18},
    {"name": "TIFERET",  "quadrant": 2, "center": {"x": 75, "y": 75}, "radius": 18},
    {"name": "CHESED",   "quadrant": 3, "center": {"x": 25, "y": 25}, "radius": 18},
    {"name": "BINAH",    "quadrant": 4, "center": {"x": 75, "y": 25}, "radius": 18},
]

QUEST_NAMES = [
    "Nono_01", "Manon_01", "OtherWorld_0", "Sahara_01",
    "Menou_01", "Michelle_01", "Momo_01", "Gadouran_01",
]

DESCRIPTIONS = [
    "此行星無法接收到充足的日照能量，不適宜人類生存。",
    "此行星地表有一層過厚的大氣層，不適宜人類生存。",
    "此行星未觀察到衛星存在，地表可能面臨劇烈變動，不適合生存。",
    "此行星表面溫度過高，液態水無法存在。",
    "此行星處於宜居帶邊緣，水分含量極高。",
    "此行星質量過大，地表重力不適合人類活動。",
    "此行星有適度的大氣層，但溫度仍然偏低。",
    "此行星環境相對穩定，具有進一步觀測的價值。",
]

def quadrant_bounds(q):
    """
    Q1(左上) Q2(右上) Q3(左下) Q4(右下)
    """
    if q == 1: return (2, 48, 52, 98)
    if q == 2: return (52, 98, 52, 98)
    if q == 3: return (2, 48, 2, 48)
    return (52, 98, 2, 48)

def rand_pos_in_system(sys_entry):
    cx, cy, r = sys_entry["center"]["x"], sys_entry["center"]["y"], sys_entry["radius"]
    angle = random.uniform(0, 6.2832)
    dist = random.uniform(0, r * 0.85)
    import math
    return round(cx + math.cos(angle) * dist, 2), round(cy + math.sin(angle) * dist, 2)

def make_planet(pid, quadrant, sys_entry, is_quest, quest_name, filter_only, quest_order=0):
    x, y = rand_pos_in_system(sys_entry)
    return {
        "id": f"EMETH-{pid}",
        "defaultName": quest_name if is_quest else "",
        "description": random.choice(DESCRIPTIONS) if is_quest else "",
        "position": {"x": x, "y": y},
        "scale": round(random.uniform(0.15, 0.5), 2),
        "radius": random.randint(3000, 15000),
        "mass": random.randint(2000, 30000),
        "temperature": random.randint(-80, 120),
        "waterPercent": random.randint(0, 100),
        "earthSimilarity": round(random.uniform(10, 95), 2),
        "isQuestTarget": is_quest,
        "filterOnly": filter_only,
        "questOrder": quest_order,
    }

def main():
    planets = []
    pid = 100
    quest_idx = 0

    for sys in STAR_SYSTEMS:
        q = sys["quadrant"]
        # 1 quest planet per quadrant (normal), 1 quest planet per quadrant (filter)
        # + 4-6 random EMETH planets

        # Quest target (normal)
        if quest_idx < len(QUEST_NAMES):
            planets.append(make_planet(pid, q, sys, True, QUEST_NAMES[quest_idx], False, quest_idx + 1))
            pid += 1; quest_idx += 1

        # Quest target (filter only)
        if quest_idx < len(QUEST_NAMES):
            planets.append(make_planet(pid, q, sys, True, QUEST_NAMES[quest_idx], True, quest_idx + 1))
            pid += 1; quest_idx += 1

        # Random EMETH planets
        count = random.randint(4, 6)
        for _ in range(count):
            filter_only = random.random() < 0.3
            planets.append(make_planet(pid, q, sys, False, "", filter_only))
            pid += 1

    config = {
        "starSystems": STAR_SYSTEMS,
        "planets": planets,
    }

    out_dir = os.path.join(os.path.dirname(__file__), "..", "Assets", "StreamingAssets")
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, "PlanetConfig.json")

    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(config, f, ensure_ascii=False, indent=2)

    print(f"Generated {len(planets)} planets -> {out_path}")

if __name__ == "__main__":
    main()
