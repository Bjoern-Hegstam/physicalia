using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicaliaRemastered.GameManagement;
using XNALibrary.Sprites;

namespace PhysicaliaRemastered.Pickups;

/// <summary>
/// Abstract class that acts as a base for all Pickups that are intended to
/// change the behavior of the game.
/// </summary>
public abstract class ModifierPickup : Pickup
{
    private const float IconTimerSpacing = 5F;
    private const float DefaultDuration = 5F;

    private float _duration;

    private Sprite _icon;

    public bool IsActive { get; set; }

    public float Duration
    {
        get => _duration;
        set => TimeRemaining = _duration = value;
    }

    public float TimeRemaining { get; set; }

    public Sprite Icon
    {
        get => _icon;
        set => _icon = value;
    }

    public ModifierPickup(Level level, Sprite icon, Sprite sprite, float duration)
        : base(level)
    {
        TimeRemaining = _duration = duration;
        Sprite = sprite;
        _icon = icon;
    }

    public sealed override void DoPickup()
    {
        // Add self to Level's collection of Modifiers
        Level.AddModifier(this);
        IsActive = true;
        PickedUp = true;

        // Activate modifier
        Activate();
    }

    public override void Update(GameTime gameTime)
    {
        if (IsActive)
        {
            // Decrease the time remaning
            TimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Should the modifier be deactivated
            if (TimeRemaining <= 0F)
            {
                Deactivate();
                IsActive = false;
            }
        }
    }

    public override void Reset()
    {
        TimeRemaining = _duration;
        IsActive = false;

        base.Reset();
    }

    public abstract void Activate();
    public abstract void Deactivate();

    public override void Draw(SpriteBatch? spriteBatch, Vector2 positionOffset)
    {
        if (!PickedUp)
        {
            spriteBatch.Draw(Sprite.Texture,
                positionOffset,
                Sprite.SourceRectangle,
                Color.White);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spriteBatch">SpriteBatch to use for drawing.</param>
    /// <param name="position">Position of the upper-left corner of the modifier.</param>
    /// <param name="font">SpriteFont to use when drawing the time left</param>
    public void DrawTimer(SpriteBatch? spriteBatch, Vector2 position, SpriteFont font)
    {
        // Draw icon
        spriteBatch.Draw(_icon.Texture,
            position,
            _icon.SourceRectangle,
            Color.White);

        // Build the time string
        var timeText = "";
        TimeSpan time = TimeSpan.FromSeconds(TimeRemaining);

        if (time.Minutes < 10)
        {
            timeText += '0';
        }

        timeText += time.Minutes.ToString();

        timeText += ':';

        if (time.Seconds < 10)
        {
            timeText += '0';
        }

        timeText += time.Seconds.ToString();

        // Find the position of the time string
        Vector2 timeStringSize = font.MeasureString(timeText);
        // TODO: Use other font (ModifierFont)

        Vector2 textPos = position;
        textPos.X += _icon.SourceRectangle.Width + IconTimerSpacing;
        textPos.Y += (_icon.SourceRectangle.Height - timeStringSize.Y) / 2;

        spriteBatch.DrawString(font, timeText, textPos, Color.White);
    }
}