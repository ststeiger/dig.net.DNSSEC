
namespace WebDig
{
    
    // Helper class to capture console output
    public class ConsoleOutputCapture 
        : System.IO.StringWriter
    {
        private readonly System.Text.StringBuilder _buffer = new System.Text.StringBuilder();

        public override void Write(char value)
        {
            _buffer.Append(value);
            base.Write(value);
        }

        public override void Write(string value)
        {
            _buffer.Append(value);
            base.Write(value);
        }

        public string GetOutput()
        {
            return _buffer.ToString();
        }

        public void Clear()
        {
            _buffer.Clear();
        }
    }


}