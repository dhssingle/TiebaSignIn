namespace TiebaSignIn.Domain
{
    public class Cookie
    {
        private readonly static Cookie cookie = new();
        private Cookie()
        {}
        public static Cookie GetInstance()
        {
            return cookie;
        }
        public string BDUSS { get; set; }

        public override string ToString() => $"{nameof(BDUSS)}={BDUSS}";
    }
}