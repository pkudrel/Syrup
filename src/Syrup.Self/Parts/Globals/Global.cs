namespace Syrup.Self.Parts.Globals
{
    public sealed class Global
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Global()
        {
        }

        private Global()
        {
            var gf = new GlobalFactory();
            Config = gf.GetConfig();
            Registry = gf.GetRegistry();
        }

        public Config Config { get; private set; }
        public Registry Registry { get; private set; }

        public static Global Instance { get; } = new Global();
    }
}