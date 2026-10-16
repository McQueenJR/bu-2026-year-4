using UnityEditor;
using UnityEngine;
 
// ต้องอยู่ในโฟลเดอร์ชื่อ Editor (เช่น Assets/Scripts/Editor/)
// วาด 5 คอลัมน์: ช่องเลขด้านบน + checkbox ด้านล่าง คั่นด้วยเส้นแนวตั้ง
[CustomPropertyDrawer(typeof(DigitAnomalyField))]
public class DigitAnomalyFieldDrawer : PropertyDrawer
{
    private const int N = DigitAnomalyField.DigitCount;
    private const float HeaderH = 18f;
    private const float DigitH = 24f;
    private const float ToggleH = 20f;
    private const float Pad = 4f;
 
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return HeaderH + DigitH + ToggleH + Pad;
    }
 
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
 
        SerializedProperty origProp = property.FindPropertyRelative("original");
        SerializedProperty flagsProp = property.FindPropertyRelative("randomize");
 
        if (flagsProp.arraySize != N)
            flagsProp.arraySize = N;
 
        // หัวข้อ
        Rect header = new Rect(position.x, position.y, position.width, HeaderH);
        EditorGUI.LabelField(header, "ID Number");
 
        // พื้นที่ 5 คอลัมน์
        Rect area = EditorGUI.IndentedRect(new Rect(position.x, position.y + HeaderH, position.width, DigitH + ToggleH));
        float colW = area.width / N;
 
        // เตรียมเลขต้นฉบับให้ครบ N ตัว
        char[] chars = new char[N];
        string s = origProp.stringValue ?? string.Empty;
        for (int i = 0; i < N; i++)
            chars[i] = (i < s.Length && char.IsDigit(s[i])) ? s[i] : '0';
 
        GUIStyle digitStyle = new GUIStyle(EditorStyles.textField)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
 
        Color lineColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        bool digitsChanged = false;
 
        for (int i = 0; i < N; i++)
        {
            float x = area.x + colW * i;
 
            // เส้นคั่นแนวตั้ง
            if (i > 0)
                EditorGUI.DrawRect(new Rect(x, area.y, 1f, DigitH + ToggleH), lineColor);
 
            // ช่องเลข
            Rect digitRect = new Rect(x + 4f, area.y + 1f, colW - 8f, DigitH - 2f);
            string input = EditorGUI.TextField(digitRect, chars[i].ToString(), digitStyle);
            if (!string.IsNullOrEmpty(input))
            {
                char c = input[input.Length - 1];
                if (char.IsDigit(c) && c != chars[i])
                {
                    chars[i] = c;
                    digitsChanged = true;
                }
            }
 
            // checkbox แยกแต่ละหลัก
            SerializedProperty flag = flagsProp.GetArrayElementAtIndex(i);
            Rect toggleRect = new Rect(x + colW * 0.5f - 7f, area.y + DigitH + 2f, 16f, 16f);
            flag.boolValue = EditorGUI.Toggle(toggleRect, new GUIContent("", $"ติ๊ก = สุ่มหลักที่ {i + 1}"), flag.boolValue);
        }
 
        if (digitsChanged)
            origProp.stringValue = new string(chars);
 
        EditorGUI.EndProperty();
    }
}
 