﻿using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VaniFine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }
    internal static class Program
    {
        private static JsonDocument AllItems;
        private static JsonDocument AllItemModels;
        private static JsonDocument AllBlockModels;
        private const int ResourcePackVersion = 49;
        #region
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName(ref OpenFileName ofn);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
        private static string ShowDialog()
        {
            var ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf(ofn);
            // Define Filter for your extensions (Excel, ...)
            ofn.lpstrFilter = "ZIP Archive\0*.zip\0All Files (*.*)\0*.*\0";
            ofn.lpstrFile = new string(new char[256]);
            ofn.nMaxFile = ofn.lpstrFile.Length;
            ofn.lpstrFileTitle = new string(new char[64]);
            ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
            ofn.lpstrTitle = "Select Resource Pack";
            if (GetOpenFileName(ref ofn))
                return ofn.lpstrFile;
            return string.Empty;
        }
        public struct Pack
        {
            [JsonPropertyName("description")]
            public string Description { get; set; }
            [JsonPropertyName("pack_format")]
            public int PackFormat { get; set; }
        }

        private static readonly string DefinitionTemplate =
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

        private static readonly string CaseTemplate =
            """
            {
                "model": {
                    "type": "minecraft:model",
                    "model": "minecraft:MODEL"
                },
                "when": WHEN
            }
            """;

        private static readonly string EmptyCaseTemplate =
            """
            {
                "model": {
                    "type": "minecraft:model",
                    "model": "minecraft:item/ITEM"
                }
            }
            """;
        private static readonly string SampleItemModelTemplate =
            """
            {
                "parent": "item/generated",
                "textures": {
                    "layer0": "item/ITEM"
                }
            }
            """;

        private static string WaterMark = "Generated by VaniFine. https://github.com/DimucaTheDev/VaniFine\r\n";
        private static string ProcessRegexInput(string input)
        {
            // Regular expression to find the content inside parentheses
            Regex regex = new Regex(@"\((.*?)\)");

            // Match to extract the first element in parentheses
            Match match = regex.Match(input);

            if (match.Success)
            {
                // Extract the content inside the parentheses
                string insideParentheses = match.Groups[1].Value;

                // Get the first element inside the parentheses (split by '|')
                string firstElement = insideParentheses.Split('|')[0];

                // Remove any regex symbols (e.g., ^, $, parentheses)
                string result = input.Replace(match.Value, firstElement);  // Replace parentheses content with first element
                result = Regex.Replace(result, @"[\^\$\(\)\|\[\]\{\}]", ""); // Remove regex symbols

                return result.Trim(); // Trim any leading or trailing spaces
            }

            // Return the original string if no match is found
            return input;
        }
        #endregion
        private static bool CantVerifyBlocks;
        private static string GetName(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            string v = value;
            if (v.Contains("regex:"))
            {
                v = ProcessRegexInput(value.Replace("iregex:", "").Replace("regex:", ""));
            }
            v = v.Replace("ipattern:", "").Replace("pattern:", "").Replace("iregex:", "").Replace("regex:", "");
            v = Regex.Unescape(v);
            return v;
        }
        private static bool IsMacFile(this ZipArchiveEntry e)
        {
            return e.FullName.Contains("__MACOSX");
        }

        static void Main(string[] args)
        {
        start:
            string packPath = ShowDialog();
            Console.WriteLine(packPath);
            ZipArchive? test = null;
            try
            {
                if (null == (test = new ZipArchive(File.OpenRead(packPath))).GetEntry("pack.mcmeta"))
                    throw new Exception(); // no pack.mcmeta = not a pack
            }
            catch
            {
                if (4 /*retry*/ ==
                    MessageBox(0, "File is not a resource pack!", "", 64 /*(X) */ + 5 /*Retry + Cancel*/))
                {
                    Console.WriteLine("File is not a resource pack!");
                    test?.Dispose();
                    goto start;
                }
                return;
            }

            try
            {
                // this code gets all items' models from the latest version of minecraft
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "OptiVine");

                Console.WriteLine("https://api.github.com/repos/InventivetalentDev/minecraft-assets/tags");

                var async = httpClient.GetAsync(
                    "https://api.github.com/repos/InventivetalentDev/minecraft-assets/tags");
                async.Wait();
                if (!async.Result.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{nameof(CantVerifyBlocks)} = {CantVerifyBlocks}");
                    CantVerifyBlocks = true;
                }
                else
                {
                    var jsonDocument = JsonDocument.Parse(async.Result.Content.ReadAsStringAsync().Result);
                    string latestTag = jsonDocument.RootElement.EnumerateArray().First().GetProperty("name")
                        .GetString();

                    var requestUri = $"https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/{latestTag}/assets/minecraft/items/_all.json";
                    var uri = $"https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/{latestTag}/assets/minecraft/models/item/_all.json";
                    var s = $"https://raw.githubusercontent.com/InventivetalentDev/minecraft-assets/{latestTag}/assets/minecraft/models/block/_all.json";

                    Console.WriteLine(requestUri);
                    var path1 = Path.Combine(Path.GetTempPath(), $"{latestTag}_items.json");
                    string content1 = File.Exists(path1) ? File.ReadAllText(path1) : httpClient.GetStringAsync(requestUri).Result;
                    File.WriteAllText(path1, content1);
                    AllItems = JsonDocument.Parse(content1);

                    Console.WriteLine(uri);
                    var path2 = Path.Combine(Path.GetTempPath(), $"{latestTag}_item_models.json");
                    string content2 = File.Exists(path2) ? File.ReadAllText(path2) : httpClient.GetStringAsync(uri).Result;
                    File.WriteAllText(path2, content2);
                    AllItemModels = JsonDocument.Parse(content2);

                    Console.WriteLine(s);
                    var path3 = Path.Combine(Path.GetTempPath(), $"{latestTag}_blocks_models.json");
                    string content3 = File.Exists(path3) ? File.ReadAllText(path3) : httpClient.GetStringAsync(s).Result;
                    File.WriteAllText(path3, content3);
                    AllBlockModels = JsonDocument.Parse(content3);
                }
            }
            catch { CantVerifyBlocks = true; }

            string outputPath = GenerateOutputPath();
            using var zipArchive = new ZipArchive(File.OpenRead(packPath), ZipArchiveMode.Read);

            // Process pack metadata
            var packMetadata = ExtractPackMetadata(zipArchive);
            ExtractPackIcon(zipArchive, outputPath);
            SavePackMetadata(packMetadata, outputPath);

            // Extract and process files
            ExtractFiles(zipArchive, outputPath);
            var itemDefinitions = ProcessCitProperties(zipArchive);

            // Generate item JSON files
            GenerateItemJsonFiles(itemDefinitions, outputPath);
            Process.Start("explorer.exe", outputPath.Replace("/", "\\"));
            Console.WriteLine("Resource pack conversion completed.");
        }
        private static string GenerateOutputPath()
        {
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/.minecraft/resourcepacks/GEN{new Random().Next(10000)}";
            Console.WriteLine($"Pack dir: {path}");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(path, "assets/minecraft/items"));
            Directory.CreateDirectory(Path.Combine(path, "assets/minecraft/models/item"));
            Directory.CreateDirectory(Path.Combine(path, "assets/minecraft/textures/item"));
            return path;
        }
        private static string GetFallbackModel(string item)
        {
            return AllItems.RootElement.TryGetProperty(item, out var value)
                ? value.GetProperty("model").GetRawText()[1..^1] // remove '{' and '}'. do not use Trim()!!
                : $"\"type\": \"minecraft:model\",\r\n            \"model\": \"minecraft:item/{item}\""; //default item model
        }
        private static Dictionary<string, Pack> ExtractPackMetadata(ZipArchive zipArchive)
        {
            var packEntry = zipArchive.GetEntry("pack.mcmeta");
            if (packEntry == null) throw new FileNotFoundException("pack.mcmeta not found in the archive.");

            using var reader = new StreamReader(packEntry.Open());
            string content = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<Dictionary<string, Pack>>(content);
        }
        private static void ExtractPackIcon(ZipArchive zipArchive, string path)
        {
            var packEntry = zipArchive.GetEntry("pack.png");
            if (packEntry == null) return;
            packEntry.ExtractToFile(Path.Combine(path, "pack.png"), overwrite: true);
            Console.WriteLine("pack.png extracted");
        }
        private static void SavePackMetadata(Dictionary<string, Pack> packMetadata, string outputPath)
        {
            var newPack = new
            {
                pack = new Pack
                {
                    PackFormat = ResourcePackVersion,
                    Description = "\u00A74[CONVERTED]\u00A7r " + packMetadata["pack"].Description
                }
            };

            string json = JsonSerializer.Serialize(newPack, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(outputPath, "pack.mcmeta"), json);
            Console.WriteLine("created pack.mcmeta");
        }
        private static void ExtractFiles(ZipArchive zipArchive, string outputPath)
        {
            foreach (var entry in zipArchive.Entries.Where(s => !s.IsMacFile()))
            {
                if (entry.FullName.EndsWith("/"))
                {
                    // Ignore directories
                    continue;
                }

                // Item model file
                if (entry.FullName.Contains("models") ||
                    (entry.Name.EndsWith(".json") && entry.FullName.Contains("cit")))
                {
                    string targetPath = Path.Combine(outputPath, "assets/minecraft/models/item", entry.Name);
                    Console.WriteLine($"extracted model {targetPath.Replace(outputPath, "")}");

                    using StreamReader stream = new StreamReader(entry.Open());
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    string content = stream.ReadToEnd();

                    content = content.Replace("./", "item/");

                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                    if (data != null && data.ContainsKey("textures"))
                    {
                        // Извлечение textures и его преобразование в словарь
                        var textures = JsonSerializer.Deserialize<Dictionary<string, string>>(data["textures"].ToString());

                        // Создание нового словаря с модифицированными значениями
                        var updatedTextures = new Dictionary<string, string>();
                        foreach (var texture in textures)
                        {
                            string filename = Path.GetFileNameWithoutExtension(texture.Value); // Получаем имя файла
                            updatedTextures[texture.Key] = $"item/{filename}";
                        }

                        // Обновление textures в исходных данных
                        data["textures"] = updatedTextures;

                        // Сериализация обратно в JSON
                        content = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    }
                    else
                    {
                        Console.WriteLine($"{entry.FullName}: Incorrect model");
                    }

                    File.WriteAllText(targetPath, content);
                }

                // Textures or sounds
                if ((entry.Name.EndsWith(".mcmeta") && entry.Name != "pack.mcmeta") || entry.FullName.Contains("textures")
                    || (entry.Name.EndsWith(".png") && entry.FullName.Contains("cit")))
                {
                    string targetPath = Path.Combine(outputPath, "assets/minecraft/textures/item", entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    entry.ExtractToFile(targetPath, overwrite: true);
                    Console.WriteLine($"extracted texture {entry.Name}");
                }
                if (entry.FullName.Contains("sounds"))
                {
                    string targetPath = Path.Combine(outputPath, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    entry.ExtractToFile(targetPath, overwrite: true);
                    Console.WriteLine($"extracted sound {entry.Name}");
                }
                if (entry.FullName.Contains("lang"))
                {
                    string targetPath = Path.Combine(outputPath, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    entry.ExtractToFile(targetPath, overwrite: true);
                    Console.WriteLine($"extracted lang file {entry.Name}");
                }
            }
        }
        private static Dictionary<string, List<Dictionary<string, string>>> ProcessCitProperties(ZipArchive zipArchive)
        {
            var citEntries = zipArchive.Entries
                .Where(e => !e.IsMacFile() && e.FullName.Contains("assets/minecraft/optifine/cit") && e.Name.EndsWith(".properties") && e.Name != "cit.properties")
                .ToList();

            var itemDefinitions = new Dictionary<string, List<Dictionary<string, string>>>();

            foreach (var entry in citEntries)
            {
                using var reader = new StreamReader(entry.Open());
                var lines = reader.ReadToEnd().Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    .ToDictionary(
                        line => line.Split('=')[0],
                        line => line.Split('=')[1].Replace("\r", "")
                    );
                lines.Add("FILE_NAME", entry.Name);

                if (lines.ContainsKey("items")) lines["items"] = lines["items"].Replace("minecraft:", "");//todo: string.Remove
                if (lines.ContainsKey("matchItems")) lines["items"] = lines["matchItems"].Replace("minecraft:", "");//todo: string.Remove

                if (!lines.ContainsKey("items") && lines.ContainsKey("matchItems"))
                    lines["items"] = lines["matchItems"];//todo: string.Remove

                if (!lines.ContainsKey("model") && !lines.ContainsKey("texture"))
                {
                    var png = Path.GetFileNameWithoutExtension(entry.FullName) + ".png";
                    var checkPngPath = Path.Combine(Path.GetDirectoryName(entry.FullName)!, png).Replace("\\", "/");
                    if (zipArchive.GetEntry(checkPngPath) != null)
                        lines.Add("texture", Path.GetFileName(png));
                }

                foreach (var itemKey in lines["items"].Split(' '))
                {
                    var itemLines = new Dictionary<string, string>(lines);

                    if (!itemDefinitions.ContainsKey(itemKey))
                        itemDefinitions[itemKey] = new List<Dictionary<string, string>>();
                    itemDefinitions[itemKey].Add(itemLines);
                }
            }

            return itemDefinitions;
        }

        private static void GenerateItemJsonFiles(Dictionary<string, List<Dictionary<string, string>>> itemDefinitions, string outputPath)
        {
            int i = 0;
            string allNames = $"{outputPath}/names.txt";
            File.WriteAllText(allNames, $"{WaterMark}\r\nItems: {itemDefinitions.Count}\r\nUnique names:{itemDefinitions.Values.SelectMany(s => s).Sum(s => s.Count + 1)}");

            foreach (var (item, definitions) in itemDefinitions)
            {
                int j = 1;
                File.AppendAllText(allNames, $"\r\n{++i}) {item}\r\n");
                List<string> used = new();
                string component = "";

                Dictionary<string, List<int>> usedEnchantments = new();

                string cases = string.Join(",", definitions
                .Select(config =>
                {
                    if (config.Keys.Any(s => s.Contains("nbt.StoredEnchantments")))
                    {
                        Console.WriteLine($"SKIPPED {config["FILE_NAME"]}: TODO");
                        return null;
                    }
                    if (config.ContainsKey("enchantments") && !config.ContainsKey("enchantmentIDs"))
                    {
                        var indexOf = config["enchantments"].IndexOf(":", StringComparison.Ordinal);
                        config.Add("enchantmentIDs", config["enchantments"].Remove(0, indexOf + 1));
                    }

                    if (string.IsNullOrWhiteSpace(component))
                        component = config.FirstOrDefault(kvp => kvp.Key.StartsWith("nbt.") || kvp.Key.StartsWith("component")).Key?.Replace("nbt.", "").ToLower()!;
                    if (string.IsNullOrWhiteSpace(component) && config.ContainsKey("enchantmentIDs"))
                        component = "stored_enchantments";

                    string value = config.FirstOrDefault(kvp => kvp.Key.StartsWith("nbt.") || kvp.Key.StartsWith("component")).Value;
                    if (used.Contains(value)) return null;
                    if (value == "minecraft:empty") return null;

                    var hasModel = config.Any(s => s.Key.StartsWith("model"));
                    var confFileName = Path.GetFileNameWithoutExtension(config["FILE_NAME"]);
                    string model =
                        hasModel
                            ? config.First(s => s.Key.StartsWith("model")).Value.Replace(" ", "").Replace(".json", "")
                            : $"item/{confFileName}";
                    model = Path.GetFileName(model);
                    if (!hasModel && config.TryGetValue("texture", out var texture))
                    {
                        File.WriteAllText(
                            Path.Combine(outputPath, "assets/minecraft/models/item", $"{confFileName}.json"),
                            SampleItemModelTemplate.Replace("ITEM", Path.GetFileNameWithoutExtension(texture.Replace(".png", "")))
                        );
                    }

                    if (!model.StartsWith("item")) model = "item/" + model;

                    if (config.ContainsKey("enchantmentIDs"))
                    {
                        var enchantments = config["enchantmentIDs"]
                            .Remove(0, config["enchantmentIDs"].IndexOf(":", StringComparison.Ordinal) + 1)
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .DistinctBy(s => s); // Удаляем дубликаты внутри одной конфигурации.

                        var enchantmentCases = enchantments
                            .SelectMany(enchantment =>
                            {
                                // спасите меня бля пожалуйста
                                if (config.ContainsKey("enchantmentLevels"))
                                {
                                    var level = int.Parse(config["enchantmentLevels"].Split(' ')[0]);
                                    if (usedEnchantments.TryGetValue(enchantment, out var levels) && levels.Contains(level)) return null;
                                    if (!usedEnchantments.ContainsKey(enchantment))
                                        usedEnchantments.Add(enchantment, [level]);
                                    else
                                        usedEnchantments[enchantment].Add(level);
                                    return new List<string>()
                                        {
                                            CaseTemplate
                                                .Replace("MODEL", $"item/{(config.TryGetValue("texture", out var value1) ? Path.GetFileNameWithoutExtension(value1) : enchantment)}{"" /*_level{level}*/}")
                                                .Replace("WHEN", JsonSerializer.Serialize(new Dictionary<string, int> { { $"minecraft:{enchantment}", level } }))
                                        };
                                }

                                return Enumerable.Range(1, 5).Select(s =>
                                {
                                    if (usedEnchantments.TryGetValue(enchantment, out var levels) && levels.Contains(s)) return null;
                                    if (!usedEnchantments.ContainsKey(enchantment))
                                        usedEnchantments.Add(enchantment, [s]);
                                    else
                                        usedEnchantments[enchantment].Add(s);

                                    return CaseTemplate
                                        .Replace("MODEL",
                                            $"item/{(config.TryGetValue("texture", out var value1) ? Path.GetFileNameWithoutExtension(value1) : enchantment)}{"" /*_level{level}*/}")
                                        .Replace("WHEN", JsonSerializer.Serialize(new Dictionary<string, int>
                                        {
                                            {
                                                $"minecraft:{enchantment}", s
                                            }
                                        }));
                                });
                            })
                                .Where(caseString => caseString != null);


                        used.Add(config["enchantmentIDs"]);
                        File.AppendAllText(allNames, $"\t{i}.{j++}) minecraft:{config["enchantmentIDs"]}\r\n");
                        return string.Join(",", enchantmentCases);
                    }

                    File.AppendAllText(allNames, $"\t{i}.{j++}) {value.GetName()}\r\n");
                    used.Add(value);
                    return CaseTemplate
                        .Replace("MODEL", model)
                        .Replace("WHEN", JsonSerializer.Serialize((object)(
                            component switch
                            {
                                "potion_contents" or "potion" => new Dictionary<string, string> { { "potion", value } },
                                "display.name" => value.GetName(),
                                "components.entity_data.variant" => value,
                                //"stored_enchantments" => new Dictionary<string, string> { { component, value } },
                                _ => throw new NotImplementedException($"Unknown component type: {component}")
                            }), new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
                }).Where(s => s is not null));

                string itemJson = string.IsNullOrWhiteSpace(cases)
                    ? EmptyCaseTemplate.Replace("ITEM", item)
                    : DefinitionTemplate
                        .Replace("NBT_NAME", component switch
                        {
                            "potion" => "minecraft:potion_contents",
                            "display.name" => "minecraft:custom_name",
                            "components.entity_data.variant" => "minecraft:painting/variant",
                            "stored_enchantments" => "minecraft:stored_enchantments",
                            _ => throw new NotImplementedException($"Unknown component type: {component}")
                        })
                        .Replace("BLOCK_OR_ITEM", "item")
                        .Replace("CASES", cases)
                        .Replace("ITEM", item)
                        .Replace("FALLBACK", GetFallbackModel(item));

                var path2 = $"assets/minecraft/items/{item}.json";
                string outputFilePath = Path.Combine(outputPath, path2);
                Console.WriteLine($"generated item {item} at {path2}");

                File.WriteAllText(outputFilePath, itemJson);
            }
        }

    }
}
