
namespace WebDig.Unneeded
{
    /// <summary>
    /// For ASP.NET Core version, we need to reimplement FeedbackWriter to work with a StringWriter instead of TextBox
    /// </summary>
    public class FeedbackWriter 
        : System.IO.TextWriter
    {
        private System.Text.StringBuilder buffer = new System.Text.StringBuilder();

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void Write(char value)
        {
            buffer.Append(value);
        }

        public override void Write(string value)
        {
            buffer.Append(value);
        }

        public string GetText()
        {
            return buffer.ToString();
        }

        public void Clear()
        {
            buffer.Clear();
        }
    }
}