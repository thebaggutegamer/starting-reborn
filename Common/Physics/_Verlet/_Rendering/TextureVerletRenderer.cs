using ReLogic.Content;

namespace StartingWeapons.Common.Physics;

public readonly struct TextureVerletRenderer : IVerletRenderer
{
    public delegate Asset<Texture2D> AssetProviderCallback(int index, int length);

    public AssetProviderCallback Provider { get; }
    
    public TextureVerletRenderer(AssetProviderCallback provider)
    {
        ArgumentNullException.ThrowIfNull(provider, nameof(provider));
        
        Provider = provider;
    }
    
    void IVerletRenderer.Render(VerletChain chain)
    {
        for (var i = 0; i < chain.Sticks.Count; i++)
        {
            var stick = chain.Sticks[i];

            var texture = Provider.Invoke(i, chain.Sticks.Count).Value;
            
            var start = stick.Start.Position - Main.screenPosition;
            var end = stick.End.Position - Main.screenPosition;

            var difference = end - start;
            
            var destination = new Rectangle((int)start.X, (int)start.Y, (int)difference.Length(), texture.Height);

            var color = Lighting.GetColor((int)stick.Start.Position.X / 16, (int)stick.Start.Position.Y / 16, Color.White);
            
            var rotation = difference.ToRotation() + MathHelper.PiOver2;

            Main.spriteBatch.Draw
            (
                texture,
                destination,
                null,
                color,
                rotation,
                texture.Size() / 2f,
                SpriteEffects.None,
                0f
            );
        }
    }
}