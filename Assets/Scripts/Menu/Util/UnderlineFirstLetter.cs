using TMPro;

namespace Menu.Util
{
    public static class UnderlineFirstLetter
    {
        public static void FormatText(TextMeshProUGUI txt)
        {
            if (!txt.text.StartsWith("<"))
                txt.text = $"<voffset=0.1em><u><b>{txt.text.Substring(0,1)}</b></u></voffset>{txt.text.Substring(1)}";
        }
    }
}