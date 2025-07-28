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
        public static readonly string DamageDefinitionTemplate =
            """
            {
                "model": {
                    "type": "minecraft:range_dispatch",
                    "property": "minecraft:damage",
                    "entries": [
                        ENTRY
                    ],
                    "fallback": {
                        FALLBACK
                    }
                }
            }
            """;

        public static readonly string DamageEntry =
            """
            {
                "threshold": THRESHOLD,
                "model": {
                    MODEL
                }
            }
            """;

        public static readonly string Combined =
            """
            {
              "parent": "item/PARENT",
              "textures": {
                "layer0": "item/TEXTURE"
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
        public static readonly string RangedCaseTemplate =
            """ 
                    "type": "minecraft:model",
                    "model": "minecraft:MODEL"
                 
            """;
        public static readonly string CaseCrossbowTemplate =
            """
            {
              "model": {
                "type": "minecraft:select",
                "cases": [
                  {
                    "model": {
                      "type": "minecraft:model",
                      "model": "minecraft:item/ARROW"
                    },
                    "when": "arrow"
                  },
                  {
                    "model": {
                      "type": "minecraft:model",
                      "model": "minecraft:item/FIREWORK"
                    },
                    "when": "rocket"
                  }
                ],
                "fallback": {
                  "type": "minecraft:condition",
                  "on_false": {
                    "type": "minecraft:model",
                    "model": "minecraft:MODEL"
                  },
                  "on_true": {
                    "type": "minecraft:range_dispatch",
                    "entries": [
                      {
                        "model": {
                          "type": "minecraft:model",
                          "model": "minecraft:item/PULLING_1"
                        },
                        "threshold": 0.58
                      },
                      {
                        "model": {
                          "type": "minecraft:model",
                          "model": "minecraft:item/PULLING_2"
                        },
                        "threshold": 1.0
                      }
                    ],
                    "fallback": {
                      "type": "minecraft:model",
                      "model": "minecraft:item/PULLING_0"
                    },
                    "property": "minecraft:crossbow/pull"
                  },
                  "property": "minecraft:using_item"
                },
                "property": "minecraft:charge_type"
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
        public static readonly string RangedCaseCrossbowTemplate =
            """ 
                "type": "minecraft:select",
                "cases": [
                  {
                    "model": {
                      "type": "minecraft:model",
                      "model": "minecraft:item/ARROW"
                    },
                    "when": "arrow"
                  },
                  {
                    "model": {
                      "type": "minecraft:model",
                      "model": "minecraft:item/FIREWORK"
                    },
                    "when": "rocket"
                  }
                ],
                "fallback": {
                  "type": "minecraft:condition",
                  "on_false": {
                    "type": "minecraft:model",
                    "model": "minecraft:MODEL"
                  },
                  "on_true": {
                    "type": "minecraft:range_dispatch",
                    "entries": [
                      {
                        "model": {
                          "type": "minecraft:model",
                          "model": "minecraft:item/PULLING_1"
                        },
                        "threshold": 0.58
                      },
                      {
                        "model": {
                          "type": "minecraft:model",
                          "model": "minecraft:item/PULLING_2"
                        },
                        "threshold": 1.0
                      }
                    ],
                    "fallback": {
                      "type": "minecraft:model",
                      "model": "minecraft:item/PULLING_0"
                    },
                    "property": "minecraft:crossbow/pull"
                  },
                  "property": "minecraft:using_item"
                },
                "property": "minecraft:charge_type"
               
            """;
        public static readonly string RangedCaseBowTemplate =
            """
             
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