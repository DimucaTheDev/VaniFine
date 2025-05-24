# VaniFine 
![Tested on](https://badge.ttsalpha.com/api?label=Tested%20on&status=1.21.5&color=009E00)
![Resource Pack Version](https://badge.ttsalpha.com/api?label=Resource%20Pack%20Version&status=55&color=ddd)
## What's This?
This is a small but helpful tool that allows you to convert [CIT](https://modrinth.com/mod/cit-resewn)/OptiFine resource packs to a vanilla compatible ones.

### Why it works?
Since snapshot [25w03a](https://minecraft.wiki/w/Java_Edition_25w03a), a feature has been added that allows Minecraft client to select a specific model of an **item** among others, depending on its component value.
For example, using a `minecraft:custom_name` component allows us to specify an item name in order to use this model

### What exactly is [sup]ported?
This is a table of what is can be ported and what is not:

| Feature | Is Supported |
| --- | --- |
| Basic Items | ✅ |
| Potions | ✅ |
| Enchantments | ✅ |
| Bows/Crossbows | ✅ |
| Blocks | ⚠️(only item model) |
| Armor | Planned* |
| Entities | ❌(not supported) |
| Paintings | ⚠️(only item model) |

* For armor models, you will probably need to install generated datapack in world folder, + obtaining these armor items will require operator permissions (to execute `/give` command)

#### What about Regular Expressions?
expressions like `iregex:(Text A | Text B)` will be replaced with `Text A`, and case-insensitivity is **not supported**.

## How to use
1. Download the latest release from the [releases page](https://github.com/DimucaTheDev/VaniFine/releases)
2. Install .NET 9.0 Runtime from the [official website](https://dotnet.microsoft.com/download/dotnet/9.0)
3. Start `VaniFine.exe`
4. Select resource pack
5. \*magic happens*

As a result a new folder will be created in `.minecraft/resourcepacks` with the converted resource pack.
And as a bonus, a `names.txt` file will be created in that folder, with all available item names/enchantments.

# Gallery
Poster on a wall. [КСЕПСП](https://www.planetminecraft.com/texture-pack/ksepsp-v9-optifine-cit/)

![Poster. Renamed painting in item frame.](https://github.com/DimucaTheDev/VaniFine/blob/cfc9b71d1077ab81efe2cc3307759e9a3a72553c/images/poster.png)

Bundles! [Bundled](https://modrinth.com/mod/bundled_)

![Bundles!](https://github.com/DimucaTheDev/VaniFine/blob/cfc9b71d1077ab81efe2cc3307759e9a3a72553c/images/renamed_bundles_and_a_potion.png)

Telescope. [КСЕПСП](https://www.planetminecraft.com/texture-pack/ksepsp-v9-optifine-cit/)

![Renamed spyglass in item frame](https://github.com/DimucaTheDev/VaniFine/blob/cfc9b71d1077ab81efe2cc3307759e9a3a72553c/images/telescope.png)

Renaming an apple to `lemon`.

![](https://github.com/DimucaTheDev/VaniFine/blob/21a6ebf2c9825bd98867db375fc3c5f7ad4e54cb/images/anvil.png)

Converting a pack.

![](https://github.com/DimucaTheDev/VaniFine/blob/cfc9b71d1077ab81efe2cc3307759e9a3a72553c/images/in_progress.png)

Enchanted books. [Enchanted+](https://www.planetminecraft.com/texture-pack/enchantments-5637237/)

![Enchanted books in creative inventory](https://github.com/DimucaTheDev/VaniFine/blob/a57096f8052472e20862fc0f507fe38f09f472f1/images/enchanted_books.png)

## TODO: 
- Add armor support (using datapack)
- Parse this: `nbt.StoredEnchantments.0.id=pattern:*`
 
## Licensing & Redistribution
This project is licensed under the [MIT License](LICENSE). You can use this project in your own projects, but you must credit me as the original author.
