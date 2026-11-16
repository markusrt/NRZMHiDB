using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ScreenshotTests;

namespace GalleryReport
{
    /// <summary>
    /// Compares baseline vs current screenshots pixel-by-pixel and emits per-view diff images plus a
    /// side-by-side HTML gallery. Lives in its own namespace so it does NOT trigger the app/browser
    /// <c>Harness</c> set-up fixture — it only reads PNGs already on disk.
    /// During the Bootstrap upgrade the gallery is a review aid (differences are expected); once
    /// re-baselined on Bootstrap 5 it becomes a regression gate.
    /// </summary>
    [TestFixture]
    public class GalleryReportTest
    {
        [Test]
        public void BuildGallery()
        {
            var baselineDir = Paths.ScreenshotDir("baseline");
            var currentDir = Paths.ScreenshotDir("current");
            BuildGalleryFromDirs(baselineDir, currentDir,
                beforeLabel: "Baseline", afterLabel: "Current",
                title: "Bootstrap upgrade – gold-master gallery",
                outputDir: Paths.ReportRoot);
        }

        /// <summary>
        /// One-time BS3 vs BS5 comparison: compares the Bootstrap 3 screenshots (extracted
        /// from git history into screenshots/bs3/) against the re-baselined Bootstrap 5
        /// screenshots (screenshots/baseline/). Run this once to review the full upgrade diff.
        /// </summary>
        [Test]
        public void BuildBs3VsBs5Gallery()
        {
            var bs3Dir = Paths.ScreenshotDir("bs3");
            var bs5Dir = Paths.ScreenshotDir("baseline");
            BuildGalleryFromDirs(bs3Dir, bs5Dir,
                beforeLabel: "Bootstrap 3", afterLabel: "Bootstrap 5",
                title: "Bootstrap 3 → 5 migration gallery",
                outputDir: Path.Combine(Paths.ReportRoot, "bs3-vs-bs5"));
        }

        private static void BuildGalleryFromDirs(
            string beforeDir, string afterDir,
            string beforeLabel, string afterLabel,
            string title, string outputDir)
        {
            if (!Directory.Exists(beforeDir) || !Directory.Exists(afterDir))
            {
                Assert.Ignore($"Need both screenshot sets. before='{beforeDir}', after='{afterDir}'.");
            }

            Directory.CreateDirectory(outputDir);
            var diffDir = Path.Combine(outputDir, "diff");
            Directory.CreateDirectory(diffDir);

            var names = Directory.GetFiles(beforeDir, "*.png")
                .Concat(Directory.GetFiles(afterDir, "*.png"))
                .Select(Path.GetFileName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var rows = new List<Row>();
            foreach (var name in names)
            {
                var beforePath = Path.Combine(beforeDir, name);
                var afterPath = Path.Combine(afterDir, name);
                var diffPath = Path.Combine(diffDir, name);

                double percent;
                if (File.Exists(beforePath) && File.Exists(afterPath))
                {
                    percent = ImageDiff.WriteDiff(beforePath, afterPath, diffPath);
                }
                else
                {
                    percent = 100.0; // present in only one set
                }

                rows.Add(new Row
                {
                    Name = name,
                    HasBaseline = File.Exists(beforePath),
                    HasCurrent = File.Exists(afterPath),
                    HasDiff = File.Exists(diffPath),
                    Percent = percent
                });
            }

            var html = BuildHtml(rows.OrderByDescending(r => r.Percent).ToList(),
                beforeDir, afterDir, diffDir, beforeLabel, afterLabel, title);
            var indexPath = Path.Combine(outputDir, "index.html");
            File.WriteAllText(indexPath, html, Encoding.UTF8);

            TestContext.WriteLine($"Gallery written to {indexPath}");
            TestContext.WriteLine($"Views compared: {rows.Count}; changed: {rows.Count(r => r.Percent > 0.01)}");
        }

        private static string BuildHtml(List<Row> rows,
            string baselineDir, string currentDir, string diffDir,
            string beforeLabel, string afterLabel, string title)
        {
            string Rel(string dir, string name) => new Uri(Path.Combine(dir, name)).AbsoluteUri;

            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html><html lang='en'><head><meta charset='utf-8'>");
            sb.AppendLine($"<title>{title}</title>");
            sb.AppendLine("<style>body{font-family:system-ui,sans-serif;margin:1rem;background:#fafafa}" +
                          "h1{font-size:1.2rem}table{border-collapse:collapse;width:100%}" +
                          "td,th{border:1px solid #ddd;padding:6px;vertical-align:top}" +
                          "img{max-width:420px;height:auto;border:1px solid #ccc;background:#fff}" +
                          ".pct{font-weight:bold}.changed{background:#fff4f4}.name{white-space:nowrap}</style>");
            sb.AppendLine("</head><body>");
            sb.AppendLine($"<h1>{title} – {rows.Count} views, {rows.Count(r => r.Percent > 0.01)} changed</h1>");
            sb.AppendLine($"<table><tr><th>View</th><th>% changed</th><th>{beforeLabel}</th><th>{afterLabel}</th><th>Diff</th></tr>");

            foreach (var row in rows)
            {
                var cls = row.Percent > 0.01 ? " class='changed'" : string.Empty;
                sb.AppendLine($"<tr{cls}>");
                sb.AppendLine($"<td class='name'>{row.Name}</td>");
                sb.AppendLine($"<td class='pct'>{row.Percent:0.00}%</td>");
                sb.AppendLine($"<td>{(row.HasBaseline ? $"<img src='{Rel(baselineDir, row.Name)}'>" : "—")}</td>");
                sb.AppendLine($"<td>{(row.HasCurrent ? $"<img src='{Rel(currentDir, row.Name)}'>" : "—")}</td>");
                sb.AppendLine($"<td>{(row.HasDiff ? $"<img src='{Rel(diffDir, row.Name)}'>" : "—")}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table></body></html>");
            return sb.ToString();
        }

        private sealed class Row
        {
            public string Name;
            public bool HasBaseline;
            public bool HasCurrent;
            public bool HasDiff;
            public double Percent;
        }
    }

    /// <summary>Fast (LockBits) pixel diff between two PNGs; writes a highlighted diff and returns % changed.</summary>
    internal static class ImageDiff
    {
        // Per-channel tolerance to ignore sub-pixel antialiasing jitter.
        private const int Tolerance = 24;

        public static double WriteDiff(string baselinePath, string currentPath, string diffPath)
        {
            using (var baseline = new Bitmap(baselinePath))
            using (var current = new Bitmap(currentPath))
            {
                var width = Math.Max(baseline.Width, current.Width);
                var height = Math.Max(baseline.Height, current.Height);

                var a = ToArgb(baseline, width, height);
                var b = ToArgb(current, width, height);

                using (var diff = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    var rect = new Rectangle(0, 0, width, height);
                    var diffData = diff.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    var buffer = new byte[width * height * 4];
                    long changed = 0;

                    for (var i = 0; i < buffer.Length; i += 4)
                    {
                        var db = Math.Abs(a[i] - b[i]);
                        var dg = Math.Abs(a[i + 1] - b[i + 1]);
                        var dr = Math.Abs(a[i + 2] - b[i + 2]);

                        if (dr > Tolerance || dg > Tolerance || db > Tolerance)
                        {
                            buffer[i] = 0;        // B
                            buffer[i + 1] = 0;    // G
                            buffer[i + 2] = 255;  // R – highlight changed pixels in red
                            buffer[i + 3] = 255;
                            changed++;
                        }
                        else
                        {
                            // Dim the unchanged current image so red pops.
                            var gray = (byte)((b[i] + b[i + 1] + b[i + 2]) / 3 / 2 + 128);
                            buffer[i] = gray;
                            buffer[i + 1] = gray;
                            buffer[i + 2] = gray;
                            buffer[i + 3] = 255;
                        }
                    }

                    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, diffData.Scan0, buffer.Length);
                    diff.UnlockBits(diffData);
                    diff.Save(diffPath, ImageFormat.Png);

                    return 100.0 * changed / (width * (double)height);
                }
            }
        }

        private static byte[] ToArgb(Bitmap source, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            var copyWidth = Math.Min(width, source.Width);
            var copyHeight = Math.Min(height, source.Height);

            var rect = new Rectangle(0, 0, source.Width, source.Height);
            var data = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var stride = data.Stride;
                var src = new byte[stride * source.Height];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, src, 0, src.Length);

                for (var y = 0; y < copyHeight; y++)
                {
                    for (var x = 0; x < copyWidth; x++)
                    {
                        var srcIndex = y * stride + x * 4;
                        var dstIndex = (y * width + x) * 4;
                        buffer[dstIndex] = src[srcIndex];
                        buffer[dstIndex + 1] = src[srcIndex + 1];
                        buffer[dstIndex + 2] = src[srcIndex + 2];
                        buffer[dstIndex + 3] = src[srcIndex + 3];
                    }
                }
            }
            finally
            {
                source.UnlockBits(data);
            }

            return buffer;
        }
    }
}
