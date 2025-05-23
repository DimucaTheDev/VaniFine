using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VaniFine
{
    internal static class Program
    {
        private static string RPPath;
        private static FileStream fileStream;
        private static ZipArchive Zip;
        private static string Description;
        private static int Version;
        private static HttpClient Http;
        private static Dictionary<string, int> VersionMap; // id -> rp version
        private static string NewMinecraftVersion;
        private static int NewPackVersion;
        private static string NewPackPath;

        private static string AllItemsJsonUrl => $"https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/{NewMinecraftVersion}/assets/minecraft/items/_all.json";
        private static string AllItemModelsJsonUrl => $"https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/{NewMinecraftVersion}/assets/minecraft/models/item/_all.json";
        private static string AllBlockModelsJsonUrl => $"https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/{NewMinecraftVersion}/assets/minecraft/models/block/_all.json";

        private static JsonDocument AllItemsJsonDocument;
        private static JsonDocument AllItemModelsJsonDocument;
        private static JsonDocument AllBlockModelsJsonDocument;

        private const string UserAgent = "VaniFine";
        private const string MapLink = "https://vf.cmdev.pw/map";
        private const string MinimumVersion = "25w03a";

        static void Main(string[] args)
        {
            Console.WriteLine($"VaniFine. CIT-to-Vanilla Converter Tool");
            RPPath = args.FirstOrDefault() ?? FileSelector.ShowDialog();
            if (string.IsNullOrEmpty(RPPath))
            {
                Console.WriteLine("No file selected.");
                return;
            }
            Console.WriteLine($"Selected file: {RPPath}");
            if (!VerifyPack())
            {
                Console.WriteLine("Pack validation failed! Is this a valid resource pack?");
                return;
            }

            Http = new HttpClient();
            Http.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            //todo: select latest version from Mojang version manifest
            var response = Http.GetString(MapLink);
            VersionMap = JsonDocument.Parse(response).RootElement
                .EnumerateObject()
                .ToDictionary(s => s.Name, s => s.Value.GetInt32());

            var pack = GetPackMeta();
            var version =
                VersionMap.First(s => s.Value == Math.Max(VersionMap[MinimumVersion], pack.Version));
            Console.WriteLine($"Pack version: {pack.Version}");
            Console.WriteLine($"New Pack version: {version.Value} ({version.Key})");
            NewMinecraftVersion = version.Key;
            NewPackVersion = version.Value;

            GetItemsAndModels();
            NewPackPath = GenerateOutputPath();
            ExtractPackIcon();
            SavePackMetadata();
            ExtractFiles();
            var files = ProcessCitProperties();
            GenerateItemJsonFiles(files);
        }

        public static bool VerifyPack()
        {
            if (!File.Exists(RPPath))
                return false;
            if (Path.GetExtension(RPPath) != ".zip")
                return false;
            try
            {
                fileStream = File.OpenRead(RPPath);
                Zip = new ZipArchive(fileStream, ZipArchiveMode.Read);

                if (Zip.GetEntry("pack.mcmeta") is null)
                    return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
        public static (int Version, string Description) GetPackMeta()
        {
            var packMeta = Zip.GetEntry("pack.mcmeta").GetString();
            var pack = JsonDocument.Parse(packMeta).RootElement.GetProperty("pack");
            var version = pack.GetProperty("pack_format").GetInt32();
            var description = pack.GetProperty("description").GetString() ?? string.Empty;
            return (Version, Description) = (version, description);
        }
        public static void GetItemsAndModels()
        {
            AllItemsJsonDocument = JsonDocument.Parse(Http.GetString(AllItemsJsonUrl));
            AllItemModelsJsonDocument = JsonDocument.Parse(Http.GetString(AllItemModelsJsonUrl));
            AllBlockModelsJsonDocument = JsonDocument.Parse(Http.GetString(AllBlockModelsJsonUrl));
        }
        public static string GenerateOutputPath()
        {
            //string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/.minecraft/resourcepacks/GEN{new Random().Next(10000)}";
            string path = Path.GetFullPath($"test/{new Random().Next(1, 1000)}");
            Console.WriteLine($"New Pack directory: {path}");
            Directory.CreateDirectory(path);
            //todo: non-minecraft id's
            Directory.CreateDirectory(Path.Combine(path, "assets/minecraft/items"));
            Directory.CreateDirectory(Path.Combine(path, "assets/minecraft/models/item"));
            Directory.CreateDirectory(Path.Combine(path, "assets/minecraft/textures/item"));
            return path;
        }
        public static void ExtractPackIcon()
        {
            var packEntry = Zip.GetEntry("pack.png");
            if (packEntry is null) return;
            packEntry.ExtractToFile(Path.Combine(NewPackPath, "pack.png"), overwrite: true);
            Console.WriteLine("pack.png Extracted");
        }
        public static void SavePackMetadata()
        {
            var meta = new
            {
                pack = new
                {
                    pack_format = NewPackVersion,
                    description = "\u00A74[VaniFine]\u00A7r " + Description
                }
            };
            string json = JsonSerializer.Serialize(meta, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(Path.Combine(NewPackPath, "pack.mcmeta"), json);
            Console.WriteLine("Created pack.mcmeta");
        }
        public static void ExtractFiles()
        {
            foreach (var entry in Zip.Entries.Where(s => !s.IsMacFile()))
            {
                if (entry.FullName.EndsWith("/"))
                    continue;

                if (entry.FullName.Contains("/models/") || (entry.Name.EndsWith(".json") && entry.FullName.Contains("/cit/")))
                    ExtractItemModels(entry);

                if ((entry.Name.EndsWith(".mcmeta") && entry.Name != "pack.mcmeta") || entry.FullName.Contains("/textures/") || (entry.Name.EndsWith(".png") && entry.FullName.Contains("/cit/")))
                    ExtractTextures(entry);

                if (entry.FullName.Contains("/sounds/"))
                    ExtractSounds(entry);

                if (entry.FullName.Contains("/lang/"))
                    ExtractLangs(entry);
            }
        }
        public static void ExtractItemModels(ZipArchiveEntry entry)
        {
            string targetPath = Path.Combine(NewPackPath, "assets/minecraft/models/item", entry.Name);
            Console.WriteLine($"Extracted model {targetPath.Replace(NewPackPath, "")}");

            using StreamReader stream = new StreamReader(entry.Open());
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            string content = stream.ReadToEnd();

            content = content.Replace("./", "item/");

            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            if (data != null && data.ContainsKey("textures"))
            {
                var textures = JsonSerializer.Deserialize<Dictionary<string, string>>(data["textures"].ToString());

                var updatedTextures = new Dictionary<string, string>();
                foreach (var texture in textures)
                {
                    string filename = Path.GetFileNameWithoutExtension(texture.Value);
                    updatedTextures[texture.Key] = $"item/{filename}";
                }

                data["textures"] = updatedTextures;

                content = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                Console.WriteLine($"{entry.FullName}: Incorrect model");
            }

            File.WriteAllText(targetPath, content);
        }
        public static void ExtractTextures(ZipArchiveEntry entry)
        {
            string targetPath = Path.Combine(NewPackPath, "assets/minecraft/textures/item", entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            entry.ExtractToFile(targetPath, overwrite: true);
            Console.WriteLine($"Extracted texture {entry.Name}");
        }
        public static void ExtractSounds(ZipArchiveEntry entry)
        {
            string targetPath = Path.Combine(NewPackPath, entry.FullName);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            entry.ExtractToFile(targetPath, overwrite: true);
            Console.WriteLine($"Extracted sound {entry.Name}");
        }
        public static void ExtractLangs(ZipArchiveEntry entry)
        {
            string targetPath = Path.Combine(NewPackPath, entry.FullName);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            entry.ExtractToFile(targetPath, overwrite: true);
            Console.WriteLine($"Extracted lang file {entry.Name}");
        }
        public static Dictionary<string, List<Dictionary<string, string>>> ProcessCitProperties()
        {
            var citEntries = Zip.Entries
                .Where(e =>
                    !e.IsMacFile() &&
                    e.FullName.Contains("assets/minecraft/optifine/cit") &&
                    e.Name.EndsWith(".properties") &&
                    e.Name != "cit.properties")
                .ToList();

            var itemDefinitions = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (var entry in citEntries)
            {
                using var reader = new StreamReader(entry.Open());
                var readToEnd = reader.ReadToEnd();
                var lines = readToEnd.Split('\n')
                    .Select(line => line.Trim())
                    .DistinctBy(s => s.Split('=')[0])
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    .Select(line => line.Split('=', 2))
                    .ToDictionary(parts => parts[0], parts => parts[1].Replace("\r", ""));

                lines["FILE_NAME"] = entry.Name;
                ProcessCitEntry(lines, entry, itemDefinitions);
            }

            return itemDefinitions;
        }
        public static void ProcessCitEntry(
            Dictionary<string, string> lines,
            ZipArchiveEntry entry,
            Dictionary<string, List<Dictionary<string, string>>> itemDefinitions)
        {
            string? GetSanitizedItemsKey()
            {
                if (lines.TryGetValue("items", out var items))
                    return items.Replace("minecraft:", "");
                if (lines.TryGetValue("matchItems", out var matchItems))
                    return matchItems.Replace("minecraft:", "");
                return null;
            }

            var itemsRaw = GetSanitizedItemsKey();
            if (itemsRaw == null) return;
            lines["items"] = itemsRaw;

            if (!lines.ContainsKey("model") && !lines.ContainsKey("texture"))
            {
                var pngName = Path.GetFileNameWithoutExtension(entry.FullName) + ".png";
                var pngPath = Path.Combine(Path.GetDirectoryName(entry.FullName)!, pngName).Replace("\\", "/");
                if (Zip.GetEntry(pngPath) != null)
                    lines["texture"] = Path.GetFileName(pngName);
            }

            foreach (var itemKey in itemsRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var itemData = new Dictionary<string, string>(lines);

                if (!itemDefinitions.ContainsKey(itemKey))
                    itemDefinitions[itemKey] = new();

                itemDefinitions[itemKey].Add(itemData);
            }
        }
        public static void GenerateItemJsonFiles(Dictionary<string, List<Dictionary<string, string>>> itemDefinitions)
        {
            int index = 0;
            string allNames = $"{NewPackPath}/names.txt";
            File.WriteAllText(allNames, $"Generated and converted using VaniFine tool. See https://github.com/DimucaTheDev/VaniFine\r\nItems: {itemDefinitions.Count}\r\nUnique names: {itemDefinitions.Values.SelectMany(s => s).Sum(s => s.Count + 1)}");

            foreach (var (item, definitions) in itemDefinitions)
            {
                File.AppendAllText(allNames, $"\r\n{++index}) {item}\r\n");
                GenerateJsonForItem(item, definitions, index, allNames);
            }
        }
        public static void GenerateJsonForItem(string item, List<Dictionary<string, string>> definitions, int itemIndex, string allNames)
        {
            int variantIndex = 1;
            var usedEnchantments = new Dictionary<string, List<int>>();
            var usedVariants = new HashSet<string>();
            var cases = new List<string>();
            string component = "";

            foreach (var config in definitions)
            {
                if (config.Keys.Any(k => k.Contains("nbt.StoredEnchantments")))
                {
                    Console.WriteLine($"SKIPPED {config["FILE_NAME"]}: TODO");
                    continue;
                }

                ProcessEnchantmentKeys(config);

                if (string.IsNullOrWhiteSpace(component))
                    component = DetectComponent(config);
                if (string.IsNullOrWhiteSpace(component) && config.ContainsKey("enchantmentIDs"))
                    component = "stored_enchantments";

                var value = ExtractComponentValue(config);
                if (string.IsNullOrWhiteSpace(value) || usedVariants.Contains(value) || value == "minecraft:empty")
                    continue;

                usedVariants.Add(value);

                if (config.ContainsKey("enchantmentIDs"))
                {
                    var enchantmentCases = GenerateEnchantmentCases(config, usedEnchantments);
                    cases.AddRange(enchantmentCases);
                    File.AppendAllText(allNames, $"\t{itemIndex}.{variantIndex++}) minecraft:{config["enchantmentIDs"]}\r\n");
                    continue;
                }

                string model = GenerateModelName(config);
                string caseString = GenerateCase(model, component, value);
                cases.Add(caseString);
                File.AppendAllText(allNames, $"\t{itemIndex}.{variantIndex++}) {value.GetName()}\r\n");
            }

            string output = CreateItemJson(item, component, cases);
            string outputFilePath = Path.Combine(NewPackPath, $"assets/minecraft/items/{item}.json");
            File.WriteAllText(outputFilePath, output);
            Console.WriteLine($"generated item {item} at assets/minecraft/items/{item}.json");
        }
        public static void ProcessEnchantmentKeys(Dictionary<string, string> config)
        {
            if (config.ContainsKey("enchantments") && !config.ContainsKey("enchantmentIDs"))
            {
                int index = config["enchantments"].IndexOf(":", StringComparison.Ordinal);
                if (index != -1)
                    config["enchantmentIDs"] = config["enchantments"][(index + 1)..];
            }
        }
        public static string DetectComponent(Dictionary<string, string> config)
        {
            return config.FirstOrDefault(kvp => kvp.Key.StartsWith("nbt.") || kvp.Key.StartsWith("component")).Key
                ?.Replace("nbt.", "").ToLower()!;
        }
        public static string ExtractComponentValue(Dictionary<string, string> config)
        {
            return config.FirstOrDefault(kvp => kvp.Key.StartsWith("nbt.") || kvp.Key.StartsWith("component")).Value;
        }
        public static string GenerateModelName(Dictionary<string, string> config)
        {
            string confFileName = Path.GetFileNameWithoutExtension(config["FILE_NAME"]);
            string model = config.FirstOrDefault(kvp => kvp.Key.StartsWith("model")).Value?.Replace(" ", "").Replace(".json", "") ?? $"item/{confFileName}";
            model = Path.GetFileName(model);

            if (!config.ContainsKey("model") && config.TryGetValue("texture", out var texture))
            {
                File.WriteAllText(
                    Path.Combine(NewPackPath, "assets/minecraft/models/item", $"{confFileName}.json"),
                    Templates.SampleItemModelTemplate.Replace("ITEM", Path.GetFileNameWithoutExtension(texture.Replace(".png", "")))
                );
            }

            return model.StartsWith("item") ? model : "item/" + model;
        }
        public static IEnumerable<string> GenerateEnchantmentCases(Dictionary<string, string> config, Dictionary<string, List<int>> usedEnchantments)
        {
            string[] enchantments = config["enchantmentIDs"]
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToArray();

            foreach (var enchantment in enchantments)
            {
                if (config.TryGetValue("enchantmentLevels", out var levelsStr))
                {
                    int level = int.Parse(levelsStr.Split(' ')[0].Split('-')[0]);
                    if (usedEnchantments.TryGetValue(enchantment, out var levels) && levels.Contains(level)) continue;
                    usedEnchantments.TryAdd(enchantment, []);
                    usedEnchantments[enchantment].Add(level);

                    yield return Templates.CaseTemplate
                        .Replace("MODEL", $"item/{Path.GetFileNameWithoutExtension(config["texture"])}")
                        .Replace("WHEN", JsonSerializer.Serialize(new Dictionary<string, int> { { $"minecraft:{enchantment}", level } }));
                }
                else
                {
                    for (int level = 1; level <= 5; level++)
                    {
                        if (usedEnchantments.TryGetValue(enchantment, out var levels) && levels.Contains(level)) continue;
                        usedEnchantments.TryAdd(enchantment, []);
                        usedEnchantments[enchantment].Add(level);

                        yield return Templates.CaseTemplate
                            .Replace("MODEL", $"item/{Path.GetFileNameWithoutExtension(config["texture"])}")
                            .Replace("WHEN", JsonSerializer.Serialize(new Dictionary<string, int> { { $"minecraft:{enchantment}", level } }));
                    }
                }
            }
        }
        public static string GenerateCase(string model, string component, string value)
        {
            object whenCondition = component switch
            {
                "potion" or "potion_contents" => new Dictionary<string, string> { { "potion", value } },
                "display.name" => value.GetName(),
                "components.entity_data.variant" => value,
                _ => throw new NotImplementedException($"Unknown component type: {component}")
            };

            return Templates.CaseTemplate
                .Replace("MODEL", model)
                .Replace("WHEN", JsonSerializer.Serialize(whenCondition, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        }
        public static string CreateItemJson(string item, string component, List<string> cases)
        {
            if (cases.Count == 0)
                return Templates.EmptyCaseTemplate.Replace("ITEM", item);

            string nbtName = component switch
            {
                "potion" => "minecraft:potion_contents",
                "display.name" => "minecraft:custom_name",
                "components.entity_data.variant" => "minecraft:painting/variant",
                "stored_enchantments" => "minecraft:stored_enchantments",
                _ => throw new NotImplementedException($"Unknown component type: {component}")
            };

            return Templates.DefinitionTemplate
                .Replace("NBT_NAME", nbtName)
                .Replace("BLOCK_OR_ITEM", "item")
                .Replace("CASES", string.Join(",", cases))
                .Replace("ITEM", item)
                .Replace("FALLBACK", GetFallbackModel(item));
        }
        public static string GetFallbackModel(string item)
        {
            return AllItemsJsonDocument.RootElement.TryGetProperty(item, out var value)
                ? value.GetProperty("model").GetRawText()[1..^1] // remove '{' and '}'. do not use Trim()!!
                : $"\"type\": \"minecraft:model\",\r\n            \"model\": \"minecraft:item/{item}\""; //default item model
        }
        public static string GetString(this HttpClient client, string url)
        {
            Console.WriteLine("GET " + url);
            return client.GetStringAsync(url).Result;
        }
        public static string GetString(this ZipArchiveEntry? entry)
        {
            using var reader = new StreamReader(entry!.Open());
            return reader.ReadToEnd();
        }
        public static bool IsMacFile(this ZipArchiveEntry e)
        {
            return e.FullName.Contains("__MACOSX");
        }
        private static string GetName(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            string v = value;
            if (v.Contains("regex:"))
            {
                v = ProcessRegexInput(value.Replace("iregex:", "").Replace("regex:", ""));
            }
            v = v.Replace("ipattern:", "").Replace("pattern:", "").Replace("iregex:", "").Replace("regex:", "");
            v = Regex.Unescape(v).Replace(".*", "");
            return v;
        }
        private static string ProcessRegexInput(string input)
        {
            Regex regex = new Regex(@"\((.*?)\)");

            Match match = regex.Match(input);

            if (match.Success)
            {
                string insideParentheses = match.Groups[1].Value;

                string firstElement = insideParentheses.Split('|')[0];
                string result = input.Replace(match.Value, firstElement);
                result = Regex.Replace(result, @"[\^\$\(\)\|\[\]\{\}]", "");
                return result.Trim();
            }

            return input;
        }
    }
}
