using Dalamud.Plugin.Services;

namespace ChilledLeves
{
    internal class Service
    {
        internal static Config Configuration { get; set; } = null!;
        internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        internal static ITextureProvider Texture { get; set; } = null!;
        public static IObjectTable ObjectTable { get; private set; } = null!;
    }
}
