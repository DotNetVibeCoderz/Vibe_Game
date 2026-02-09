using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;

namespace FightingGame
{
    public enum ActionState
    {
        Idle,
        Walk,
        Jump,
        Punch,
        Kick,
        Block,
        Hurt,
        Dead
    }

    public class Character
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Health { get; set; } = 100;
        public bool IsFacingRight { get; set; } = true;
        
        private ActionState _state = ActionState.Idle;
        public ActionState State 
        { 
            get => _state; 
            set 
            {
                if (_state != value)
                {
                    _state = value;
                    _currentFrame = 0;
                    _frameTimer = 0;
                    // Reset visuals immediately when state changes
                    UpdateVisuals(); 
                }
            }
        }
        
        // Timer for logic states (attacking duration etc)
        public double StateTimer { get; set; }
        public const double AttackDuration = 20.0; // Total frames for attack

        // Animation Fields
        public Dictionary<ActionState, List<Bitmap>> Animations { get; set; } = new();
        private int _currentFrame = 0;
        private double _frameTimer = 0;
        private double _frameSpeed = 5; // Frames to wait before switching image

        // Constants
        public const double Gravity = 0.8;
        public const double JumpForce = -15.0;
        public const double WalkSpeed = 5.0;
        public const double GroundLevel = 400.0;

        public Image SpriteControl { get; set; }
        public ProgressBar HealthBar { get; set; }
        private Bitmap? _baseSprite; // Made nullable

        public Character(Image sprite, ProgressBar healthBar, double startX)
        {
            SpriteControl = sprite;
            HealthBar = healthBar;
            X = startX;
            Y = GroundLevel;
            
            // Store base sprite if available
            if (SpriteControl.Source is Bitmap bmp)
            {
                _baseSprite = bmp;
            }
        }

        public void Update()
        {
            // Apply Gravity
            VelocityY += Gravity;
            Y += VelocityY;

            // Ground Collision
            if (Y >= GroundLevel)
            {
                Y = GroundLevel;
                VelocityY = 0;
                if (State == ActionState.Jump) State = ActionState.Idle;
            }

            // Apply Horizontal Velocity
            X += VelocityX;

            // Friction
            VelocityX *= 0.8; 

            // Update UI Position
            Canvas.SetLeft(SpriteControl, X);
            Canvas.SetTop(SpriteControl, Y);

            // Update Visuals (Animation & Transforms)
            UpdateVisuals();

            // Update Health UI
            HealthBar.Value = Health;

            // State Logic Timers
            if (StateTimer > 0)
            {
                StateTimer -= 1;
                if (StateTimer <= 0)
                {
                    if (State == ActionState.Punch || State == ActionState.Kick || State == ActionState.Hurt || State == ActionState.Block)
                    {
                        State = ActionState.Idle;
                    }
                }
            }
        }

        private void UpdateVisuals()
        {
            // Determine Flip Logic
            double scaleX = IsFacingRight ? 1 : -1;
            var transformGroup = new TransformGroup();

            // 1. Handle Frame Animation if available
            if (Animations.ContainsKey(State) && Animations[State].Count > 0)
            {
                _frameTimer++;
                if (_frameTimer >= _frameSpeed)
                {
                    _frameTimer = 0;
                    _currentFrame++;
                    if (_currentFrame >= Animations[State].Count)
                    {
                        _currentFrame = 0; 
                    }
                }
                SpriteControl.Source = Animations[State][_currentFrame];
                
                // Pure frame animation usually just needs flipping
                 if (!IsFacingRight)
                 {
                     transformGroup.Children.Add(new ScaleTransform(-1, 1));
                 }
            }
            else
            {
                // 2. Procedural Animation Fallback (Dynamic!)
                // Reset to base sprite if source was changed
                if (_baseSprite != null && SpriteControl.Source != _baseSprite)
                {
                    SpriteControl.Source = _baseSprite;
                }

                // Animation Logic based on StateTimer for smooth motion
                double progress = 0;
                if (State == ActionState.Punch || State == ActionState.Kick)
                {
                    // Calculate progress from 0 to 1 based on StateTimer (20 -> 0)
                    progress = 1.0 - (StateTimer / AttackDuration);
                    if (progress < 0) progress = 0;
                    if (progress > 1) progress = 1;
                }

                switch (State)
                {
                    case ActionState.Idle:
                        // Breathing effect (scale Y slightly)
                        double breath = 1.0 + Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 5) * 0.02;
                        transformGroup.Children.Add(new ScaleTransform(scaleX, breath));
                        break;

                    case ActionState.Walk:
                        // Bobbing (Rotate slightly)
                        double walkAngle = Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 15) * 5;
                        transformGroup.Children.Add(new ScaleTransform(scaleX, 1));
                        transformGroup.Children.Add(new RotateTransform(walkAngle));
                        break;

                    case ActionState.Jump:
                        // Stretch
                        transformGroup.Children.Add(new ScaleTransform(scaleX * 0.8, 1.2));
                        break;

                    case ActionState.Punch:
                        // Move forward and back (Sine wave)
                        // progress 0 -> 0.5 (max extension) -> 1.0 (back)
                        double punchDistance = Math.Sin(progress * Math.PI) * 30; // Max 30 pixels forward
                        transformGroup.Children.Add(new ScaleTransform(scaleX, 1));
                        transformGroup.Children.Add(new TranslateTransform(IsFacingRight ? punchDistance : -punchDistance, 0));
                        break;

                    case ActionState.Kick:
                        // Rotate body/leg up and down
                        // progress 0 -> 0.5 (max rotation) -> 1.0 (back)
                        double kickAngle = Math.Sin(progress * Math.PI) * 45; // Max 45 degrees
                        transformGroup.Children.Add(new ScaleTransform(scaleX, 1));
                        // Pivot point should ideally be lower body, but center works for simple sprites
                        transformGroup.Children.Add(new RotateTransform(IsFacingRight ? -kickAngle : kickAngle));
                        // Also move slightly forward
                        double kickDistance = Math.Sin(progress * Math.PI) * 15;
                        transformGroup.Children.Add(new TranslateTransform(IsFacingRight ? kickDistance : -kickDistance, 0));
                        break;

                    case ActionState.Block:
                        // Scale X (hide/crouch)
                        transformGroup.Children.Add(new ScaleTransform(scaleX * 0.8, 0.9));
                        break;

                    case ActionState.Hurt:
                        // Shake heavily
                        double shake = Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 50) * 5;
                        transformGroup.Children.Add(new ScaleTransform(scaleX, 1));
                        transformGroup.Children.Add(new TranslateTransform(shake, 0));
                        // Flash red logic could be here if we had color filters
                        break;

                    case ActionState.Dead:
                        // Lie down
                        transformGroup.Children.Add(new RotateTransform(IsFacingRight ? -90 : 90));
                        transformGroup.Children.Add(new TranslateTransform(0, 30)); // Move down to ground
                        break;
                }
            }
            
            SpriteControl.RenderTransform = transformGroup;
            SpriteControl.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        }

        public bool Jump()
        {
            if (Y >= GroundLevel && State != ActionState.Hurt && State != ActionState.Dead)
            {
                VelocityY = JumpForce;
                State = ActionState.Jump;
                return true;
            }
            return false;
        }

        public void Move(double direction)
        {
            if (State == ActionState.Hurt || State == ActionState.Dead || State == ActionState.Block) return;
            
            VelocityX = direction * WalkSpeed;
            if (direction > 0) IsFacingRight = true;
            if (direction < 0) IsFacingRight = false;
            
            if (State == ActionState.Idle) State = ActionState.Walk;
        }

        public bool Attack(ActionState attackType)
        {
            if (State == ActionState.Idle || State == ActionState.Walk)
            {
                State = attackType;
                StateTimer = AttackDuration; // 20 frames duration
                VelocityX = 0; // Stop moving when attacking
                return true;
            }
            return false;
        }

        public bool Block()
        {
             if (State == ActionState.Idle || State == ActionState.Walk)
            {
                State = ActionState.Block;
                StateTimer = 10;
                VelocityX = 0;
                return true;
            }
            return false;
        }

        public void TakeDamage(double amount)
        {
            if (State == ActionState.Block)
            {
                amount /= 5; // Reduced damage
            }
            else
            {
                State = ActionState.Hurt;
                StateTimer = 15;
            }

            Health -= amount;
            if (Health < 0) Health = 0;
            if (Health <= 0) State = ActionState.Dead;
        }
    }
}