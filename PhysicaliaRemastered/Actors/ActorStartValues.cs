using System.Xml;
using Microsoft.Xna.Framework;

namespace PhysicaliaRemastered.Actors;

/// <summary>
/// Struct for defining the start values for a Enemy.
/// </summary>
public struct ActorStartValues
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;

    public ActorStartValues(Vector2 position, Vector2 velocity, Vector2 acceleration)
    {
            Position = position;
            Velocity = velocity;
            Acceleration = acceleration;
        }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="endElement">Local name of the last element to read.</param>
    /// <returns></returns>
    public static ActorStartValues FromXml(XmlReader reader, string endElement)
    {
            ActorStartValues startValues = new ActorStartValues();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Position")
                    startValues.Position = ReadVector2(reader);

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Velocity")
                    startValues.Velocity = ReadVector2(reader);

                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName == "Acceleration")
                    startValues.Acceleration = ReadVector2(reader);

                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.LocalName == endElement)
                    return startValues;
            }

            return startValues;
        }

    public static ActorStartValues FromXml(XmlReader reader)
    {
            return FromXml(reader, "ActorStartValues");
        }

    private static Vector2 ReadVector2(XmlReader reader)
    {
            float x = float.Parse(reader.GetAttribute("x"));
            float y = float.Parse(reader.GetAttribute("y"));

            return new Vector2(x, y);
        }
}