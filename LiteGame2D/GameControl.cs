using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using LiteGame2D.Engine;
using System;

namespace LiteGame2D
{
    public class GameControl : Control
    {
        public IGame CurrentGame { get; set; }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            
            // Draw Background
            context.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));

            if (CurrentGame != null)
            {
                CurrentGame.Draw(context, Bounds.Size);
            }
            
            // Request next frame
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
        }
    }
}