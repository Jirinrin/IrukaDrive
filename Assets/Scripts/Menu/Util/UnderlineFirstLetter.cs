using TMPro;

namespace Menu.Util
{
    public static class UnderlineFirstLetter
    {
        public static void FormatText(TextMeshProUGUI txt, char triggerCharacter)
        {
            if (txt.text.StartsWith("<")) return;

            var index = txt.text.ToLower().IndexOf(triggerCharacter);
            if (index < 0) return;

            txt.text = txt.text.Substring(0,index) + $"<voffset=0.1em><u><b>{txt.text[index]}</b></u></voffset>" +
                       txt.text.Substring(index + 1);
        }
    }
}