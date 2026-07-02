using System.Collections.Generic;

namespace StartingWeapons.Core.Graphics;

[Autoload(Side = ModSide.Client)]
public sealed class PixellationSystem : ModSystem
{
    public static RenderTarget2D Target { get; private set; }
    
    public static List<Action> Actions { get; private set; }
    
    public static readonly Matrix Scale = Matrix.CreateScale(0.5f, 0.5f, 1f);

    public override void Load()
    {
        base.Load();

        Actions = new List<Action>();
        
        Main.RunOnMainThread
        (
            static () =>
            {
                Target = new RenderTarget2D
                (
                    Main.graphics.GraphicsDevice,
                    Main.screenWidth / 2,
                    Main.screenHeight / 2
                );
            }
        );
        
        On_Main.CheckMonoliths += Main_CheckMonoliths_FillBuffer;
        On_Main.DrawProjectiles += Main_DrawProjectiles_DrawBuffer;

        Main.OnResolutionChanged += Main_OnResolutionChanged_ResizeBuffer;
    }

    public override void Unload()
    {
        base.Unload();
        
        Main.OnResolutionChanged -= Main_OnResolutionChanged_ResizeBuffer;

        Main.RunOnMainThread
        (
            static () =>
            {
                Target?.Dispose();
                Target = null;
            }
        );
        
        Actions?.Clear();
        Actions = null;
    }

    public static void Queue(Action action)
    {
        Actions.Add(action);
    }
    
    private static void Main_CheckMonoliths_FillBuffer(On_Main.orig_CheckMonoliths orig)
    {
        orig();
        
        FillBuffer();
    }

    private static void FillBuffer()
    {
        if (Target == null || Target.IsDisposed)
        {
            return;
        }
        
        var device = Main.graphics.GraphicsDevice;

        var bindings = device.GetRenderTargets();

        device.SetRenderTarget(Target);
        device.Clear(Color.Transparent);
        
        Main.spriteBatch.Begin
        (
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Scale
        );

        foreach (var action in Actions) 
        {
            action?.Invoke();
        }
        
        Main.spriteBatch.End();
        
        device.SetRenderTargets(bindings);

        Actions.Clear();
    }
    
    private static void Main_DrawProjectiles_DrawBuffer(On_Main.orig_DrawProjectiles orig, Main self)
    {
        DrawBuffer();
        
        orig(self);
    }

    private static void DrawBuffer()
    {
        if (Target == null || Target.IsDisposed)
        {
            return;
        }

        Main.spriteBatch.Begin
        (
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );

        Main.spriteBatch.Draw(Target, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

        Main.spriteBatch.End();
    }

    private static void Main_OnResolutionChanged_ResizeBuffer(Vector2 size)
    {
        ResizeBuffer(size);
    }

    private static void ResizeBuffer(Vector2 size)
    {
        if (Target == null || Target.IsDisposed)
        {
            return;
        }
        
        Main.RunOnMainThread
        (
            () =>
            {
                Target?.Dispose();

                Target = new RenderTarget2D
                (
                    Main.graphics.GraphicsDevice,
                    (int)(size.X / 2f),
                    (int)(size.Y / 2f)
                );
            }
        );
    }
}