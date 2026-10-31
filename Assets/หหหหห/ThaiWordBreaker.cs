using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ตัดคำภาษาไทยอัตโนมัติสำหรับ TextMeshPro
// TMP ไม่รู้ขอบเขตคำไทย จึงตัดบรรทัดกลางคำได้ (เช่น "แป|ลก")
// คลาสนี้แทรก Zero-Width Space (U+200B) ระหว่างคำ ซึ่ง TMP ถือเป็นจุดตัดบรรทัดที่ได้
//
// ใช้พจนานุกรมจาก Resources/ThaiWordBreak/thai_words.txt (บรรทัดละ 1 คำ)
// และคำเพิ่มเติมของเกมจาก Resources/ThaiWordBreak/thai_words_extra.txt (ไม่บังคับ)
//
// ข้ามแท็ก TMP เช่น <b>...</b> ไม่แทรกอะไรลงในแท็ก
// "|" ในข้อความถูกแปลงเป็นจุดตัดคำ (เผื่อใส่เองเฉพาะจุด)
public static class ThaiWordBreaker
{
    private const char ZeroWidthSpace = '\u200B';
    private const string MainDictionaryPath = "ThaiWordBreak/thai_words";
    private const string ExtraDictionaryPath = "ThaiWordBreak/thai_words_extra";

    private static HashSet<string> words;
    private static int maxWordLength;
    private static bool loadAttempted;

    // กันค่าค้างเมื่อปิด Domain Reload ใน Enter Play Mode Options
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        words = null;
        maxWordLength = 0;
        loadAttempted = false;
    }

    // โหลดพจนานุกรมล่วงหน้า (ไม่บังคับ) เพื่อไม่ให้กระตุกตอนเปิดหนังสือพิมพ์ครั้งแรก
    public static void Preload()
    {
        EnsureLoaded();
    }

    public static string Process(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // "|" = จุดตัดคำที่ใส่เอง
        text = text.Replace('|', ZeroWidthSpace);

        if (!EnsureLoaded())
            return text;

        StringBuilder sb = new StringBuilder(text.Length * 2);
        int n = text.Length;
        int i = 0;

        while (i < n)
        {
            char c = text[i];

            // ข้ามแท็ก TMP ทั้งก้อน (<b>, <color=#fff>, </b> ฯลฯ)
            if (c == '<')
            {
                int close = text.IndexOf('>', i);

                if (close > i && close - i < 100)
                {
                    sb.Append(text, i, close - i + 1);
                    i = close + 1;
                    continue;
                }
            }

            if (IsThai(c))
            {
                int start = i;

                while (i < n && IsThai(text[i]))
                    i++;

                AppendSegmented(sb, text.Substring(start, i - start));
                continue;
            }

            sb.Append(c);
            i++;
        }

        return sb.ToString();
    }

    // =====================================================
    // ตัดคำ: เลือกการแบ่งที่มี "ส่วนที่ไม่รู้จัก" น้อยที่สุด แล้วใช้จำนวนคำน้อยที่สุด (คำยาวชนะ)
    // =====================================================
    private static void AppendSegmented(StringBuilder sb, string s)
    {
        int n = s.Length;

        if (n < 2)
        {
            sb.Append(s);
            return;
        }

        // ตำแหน่งที่ตัดได้จริงตามกฎการเขียนไทย (ห้ามตัดก่อนสระ/วรรณยุกต์ และหลังสระหน้า)
        bool[] canBreak = new bool[n + 1];

        for (int i = 1; i < n; i++)
            canBreak[i] = !IsNoBreakBefore(s[i]) && !IsLeadingVowel(s[i - 1]);

        canBreak[n] = true;

        const int Inf = int.MaxValue / 2;

        int[] best = new int[n + 1];
        int[] prev = new int[n + 1];
        bool[] unknown = new bool[n + 1];

        for (int i = 1; i <= n; i++)
            best[i] = Inf;

        for (int i = 0; i < n; i++)
        {
            if (best[i] >= Inf)
                continue;

            // คำในพจนานุกรม (อย่างน้อย 2 ตัวอักษร)
            int maxJ = Mathf.Min(n, i + maxWordLength);

            for (int j = i + 2; j <= maxJ; j++)
            {
                if (!canBreak[j])
                    continue;

                if (!words.Contains(s.Substring(i, j - i)))
                    continue;

                int cost = best[i] + 1;

                if (cost < best[j])
                {
                    best[j] = cost;
                    prev[j] = i;
                    unknown[j] = false;
                }
            }

            // ไม่รู้จัก: กินไปถึงจุดตัดที่ถัดไป (1 พยางค์/กลุ่มอักษร) โทษหนักกว่าคำปกติมาก
            int u = i + 1;

            while (u < n && !canBreak[u])
                u++;

            int unknownCost = best[i] + 1000 * (u - i);

            if (unknownCost < best[u])
            {
                best[u] = unknownCost;
                prev[u] = i;
                unknown[u] = true;
            }
        }

        // ย้อนกลับเก็บช่วงของแต่ละคำ (ได้ลำดับกลับด้าน)
        List<int> starts = new List<int>();
        List<int> ends = new List<int>();
        List<bool> unknowns = new List<bool>();

        int p = n;

        while (p > 0)
        {
            int q = prev[p];
            starts.Add(q);
            ends.Add(p);
            unknowns.Add(unknown[p]);
            p = q;
        }

        bool first = true;
        bool lastUnknown = false;

        for (int k = starts.Count - 1; k >= 0; k--)
        {
            bool isUnknown = unknowns[k];

            // ส่วนที่ไม่รู้จักติดกัน (เช่น ชื่อเฉพาะ) ไม่ตัดแยก
            if (!first && !(isUnknown && lastUnknown))
                sb.Append(ZeroWidthSpace);

            sb.Append(s, starts[k], ends[k] - starts[k]);

            first = false;
            lastUnknown = isUnknown;
        }
    }

    // อักษรไทย: พยัญชนะ สระ วรรณยุกต์ ๆ ฯ (ไม่รวมเลขไทยและ ฿)
    private static bool IsThai(char c)
    {
        return (c >= '\u0E01' && c <= '\u0E3A') || (c >= '\u0E40' && c <= '\u0E4E');
    }

    // ตัวที่ห้ามขึ้นต้นบรรทัด/ห้ามตัดก่อน: ะ ั า ำ สระบน-ล่าง ๅ ๆ ฯ และวรรณยุกต์
    private static bool IsNoBreakBefore(char c)
    {
        return c == '\u0E2F'
            || c == '\u0E30' || c == '\u0E31' || c == '\u0E32' || c == '\u0E33'
            || (c >= '\u0E34' && c <= '\u0E3A')
            || (c >= '\u0E45' && c <= '\u0E4E');
    }

    // สระหน้า เ แ โ ใ ไ ห้ามตัดหลังมัน
    private static bool IsLeadingVowel(char c)
    {
        return c >= '\u0E40' && c <= '\u0E44';
    }

    // =====================================================
    // โหลดพจนานุกรม
    // =====================================================
    private static bool EnsureLoaded()
    {
        if (words != null)
            return true;

        if (loadAttempted)
            return false;

        loadAttempted = true;

        TextAsset main = Resources.Load<TextAsset>(MainDictionaryPath);

        if (main == null)
        {
            Debug.LogWarning(
                "[ThaiWordBreaker] ไม่พบ Resources/" + MainDictionaryPath +
                ".txt → ไม่ตัดคำอัตโนมัติ (ข้อความแสดงตามปกติ)");
            return false;
        }

        HashSet<string> set = new HashSet<string>();
        int max = 0;

        AddLines(set, main.text, ref max);

        TextAsset extra = Resources.Load<TextAsset>(ExtraDictionaryPath);

        if (extra != null)
            AddLines(set, extra.text, ref max);

        words = set;
        maxWordLength = max;

        return true;
    }

    private static void AddLines(HashSet<string> set, string content, ref int max)
    {
        string[] lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string w = lines[i].Trim();

            if (w.Length < 2 || w[0] == '#')
                continue;

            set.Add(w);

            if (w.Length > max)
                max = w.Length;
        }
    }
}
