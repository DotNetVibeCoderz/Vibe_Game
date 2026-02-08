using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace ZombieNet
{
    public static class TextureLoader
    {
        public static Texture2D Load(GraphicsDevice device, string path)
        {
            // Try relative path first
            string finalPath = Path.Combine("Content", path);

            if (!File.Exists(finalPath))
            {
                // Try to find it relative to execution
                if (File.Exists(path))
                    finalPath = path;
                else
                {
                    // Fallback to absolute path or other checks if needed
                    // For now, assume it's in Content folder copied to output
                    throw new FileNotFoundException($"Could not find texture: {finalPath}");
                }
            }

            using (var stream = new FileStream(finalPath, FileMode.Open))
            {
                return Texture2D.FromStream(device, stream);
            }
        }
    }
}
