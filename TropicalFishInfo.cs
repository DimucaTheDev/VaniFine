namespace VaniFine
{
    internal struct TropicalFishInfo
    {
        public static TropicalFishInfo FromInt(int variant, Dictionary<string, string> definition)
        {
            //  https://minecraft.wiki/w/Tropical_Fish#Entity_data

            byte patternColor = (byte)((variant >> 24) & 0xFF); // MSB
            byte baseColor = (byte)((variant >> 16) & 0xFF); // 2nd MSB
            byte pattern = (byte)((variant >> 8) & 0xFF);  // 2nd LSB
            byte shape = (byte)(variant & 0xFF);         // LSB 
            return new TropicalFishInfo
            {
                PatternColor = (Color)patternColor,
                BaseColor = (Color)baseColor,
                Pattern = pattern,
                Shape = shape,
                Definition = definition,
                Integer = variant
            };
        }
        public static Dictionary<byte, Dictionary<byte, string>> FishNames = new()
        {
            [0] = new()
            {
                [0] = "Kob",
                [1] = "Sunstreak",
                [2] = "Snooper",
                [3] = "Dasher",
                [4] = "Brinely",
                [5] = "Spotty",
            },
            [1] = new()
            {
                [0] = "Flopper",
                [1] = "Stripey",
                [2] = "Glitter",
                [3] = "Blockfish",
                [4] = "Betty",
                [5] = "Clayfish",
            }
        };

        public string GetName() => FishNames[Shape][Pattern];

        public Color PatternColor { get; set; }
        public Color BaseColor { get; set; }
        public byte Pattern { get; set; }
        public byte Shape { get; set; }
        public int Integer { get; set; }
        public Dictionary<string, string> Definition { get; set; }
    }
    enum Color : byte
    {
        White,
        Orange,
        Magenta,
        Light_Blue,
        Yellow,
        Lime,
        Pink,
        Gray,
        Light_Gray,
        Cyan,
        Purple,
        Blue,
        Brown,
        Green,
        Red,
        Black
    }
}
