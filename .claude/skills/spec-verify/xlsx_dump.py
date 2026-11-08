import sys, zipfile, re, json
sys.stdout.reconfigure(encoding="utf-8")

def col_index(ref):
    letters = re.match(r"[A-Z]+", ref).group(0)
    n = 0
    for ch in letters:
        n = n * 26 + (ord(ch) - 64)
    return n - 1

def read(path):
    z = zipfile.ZipFile(path)
    shared = []
    if "xl/sharedStrings.xml" in z.namelist():
        ss = z.read("xl/sharedStrings.xml").decode("utf-8")
        for si in re.findall(r"<x:si>(.*?)</x:si>|<si>(.*?)</si>", ss, re.S):
            s = si[0] or si[1]
            shared.append("".join(re.findall(r"<(?:x:)?t[^>]*>(.*?)</(?:x:)?t>", s, re.S)))
    sheet = z.read("xl/worksheets/sheet1.xml").decode("utf-8")
    rows = []
    for row in re.findall(r"<(?:x:)?row[^>]*>(.*?)</(?:x:)?row>", sheet, re.S):
        cells = {}
        for attrs, body in re.findall(r"<(?:x:)?c ([^>]*?)(?:/>|>(.*?)</(?:x:)?c>)", row, re.S):
            ref = re.search(r'r="([A-Z]+\d+)"', attrs).group(1)
            t = re.search(r't="(\w+)"', attrs)
            t = t.group(1) if t else None
            v = re.search(r"<(?:x:)?v>(.*?)</(?:x:)?v>", body or "", re.S)
            isv = re.search(r"<(?:x:)?t[^>]*>(.*?)</(?:x:)?t>", body or "", re.S)
            val = v.group(1) if v else (isv.group(1) if isv else "")
            if t == "s" and val != "":
                val = shared[int(val)]
            cells[col_index(ref)] = (val, t)
        rows.append(cells)
    wb = z.read("xl/workbook.xml").decode("utf-8")
    sheets = re.findall(r"<(?:x:)?sheet [^>]*>", wb)
    return rows, sheets

if __name__ == "__main__":
    path = sys.argv[1]
    mode = sys.argv[2] if len(sys.argv) > 2 else "table"
    rows, sheets = read(path)
    header = rows[0] if rows else {}
    ncol = max(header.keys()) + 1 if header else 0
    names = [header.get(i, ("", None))[0] for i in range(ncol)]
    print("FILE", path.split("\\")[-1], "rows", len(rows) - 1, "cols", ncol, "sheets", sheets)
    if mode == "header":
        print(" | ".join(names))
    else:
        cols = sys.argv[3].split(",") if len(sys.argv) > 3 else names
        idx = [names.index(c) for c in cols if c in names]
        print(" | ".join(names[i] for i in idx))
        for r in rows[1:]:
            print(" | ".join(r.get(i, ("", None))[0] + ("#" if r.get(i, ("", None))[1] in (None, "n") and r.get(i, ("", None))[0] != "" else "") for i in idx))
