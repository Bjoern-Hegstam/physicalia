using System;

namespace PhysicaliaRemastered;

public class InvalidGameStateException(string message) : Exception(message);