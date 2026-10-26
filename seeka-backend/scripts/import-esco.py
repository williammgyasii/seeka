#!/usr/bin/env python3
"""Turn ESCO's English occupations CSV into the job title list the API ships.

Usage: python3 seeka-backend/scripts/import-esco.py ~/Downloads/occupations_en.csv

ESCO is published by the European Commission and may be reused with credit to
the source. Only released occupations are kept. Alternative and hidden labels
become search names; the preferred label is the name Seeka stores.
"""

import csv
import json
import sys
from pathlib import Path

OUTPUT = Path(__file__).resolve().parent.parent / "Seeka.ResourceAccess" / "Reference" / "esco-occupations.json"


def names(cell: str) -> list[str]:
    return [line.strip() for line in (cell or "").splitlines() if line.strip()]


def main(source: Path) -> None:
    with source.open(newline="", encoding="utf-8") as file:
        rows = [row for row in csv.DictReader(file) if row.get("status", "released") == "released"]

    occupations = []
    for row in rows:
        title = row["preferredLabel"].strip()
        alternatives = sorted({name for name in names(row.get("altLabels")) + names(row.get("hiddenLabels"))
                               if name.lower() != title.lower()}, key=str.lower)
        occupations.append({"title": title, "alternatives": alternatives})

    occupations.sort(key=lambda occupation: occupation["title"].lower())
    OUTPUT.write_text(json.dumps(occupations, ensure_ascii=False, separators=(",", ":")) + "\n", encoding="utf-8")
    print(f"Wrote {len(occupations)} occupations to {OUTPUT}")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        sys.exit(__doc__)
    main(Path(sys.argv[1]).expanduser())
