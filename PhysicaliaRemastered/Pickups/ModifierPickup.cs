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
    #region Fields

    private const float ICON_TIMER_SPACING = 5F;
    private const float DEFAULT_DURATION = 5F;

    private bool active;
    private float duration;
    private float timeRemaining;

    private Sprite icon;

    #endregion

    #region Properties

    public bool IsActive
    {
        get { return this.active; }
        set { this.active = value; }
    }

    public float Duration
    {
        get { return this.duration; }
        set { this.timeRemaining = this.duration = value; }
    }

    public float TimeRemaining
    {
        get { return this.timeRemaining; }
        set { this.timeRemaining = value; }
    }

    public Sprite Icon
    {
        get { return this.icon; }
        set { this.icon = value; }
    }

    #endregion

    public ModifierPickup(Level level, Sprite icon, Sprite sprite, float duration)
        : base(level)
    {
            this.timeRemaining = this.duration = duration;
            this.Sprite = sprite;
            this.icon = icon;
        }

    #region Methods

    public sealed override void DoPickup()
    {
            // Add self to Level's collection of Modifiers
            this.Level.AddModifier(this);
            this.active = true;
            this.PickedUp = true;

            // Activate modifier
            this.Activate();
        }

    public override void Update(GameTime gameTime)
    {
            if (this.IsActive)
            {
                // Decrease the time remaning
                this.timeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Should the modifier be deactivated
                if (this.timeRemaining <= 0F)
                {
                    this.Deactivate();
                    this.active = false;
                }
            }
        }

    public override void Reset()
    {
            this.timeRemaining = this.duration;
            this.IsActive = false;

            base.Reset();
        }

    public abstract void Activate();
    public abstract void Deactivate();

    public override void Draw(SpriteBatch spriteBatch, Vector2 positionOffset)
    {
            if (!this.PickedUp)
                spriteBatch.Draw(this.Sprite.Texture,
                                 positionOffset,
                                 this.Sprite.SourceRectangle,
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
            spriteBatch.Draw(this.icon.Texture,
                            position,
                            this.icon.SourceRectangle,
                            Color.White);

            // Build the time string
            string timeText = "";
            TimeSpan time = TimeSpan.FromSeconds(this.timeRemaining);

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
            textPos.X += this.icon.SourceRectangle.Width + ICON_TIMER_SPACING;
            textPos.Y += (this.icon.SourceRectangle.Height - timeStringSize.Y) / 2;

            spriteBatch.DrawString(font, timeText, textPos, Color.White);
        }

    #endregion
}