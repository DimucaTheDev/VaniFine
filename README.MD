# VaniFine
![Tested on](https://badge.ttsalpha.com/api?label=Tested on&status=25w03a&color=a64B00)
![Resource Pack Version](https://badge.ttsalpha.com/api?label=Resource Pack Version&status=48&color=ddd)
## What's That
This is a small but helpful tool that allows you to convert [CIT](https://modrinth.com/mod/cit-resewn)/OptiFine resource packs to vanilla ones.

### Why it works?
Since snapshot [25w03a](https://minecraft.wiki/w/Java_Edition_25w03a) there is a feature has been added that allows Minecraft client to select a specific model of an **item** among others, depending on its component value.
For example, using a `minecraft:custom_name` component allows us to specify an item name in order to use this model

### What exactly is [sup]ported?
This is a table of what is can be ported and what is not:

| Feature | Is Supported |
| --- | --- |
| Items | ✅ |
| Potions | ✅ |
| Enchantments | ❌([MC-279504](https://bugs.mojang.com/browse/MC-279504)) |
| Block items | ✅ |
| Blocks | ❌(not supported) |
| Armor | ❌(only item model) |
| Entities | ❌(not supported) |

#### What about Regular Expressions?
expressions like `iregex:(Text A | Text B)` will be replaced with `Text A`, and case-insensitivity is **not supported**.

## How to use
1. Download the latest release from the [releases page](https://github.com/DimucaTheDev/VaniFine/releases)
2. Install .NET 9.0 Runtime from the [official website](https://dotnet.microsoft.com/download/dotnet/9.0)
3. Start `VaniFine.exe`
4. Select resource pack
5. \*magic happens*

As a result a new folder will be created in `.minecraft/resourcepacks` with the converted resource pack.
And as a bonus, a `names.txt` file will be created in that folder, with all available item names.

## Licensing & Redistribution
This project is licensed under the [MIT License](LICENSE). You can use this project in your own projects, but you must credit me as the original author.