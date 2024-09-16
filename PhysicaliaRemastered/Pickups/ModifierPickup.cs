using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicaliaRemastered.Pickups;

/// <summary>
/// Abstract class that acts as a base for all Pickups that are intended to
/// change the behavior of the game.
/// </summary>
public abstract class ModifierPickup : Pickup
{
    private const float ICON_TIMER_SPACING = 5F;
    private const float DEFAULT_DURATION = 5F;

    private bool active;
    private float duration;
    private float timeRemaining;

    private Sprite icon;

    public bool IsActive
    {
        get => active;
        set => active = value;
    }

    public float Duration
    {
        get => duration;
        set => timeRemaining = duration = value;
    }

    public float TimeRemaining
    {
        get => timeRemaining;
        set => timeRemaining = value;
    }

    public Sprite Icon
    {
        get => icon;
        set => icon = value;
    }

    public ModifierPickup(Level level, Sprite icon, Sprite sprite, float duration)
        : base(level)
    {
            timeRemaining = this.duration = duration;
            Sprite = sprite;
            this.icon = icon;
        }

    public sealed override void DoPickup()
    {
            // Add self to Level's collection of Modifiers
            Level.AddModifier(this);
            active = true;
            PickedUp = true;

            // Activate modifier
            Activate();
        }

    public override void Update(GameTime gameTime)
    {
            if (IsActive)
            {
                // Decrease the time remaning
                timeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Should the modifier be deactivated
                if (timeRemaining <= 0F)
                {
                    Deactivate();
                    active = false;
                }
            }
        }

    public override void Reset()
    {
            timeRemaining = duration;
            IsActive = false;

            base.Reset();
        }

    public abstract void Activate();
    public abstract void Deactivate();

    public override void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
            if (!PickedUp)
                spriteBatch.Draw(Sprite.Texture,
                                 positionOffset,
                                 Sprite.SourceRectangle,
                                 Color.White);
        }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
    /// <param name="position">Position of the upper-left corner of the modifier.</param>
    /// <param name="font">SpriteFont to use when drawing the time left</param>
    public void DrawTimer(SpriteBatch spriteBatch, Vector2 position, SpriteFont font)
    {
            // Draw icon
            spriteBatch.Draw(icon.Texture,
                            position,
                            icon.SourceRectangle,
                            Color.White);

            // Build the time string
            string timeText = "";
            TimeSpan time = TimeSpan.FromSeconds(timeRemaining);

            if (time.Minutes < 10)
                timeText += '0';

            timeText += time.Minutes.ToString();

            timeText += ':';

            if (time.Seconds < 10)
                timeText += '0';

            timeText += time.Seconds.ToString();

            // Find the position of the time string
            Vector2 timeStringSize = font.MeasureString(timeText);
            // TODO: Use other font (ModifierFont)

            Vector2 textPos = position;
            textPos.X += icon.SourceRectangle.Width + ICON_TIMER_SPACING;
            textPos.Y += (icon.SourceRectangle.Height - timeStringSize.Y) / 2;

            spriteBatch.DrawString(font, timeText, textPos, Color.White);
        }
}