using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace RubikCube3D
{
    public class Renderer
    {
        private float _rotationX = 30;
        private float _rotationY = -45;
        private float _scale = 200; 

        public float RotationX { get => _rotationX; set => _rotationX = value; }
        public float RotationY { get => _rotationY; set => _rotationY = value; }
        public float Scale { get => _scale; set => _scale = value; }

        struct Point3D { public float X, Y, Z; public Point3D(float x, float y, float z) { X = x; Y = y; Z = z; } }

        public void Render(SKCanvas canvas, CubeModel model, float width, float height)
        {
            // Draw background
            using (var paintBg = new SKPaint())
            {
                paintBg.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(0, 0),
                    new SKPoint(0, height),
                    new SKColor[] { new SKColor(40, 40, 40), new SKColor(20, 20, 20) },
                    null,
                    SKShaderTileMode.Clamp);
                canvas.DrawRect(0, 0, width, height, paintBg);
            }

            float cx = width / 2;
            float cy = height / 2;

            List<FaceRenderData> facesToDraw = new List<FaceRenderData>();

            // Pre-calculate rotation matrices
            float radX = _rotationX * (float)Math.PI / 180f;
            float radY = _rotationY * (float)Math.PI / 180f;

            float cosX = (float)Math.Cos(radX);
            float sinX = (float)Math.Sin(radX);
            float cosY = (float)Math.Cos(radY);
            float sinY = (float)Math.Sin(radY);

            // Camera setup: Camera at +Z (Viewer)
            // Increased camera distance because coordinates are now larger (step 2)
            float cameraDist = 16.0f; 

            foreach (var cubie in model.Cubies)
            {
                // Size slightly less than 1.0 (since step is 2, half-step is 1.0, we want a gap)
                float size = 0.95f; 
                float cx0 = cubie.X;
                float cy0 = cubie.Y;
                float cz0 = cubie.Z;

                Point3D[] verts = new Point3D[8];
                for(int i=0; i<8; i++)
                {
                    float vx = ((i & 1) == 0 ? -1 : 1) * size;
                    float vy = ((i & 2) == 0 ? -1 : 1) * size;
                    float vz = ((i & 4) == 0 ? -1 : 1) * size;
                    verts[i] = new Point3D(cx0 + vx, cy0 + vy, cz0 + vz);
                }

                Point3D[] transformed = new Point3D[8];
                for(int i=0; i<8; i++)
                {
                    float x = verts[i].X;
                    float y = verts[i].Y;
                    float z = verts[i].Z;

                    // Rotate Y (Horizontal Mouse)
                    float x1 = x * cosY - z * sinY;
                    float z1 = x * sinY + z * cosY;
                    float y1 = y;

                    // Rotate X (Vertical Mouse)
                    float y2 = y1 * cosX - z1 * sinX; 
                    float z2 = y1 * sinX + z1 * cosX;
                    float x2 = x1;

                    transformed[i] = new Point3D(x2, y2, z2);
                }

                // Indices for 6 faces (CCW winding for Front-Facing)
                int[][] faceIndices = new int[][]
                {
                    new int[] { 4, 5, 7, 6 }, // Front (Z+)
                    new int[] { 1, 0, 2, 3 }, // Back (Z-)
                    new int[] { 2, 6, 7, 3 }, // Up (Y+)
                    new int[] { 0, 1, 5, 4 }, // Down (Y-)
                    new int[] { 0, 4, 6, 2 }, // Left (X-)
                    new int[] { 5, 1, 3, 7 }  // Right (X+)
                };

                for(int f=0; f<6; f++)
                {
                    var color = cubie.FaceColors[f];
                    if (color == SKColors.Black) continue; 

                    SKPoint[] poly2d = new SKPoint[4];
                    float avgDepth = 0;
                    
                    bool valid = true;

                    for(int k=0; k<4; k++)
                    {
                        var v = transformed[faceIndices[f][k]];
                        
                        // Perspective Projection
                        float depth = cameraDist - v.Z;
                        
                        if (depth < 0.1f) { valid = false; break; }

                        float factor = _scale * 5.0f / depth;
                        
                        // Screen Y is down
                        poly2d[k] = new SKPoint(cx + v.X * factor, cy - v.Y * factor);
                        
                        avgDepth += depth;
                    }
                    
                    if (!valid) continue;
                    
                    avgDepth /= 4.0f;

                    // Backface Culling
                    var p0 = poly2d[0]; 
                    var p1 = poly2d[1]; 
                    var p2 = poly2d[2];
                    
                    float cross = (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);
                    
                    if (cross < 0) 
                    {
                        facesToDraw.Add(new FaceRenderData 
                        { 
                            Depth = avgDepth, 
                            Points = poly2d, 
                            Color = color
                        });
                    }
                }
            }

            // Sort by Depth Descending (Farthest First)
            facesToDraw.Sort((a, b) => b.Depth.CompareTo(a.Depth));

            using var paint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };
            using var border = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true, StrokeJoin = SKStrokeJoin.Round };
            
            foreach (var face in facesToDraw)
            {
                var path = new SKPath();
                path.MoveTo(face.Points[0]);
                path.LineTo(face.Points[1]);
                path.LineTo(face.Points[2]);
                path.LineTo(face.Points[3]);
                path.Close();

                paint.Color = face.Color;
                canvas.DrawPath(path, paint);
                canvas.DrawPath(path, border);
            }
        }

        private class FaceRenderData
        {
            public float Depth;
            public SKPoint[] Points;
            public SKColor Color;
        }
    }
}