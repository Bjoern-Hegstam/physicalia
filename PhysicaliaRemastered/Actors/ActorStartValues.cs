using System.Xml;
using Microsoft.Xna.Framework;
using XNALibrary;

namespace PhysicaliaRemastered.Actors;

/// <summary>
/// Struct for defining the start values for a Enemy.
/// </summary>
public struct ActorStartValues(Vector2 position, Vector2 velocity, Vector2 acceleration)
{
    public Vector2 Position = position;
    public Vector2 Velocity = velocity;
    public Vector2 Acceleration = acceleration;

    public static ActorStartValues FromXml(XmlReader reader, string endElement)
    {
        var startValues = new ActorStartValues();

        while (reader.Read())
        {
            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Position" })
            {
                startValues.Position = ReadVector2(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Velocity" })
            {
                startValues.Velocity = ReadVector2(reader);
            }

            if (reader is { NodeType: XmlNodeType.Element, LocalName: "Acceleration" })
            {
                startValues.Acceleration = ReadVector2(reader);
            }

            if (reader.NodeType == XmlNodeType.EndElement &&
                reader.LocalName == endElement)
            {
                return startValues;
            }
        }

        return startValues;
    }

    private static Vector2 ReadVector2(XmlReader reader)
    {
        float x = float.Parse(reader.GetAttribute("x") ?? throw new ResourceLoadException());
        float y = float.Parse(reader.GetAttribute("y") ?? throw new ResourceLoadException());

        return new Vector2(x, y);
    }
}