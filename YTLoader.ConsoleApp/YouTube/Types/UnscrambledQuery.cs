namespace YTLoader.ConsoleApp.YouTube.Types
{
    public readonly struct UnscrambledQuery
    {
        public string Uri { get; }

        public bool IsEncrypted { get; }

        public UnscrambledQuery(string uri, bool encrypted)
        {
            Uri = uri;
            IsEncrypted = encrypted;
        }
    }
}
