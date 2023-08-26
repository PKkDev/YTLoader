namespace YTLoader.ConsoleApp.YouTube
{
    public readonly struct UnscrambledQuery
    {
        public string Uri { get; }

        public bool IsEncrypted { get; }

        public UnscrambledQuery(string uri, bool encrypted)
        {
            this.Uri = uri;
            this.IsEncrypted = encrypted;
        }
    }
}
