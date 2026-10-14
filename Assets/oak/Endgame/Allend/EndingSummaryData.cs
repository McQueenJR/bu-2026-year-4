public enum EndingType { None, Good, Bad }

// snapshot ข้ามซีน (static ไม่ใช้ DontDestroyOnLoad)
public static class EndingSummaryData
{
    public static bool HasData;
    public static EndingType type = EndingType.None;
    public static int endDay;

    // สถิติ 6 รายการ
    public static int ghostsEliminated;
    public static int ghostsEntered;
    public static int ghostsRejected;
    public static int villagersWronglyJudged;
    public static int villagersRejected;
    public static int villagersKilledByGhost;

    // เผื่อระบบคะแนนรอบหน้า (ตอนนี้ยังไม่คิด)
    public static int scoreEarned;
    public static int scoreMax;
    public static string rankText = "-";

    public static void Clear()
    {
        HasData = false;
        type = EndingType.None;
        endDay = 0;
        ghostsEliminated = ghostsEntered = ghostsRejected = 0;
        villagersWronglyJudged = villagersRejected = villagersKilledByGhost = 0;
        scoreEarned = scoreMax = 0;
        rankText = "-";
    }
}