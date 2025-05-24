namespace VaniFine
{
    internal static class Templates
    {
        public static readonly string DefinitionTemplate =
            """
            {
                "model": {
                    "type": "minecraft:select",
                    "property": "minecraft:component",
                    "component": "NBT_NAME",
                    "cases": [
                        CASES
                    ],
                    "fallback": {
                        FALLBACK
                    }
                }
            }
            """;

        public static readonly string CaseTemplate =
            """
            {
                "model": {
                    "type": "minecraft:model",
                    "model": "minecraft:MODEL"
                },
                "when": WHEN
            }
            """;

        public static readonly string CaseBowTemplate =
            """
            {
                "model": 
                {
                    "type": "minecraft:condition",
                    "on_false": 
                    {
                        "type": "minecraft:model",
                        "model": "minecraft:MODEL"
                    },
                    "on_true": 
                    {
                        "type": "minecraft:range_dispatch",
                        "entries": 
                        [
                            {
                                "model": 
                                {
                                    "type": "minecraft:model",
                                    "model": "minecraft:item/PULLING_1"
                                },
                                "threshold": 0.65
                            },
                            {
                                "model": 
                                {
                                    "type": "minecraft:model",
                                    "model": "minecraft:item/PULLING_2"
                                },
                                "threshold": 0.9
                            }
                        ],
                        "fallback": 
                        {
                            "type": "minecraft:model",
                            "model": "minecraft:item/PULLING_0"
                        },
                        "property": "minecraft:use_duration",
                        "scale": 0.05
                    },
                    "property": "minecraft:using_item"
                },
                "when": WHEN
            }
            """;

        public static readonly string EmptyCaseTemplate =
            """
            {
                "model": {
                    "type": "minecraft:model",
                    "model": "minecraft:item/ITEM"
                }
            }
            """;
        public static readonly string SampleItemModelTemplate =
            """
            {
                "parent": "item/generated",
                "textures": {
                    "layer0": "item/ITEM"
                }
            }
            """;
    }
}